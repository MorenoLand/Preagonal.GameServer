using GServ.Game;

namespace GServ.Game.Tests;

public sealed class ServerFileSystemTests
{
    [Fact]
    public void AddDirectoryIndexesMatchingFilesByFilenameForExactFind()
    {
        using var temp = new TemporaryDirectory();
        var world = Directory.CreateDirectory(Path.Combine(temp.Path, "world"));
        var levelPath = Path.Combine(world.FullName, "start.nw");
        File.WriteAllText(levelPath, "GLEVNW01");
        File.WriteAllText(Path.Combine(world.FullName, "readme.txt"), "ignored");

        var fileSystem = new IndexedServerFileSystem(temp.Path);
        fileSystem.AddDirectory("world", "*.nw");

        Assert.Equal(levelPath, fileSystem.Find("start.nw"));
        Assert.Equal(string.Empty, fileSystem.Find("START.NW"));
        Assert.Equal(string.Empty, fileSystem.Find("readme.txt"));
    }

    [Fact]
    public void AddDirectoryUsesRecursiveTraversalWhenRequested()
    {
        using var temp = new TemporaryDirectory();
        var nested = Directory.CreateDirectory(Path.Combine(temp.Path, "world", "inside"));
        var levelPath = Path.Combine(nested.FullName, "start.nw");
        File.WriteAllText(levelPath, "GLEVNW01");

        var fileSystem = new IndexedServerFileSystem(temp.Path);
        fileSystem.AddDirectory("world", "*.nw", forceRecursive: true);

        Assert.Equal(levelPath, fileSystem.Find("start.nw"));
    }

    [Fact]
    public void LoadAndGetModTimeReturnEmptyAndZeroWhenFindFails()
    {
        using var temp = new TemporaryDirectory();
        var fileSystem = new IndexedServerFileSystem(temp.Path);

        Assert.Equal(string.Empty, fileSystem.Load("missing.nw"));
        Assert.Equal(0, fileSystem.GetModTime("missing.nw"));
    }

    [Fact]
    public void ResyncPreservesRecursiveDirectories()
    {
        using var temp = new TemporaryDirectory();
        var nested = Directory.CreateDirectory(Path.Combine(temp.Path, "world", "inside"));
        var originalPath = Path.Combine(nested.FullName, "start.nw");
        File.WriteAllText(originalPath, "GLEVNW01");

        var fileSystem = new IndexedServerFileSystem(temp.Path);
        fileSystem.AddDirectory("world", "*.nw", forceRecursive: true);
        Assert.Equal(originalPath, fileSystem.Find("start.nw"));

        File.Delete(originalPath);
        var newPath = Path.Combine(nested.FullName, "next.nw");
        File.WriteAllText(newPath, "GLEVNW01");

        fileSystem.Resync();

        Assert.Equal(string.Empty, fileSystem.Find("start.nw"));
        Assert.Equal(newPath, fileSystem.Find("next.nw"));
    }

    private sealed class TemporaryDirectory : IDisposable
    {
        public TemporaryDirectory()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public void Dispose() => Directory.Delete(Path, recursive: true);
    }
}
