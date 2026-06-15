namespace GServ.Protocol;

/// <summary>
/// Wire-level constants directly confirmed from the C++ protocol layer.
/// Source: ai_resources/GServer-CPP-ORIGINAL/server/include/network/IPacketHandler.h
/// and ai_resources/GServer-CPP-ORIGINAL/server/src/player/Player.cpp.
/// </summary>
public static class ProtocolWireConstants
{
    public const int PacketBundleLengthPrefixBytes = 2;
    public const byte PacketTerminator = 10;
    public const int HandlerTableSize = 256;
}
