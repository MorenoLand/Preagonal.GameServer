namespace GServ.Protocol;

public sealed class GraalFileQueue
{
    private readonly Queue<byte[]> _normalBuffer = new();
    private readonly Queue<byte[]> _fileBuffer = new();
    private readonly List<byte> _outputBuffer = [];
    private readonly List<byte> _socketOutputBuffer = [];
    private bool _previousRawData;
    private int _rawDataSize;
    private byte[] _rawDataHeader = [];
    private int _bytesSentWithoutFile;
    private int _sendCallsWithoutData;
    private EncryptionGeneration _outboundGeneration = EncryptionGeneration.Gen1;
    private GraalEncryption _outboundCodec = new(EncryptionGeneration.Gen1);

    public void AddPacket(ReadOnlySpan<byte> packetBytes)
    {
        var packet = packetBytes.ToArray();
        var position = 0;

        while (position < packet.Length)
        {
            if (packet[position] < 0x20)
                break;

            var packetId = unchecked((byte)(packet[position] - 32));
            if (packetId == (byte)ServerToPlayerPacketId.RawData)
            {
                var newline = Array.IndexOf(packet, (byte)'\n', position);
                var end = newline < 0 ? packet.Length : newline + 1;
                _rawDataHeader = packet[position..end];
                var reader = new GraalBinaryReader(_rawDataHeader);
                reader.ReadGChar();
                _rawDataSize = reader.ReadGInt();
                _previousRawData = true;
                position = end;
                continue;
            }

            if (_previousRawData)
            {
                var payloadLength = Math.Clamp(_rawDataSize, 0, packet.Length - position);
                var payload = packet[position..(position + payloadLength)];
                var combined = Combine(_rawDataHeader, payload);
                var firstPayloadId = payload.Length == 0 ? (byte)0 : unchecked((byte)(payload[0] - 32));
                if (firstPayloadId == (byte)ServerToPlayerPacketId.BoardPacket)
                    _normalBuffer.Enqueue(combined);
                else
                    _fileBuffer.Enqueue(combined);

                _previousRawData = false;
                position += payloadLength;
                continue;
            }

            var packetEnd = Array.IndexOf(packet, (byte)'\n', position);
            packetEnd = packetEnd < 0 ? packet.Length : packetEnd + 1;
            var normalPacket = packet[position..packetEnd];

            switch ((ServerToPlayerPacketId)packetId)
            {
                case ServerToPlayerPacketId.LargeFileStart:
                case ServerToPlayerPacketId.LargeFileEnd:
                case ServerToPlayerPacketId.LargeFileSize:
                    _fileBuffer.Enqueue(normalPacket);
                    break;
                default:
                    _normalBuffer.Enqueue(normalPacket);
                    break;
            }

            position = packetEnd;
        }
    }

    public void SetCodec(EncryptionGeneration generation, byte key)
    {
        _outboundGeneration = generation;
        _outboundCodec = new GraalEncryption(generation);
        _outboundCodec.Reset(key);
    }

    public byte[] FlushUncompressed(bool forceSendFiles = false, int? maxBytes = null)
    {
        FillOutputBuffer(forceSendFiles);
        if (_outputBuffer.Count == 0)
            return [];

        var count = Math.Min(maxBytes ?? _outputBuffer.Count, _outputBuffer.Count);
        var sent = _outputBuffer.GetRange(0, count).ToArray();
        _outputBuffer.RemoveRange(0, count);
        return sent;
    }

    public byte[] FlushSocket(bool forceSendFiles = false, int? maxBytes = null)
    {
        FillSocketOutputBuffer(forceSendFiles);
        if (_socketOutputBuffer.Count == 0)
            return [];

        var count = Math.Min(maxBytes ?? _socketOutputBuffer.Count, _socketOutputBuffer.Count);
        var sent = _socketOutputBuffer.GetRange(0, count).ToArray();
        _socketOutputBuffer.RemoveRange(0, count);
        return sent;
    }

    private void FillOutputBuffer(bool forceSendFiles)
    {
        if (_outputBuffer.Count != 0)
            return;

        var pending = new List<byte>();
        if (_normalBuffer.Count != 0 && _normalBuffer.Peek().Length > 0xF000)
            pending.AddRange(_normalBuffer.Dequeue());

        if (pending.Count == 0 &&
            (_bytesSentWithoutFile > 0x7FFF || forceSendFiles || _sendCallsWithoutData >= 4) &&
            _fileBuffer.Count != 0)
        {
            _bytesSentWithoutFile = 0;
            pending.AddRange(_fileBuffer.Dequeue());
        }

        while (pending.Count < 0xC000 && _normalBuffer.Count != 0)
        {
            if (pending.Count + _normalBuffer.Peek().Length > 0xF000)
                break;
            pending.AddRange(_normalBuffer.Dequeue());
        }
        _bytesSentWithoutFile += pending.Count;

        if (pending.Count < 0x4000 && _fileBuffer.Count != 0)
        {
            if (pending.Count + _fileBuffer.Peek().Length <= 0xF000)
            {
                _bytesSentWithoutFile = 0;
                pending.AddRange(_fileBuffer.Dequeue());
            }
        }

        if (_fileBuffer.Count == 0)
            _bytesSentWithoutFile = 0;

        if (pending.Count == 0)
        {
            if (_sendCallsWithoutData < 5)
                _sendCallsWithoutData++;
            return;
        }

        _sendCallsWithoutData = 0;
        _outputBuffer.AddRange(pending);
    }

    private void FillSocketOutputBuffer(bool forceSendFiles)
    {
        if (_socketOutputBuffer.Count != 0)
            return;

        FillOutputBuffer(forceSendFiles);
        if (_outputBuffer.Count == 0)
            return;

        switch (_outboundGeneration)
        {
            case EncryptionGeneration.Gen1:
            case EncryptionGeneration.Gen6:
                _socketOutputBuffer.AddRange(_outputBuffer);
                _outputBuffer.Clear();
                return;

            case EncryptionGeneration.Gen5:
                if (_outputBuffer.Count > 55)
                    throw new NotSupportedException("Gen5 compressed socket flush is blocked until zlib/bzip2 bytes are confirmed against gs2lib.");

                var payload = _outputBuffer.ToArray();
                AddGen5SocketPayload(payload);
                _outputBuffer.Clear();
                return;

            case EncryptionGeneration.Gen2:
            case EncryptionGeneration.Gen3:
            case EncryptionGeneration.Gen4:
                throw new NotSupportedException(
                    $"Socket flush for {_outboundGeneration} is blocked until zlib/bzip2 bytes are confirmed against gs2lib.");

            default:
                throw new NotSupportedException($"Socket flush for {_outboundGeneration} is not confirmed.");
        }
    }

    private void AddGen5SocketPayload(byte[] payload)
    {
        _outboundCodec.LimitFromCompressionType(CompressionType.Uncompressed);
        var encrypted = _outboundCodec.Encrypt(payload);
        if (encrypted.Length > 0xFFFC)
            return;

        var framedLength = encrypted.Length + 1;
        _socketOutputBuffer.Add((byte)(framedLength >> 8));
        _socketOutputBuffer.Add((byte)framedLength);
        _socketOutputBuffer.Add((byte)CompressionType.Uncompressed);
        _socketOutputBuffer.AddRange(encrypted);
    }

    private static byte[] Combine(ReadOnlySpan<byte> first, ReadOnlySpan<byte> second)
    {
        var combined = new byte[first.Length + second.Length];
        first.CopyTo(combined);
        second.CopyTo(combined.AsSpan(first.Length));
        return combined;
    }
}
