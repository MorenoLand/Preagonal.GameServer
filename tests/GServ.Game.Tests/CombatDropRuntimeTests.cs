namespace GServ.Game.Tests;

public sealed class CombatDropRuntimeTests
{
    private sealed class SequenceCombatRandom : ICombatRandom
    {
        private readonly int[] _values;
        private int _index;

        public SequenceCombatRandom(params int[] values)
        {
            _values = values;
            _index = 0;
        }

        public int Next(int maxExclusive)
        {
            var value = _values[_index % _values.Length];
            _index++;
            return value;
        }
    }

    [Fact]
    public void DecomposeGralatsUsesGreedyCppBuckets()
    {
        var drops = CombatDropRuntime.DecomposeGralats(134);

        Assert.Equal(
            [
                LevelItemType.GoldRupee,
                LevelItemType.RedRupee,
                LevelItemType.GreenRupee,
                LevelItemType.GreenRupee,
                LevelItemType.GreenRupee,
                LevelItemType.GreenRupee
            ],
            drops);
    }

    [Fact]
    public void ComputeDroppedArrowsAndBombsHonorInventoryCaps()
    {
        Assert.Equal(1, CombatDropRuntime.ComputeDroppedArrows(7, new SequenceCombatRandom(3)));
        Assert.Equal(1, CombatDropRuntime.ComputeDroppedBombs(9, new SequenceCombatRandom(3)));
        Assert.Equal(3, CombatDropRuntime.ComputeDroppedArrows(99, new SequenceCombatRandom(3)));
    }

    [Fact]
    public void ApplyPlayerDeathDropsMutatesInventoryAndBuildsItemPackets()
    {
        var player = new DurablePlayerInventoryState
        {
            Rupees = 41,
            Arrows = 9,
            Bombs = 9,
            MaxPower = 20,
            Hitpoints = 6.0f
        };

        var rng = new SequenceCombatRandom(75, 4, 0, 7, 1, 5, 0, 3, 2, 6, 7);
        var result = CombatDropRuntime.ApplyPlayerDeathDrops(
            player,
            dropItemsDead: true,
            minDeathGralats: 1,
            maxDeathGralats: 50,
            playerX: 10.0f,
            playerY: 20.0f,
            rng);

        Assert.Equal(16, player.Rupees);
        Assert.Equal(9, player.Arrows);
        Assert.Equal(9, player.Bombs);
        Assert.Equal(16, result.RemainingRupees);
        Assert.Equal(5, result.DropPackets.Count);
        Assert.True(result.DropPackets.Count >= 3);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(4, 4)]
    [InlineData(7, 7)]
    [InlineData(9, 9)]
    public void ComputeDroppedGralatsMatchesCppRangeAndInventoryClamp(int randomValue, int expected)
    {
        var result = CombatDropRuntime.ComputeDroppedGralats(
            maxDrop: 10,
            minDrop: 1,
            currentRupees: randomValue,
            new SequenceCombatRandom(randomValue));

        var expectedDrop = Math.Clamp(expected, 1, 10);
        Assert.Equal(Math.Min(expectedDrop, randomValue), result);
    }

    [Fact]
    public void BuildBaddyDropMappingFromObservedRolls()
    {
        var rng = new SequenceCombatRandom(0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11);
        var expected = new[]
        {
            LevelItemType.GreenRupee,
            LevelItemType.BlueRupee,
            LevelItemType.RedRupee,
            LevelItemType.Bombs,
            LevelItemType.Darts,
            LevelItemType.Heart,
            LevelItemType.GreenRupee,
            LevelItemType.GreenRupee,
            LevelItemType.GreenRupee,
            LevelItemType.GreenRupee,
            LevelItemType.Invalid,
            LevelItemType.Invalid
        };

        for (var i = 0; i < 11; i++)
        {
            var expectedItem = i switch
            {
                10 or 11 => LevelItemType.Invalid,
                _ => expected[i]
            };

            var hasDrop = CombatDropRuntime.TryRollBaddyDrop(rng, out var itemType);
            Assert.Equal(expectedItem != LevelItemType.Invalid, hasDrop);
            Assert.Equal(expectedItem, itemType);
        }
    }

    [Fact]
    public void BuildBaddyDropPacketUsesBaddyPositionScaling()
    {
        var packet = CombatDropRuntime.BuildBaddyDropPacket(1.25f, 4.75f, LevelItemType.RedRupee);

        Assert.Equal(
            [
                (byte)GServ.Protocol.ServerToPlayerPacketId.ItemAdd + 32,
                (byte)(1.25f * 2 + 32),
                (byte)(4.75f * 2 + 32),
                (byte)(LevelItemType.RedRupee + 32),
                (byte)'\n'
            ],
            packet);
    }
}
