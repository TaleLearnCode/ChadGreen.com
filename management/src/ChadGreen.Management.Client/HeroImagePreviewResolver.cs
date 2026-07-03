namespace ChadGreen.Management.Client;

internal static class HeroImagePreviewResolver
{
    private const string LocalSiteBaseUrl = "http://localhost:4321";

    public static string? Resolve(string? heroImage)
    {
        if (string.IsNullOrWhiteSpace(heroImage))
        {
            return null;
        }

        var value = heroImage.Trim().Replace('\\', '/');
        if (Uri.TryCreate(value, UriKind.Absolute, out var absoluteUri))
        {
            if (absoluteUri.Scheme.Equals(Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) ||
                absoluteUri.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
            {
                return absoluteUri.ToString();
            }

            if (absoluteUri.Scheme.Equals(Uri.UriSchemeFile, StringComparison.OrdinalIgnoreCase))
            {
                value = absoluteUri.LocalPath.Replace('\\', '/');
            }
        }

        while (value.StartsWith("./", StringComparison.Ordinal))
        {
            value = value[2..];
        }

        while (value.StartsWith("../", StringComparison.Ordinal))
        {
            value = value[3..];
        }

        if (value.StartsWith("~/", StringComparison.Ordinal))
        {
            value = value[2..];
        }

        if (value.StartsWith("public/", StringComparison.OrdinalIgnoreCase))
        {
            value = value["public".Length..];
        }
        else
        {
            var publicSegmentIndex = value.LastIndexOf("/public/", StringComparison.OrdinalIgnoreCase);
            if (publicSegmentIndex >= 0)
            {
                value = value[(publicSegmentIndex + "/public".Length)..];
            }
        }

        if (!value.StartsWith("/", StringComparison.Ordinal))
        {
            value = "/" + value;
        }

        return $"{LocalSiteBaseUrl}{value}";
    }
}
