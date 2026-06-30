using Microsoft.Extensions.Options;
using Preagonal.GameServer.Configuration;

namespace Preagonal.GameServer.Persistence;

public sealed record ServerStartupOverrides(
    string? Server,
    string? Port,
    string? LocalIp,
    string? ServerIp,
    string? ServerInterface,
    string? StaffAccount,
    string? ServerName,
    bool ShowHelp,
    bool HasOverrides)
{
    public static ServerStartupOverrides Empty        { get; } = new(null, null, null, null, null, null, null, false, false);
}

public enum ServerStartupSource
{
    None,
    CommandLineOrEnvironment,
    StartupServerFile,
    SingleServersDirectory,
}

public sealed record ServerStartupResolution(
    bool Success,
    string? ServerName,
    string? ServerPath,
    ServerStartupSource Source,
    string? Diagnostic)
{
    public static ServerStartupResolution Failed(string diagnostic) =>
        new(false, null, null, ServerStartupSource.None, diagnostic);
}

public static class ServerStartupCommandLine
{
    public static ServerStartupOverrides Parse(IReadOnlyList<string> args, Func<string, string?> getEnvironmentVariable)
    {
        var useEnvironment = getEnvironmentVariable("USE_ENV") is not null;
        if (useEnvironment)
            return ParseEnvironment(getEnvironmentVariable);

        var result = MutableOverrides.Empty;
        for (var i = 0; i < args.Count; i++)
        {
            var arg = args[i];
            if (arg.StartsWith("--", StringComparison.Ordinal))
            {
                var key = arg[2..];
                if (key == "help")
                    return result.ToImmutable(showHelp: true);

                if (++i == args.Count)
                    return result.ToImmutable(showHelp: true);

                ApplyLongOption(ref result, key, args[i]);
            }
            else if (arg.Length > 0 && arg[0] == '-')
            {
                for (var j = 1; j < arg.Length; j++)
                {
                    switch (arg[j])
                    {
                        case 'h':
                            return result.ToImmutable(showHelp: true);
                        case 's':
                            if (++i == args.Count)
                                return result.ToImmutable(showHelp: true, hasOverrides: true);
                            result.Server = args[i];
                            break;
                        case 'p':
                            if (string.IsNullOrEmpty(result.Server))
                                break;
                            if (++i == args.Count)
                                return result.ToImmutable(showHelp: true, hasOverrides: true);
                            result.Port = args[i];
                            break;
                    }
                }
            }
        }

        return result.ToImmutable(showHelp: false);
    }

    private static ServerStartupOverrides ParseEnvironment(Func<string, string?> getEnvironmentVariable)
    {
        var result = MutableOverrides.Empty;
        result.Server = getEnvironmentVariable("SERVER");
        if (string.IsNullOrEmpty(result.Server))
            return result.ToImmutable(showHelp: false);

        result.Port = getEnvironmentVariable("PORT");
        result.LocalIp = getEnvironmentVariable("LOCALIP");
        result.ServerIp = getEnvironmentVariable("SERVERIP");
        result.ServerInterface = getEnvironmentVariable("INTERFACE");
        result.StaffAccount = getEnvironmentVariable("STAFFACCOUNT");
        result.ServerName = getEnvironmentVariable("SERVERNAME");
        return result.ToImmutable(showHelp: false, hasOverrides: true);
    }

    private static void ApplyLongOption(ref MutableOverrides result, string key, string value)
    {
        switch (key)
        {
            case "server":
                result.Server = value;
                break;
            case "port" when !string.IsNullOrEmpty(result.Server):
                result.Port = value;
                break;
            case "localip" when !string.IsNullOrEmpty(result.Server):
                result.LocalIp = value;
                break;
            case "serverip" when !string.IsNullOrEmpty(result.Server):
                result.ServerIp = value;
                break;
            case "interface" when !string.IsNullOrEmpty(result.Server):
                result.ServerInterface = value;
                break;
            case "staff" when !string.IsNullOrEmpty(result.Server):
                result.StaffAccount = value;
                break;
            case "name" when !string.IsNullOrEmpty(result.Server):
                result.ServerName = value;
                break;
        }
    }

    private struct MutableOverrides
    {
        public string? Server;
        public string? Port;
        public string? LocalIp;
        public string? ServerIp;
        public string? ServerInterface;
        public string? StaffAccount;
        public string? ServerName;

        public static MutableOverrides Empty => new();

        public readonly ServerStartupOverrides ToImmutable(bool showHelp, bool hasOverrides = false) =>
            new(Server, Port, LocalIp, ServerIp, ServerInterface, StaffAccount, ServerName, showHelp, hasOverrides);
    }
}

