using GServ.Core.Configuration;

namespace GServ.Protocol.Tests;

public sealed class CompatibilityDefaultsTests
{
    [Fact]
    public void ServerCompatibilityOptionsMirrorCppCachedDefaults()
    {
        var options = ServerCompatibilityOptions.Default;

        Assert.Equal(128u, options.MaxPlayers);
        Assert.True(options.SleepWhenNoPlayers);
        Assert.Equal("onlinestartlocal.nw", options.UnstickMeLevel);
        Assert.Equal(30.0f, options.UnstickMeX);
        Assert.Equal(30.5f, options.UnstickMeY);
        Assert.Equal(30, options.UnstickMeSeconds);
        Assert.True(options.EnableFlagCropping);
        Assert.Equal(1200, options.IdleTimeoutSeconds);
        Assert.Equal([30, 90, 300, 600, 1200], options.ApSystemThresholdSeconds);
    }
}
