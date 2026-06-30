namespace Preagonal.GameServer.Configuration;

[AttributeUsage(AttributeTargets.Property)]
public sealed class IniKeyAttribute(string key) : Attribute
{
	public string Key { get; } = key;
}