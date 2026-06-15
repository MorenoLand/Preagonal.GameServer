namespace GServ.Core.Compatibility;

/// <summary>
/// Compatibility constants identified from the original C++ server.
/// Source: ai_resources/GServer-CPP-ORIGINAL/server/src/Server.cpp and Player.cpp.
/// </summary>
public static class CompatibilityConstants
{
    public const int GraalAsciiOffset = 32;
    public const long NewWorldTimeEpochUnixSeconds = 981_048_814;
    public const int NewWorldTimeDivisorSeconds = 5;
    public const int ServerSocketUpdateWaitMicroseconds = 5_000;
    public const int TimedEventsIntervalSeconds = 1;
    public const int NewWorldTimeBroadcastIntervalSeconds = 5;
    public const int SaveIntervalSeconds = 60;
    public const int MaintenanceIntervalSeconds = 300;
    public const int PlayerNoDataTimeoutSeconds = 300;
    public const int PlayerAccountSaveIntervalSeconds = 300;
    public const int InvalidPacketDisconnectThreshold = 5;
    public const int PrivateMessageLimit = 1_024;
    public const int ServerSignatureByte = 73;
    public const int FileChunkThresholdBytes = 32_000;
    public const int LargeFileWarningBytes = 3 * 1_024 * 1_024;
    public const int LevelTileWidth = 64;
    public const int LevelTileHeight = 64;
    public const int LevelTileCount = LevelTileWidth * LevelTileHeight;
    public const int GMapSubLevelPixelSpan = 1_024;
    public const string AccountFileHeader = "GRACC001";
}
