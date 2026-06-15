using System.Text.RegularExpressions;

namespace GServ.Game;

public interface IServerFileSystem
{
    string Find(string file);
    string Load(string file);
    long GetModTime(string file);
}

public sealed class IndexedServerFileSystem : IServerFileSystem
{
    private readonly string _serverPath;
    private readonly Dictionary<string, string> _files = new(StringComparer.Ordinal);
    private readonly List<(string Directory, string Wildcard, bool Recursive)> _directories = [];

    public IndexedServerFileSystem(string serverPath)
    {
        _serverPath = Path.GetFullPath(serverPath);
    }

    public void Clear()
    {
        _files.Clear();
        _directories.Clear();
    }

    public void AddDirectory(string directory, string wildcard = "*", bool forceRecursive = false, bool noFoldersConfig = false)
    {
        var normalizedDirectory = FixPathSeparators(directory);
        var fullDirectory = Path.GetFullPath(Path.Combine(_serverPath, normalizedDirectory));

        var recursive = forceRecursive || noFoldersConfig;
        _directories.Add((fullDirectory, wildcard, recursive));
        LoadAllDirectories(fullDirectory, wildcard, recursive);
    }

    public string Find(string file) =>
        _files.TryGetValue(file, out var path) ? path : string.Empty;

    public string FindInsensitive(string file)
    {
        foreach (var entry in _files)
        {
            if (string.Equals(entry.Key, file, StringComparison.OrdinalIgnoreCase))
                return entry.Value;
        }

        return string.Empty;
    }

    public string FileExistsAs(string file)
    {
        foreach (var entry in _files)
        {
            if (string.Equals(entry.Key, file, StringComparison.OrdinalIgnoreCase))
                return entry.Key;
        }

        return string.Empty;
    }

    public string Load(string file)
    {
        var path = Find(file);
        return path.Length == 0 ? string.Empty : File.ReadAllText(path);
    }

    public long GetModTime(string file)
    {
        var path = Find(file);
        return path.Length == 0 ? 0 : new DateTimeOffset(File.GetLastWriteTimeUtc(path)).ToUnixTimeSeconds();
    }

    public int GetFileSize(string file)
    {
        var path = Find(file);
        return path.Length == 0 ? 0 : checked((int)new FileInfo(path).Length);
    }

    public void Resync()
    {
        var directories = _directories.ToArray();
        _files.Clear();

        foreach (var (directory, wildcard, recursive) in directories)
            LoadAllDirectories(directory, wildcard, recursive);
    }

    private void LoadAllDirectories(string directory, string wildcard, bool recursive)
    {
        if (!Directory.Exists(directory))
            return;

        foreach (var file in Directory.EnumerateFiles(directory))
        {
            var filename = Path.GetFileName(file);
            if (MatchesWildcard(filename, wildcard))
                _files[filename] = file;
        }

        if (!recursive)
            return;

        foreach (var child in Directory.EnumerateDirectories(directory))
            LoadAllDirectories(child, wildcard, recursive: true);
    }

    private static string FixPathSeparators(string path) =>
        path.Replace(Path.DirectorySeparatorChar == '\\' ? '/' : '\\', Path.DirectorySeparatorChar);

    private static bool MatchesWildcard(string filename, string wildcard)
    {
        if (wildcard == "*")
            return true;

        var pattern = "^" + Regex.Escape(wildcard).Replace("\\*", ".*").Replace("\\?", ".") + "$";
        return Regex.IsMatch(filename, pattern, RegexOptions.CultureInvariant);
    }
}
