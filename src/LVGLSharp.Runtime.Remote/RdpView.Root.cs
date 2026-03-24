namespace LVGLSharp.Runtime.Remote;

public unsafe sealed class RdpView : RemoteViewBase
{
    private const int DefaultWidth = 800;
    private const int DefaultHeight = 600;

    public RdpView()
        : this(new Rdp.RdpTransportSkeleton(new Rdp.RdpSessionOptions()))
    {
    }

    public RdpView(Rdp.RdpSessionOptions options)
        : this(new Rdp.RdpTransportSkeleton(options))
    {
    }

    public RdpView(Rdp.RdpTransportSkeleton transport)
        : base(transport, transport.Options, DefaultWidth, DefaultHeight, publishFrames: false)
    {
    }

    public new Rdp.RdpTransportSkeleton Transport => (Rdp.RdpTransportSkeleton)base.Transport;
}
