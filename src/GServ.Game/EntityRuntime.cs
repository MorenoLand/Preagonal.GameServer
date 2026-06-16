using System.Text;
using GServ.Protocol;

namespace GServ.Game;

public sealed record RuntimeLevelItem(float X, float Y, LevelItemType ItemType);

public sealed record RuntimeHorse(string Image, float X, float Y, byte Direction, byte Bushes);

public sealed record LevelItemRuntimeResult(
    bool ChangedLevel,
    byte[] ForwardPacket,
    byte[] SelfPacket);

public static class LevelItemRuntime
{
    public static LevelItemRuntimeResult SpawnLevelItem(
        RuntimeLevel level,
        byte encodedX,
        byte encodedY,
        byte itemId,
        bool playerDrop,
        DurablePlayerInventoryState player)
    {
        var itemType = LevelItemCatalog.GetItemId(itemId);
        if (itemType == LevelItemType.Invalid)
            return NoChange();

        if (playerDrop && !InventoryItemRules.TryRemoveForPlayerDrop(itemType, player))
            return NoChange();

        var x = encodedX / 2.0f;
        var y = encodedY / 2.0f;
        if (level.AddItem(x, y, itemType))
            return new LevelItemRuntimeResult(true, EntityPackets.ItemAdd(encodedX, encodedY, itemId), []);

        return new LevelItemRuntimeResult(false, [], EntityPackets.ItemDelete(encodedX, encodedY));
    }

    public static LevelItemRuntimeResult DeleteOrTakeLevelItem(
        RuntimeLevel level,
        byte encodedX,
        byte encodedY,
        bool takeItem,
        DurablePlayerInventoryState player)
    {
        var forwardPacket = EntityPackets.ItemDelete(encodedX, encodedY);
        var itemType = level.RemoveItem(encodedX / 2.0f, encodedY / 2.0f);
        if (itemType == LevelItemType.Invalid)
            return new LevelItemRuntimeResult(false, forwardPacket, []);

        if (takeItem)
        {
            var payload = InventoryItemRules.BuildPickupPlayerProps(itemType, player);
            InventoryItemRules.ApplyPickupPlayerProps(payload, player);
        }

        return new LevelItemRuntimeResult(true, forwardPacket, []);
    }

    private static LevelItemRuntimeResult NoChange() =>
        new(false, [], []);
}

public enum BaddyMode : byte
{
    Walk = 0,
    Look = 1,
    Hunt = 2,
    Hurt = 3,
    Bumped = 4,
    Die = 5,
    SwampShot = 6,
    HareJump = 7,
    OctoShot = 8,
    Dead = 9
}

public sealed class RuntimeBaddy
{
    private static readonly string[] Images =
    [
        "baddygray.png", "baddyblue.png", "baddyred.png", "baddyblue.png", "baddygray.png",
        "baddyhare.png", "baddyoctopus.png", "baddygold.png", "baddylizardon.png", "baddydragon.png"
    ];

    private static readonly byte[] StartModes = [0, 0, 0, 0, 6, 7, 0, 0, 0, 0];
    private static readonly byte[] Powers = [2, 3, 4, 3, 2, 1, 1, 6, 12, 8];

    private RuntimeBaddy(byte id, float x, float y, byte type)
    {
        Id = id;
        X = x;
        Y = y;
        Type = type > Images.Length ? (byte)0 : type;
        Power = Powers[Type];
        Image = Images[Type];
        Mode = StartModes[Type];
        Direction = (2 << 2) | 2;
    }

    public byte Id { get; }
    public float X { get; }
    public float Y { get; }
    public byte Type { get; }
    public byte Power { get; }
    public string Image { get; }
    public byte Mode { get; }
    public byte Ani { get; } = 0;
    public byte Direction { get; }
    public IReadOnlyList<string> Verses { get; } = ["", "", ""];

    public static RuntimeBaddy Create(byte id, float x, float y, byte type) =>
        new(id, x, y, type);
}

public static class EntityRuntimePackets
{
    public static byte[] BaddyProps(RuntimeBaddy baddy, int clientVersion)
    {
        var writer = new GraalBinaryWriter();
        writer.WriteGChar((byte)ServerToPlayerPacketId.BaddyProps);
        writer.WriteGChar(baddy.Id);
        WriteBaddyProps(writer, baddy, clientVersion);
        writer.WriteByte((byte)'\n');
        return writer.ToArray();
    }

    private static void WriteBaddyProps(GraalBinaryWriter writer, RuntimeBaddy baddy, int clientVersion)
    {
        writer.WriteGChar(1);
        writer.WriteGChar((byte)(baddy.X * 2));
        writer.WriteGChar(2);
        writer.WriteGChar((byte)(baddy.Y * 2));
        writer.WriteGChar(3);
        writer.WriteGChar(baddy.Type);
        writer.WriteGChar(4);
        writer.WriteGChar(baddy.Power);
        var image = clientVersion < 210 ? baddy.Image.Replace(".png", ".gif", StringComparison.Ordinal) : baddy.Image;
        var imageBytes = Encoding.ASCII.GetBytes(image);
        writer.WriteGChar((byte)imageBytes.Length);
        writer.WriteBytes(imageBytes);
        writer.WriteGChar(5);
        writer.WriteGChar(baddy.Mode);
        writer.WriteGChar(6);
        writer.WriteGChar(baddy.Ani);
        writer.WriteGChar(7);
        writer.WriteGChar(baddy.Direction);

        for (byte propId = 8; propId <= 10; propId++)
        {
            writer.WriteGChar(propId);
            writer.WriteGChar(0);
        }
    }
}
