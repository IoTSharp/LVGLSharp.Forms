using LVGLSharp.Runtime.MacOs;
using Xunit;

namespace LVGLSharp.Headless.Tests;

public sealed class MacOsDiagnosticsTests
{
    [Fact]
    public void MacOsFrameBuffer_AllocatesExpectedSize()
    {
        var buffer = new MacOsFrameBuffer(100, 50);

        Assert.Equal(100, buffer.Width);
        Assert.Equal(50, buffer.Height);
        Assert.Equal(400, buffer.Stride);
        Assert.Equal(100 * 50 * 4, buffer.Argb8888Bytes.Length);
    }

    [Fact]
    public void MacOsView_DiagnosticsReflectConfiguredSize()
    {
        var view = new MacOsView(new MacOsViewOptions
        {
            Title = "macOS diag",
            Width = 320,
            Height = 240,
            Dpi = 96f,
        });

        using (view)
        {
            var diagnostics = view.Diagnostics;
            Assert.Equal("macOS diag", diagnostics.Title);
            Assert.Equal(320, diagnostics.Width);
            Assert.Equal(240, diagnostics.Height);
            Assert.False(diagnostics.SurfaceCreated);
            Assert.True(diagnostics.FrameBufferAllocated);
        }
    }
}