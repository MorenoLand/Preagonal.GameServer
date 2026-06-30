using System.ComponentModel;
using Preagonal.GameServer.Persistence;

namespace Preagonal.GameServer.Configuration;

public class AdminConfig : ISettings
{
	private Gs2Settings? _settings;
	public  void         SetSettings(Gs2Settings settings) => _settings = settings;

	/// <summary>
	/// ServerHQ password for the server.
	/// </summary>
	[IniKey("hq_password")]
	[Description("ServerHQ password for the server.")]
	public string? HQPassword
	{
		get => _settings?.GetString("hq_password", null) ?? null;
		set => _settings?.SetValue("", "hq_password", value);
	}

	/// <summary>
	/// Current level the server is set to.
	/// If you specify a level that is higher than the server's allowed level,
	/// it will pick the next highest.
	/// 3 = Gold
	/// 2 = Silver
	/// 1 = Bronze
	/// 0 = Hidden
	/// </summary>
	[IniKey("hq_level")]
	[Description("Current level the server is set to.")]
	public int HQLevel
	{
		get => GetInt("hq_level", 3);
		set => _settings?.SetValue("", "hq_level", value.ToString());
	}

	/// <summary>
	/// NPC-Server address (to send to RC's, it should be the same as gserver)
	/// If you want it to automatically set the correct IP, leave it blank.
	/// </summary>
	[IniKey("ns_ip")]
	[Description("NPC-Server address (to send to RC's, it should be the same as gserver).")]
	public string? NSIP
	{
		get => _settings?.GetString("ns_ip", null) ?? null;
		set => _settings?.SetValue("", "ns_ip", value);
	}

	public bool Exists(string key) => _settings?.ContainsKey(key) ?? false;
	public string? GetString(string key, string? defaultValue) => _settings?.GetString(key, defaultValue) ?? defaultValue;
	public float? GetFloat(string key, float? defaultValue) => _settings?.GetFloat(key, defaultValue) ?? defaultValue;
	public bool GetBool(string key, bool defaultValue = true) => _settings?.GetBool(key, defaultValue) ?? defaultValue;
	public int GetInt(string key, int defaultValue = 1) => _settings?.GetInt(key, defaultValue) ?? defaultValue;
}