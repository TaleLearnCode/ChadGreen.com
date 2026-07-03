using ChadGreen.Management.Api.Options;
using ChadGreen.Management.Shared.Contracts;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace ChadGreen.Management.Api.Services;

public interface IAboutManagementService
{
    Task<AboutProfileDetailDto> GetAsync(CancellationToken cancellationToken = default);

    Task<AboutProfileDetailDto> UpdateAsync(AboutProfileUpsertRequest request, CancellationToken cancellationToken = default);
}

public sealed class AboutManagementService(
    IOptions<ManagementOptions> options,
    IMarkdownFrontmatterFileService markdownService,
    IIntegrityValidationService integrityValidationService) : IAboutManagementService
{
    private readonly ManagementOptions _options = options.Value;
    private const string DefaultAuthorSlug = "chad-green";
    private const string DefaultAvatar = "/images/authors/default-avatar.svg";

    public async Task<AboutProfileDetailDto> GetAsync(CancellationToken cancellationToken = default)
    {
        var filePath = await ResolveAuthorPathAsync(cancellationToken);
        return await MapDetailAsync(filePath, cancellationToken);
    }

    public async Task<AboutProfileDetailDto> UpdateAsync(AboutProfileUpsertRequest request, CancellationToken cancellationToken = default)
    {
        Validate(request);

        var filePath = await ResolveAuthorPathAsync(cancellationToken);
        var existingDocument = File.Exists(filePath)
            ? await markdownService.ReadAsync(filePath, cancellationToken)
            : new MarkdownDocument(new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase), string.Empty);
        var siteRoot = ResolveSiteRoot();
        var slug = Path.GetFileNameWithoutExtension(filePath);
        var frontmatter = MergeManagedFrontmatter(existingDocument.Frontmatter, request);

        var relativePath = ContentModelHelpers.ToRelativePath(siteRoot, filePath);
        var integrity = await ValidateIntegrityAsync(
            "authors",
            relativePath,
            slug,
            frontmatter,
            cancellationToken);
        ThrowWhenBlocked(integrity);

        await markdownService.EnsureNotModifiedSinceAsync(filePath, request.ExpectedLastModifiedUtc, cancellationToken);
        await markdownService.WriteAsync(filePath, new MarkdownDocument(frontmatter, request.MarkdownBody ?? string.Empty), cancellationToken);
        return await MapDetailAsync(filePath, cancellationToken);
    }

    private async Task<AboutProfileDetailDto> MapDetailAsync(string filePath, CancellationToken cancellationToken)
    {
        var siteRoot = ResolveSiteRoot();
        var slug = Path.GetFileNameWithoutExtension(filePath);

        if (!File.Exists(filePath))
        {
            return new AboutProfileDetailDto(
                slug,
                "Chad Green",
                null,
                null,
                null,
                DefaultAvatar,
                null,
                null,
                string.Empty,
                ContentModelHelpers.ToRelativePath(siteRoot, filePath));
        }

        var document = await markdownService.ReadAsync(filePath, cancellationToken);
        var frontmatter = document.Frontmatter;

        AboutSocialDto? social = null;
        if (frontmatter.TryGetValue("social", out var socialValue) && socialValue is not null)
        {
            var socialMap = ContentModelHelpers.AsDictionary(socialValue);
            social = new AboutSocialDto(
                socialMap.TryGetValue("twitter", out var twitter) ? twitter?.ToString() : null,
                socialMap.TryGetValue("linkedin", out var linkedin) ? linkedin?.ToString() : null,
                socialMap.TryGetValue("github", out var github) ? github?.ToString() : null,
                socialMap.TryGetValue("youtube", out var youtube) ? youtube?.ToString() : null,
                socialMap.TryGetValue("website", out var website) ? website?.ToString() : null,
                socialMap.TryGetValue("sessionize", out var sessionize) ? sessionize?.ToString() : null);
        }

        return new AboutProfileDetailDto(
            slug,
            ContentModelHelpers.GetString(frontmatter, "name") ?? "Chad Green",
            ContentModelHelpers.GetString(frontmatter, "tagline"),
            ContentModelHelpers.GetString(frontmatter, "bio"),
            ContentModelHelpers.GetString(frontmatter, "shortBio"),
            ContentModelHelpers.GetString(frontmatter, "avatar") ?? DefaultAvatar,
            ContentModelHelpers.GetString(frontmatter, "email"),
            social,
            document.Body,
            ContentModelHelpers.ToRelativePath(siteRoot, filePath),
            File.GetLastWriteTimeUtc(filePath));
    }

    private static Dictionary<string, object?> MergeManagedFrontmatter(Dictionary<string, object?> existingFrontmatter, AboutProfileUpsertRequest request)
    {
        var frontmatter = new Dictionary<string, object?>(existingFrontmatter, StringComparer.OrdinalIgnoreCase)
        {
            ["name"] = request.Name.Trim(),
            ["avatar"] = string.IsNullOrWhiteSpace(request.Avatar) ? DefaultAvatar : request.Avatar.Trim()
        };

        SetOptionalStringField(frontmatter, "tagline", request.Tagline);
        SetOptionalStringField(frontmatter, "bio", request.Bio);
        SetOptionalStringField(frontmatter, "shortBio", request.ShortBio);
        SetOptionalStringField(frontmatter, "email", request.Email);

        var socialPayload = frontmatter.TryGetValue("social", out var existingSocial)
            ? new Dictionary<string, object?>(ContentModelHelpers.AsDictionary(existingSocial), StringComparer.OrdinalIgnoreCase)
            : new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

        foreach (var managedSocialField in new[] { "twitter", "linkedin", "github", "youtube", "website", "sessionize" })
        {
            socialPayload.Remove(managedSocialField);
        }

        if (request.Social is not null)
        {
            SetOptionalStringField(socialPayload, "twitter", request.Social.Twitter);
            SetOptionalStringField(socialPayload, "linkedin", request.Social.Linkedin);
            SetOptionalStringField(socialPayload, "github", request.Social.Github);
            SetOptionalStringField(socialPayload, "youtube", request.Social.Youtube);
            SetOptionalStringField(socialPayload, "website", request.Social.Website);
            SetOptionalStringField(socialPayload, "sessionize", request.Social.Sessionize);
        }

        if (socialPayload.Count > 0)
        {
            frontmatter["social"] = socialPayload;
        }
        else
        {
            frontmatter.Remove("social");
        }

        return frontmatter;
    }

    private async Task<string> ResolveAuthorPathAsync(CancellationToken cancellationToken)
    {
        var root = GetAuthorsRoot(ResolveSiteRoot());
        Directory.CreateDirectory(root);

        var preferredPath = Path.Combine(root, $"{DefaultAuthorSlug}.md");
        if (File.Exists(preferredPath))
        {
            return preferredPath;
        }

        var firstExisting = Directory.EnumerateFiles(root, "*.md", SearchOption.TopDirectoryOnly).OrderBy(path => path, StringComparer.OrdinalIgnoreCase).FirstOrDefault();
        if (firstExisting is not null)
        {
            return firstExisting;
        }

        cancellationToken.ThrowIfCancellationRequested();
        return preferredPath;
    }

    private static void Validate(AboutProfileUpsertRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new InvalidOperationException("Name is required.");
        }
    }

    private static void SetOptionalStringField(Dictionary<string, object?> target, string key, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            target[key] = value.Trim();
        }
        else
        {
            target.Remove(key);
        }
    }

    private string ResolveSiteRoot()
        => SiteRootResolver.Resolve(_options.SiteRoot, AppContext.BaseDirectory);

    private static string GetAuthorsRoot(string siteRoot)
        => Path.Combine(siteRoot, "src", "content", "authors");

    private async Task<IntegrityValidationResponse> ValidateIntegrityAsync(
        string collection,
        string relativePath,
        string entryId,
        Dictionary<string, object?> frontmatter,
        CancellationToken cancellationToken)
    {
        var request = new IntegrityValidationRequest(
            Source: "manual",
            Collection: collection,
            FilePath: relativePath,
            EntryId: entryId,
            Frontmatter: JsonSerializer.SerializeToElement(frontmatter),
            Body: null,
            ChangedFields: null,
            SlugMutation: null,
            IncludeExternalLinkChecks: true);

        return await integrityValidationService.ValidateAsync(request, cancellationToken);
    }

    private static void ThrowWhenBlocked(IntegrityValidationResponse integrity)
    {
        if (integrity.Summary.BlocksSave)
        {
            throw new IntegrityGateException(integrity);
        }
    }
}
