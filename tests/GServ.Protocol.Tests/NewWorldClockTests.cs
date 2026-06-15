using GServ.Core.Time;

namespace GServ.Protocol.Tests;

public sealed class NewWorldClockTests
{
    [Fact]
    public void NewWorldTimeUsesCppEpochAndFiveSecondTicks()
    {
        Assert.Equal(0u, NewWorldClock.FromUnixTimeSeconds(981_048_814));
        Assert.Equal(1u, NewWorldClock.FromUnixTimeSeconds(981_048_819));
        Assert.Equal(12u, NewWorldClock.FromUnixTimeSeconds(981_048_874));
    }
}
