using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace LVGLSharp.Runtime.Remote.Vnc
{
    /// <summary>
    /// 极简 VNC 服务器实现：监听端口、推送帧、接收输入。
    /// 仅支持最基础的 RFB 协议流程，适合演示和自定义扩展。
    /// </summary>
    public sealed class VncTransport : RemoteTransportBase, IDisposable
    {
        private readonly VncSessionOptions _options;
        private TcpListener? _listener;
        private CancellationTokenSource? _cts;
        private Task? _acceptTask;
        private readonly List<TcpClient> _clients = new();
        private readonly object _clientsLock = new();
        private RemoteFrame? _latestFrame;
        private Task? _framePushTask;

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
using LVGLSharp.Interop;

        public void Start()
        {
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
        }

        private async Task AcceptLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    var client = await _listener!.AcceptTcpClientAsync(token);
                    lock (_clientsLock) _clients.Add(client);
                    _ = Task.Run(() => HandleClient(client, token));
                }
                catch (Exception)
                {
                    if (token.IsCancellationRequested) break;
                }
            }
        }

        private async Task HandleClient(TcpClient client, CancellationToken token)
        {
            try
            {
                using (client)
                using (var stream = client.GetStream())
                {
                    // 1. 发送 RFB 协议版本
                    var version = "RFB 003.003\n";
                    var versionBytes = System.Text.Encoding.ASCII.GetBytes(version);
                    await stream.WriteAsync(versionBytes, 0, versionBytes.Length, token);
                    await stream.FlushAsync(token);
                    // 2. 简单握手（无认证）
                    await stream.WriteAsync(new byte[] { 1, 1 }, 0, 2, token); // No auth
                    await stream.FlushAsync(token);
                    var clientInit = new byte[1];
                    await stream.ReadAsync(clientInit, 0, 1, token); // ClientInit
                    // 3. 发送服务端初始化（分辨率、像素格式等）
                    var width = (ushort)_options.Width;
                    var height = (ushort)_options.Height;
                    var serverInit = new byte[24];
                    serverInit[0] = (byte)(width >> 8);
                    serverInit[1] = (byte)(width & 0xFF);
                    serverInit[2] = (byte)(height >> 8);
                    serverInit[3] = (byte)(height & 0xFF);
                    // 其余像素格式、名称等可补充
                    await stream.WriteAsync(serverInit, 0, serverInit.Length, token);
                    await stream.FlushAsync(token);

                    // 4. 客户端保持连接，循环读取输入事件
                    var inputBuffer = new byte[32];
                    while (!token.IsCancellationRequested)
                    {
                        // RFB 客户端消息类型：0=SetPixelFormat, 2=SetEncodings, 3=FramebufferUpdateRequest, 4=KeyEvent, 5=PointerEvent, 6=ClientCutText
                        int read = await stream.ReadAsync(inputBuffer, 0, 1, token);
                        if (read == 0) break; // 客户端断开
                        byte msgType = inputBuffer[0];
                        switch (msgType)
                        {
                            case 4: // KeyEvent
                                await stream.ReadAsync(inputBuffer, 1, 7, token); // 1+7=8字节
                                bool down = inputBuffer[1] != 0;
                                uint key = (uint)((inputBuffer[4] << 24) | (inputBuffer[5] << 16) | (inputBuffer[6] << 8) | inputBuffer[7]);
                                // 转为 RemoteInputEvent 并注入 LVGL
                                InjectKeyEvent(key, down);
                                break;
                            case 5: // PointerEvent
                                await stream.ReadAsync(inputBuffer, 1, 5, token); // 1+5=6字节
                                byte buttonMask = inputBuffer[1];
                                ushort x = (ushort)((inputBuffer[2] << 8) | inputBuffer[3]);
                                ushort y = (ushort)((inputBuffer[4] << 8) | inputBuffer[5]);
                                InjectPointerEvent(x, y, buttonMask);
                                break;
                            default:
                                // 其它消息类型暂不处理，直接跳过
                                await Task.Delay(10, token);
                                break;
                        }
                    }
                }
            }
            catch (Exception)
            {
                // 忽略单个客户端异常
            }
            finally
            {
                lock (_clientsLock) _clients.Remove(client);
            }

        // 静态输入事件注入方法，AOT安全
        private static void InjectKeyEvent(uint key, bool down)
        {
            var indev = VncView.GetKeyboardIndev();
            if (indev == null) return;
            var state = down ? lv_indev_state_t.LV_INDEV_STATE_PRESSED : lv_indev_state_t.LV_INDEV_STATE_RELEASED;
            // 构造 lv_indev_data_t 并注入
            var data = new lv_indev_data_t
            {
                state = state,
                key = key
            };
            // 这里只能通过事件注入，实际 LVGL 需有专用 API
            LVGLSharp.Interop.Lvgl.lv_indev_send_event(indev, lv_event_code_t.LV_EVENT_KEY, &data);
        }

        private static void InjectPointerEvent(ushort x, ushort y, byte buttonMask)
        {
            var indev = VncView.GetPointerIndev();
            if (indev == null) return;
            var state = (buttonMask & 1) != 0 ? lv_indev_state_t.LV_INDEV_STATE_PRESSED : lv_indev_state_t.LV_INDEV_STATE_RELEASED;
            var data = new lv_indev_data_t
            {
                state = state,
                point = new lv_point_t { x = x, y = y }
            };
            LVGLSharp.Interop.Lvgl.lv_indev_send_event(indev, lv_event_code_t.LV_EVENT_PRESSED, &data);
        }
        }

        public override Task SendFrameAsync(RemoteFrame frame, CancellationToken cancellationToken = default)
        {
            // 记录最新帧，FramePushLoop 会自动分发
            _latestFrame = frame;
            return Task.CompletedTask;
        }

        public override Task SendInputAsync(RemoteInputEvent inputEvent, CancellationToken cancellationToken = default)
        {
            // TODO: 处理输入事件
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            Stop();

        private async Task FramePushLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                RemoteFrame? frame = _latestFrame;
                if (frame != null)
                {
                    List<TcpClient> clientsCopy;
                    lock (_clientsLock)
                        clientsCopy = new List<TcpClient>(_clients);
                    foreach (var client in clientsCopy)
                    {
                        try
                        {
                            if (client.Connected)
                            {
                                var stream = client.GetStream();
                                // 按RFB协议推送原始像素数据（极简实现，未分块/未压缩）
                                // 这里只推送全量 ARGB8888 像素，客户端需自定义解析
                                // 实际生产应严格按RFB协议分块、压缩、同步
                                var pixelBytes = frame.Argb8888Bytes;
                                await stream.WriteAsync(pixelBytes, 0, pixelBytes.Length, token);
                                await stream.FlushAsync(token);
                            }
                        }
                        catch
                        {
                            // 忽略单个客户端异常
                        }
                    }
                }
                await Task.Delay(100, token);
            }
        }
        }
    }
}
