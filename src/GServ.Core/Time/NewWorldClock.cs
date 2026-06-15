using GServ.Core.Compatibility;

namespace GServ.Core.Time;

/// <summary>
/// Computes the legacy New World time value used by PLO_NEWWORLDTIME.
/// Source: Server::calculateNWTime in ai_resources/GServer-CPP-ORIGINAL/server/src/Server.cpp.
/// </summary>
public static class NewWorldClock
{
    public static uint FromUnixTimeSeconds(long unixTimeSeconds)
    {
        var elapsed = unixTimeSeconds - CompatibilityConstants.NewWorldTimeEpochUnixSeconds;
        return (uint)(elapsed / CompatibilityConstants.NewWorldTimeDivisorSeconds);
    }

    public static uint Now(TimeProvider? timeProvider = null)
    {
        var now = (timeProvider ?? TimeProvider.System).GetUtcNow().ToUnixTimeSeconds();
        return FromUnixTimeSeconds(now);
    }
}
