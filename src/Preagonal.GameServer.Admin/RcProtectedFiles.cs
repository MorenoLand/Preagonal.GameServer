namespace Preagonal.GameServer.Admin;

public sealed record RcProtectedFileDecision(bool Allowed, string? Message);

public static class RcProtectedFiles
{
    private static readonly HashSet<string> ProtectedFiles = new(StringComparer.OrdinalIgnoreCase)
    {
        "accounts/defaultaccount.txt",
        "config/adminconfig.txt",
        "config/allowedversions.txt",
        "config/rchelp.txt"
    };

    public static RcProtectedFileDecision EvaluateDownload(string path, AdminRight rights)
    {
        var normalized = path.Replace('\\', '/');
        if (ProtectedFiles.Contains(normalized) && !AdminRights.HasRight(rights, AdminRight.ModifyStaffAccount))
        {
            return new(false, $"Insufficient rights to download/view {normalized}");
        }

        return new(true, null);
    }
}
