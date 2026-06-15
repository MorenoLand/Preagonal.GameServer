namespace GServ.Protocol;

/// <summary>
/// Names of packet families found in the C++ packet handlers.
/// Numeric values are intentionally not filled in until IEnums.h is recovered.
/// </summary>
public static class PacketIdSourceStatus
{
    public const string AuthoritativeEnumHeader = "IEnums.h";
    public const bool NumericPacketIdsRecovered = false;
    public const string MissingNumericPacketIdsReason = "The original C++ checkout references IEnums.h but does not include it.";
}

/// <summary>
/// Source status for external C++ protocol dependencies that are required before
/// byte-compatible networking can be implemented.
/// </summary>
public static class ProtocolDependencySourceStatus
{
    public const string ExpectedSourceDependency = "gs2lib";
    public const string ExpectedSourceIncludePath = "gs2lib_SOURCE_DIR/include";
    public const bool IEnumsHeaderRecovered = false;
    public const bool CStringHeaderRecovered = false;
    public const bool CEncryptionHeaderRecovered = false;
    public const bool CFileQueueHeaderRecovered = false;
    public const bool CSocketHeaderRecovered = false;
}
