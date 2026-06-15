using GServ.Protocol;
using Xunit;

namespace GServ.Protocol.Tests;

public sealed class ProtocolConstantTests
{
    [Fact]
    public void ConfirmedCorePacketIdsMatchGs2libIEnums()
    {
        Assert.Equal(50, (int)PlayerToServerPacketId.RawData);
        Assert.Equal(252, (int)PlayerToServerPacketId.SetEncryptionKey);
        Assert.Equal(253, (int)PlayerToServerPacketId.Bundle);
        Assert.Equal(16, (int)ServerToPlayerPacketId.DisconnectMessage);
        Assert.Equal(25, (int)ServerToPlayerPacketId.Signature);
        Assert.Equal(100, (int)ServerToPlayerPacketId.RawData);
        Assert.Equal(101, (int)ServerToPlayerPacketId.BoardPacket);
        Assert.Equal(102, (int)ServerToPlayerPacketId.File);
        Assert.Equal(252, (int)ServerToPlayerPacketId.SetEncryptionKey);
        Assert.Equal(253, (int)ServerToPlayerPacketId.Bundle);
    }

    [Fact]
    public void PlayerTypeBitsMatchGs2libIEnums()
    {
        Assert.Equal(1, (int)PlayerSessionType.Client);
        Assert.Equal(2, (int)PlayerSessionType.RemoteControl);
        Assert.Equal(4, (int)PlayerSessionType.NpcServer);
        Assert.Equal(8, (int)PlayerSessionType.NpcControl);
        Assert.Equal(16, (int)PlayerSessionType.Client2);
        Assert.Equal(32, (int)PlayerSessionType.Client3);
        Assert.Equal(64, (int)PlayerSessionType.RemoteControl2);
        Assert.Equal(256, (int)PlayerSessionType.Web);
    }
}
