using LVGLSharp.Runtime.Remote.Rdp;
using LVGLSharp.Runtime.Remote.Vnc;
using LVGLSharp.Runtime.Headless;
using LVGLSharp.Runtime.Remote.Headless;

namespace LVGLSharp.Runtime.Remote;

public static class RemoteRuntimeFactory
{
    public static IRemoteFrameSource CreateHeadlessFrameSource(OffscreenView view, bool ownsView = false)
    {
        return new HeadlessRemoteFrameSource(view, ownsView);
    }

    public static IRemoteTransport CreateVncTransport(VncSessionOptions options)
    {
        return new Vnc.VncTransport(options);
    }

    public static IRemoteTransport CreateRdpTransport(RdpSessionOptions options)
    {
        return new RdpTransportSkeleton(options);
    }
}