public static class ServerStartupResolver
{
    public static ServerStartupResolution Resolve(string homePath, ServerStartupOverrides overrides)
    {
        var normalizedHome = EnsureTrailingSeparator(Path.GetFullPath(homePath));
        if (!string.IsNullOrEmpty(overrides.Server))
            return Success(normalizedHome, overrides.Server, ServerStartupSource.CommandLineOrEnvironment);

        var startupPath = Path.Combine(normalizedHome, "startupserver.txt");
        if (File.Exists(startupPath))
        {
            var startup = File.ReadAllText(startupPath);
            if (startup.Length > 0)
                return Success(normalizedHome, startup, ServerStartupSource.StartupServerFile);
        }

        var serversPath = Path.Combine(normalizedHome, "servers");
        if (Directory.Exists(serversPath))
        {
            var servers = Directory.EnumerateDirectories(serversPath).Select(Path.GetFileName).ToArray();
            if (servers.Length == 1 && !string.IsNullOrEmpty(servers[0]))
                return Success(normalizedHome, servers[0]!, ServerStartupSource.SingleServersDirectory);
        }

        return ServerStartupResolution.Failed(
            "C++ startup would return ERR_SETTINGS because no override server, non-empty startupserver.txt, or single servers/ directory was found.");
    }

    public static string BuildServerPath(string homePath, string serverName) =>
        EnsureTrailingSeparator(Path.Combine(EnsureTrailingSeparator(Path.GetFullPath(homePath)), "servers", serverName));

    private static ServerStartupResolution Success(string normalizedHome, string serverName, ServerStartupSource source) =>
        new(true, serverName, BuildServerPath(normalizedHome, serverName), source, null);

    private static string EnsureTrailingSeparator(string path) =>
        path.EndsWith(Path.DirectorySeparatorChar)
            ? path
            : path + Path.DirectorySeparatorChar;
}

public sealed record ServerStartupSnapshot(
    ServerStartupResolution Resolution,
    IReadOnlyList<string>? AllowedVersions = null)
{
    public IReadOnlyList<string> EffectiveAllowedVersions => AllowedVersions ?? [];

    public bool IsProductionRuntimeBlocked => true;
    public string BlockedReason =>
        "Live sockets/auth/gameplay are intentionally not started until later milestones port Server::init, ServerList, and gameplay runtime behavior.";
}

public static class ServerStartupLoader
{
    public static ServerStartupSnapshot Load(string homePath, ServerStartupOverrides overrides, IOptions<ServerOptions> serverOptions, IOptions<AdminConfig> adminConfig)
    {
        var resolution = ServerStartupResolver.Resolve(homePath, overrides);
        if (!resolution.Success || resolution.ServerPath is null)
            return new(resolution);

        var configPath = Path.Combine(resolution.ServerPath, "config");
        serverOptions.Value.SetSettings(Gs2Settings.LoadFile(Path.Combine(configPath, "serveroptions.txt")));;

        if (overrides.HasOverrides)
        {
	        if (!string.IsNullOrEmpty(overrides.ServerName))
		        serverOptions.Value.Name = overrides.ServerName;

	        if (!string.IsNullOrEmpty(overrides.ServerIp))
		        serverOptions.Value.ServerIp = overrides.ServerIp;

	        if (!string.IsNullOrEmpty(overrides.LocalIp))
		        serverOptions.Value.LocalIp = overrides.LocalIp;

	        if (!string.IsNullOrEmpty(overrides.ServerInterface))
		        serverOptions.Value.ServerInterface = overrides.ServerInterface;

	        if (!string.IsNullOrEmpty(overrides.StaffAccount))
	        {
		        var staff = serverOptions.Value.Staff.ToList();
		        if (staff.Any(a => a.Equals(overrides.StaffAccount, StringComparison.OrdinalIgnoreCase)))
		        {
			        staff.Add(overrides.StaffAccount);
			        serverOptions.Value.Staff = staff.Distinct().ToArray();
		        }
	        }

	        if (!string.IsNullOrEmpty(overrides.Port) && int.TryParse(overrides.Port, out var port) && port > 0)
		        serverOptions.Value.ServerPort = port;
        }

        adminConfig.Value.SetSettings(Gs2Settings.LoadFile(Path.Combine(configPath, "adminconfig.txt")));

        var allowedVersions = LoadAllowedVersions(Path.Combine(configPath, "allowedversions.txt"));
        return new(resolution, allowedVersions);
    }

    private static IReadOnlyList<string> LoadAllowedVersions(string path)
    {
        if (!File.Exists(path))
            return [];

        return File.ReadAllLines(path)
            .Select(StripComment)
            .Select(line => line.Trim())
            .Where(line => line.Length > 0)
            .ToArray();
    }

    private static string StripComment(string line)
    {
        var slashComment = line.IndexOf("//", StringComparison.Ordinal);
        if (slashComment >= 0)
            line = line[..slashComment];

        var hashComment = line.IndexOf('#');
        return hashComment >= 0 ? line[..hashComment] : line;
    }
}