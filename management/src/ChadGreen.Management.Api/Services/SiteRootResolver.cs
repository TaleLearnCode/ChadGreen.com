namespace ChadGreen.Management.Api.Services;

internal static class SiteRootResolver
{
    public static string Resolve(string? configuredSiteRoot, string startPath)
    {
        if (!string.IsNullOrWhiteSpace(configuredSiteRoot))
        {
            return Path.GetFullPath(configuredSiteRoot);
        }

        var current = new DirectoryInfo(Path.GetFullPath(startPath));
        while (current is not null)
        {
            if (Directory.Exists(Path.Combine(current.FullName, ".git")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        return Path.GetFullPath(startPath);
    }
}
