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
