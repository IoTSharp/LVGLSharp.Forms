using LVGLSharp.Runtime.MacOs;
using Xunit;

namespace LVGLSharp.Headless.Tests;

public sealed class MacOsViewOptionsTests
{
    [Fact]
    public void MacOsViewOptions_Validate_AcceptsPositiveValues()
    {
        var options = new MacOsViewOptions
        {
            Title = "MacOs Skeleton",
            Width = 1024,
            Height = 768,
            Dpi = 110f,
        };

        options.Validate();
    }

    [Fact]
    public void MacOsSurfaceSkeleton_Create_TracksCreatedState()
    {
        var surface = new MacOsSurfaceSkeleton(new MacOsViewOptions
        {
            Title = "MacOs Skeleton",
            Width = 640,
            Height = 480,
            Dpi = 96f,
        });

        Assert.False(surface.IsCreated);
        surface.Create();
        Assert.True(surface.IsCreated);
        surface.Dispose();
        Assert.False(surface.IsCreated);
    }
}