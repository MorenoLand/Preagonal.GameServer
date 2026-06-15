using System.Text;
using GServ.Protocol;
using Xunit;

namespace GServ.Protocol.Tests;

public sealed class FileQueueCompatibilityTests
{
    [Fact]
    public void Gen1FlushSendsNormalNewlinePacketsUncompressedInQueueOrder()
    {
        var queue = new GraalFileQueue();

        queue.AddPacket(Encoding.ASCII.GetBytes("abc\nxyz\n"));

        Assert.Equal(
            Encoding.ASCII.GetBytes("abc\nxyz\n"),
            queue.FlushUncompressed());
    }

    [Fact]
    public void RawDataHeaderAndBoardPayloadStayInNormalQueue()
    {
        var writer = new GraalBinaryWriter();
        writer.WriteGChar((byte)ServerToPlayerPacketId.RawData);
        writer.WriteGInt(2);
        writer.WriteByte((byte)'\n');
        writer.WriteGChar((byte)ServerToPlayerPacketId.BoardPacket);
        writer.WriteByte((byte)'x');

        var queue = new GraalFileQueue();
        queue.AddPacket(writer.ToArray());

        Assert.Equal(writer.ToArray(), queue.FlushUncompressed());
    }

    [Fact]
    public void PartialSocketSendLeavesRemainingOutputBufferedForNextFlush()
    {
        var queue = new GraalFileQueue();
        queue.AddPacket(Encoding.ASCII.GetBytes("abcdef\n"));

        Assert.Equal(Encoding.ASCII.GetBytes("abc"), queue.FlushUncompressed(maxBytes: 3));
        Assert.Equal(Encoding.ASCII.GetBytes("def\n"), queue.FlushUncompressed());
    }

    [Fact]
    public void Gen5ShortPayloadFlushPrefixesLengthCompressionTypeAndEncryptedPayload()
    {
        var queue = new GraalFileQueue();
        queue.SetCodec(EncryptionGeneration.Gen5, key: 0);
        queue.AddPacket(Encoding.ASCII.GetBytes("abc\n"));

        Assert.Equal(
            new byte[] { 0x00, 0x05, 0x02, 0x79, 0x7A, 0xB2, 0xDC },
            queue.FlushSocket());
    }

    [Fact]
    public void Gen5ShortPayloadPartialSocketSendLeavesRemainingFramedBytesBuffered()
    {
        var queue = new GraalFileQueue();
        queue.SetCodec(EncryptionGeneration.Gen5, key: 0);
        queue.AddPacket(Encoding.ASCII.GetBytes("abc\n"));

        Assert.Equal(new byte[] { 0x00, 0x05, 0x02 }, queue.FlushSocket(maxBytes: 3));
        Assert.Equal(new byte[] { 0x79, 0x7A, 0xB2, 0xDC }, queue.FlushSocket());
    }

    [Fact]
    public void Gen1SocketFlushSendsQueuedBytesWithoutOuterLengthPrefix()
    {
        var queue = new GraalFileQueue();
        queue.SetCodec(EncryptionGeneration.Gen1, key: 0);
        queue.AddPacket(Encoding.ASCII.GetBytes("abc\n"));

        Assert.Equal(Encoding.ASCII.GetBytes("abc\n"), queue.FlushSocket());
    }

    [Fact]
    public void Gen5PayloadOverUncompressedThresholdRemainsBlockedUntilCompressionBytesAreConfirmed()
    {
        var queue = new GraalFileQueue();
        queue.SetCodec(EncryptionGeneration.Gen5, key: 0);
        var payload = Encoding.ASCII.GetBytes(new string('a', 56) + "\n");
        queue.AddPacket(payload);

        var ex = Assert.Throws<NotSupportedException>(() => queue.FlushSocket());

        Assert.Equal("Gen5 compressed socket flush is blocked until zlib/bzip2 bytes are confirmed against gs2lib.", ex.Message);
        Assert.Equal(payload, queue.FlushUncompressed());
    }

    [Fact]
    public void Gen2FlushRemainsBlockedUntilZlibBytesAreConfirmed()
    {
        var queue = new GraalFileQueue();
        queue.SetCodec(EncryptionGeneration.Gen2, key: 0);
        queue.AddPacket(Encoding.ASCII.GetBytes("abc\n"));

        var ex = Assert.Throws<NotSupportedException>(() => queue.FlushSocket());

        Assert.Equal("Socket flush for Gen2 is blocked until zlib/bzip2 bytes are confirmed against gs2lib.", ex.Message);
    }
}
