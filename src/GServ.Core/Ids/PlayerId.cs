namespace GServ.Core.Ids;

/// <summary>
/// Strongly typed player identifier. The numeric allocation rules will be ported from C++ IdGenerator later.
/// </summary>
public readonly record struct PlayerId(ushort Value)
{
    public override string ToString() => Value.ToString();
}
