namespace GServ.Core.Configuration;

/// <summary>
/// Cached server settings whose defaults are visible to client behavior.
/// Source: ExternalServerCachedSettings in ai_resources/GServer-CPP-ORIGINAL/server/include/Server.h.
/// </summary>
public sealed record ServerCompatibilityOptions
{
    public static ServerCompatibilityOptions Default { get; } = new();

    public uint MaxPlayers { get; init; } = 128;
    public bool SleepWhenNoPlayers { get; init; } = true;
    public string UnstickMeLevel { get; init; } = "onlinestartlocal.nw";
    public float UnstickMeX { get; init; } = 30.0f;
    public float UnstickMeY { get; init; } = 30.5f;
    public int UnstickMeSeconds { get; init; } = 30;
    public bool EnableBushItemDrops { get; init; } = true;
    public bool EnableVaseItemDrops { get; init; } = true;
    public bool DisableItemDropping { get; init; }
    public bool EnableInsideSyncDistance { get; init; }
    public uint SyncDistanceX { get; init; } = 192;
    public uint SyncDistanceY { get; init; } = 192;
    public uint EventDistance { get; init; } = 64;
    public uint TriggerDistance { get; init; } = 10;
    public bool SendTriggerActionsToPlayers { get; init; } = true;
    public bool EnableFlagCropping { get; init; } = true;
    public bool DisableExplosions { get; init; }
    public bool EnableClientsidePushPull { get; init; } = true;
    public ushort TileRespawnTime { get; init; } = 15;
    public bool EnableIdleDisconnect { get; init; } = true;
    public int IdleTimeoutSeconds { get; init; } = 1_200;
    public bool EnablePermanentTileChanges { get; init; }
    public bool SaveTileChangesToLevelFile { get; init; }
    public bool EnableDefaultWeapons { get; init; } = true;
    public byte MaxHeartLimit { get; init; } = 3;
    public bool EnableApSystem { get; init; } = true;
    public int[] ApSystemThresholdSeconds { get; init; } = [30, 90, 300, 600, 1_200];
}
