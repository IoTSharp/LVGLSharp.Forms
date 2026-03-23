using LVGLSharp.Runtime.Headless;
using LVGLSharp.Runtime.Remote;
using SixLabors.ImageSharp.PixelFormats;
using Xunit;

namespace LVGLSharp.Headless.Tests;

public sealed class HeadlessRemoteFrameSourceTests
{
    [Fact]
    public void CreateHeadlessFrameSource_CanCaptureArgbFrame()
    {
        var view = new OffscreenView(new OffscreenOptions
        {
            Width = 32,
            Height = 16,
            Dpi = 96f,
            BackgroundColor = new Rgba32(0x20, 0x40, 0x60, 0xFF),
        });

        using (view)
        {
            view.Open();

            var source = RemoteRuntimeFactory.CreateHeadlessFrameSource(view);
            var frame = source.CaptureFrame();

            Assert.Equal(32, frame.Width);
            Assert.Equal(16, frame.Height);
            Assert.Equal(32 * 16 * 4, frame.Argb8888Bytes.Length);
            Assert.Equal(32 * 4, frame.Stride);
        }
    }
}