namespace LVGLSharp.Runtime.Remote;

public unsafe sealed class VncView : RemoteViewBase
{
    public VncView()
        : this(new Vnc.VncTransport(new Vnc.VncSessionOptions()))
    {
    }

    public VncView(Vnc.VncSessionOptions options)
        : this(new Vnc.VncTransport(options))
    {
    }

    public VncView(Vnc.VncTransport transport)
        : base(transport, transport.Options, transport.Options.Width, transport.Options.Height)
    {
    }

    public new Vnc.VncTransport Transport => (Vnc.VncTransport)base.Transport;
}
