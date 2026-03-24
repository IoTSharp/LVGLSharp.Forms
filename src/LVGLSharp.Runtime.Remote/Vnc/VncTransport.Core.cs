using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using LVGLSharp.Interop;

namespace LVGLSharp.Runtime.Remote.Vnc;

public sealed class VncTransport : RemoteTransportBase, IRemoteHostedTransport, IDisposable
{
    private sealed class ClientSession
    {
        public ClientSession(TcpClient client, NetworkStream stream)
        {
            Client = client;
            Stream = stream;
        }

        public TcpClient Client { get; }

        public NetworkStream Stream { get; }

        public SemaphoreSlim SendLock { get; } = new(1, 1);

        public bool UpdateRequested { get; set; } = true;

        public byte LastButtonMask { get; set; }

        public int LastFrameRevision { get; set; } = -1;
    }

    private readonly VncSessionOptions _options;
    private readonly List<ClientSession> _clients = new();
    private readonly object _clientsLock = new();
    private TcpListener? _listener;
    private CancellationTokenSource? _cts;
    private Task? _acceptTask;
    private Task? _framePushTask;
    private RemoteFrame? _latestFrame;
    private int _latestFrameRevision;
    private IRemoteInputSink? _inputSink;

    public VncTransport(VncSessionOptions options)
        : base("vnc", new RemoteTransportCapabilities(
            SupportsClipboardSync: false,
            SupportsPointerInput: true,
            SupportsKeyboardInput: true,
            SupportsFrameStreaming: true))
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _options.Validate();
    }

    public VncSessionOptions Options => _options;

    public void Start()
    {
        if (_cts != null)
        {
            return;
        }

        _cts = new CancellationTokenSource();
        _listener = new TcpListener(IPAddress.Parse(_options.Host), _options.Port);
        _listener.Start();
        _acceptTask = Task.Run(() => AcceptLoop(_cts.Token));
        _framePushTask = Task.Run(() => FramePushLoop(_cts.Token));
    }

    public void Stop()
    {
        _cts?.Cancel();
        _listener?.Stop();

        lock (_clientsLock)
        {
            foreach (var session in _clients)
            {
                try
                {
                    session.Client.Close();
                }
                catch
                {
                }
            }

            _clients.Clear();
        }

        _cts?.Dispose();
        _cts = null;
        _listener = null;
    }

    void IRemoteHostedTransport.AttachInputSink(IRemoteInputSink inputSink)
    {
        _inputSink = inputSink ?? throw new ArgumentNullException(nameof(inputSink));
    }

    public override Task SendFrameAsync(RemoteFrame frame, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(frame);
        _latestFrame = frame;
        Interlocked.Increment(ref _latestFrameRevision);
        return Task.CompletedTask;
    }

    public override Task SendInputAsync(RemoteInputEvent inputEvent, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(inputEvent);
        QueueInput(inputEvent);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        Stop();
    }

    private async Task AcceptLoop(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                var client = await _listener!.AcceptTcpClientAsync(token);
                _ = Task.Run(() => HandleClient(client, token), token);
            }
            catch (Exception)
            {
                if (token.IsCancellationRequested)
                {
                    break;
                }
            }
        }
    }

    private async Task HandleClient(TcpClient client, CancellationToken token)
    {
        ClientSession? session = null;
        try
        {
            using (client)
            using (var stream = client.GetStream())
            {
                session = new ClientSession(client, stream);
                lock (_clientsLock)
                {
                    _clients.Add(session);
                }

                await PerformHandshakeAsync(stream, token);

                var inputBuffer = new byte[32];
                while (!token.IsCancellationRequested)
                {
                    var read = await stream.ReadAsync(inputBuffer.AsMemory(0, 1), token);
                    if (read == 0)
                    {
                        break;
                    }

                    switch (inputBuffer[0])
                    {
                        case 0:
                            await ReadExactAsync(stream, inputBuffer, 0, 19, token);
                            break;
                        case 2:
                            await ReadExactAsync(stream, inputBuffer, 0, 3, token);
                            var encodings = (inputBuffer[1] << 8) | inputBuffer[2];
                            if (encodings > 0)
                            {
                                var encodingBytes = new byte[encodings * 4];
                                await ReadExactAsync(stream, encodingBytes, 0, encodingBytes.Length, token);
                            }
                            break;
                        case 3:
                            await ReadExactAsync(stream, inputBuffer, 0, 9, token);
                            session.UpdateRequested = true;
                            await TrySendFrameAsync(session, token);
                            break;
                        case 4:
                            await ReadExactAsync(stream, inputBuffer, 0, 7, token);
                            var isDown = inputBuffer[0] != 0;
                            var key = (uint)((inputBuffer[3] << 24) | (inputBuffer[4] << 16) | (inputBuffer[5] << 8) | inputBuffer[6]);
                            QueueInput(new RemoteInputEvent(
                                isDown ? RemoteInputEventKind.KeyDown : RemoteInputEventKind.KeyUp,
                                0,
                                0,
                                0,
                                NormalizeVncKey(key),
                                null));
                            break;
                        case 5:
                            await ReadExactAsync(stream, inputBuffer, 0, 5, token);
                            var buttonMask = inputBuffer[0];
                            var x = (ushort)((inputBuffer[1] << 8) | inputBuffer[2]);
                            var y = (ushort)((inputBuffer[3] << 8) | inputBuffer[4]);
                            QueueInput(BuildPointerEvent(session, x, y, buttonMask));
                            session.LastButtonMask = buttonMask;
                            break;
                        case 6:
                            await ReadExactAsync(stream, inputBuffer, 0, 7, token);
                            var textLength = (inputBuffer[3] << 24) | (inputBuffer[4] << 16) | (inputBuffer[5] << 8) | inputBuffer[6];
                            if (textLength > 0)
                            {
                                var cutText = new byte[textLength];
                                await ReadExactAsync(stream, cutText, 0, cutText.Length, token);
                            }
                            break;
                        default:
                            await Task.Delay(10, token);
                            break;
                    }
                }
            }
        }
        catch (Exception)
        {
        }
        finally
        {
            if (session != null)
            {
                lock (_clientsLock)
                {
                    _clients.Remove(session);
                }
            }
        }
    }

    private async Task PerformHandshakeAsync(NetworkStream stream, CancellationToken token)
    {
        var versionBytes = System.Text.Encoding.ASCII.GetBytes("RFB 003.003\n");
        await stream.WriteAsync(versionBytes.AsMemory(0, versionBytes.Length), token);

        var clientVersion = new byte[12];
        await ReadExactAsync(stream, clientVersion, 0, clientVersion.Length, token);

        var requiresAuth = !string.IsNullOrEmpty(_options.Password);
        var securityType = requiresAuth
            ? new byte[] { 0, 0, 0, 2 }
            : new byte[] { 0, 0, 0, 1 };
        await stream.WriteAsync(securityType.AsMemory(0, securityType.Length), token);

        if (requiresAuth)
        {
            var authSucceeded = await PerformVncAuthenticationAsync(stream, _options.Password!, token);
            if (!authSucceeded)
            {
                throw new InvalidOperationException("VNC password authentication failed.");
            }
        }

        var clientInit = new byte[1];
        await ReadExactAsync(stream, clientInit, 0, 1, token);

        var nameBytes = System.Text.Encoding.ASCII.GetBytes("LVGLSharp VNC");
        var serverInit = new byte[24 + nameBytes.Length];
        WriteUInt16(serverInit, 0, (ushort)_options.Width);
        WriteUInt16(serverInit, 2, (ushort)_options.Height);
        serverInit[4] = 32;
        serverInit[5] = 24;
        serverInit[6] = 0;
        serverInit[7] = 1;
        WriteUInt16(serverInit, 8, 255);
        WriteUInt16(serverInit, 10, 255);
        WriteUInt16(serverInit, 12, 255);
        serverInit[14] = 16;
        serverInit[15] = 8;
        serverInit[16] = 0;
        WriteInt32(serverInit, 20, nameBytes.Length);
        Buffer.BlockCopy(nameBytes, 0, serverInit, 24, nameBytes.Length);
        await stream.WriteAsync(serverInit.AsMemory(0, serverInit.Length), token);
        await stream.FlushAsync(token);
    }

    private static async Task<bool> PerformVncAuthenticationAsync(NetworkStream stream, string password, CancellationToken token)
    {
        var challenge = new byte[16];
        RandomNumberGenerator.Fill(challenge);
        await stream.WriteAsync(challenge.AsMemory(0, challenge.Length), token);

        var response = new byte[16];
        await ReadExactAsync(stream, response, 0, response.Length, token);

        var expectedResponse = EncryptVncChallenge(challenge, password);
        var success = CryptographicOperations.FixedTimeEquals(expectedResponse, response);

        var securityResult = new byte[4];
        WriteInt32(securityResult, 0, success ? 0 : 1);
        await stream.WriteAsync(securityResult.AsMemory(0, securityResult.Length), token);
        await stream.FlushAsync(token);
        return success;
    }

    private RemoteInputEvent BuildPointerEvent(ClientSession session, ushort x, ushort y, byte buttonMask)
    {
        var buttons = ConvertButtonMask(buttonMask);
        var previousButtons = ConvertButtonMask(session.LastButtonMask);
        var kind = RemoteInputEventKind.PointerMove;

        if (buttonMask == 0 && session.LastButtonMask != 0)
        {
            kind = RemoteInputEventKind.PointerUp;
            buttons = previousButtons;
        }
        else if (buttonMask != 0 && session.LastButtonMask == 0)
        {
            kind = RemoteInputEventKind.PointerDown;
        }

        return new RemoteInputEvent(kind, x, y, buttons, 0, null);
    }

    private async Task FramePushLoop(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            List<ClientSession> clientsCopy;
            lock (_clientsLock)
            {
                clientsCopy = new List<ClientSession>(_clients);
            }

            foreach (var session in clientsCopy)
            {
                try
                {
                    await TrySendFrameAsync(session, token);
                }
                catch
                {
                }
            }

            await Task.Delay(15, token);
        }
    }

    private void QueueInput(RemoteInputEvent inputEvent)
    {
        _inputSink?.PostInput(inputEvent);
    }

    private async Task TrySendFrameAsync(ClientSession session, CancellationToken token)
    {
        if (!session.UpdateRequested)
        {
            return;
        }

        var frame = _latestFrame;
        if (frame == null)
        {
            return;
        }

        var frameRevision = Volatile.Read(ref _latestFrameRevision);
        if (session.LastFrameRevision == frameRevision)
        {
            return;
        }

        var pixelBytes = ConvertArgbToBgrx(frame.Argb8888Bytes);
        var header = new byte[16];
        header[0] = 0;
        header[1] = 0;
        WriteUInt16(header, 2, 1);
        WriteUInt16(header, 4, 0);
        WriteUInt16(header, 6, 0);
        WriteUInt16(header, 8, (ushort)frame.Width);
        WriteUInt16(header, 10, (ushort)frame.Height);
        WriteInt32(header, 12, 0);

        await session.SendLock.WaitAsync(token);
        try
        {
            await session.Stream.WriteAsync(header.AsMemory(0, header.Length), token);
            await session.Stream.WriteAsync(pixelBytes.AsMemory(0, pixelBytes.Length), token);
            await session.Stream.FlushAsync(token);
            session.LastFrameRevision = frameRevision;
            session.UpdateRequested = false;
        }
        finally
        {
            session.SendLock.Release();
        }
    }

    private static async Task ReadExactAsync(NetworkStream stream, byte[] buffer, int offset, int count, CancellationToken token)
    {
        while (count > 0)
        {
            var read = await stream.ReadAsync(buffer.AsMemory(offset, count), token);
            if (read == 0)
            {
                throw new IOException("VNC client disconnected.");
            }

            offset += read;
            count -= read;
        }
    }

    private static byte[] EncryptVncChallenge(byte[] challenge, string password)
    {
        var key = CreatePasswordKey(password);
        using var des = DES.Create();
        des.Mode = CipherMode.ECB;
        des.Padding = PaddingMode.None;
        des.Key = key;

        using var encryptor = des.CreateEncryptor();
        return encryptor.TransformFinalBlock(challenge, 0, challenge.Length);
    }

    private static byte[] CreatePasswordKey(string password)
    {
        var passwordBytes = System.Text.Encoding.Latin1.GetBytes(password);
        var key = new byte[8];
        var count = Math.Min(passwordBytes.Length, key.Length);
        Array.Copy(passwordBytes, key, count);

        for (var i = 0; i < key.Length; i++)
        {
            key[i] = ReverseBits(key[i]);
        }

        return key;
    }

    private static byte ReverseBits(byte value)
    {
        value = (byte)(((value & 0xF0) >> 4) | ((value & 0x0F) << 4));
        value = (byte)(((value & 0xCC) >> 2) | ((value & 0x33) << 2));
        value = (byte)(((value & 0xAA) >> 1) | ((value & 0x55) << 1));
        return value;
    }

    private static uint NormalizeVncKey(uint key)
    {
        return key switch
        {
            0xFF08 => (uint)lv_key_t.LV_KEY_BACKSPACE,
            0xFF09 => (uint)lv_key_t.LV_KEY_NEXT,
            0xFF0D => (uint)lv_key_t.LV_KEY_ENTER,
            0xFF1B => (uint)lv_key_t.LV_KEY_ESC,
            0xFF50 => (uint)lv_key_t.LV_KEY_HOME,
            0xFF51 => (uint)lv_key_t.LV_KEY_LEFT,
            0xFF52 => (uint)lv_key_t.LV_KEY_UP,
            0xFF53 => (uint)lv_key_t.LV_KEY_RIGHT,
            0xFF54 => (uint)lv_key_t.LV_KEY_DOWN,
            0xFF55 => (uint)lv_key_t.LV_KEY_PREV,
            0xFF56 => (uint)lv_key_t.LV_KEY_NEXT,
            0xFF57 => (uint)lv_key_t.LV_KEY_END,
            0xFF63 => (uint)lv_key_t.LV_KEY_DEL,
            0xFFFF => (uint)lv_key_t.LV_KEY_DEL,
            _ when key <= 0x7Fu => key,
            _ => 0,
        };
    }

    private static uint ConvertButtonMask(byte buttonMask)
    {
        uint buttons = 0;
        if ((buttonMask & 0x01) != 0)
        {
            buttons |= 1;
        }

        if ((buttonMask & 0x04) != 0)
        {
            buttons |= 2;
        }

        if ((buttonMask & 0x02) != 0)
        {
            buttons |= 4;
        }

        return buttons;
    }

    private static byte[] ConvertArgbToBgrx(byte[] argbBytes)
    {
        var output = new byte[argbBytes.Length];
        for (var i = 0; i < argbBytes.Length; i += 4)
        {
            output[i] = argbBytes[i + 3];
            output[i + 1] = argbBytes[i + 2];
            output[i + 2] = argbBytes[i + 1];
            output[i + 3] = 0;
        }

        return output;
    }

    private static void WriteUInt16(byte[] buffer, int offset, ushort value)
    {
        buffer[offset] = (byte)(value >> 8);
        buffer[offset + 1] = (byte)(value & 0xFF);
    }

    private static void WriteInt32(byte[] buffer, int offset, int value)
    {
        buffer[offset] = (byte)((value >> 24) & 0xFF);
        buffer[offset + 1] = (byte)((value >> 16) & 0xFF);
        buffer[offset + 2] = (byte)((value >> 8) & 0xFF);
        buffer[offset + 3] = (byte)(value & 0xFF);
    }
}
