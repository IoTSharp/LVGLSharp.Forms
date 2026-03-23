using LVGLSharp.Runtime.Headless;

namespace LVGLSharp.Runtime.Remote.Headless;

public sealed class HeadlessRemoteFrameSource : IRemoteFrameSource, IDisposable
{
    private readonly OffscreenView _view;
    private readonly bool _ownsView;

    public HeadlessRemoteFrameSource(OffscreenView view, bool ownsView = false)
    {
        _view = view ?? throw new ArgumentNullException(nameof(view));
        _ownsView = ownsView;
    }

    public OffscreenView View => _view;

    public RemoteFrame CaptureFrame()
    {
        using var image = _view.RenderSnapshot();
        var bytes = new byte[image.Width * image.Height * 4];

        image.ProcessPixelRows(accessor =>
        {
            for (var y = 0; y < image.Height; y++)
            {
                var row = accessor.GetRowSpan(y);
                var rowOffset = y * image.Width * 4;
                for (var x = 0; x < image.Width; x++)
                {
                    var pixel = row[x];
                    var offset = rowOffset + x * 4;
                    bytes[offset] = pixel.A;
                    bytes[offset + 1] = pixel.R;
                    bytes[offset + 2] = pixel.G;
                    bytes[offset + 3] = pixel.B;
                }
            }
        });

        return new RemoteFrame(image.Width, image.Height, bytes);
    }

    public bool TryHandleInput(RemoteInputEvent inputEvent)
    {
        ArgumentNullException.ThrowIfNull(inputEvent);
        return false;
    }

    public void Dispose()
    {
        if (_ownsView)
        {
            _view.Dispose();
        }
    }
}