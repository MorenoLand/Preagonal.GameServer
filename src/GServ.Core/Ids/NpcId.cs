namespace GServ.Core.Ids;

/// <summary>
/// Strongly typed NPC identifier. Segment behavior must preserve the C++ NPC ID generator.
/// </summary>
public readonly record struct NpcId(uint Value)
{
    public override string ToString() => Value.ToString();
}
