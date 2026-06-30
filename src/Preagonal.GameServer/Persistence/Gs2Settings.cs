using System.Globalization;
using Sini;

namespace Preagonal.GameServer.Persistence;

public class Gs2Settings : IniFile, ISettings
{
	private Gs2Settings(bool isLoaded, string path, IniConfig? config = null) : base(path, config) => IsLoaded = isLoaded;

	private Gs2Settings(bool isLoaded, string[] fileContent, IniConfig? config = null) : base(fileContent, config) => IsLoaded = isLoaded;

	public bool IsLoaded { get; }

    public static Gs2Settings Parse(string settings, string separator = "=", bool fromRc = false) =>
	    new(isLoaded: true, settings.Split(['\n']));

    public static Gs2Settings LoadFile(string path, string separator = "=")
    {
        if (!File.Exists(path))
            return (Gs2Settings)CreateEmpty();

        return new(
	        isLoaded: true,
	        path,
	        new IniConfig
	        {
		        AllowCommentsAfterValue = true,
		        AllowSections = true,
	        }
        );
    }

    public bool Exists(string key) =>
        ContainsKey("", key.ToLowerInvariant());

    public bool ContainsKey(string key) =>
        ContainsKey("", key.ToLowerInvariant());

    public bool GetBool(string key, bool defaultValue = true)
    {
        var value = GetValue(key);
        return value is null ? defaultValue : value is "true" or "1";
    }

    public float? GetFloat(string key, float? defaultValue = 1.0f)
    {
        var value = GetValue(key);
        if (float.TryParse(value, out var parsed))
        {
	        return parsed;
        }
        return defaultValue;
    }

    public int GetInt(string key, int defaultValue = 1)
    {
        var value = GetValue(key);
        if (int.TryParse(value, out var parsed))
        {
	        return parsed;
        }
        return defaultValue;
    }

    public string? GetString(string key, string? defaultValue = "")
    {
        var value = GetValue(key);
        return value ?? defaultValue;
    }

    private string? GetValue(string key) =>
		AsDictionary("").GetValueOrDefault(key.ToLowerInvariant());

/*
    private void LoadSettings(string settings, string separator, bool fromRc)
    {
        settings = settings.Replace("\r", string.Empty, StringComparison.Ordinal);
        var lines = settings.Split('\n').ToList();
        if (lines.Count > 0 && lines[^1].Trim().Length == 0)
            lines.RemoveAt(lines.Count - 1);

        foreach (var parts in from originalLine in lines where !originalLine.AsSpan().StartsWith(['#'], StringComparison.Ordinal) where originalLine.Length != 0 && originalLine.Contains(separator, StringComparison.Ordinal) select originalLine.Split(separator))
        {
	        parts[0] = parts[0].ToLowerInvariant();
	        if (parts.Length == 1)
		        continue;

	        if (parts.Length > 2)
	        {
		        for (var i = 2; i < parts.Length; i++)
			        parts[1] += separator + parts[i];
	        }

	        var name     = parts[0].Trim();
	        var rawValue = parts[1].Trim();
	        var incoming = SettingValue.FromRaw(rawValue);

	        if (!TryGetValue(name, out var existing))
	        {
		        Add(name, incoming);
		        continue;
	        }

	        this[name] = fromRc
		        ? incoming
		        : existing with { Value = existing.Value + "," + incoming.Value };
        }
    }
*/

}

public sealed record SettingValue(string Value, string RawValue)
{
	public static SettingValue FromRaw(string raw)
	{
		var commentPosition = raw.IndexOf('#', StringComparison.Ordinal);
		if (commentPosition == -1)
			return new(raw, string.Empty);

		var value = raw[..commentPosition].Trim();
		return new(value, raw[value.Length..]);
	}
}