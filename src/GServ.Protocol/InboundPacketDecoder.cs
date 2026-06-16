using System.IO.Compression;

namespace GServ.Protocol;

public sealed record InboundPacketDecodeResult(IReadOnlyList<byte[]> Packets);

public sealed class InboundPacketDecoder
{
    private readonly EncryptionGeneration _generation;
    private readonly GraalEncryption _codec;

    public InboundPacketDecoder(EncryptionGeneration generation, byte key)
    {
        _generation = generation;
        _codec = new GraalEncryption(generation);
        _codec.Reset(key);
    }

    public InboundPacketDecodeResult DecodeSocketFramePayload(ReadOnlySpan<byte> framePayload)
    {
        var decoded = _generation switch
        {
            EncryptionGeneration.Gen1 or EncryptionGeneration.Gen6 => framePayload.ToArray(),
            EncryptionGeneration.Gen2 => ZlibDecompress(framePayload),
            EncryptionGeneration.Gen3 => ZlibDecompress(framePayload),
            EncryptionGeneration.Gen4 => throw new NotSupportedException("Inbound gen4 bzip2 decrypt/decompress is not implemented yet."),
            EncryptionGeneration.Gen5 => DecodeGen5(framePayload),
            _ => throw new NotSupportedException($"Inbound generation {_generation} is not source-confirmed.")
        };

        var packets = SplitNewlinePackets(decoded);
        if (_generation == EncryptionGeneration.Gen3)
            packets = packets.Select(packet => _codec.Decrypt(packet)).ToArray();

        return new InboundPacketDecodeResult(packets);
    }

    private byte[] DecodeGen5(ReadOnlySpan<byte> framePayload)
    {
        if (framePayload.IsEmpty)
            return [];

        var compressionType = (CompressionType)framePayload[0];
        if (!_codec.LimitFromCompressionType(compressionType))
            throw new NotSupportedException($"Inbound gen5 compression type 0x{framePayload[0]:X2} is not source-confirmed.");

        var decrypted = _codec.Decrypt(framePayload[1..]);
        return compressionType switch
        {
            CompressionType.Uncompressed => decrypted,
            CompressionType.Zlib => ZlibDecompress(decrypted),
            CompressionType.Bz2 => throw new NotSupportedException("Inbound gen5 bzip2 decrypt/decompress is not implemented yet."),
            _ => throw new NotSupportedException($"Inbound gen5 compression type 0x{framePayload[0]:X2} is not source-confirmed.")
        };
    }

    private static byte[] ZlibDecompress(ReadOnlySpan<byte> payload)
    {
        using var input = new MemoryStream(payload.ToArray());
        using var zlib = new ZLibStream(input, CompressionMode.Decompress);
        using var output = new MemoryStream();
        zlib.CopyTo(output);
        return output.ToArray();
    }

    private static byte[][] SplitNewlinePackets(byte[] decoded)
    {
        var packets = new List<byte[]>();
        var start = 0;
        for (var i = 0; i < decoded.Length; i++)
        {
            if (decoded[i] != (byte)'\n')
                continue;

            packets.Add(decoded.AsSpan(start, i - start).ToArray());
            start = i + 1;
        }

        if (start < decoded.Length)
            packets.Add(decoded.AsSpan(start).ToArray());

        return packets.ToArray();
    }
}
