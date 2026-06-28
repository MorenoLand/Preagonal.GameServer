namespace Preagonal.GameServer.Network.Protocol;

public readonly record struct LoginPrelude(PlayerSessionType Type, EncryptionGeneration InboundGeneration, bool ReadsEncryptionKeyBeforeVersion);

public static class LoginPreludeParser
{
    public static LoginPrelude Parse(ReadOnlySpan<byte> payload)
    {
        var reader = new GraalBinaryReader(payload);
        var shiftedType = 1 << reader.ReadGChar();
        var type = (PlayerSessionType)shiftedType;
        return type switch
        {
            PlayerSessionType.Client         => new(type, EncryptionGeneration.Gen2, false),
            PlayerSessionType.RemoteControl  => new(type, EncryptionGeneration.Gen3, false),
            PlayerSessionType.NpcServer      => new(type, EncryptionGeneration.Gen3, false),
            PlayerSessionType.NpcControl     => new(type, EncryptionGeneration.Gen3, false),
            PlayerSessionType.Client2        => new(type, EncryptionGeneration.Gen4, false),
            PlayerSessionType.Client3        => new(type, EncryptionGeneration.Gen5, false),
            PlayerSessionType.RemoteControl2 => new(type, EncryptionGeneration.Gen5, true),
            PlayerSessionType.Web            => new(type, EncryptionGeneration.Gen1, false),
            _                                => new(type, EncryptionGeneration.Gen3, false)
        };
    }
}
