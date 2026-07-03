using ChadGreen.Management.Api.Options;
using ChadGreen.Management.Shared.Contracts;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace ChadGreen.Management.Api.Services;

public interface IPresentationManagementService
{
    Task<IReadOnlyList<PresentationListItemDto>> ListAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PresentationOptionDto>> ListOptionsAsync(CancellationToken cancellationToken = default);

    Task<PresentationDetailDto?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);

    Task<PresentationDetailDto> CreateAsync(PresentationUpsertRequest request, CancellationToken cancellationToken = default);

    Task<PresentationDetailDto?> UpdateAsync(string slug, PresentationUpsertRequest request, CancellationToken cancellationToken = default);

    Task<PresentationDetailDto?> SetFeaturedAsync(string slug, bool featured, CancellationToken cancellationToken = default);

    Task<PresentationDetailDto?> SetStatusAsync(string slug, string status, CancellationToken cancellationToken = default);

    Task<ArchiveOperationResponse> ArchiveAsync(string slug, CancellationToken cancellationToken = default);
}

public sealed class PresentationManagementService(
    IOptions<ManagementOptions> options,
    IMarkdownFrontmatterFileService markdownService,
    IArchiveService archiveService,
    IIntegrityValidationService integrityValidationService) : IPresentationManagementService
{
    private static readonly HashSet<string> AllowedStatuses = ["active", "retired", "in-development"];
    private static readonly HashSet<string> AllowedTypes = ["session", "workshop", "lightning-talk", "keynote", "panel", "webinar"];
    private static readonly HashSet<string> AllowedLevels = ["introductory", "intermediate", "advanced", "all"];
    private static readonly HashSet<string> ResourceTypes = ["slides", "video", "github", "code", "demo", "blog", "download", "documentation", "other"];
    private readonly ManagementOptions _options = options.Value;

    public async Task<IReadOnlyList<PresentationListItemDto>> ListAsync(CancellationToken cancellationToken = default)
    {
        var siteRoot = ResolveSiteRoot();
        var filePaths = Directory.Exists(GetPresentationsRoot(siteRoot))
            ? Directory.EnumerateFiles(GetPresentationsRoot(siteRoot), "*.md", SearchOption.AllDirectories)
            : Enumerable.Empty<string>();

        var items = new List<PresentationListItemDto>();
        foreach (var filePath in filePaths)
        {
            var document = await markdownService.ReadAsync(filePath, cancellationToken);
            var frontmatter = document.Frontmatter;
            var slug = ContentModelHelpers.GetString(frontmatter, "slug") ?? Path.GetFileNameWithoutExtension(filePath);
            var title = ContentModelHelpers.GetString(frontmatter, "title") ?? slug;
            var description = ContentModelHelpers.GetString(frontmatter, "description") ?? string.Empty;
            var status = NormalizeStatus(ContentModelHelpers.GetString(frontmatter, "status"));
            var featured = ContentModelHelpers.GetBool(frontmatter, "featured");

            items.Add(new PresentationListItemDto(
                slug,
                title,
                description,
                status,
                featured,
                ContentModelHelpers.ToRelativePath(siteRoot, filePath),
                File.GetLastWriteTimeUtc(filePath)));
        }

        return items.OrderBy(item => item.Title, StringComparer.OrdinalIgnoreCase).ToList();
    }

    public async Task<IReadOnlyList<PresentationOptionDto>> ListOptionsAsync(CancellationToken cancellationToken = default)
    {
        var items = await ListAsync(cancellationToken);
        return items
            .Select(item => new PresentationOptionDto(item.Slug, item.Title, item.Status))
            .ToList();
    }

    public async Task<PresentationDetailDto?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        var filePath = await FindFileBySlugAsync(slug, cancellationToken);
        if (filePath is null)
        {
            return null;
        }

        return await MapDetailAsync(filePath, cancellationToken);
    }

    public async Task<PresentationDetailDto> CreateAsync(PresentationUpsertRequest request, CancellationToken cancellationToken = default)
    {
        Validate(request);

        var siteRoot = ResolveSiteRoot();
        var presentationsRoot = GetPresentationsRoot(siteRoot);
        Directory.CreateDirectory(presentationsRoot);

        var slug = ContentModelHelpers.NormalizeSlug(request.Slug, request.Title);
        if (await FindFileBySlugAsync(slug, cancellationToken) is not null)
        {
            throw new InvalidOperationException($"A presentation with slug '{slug}' already exists.");
        }

        var filePath = Path.Combine(presentationsRoot, $"{slug}.md");
        var frontmatter = BuildFrontmatter(request, slug);
        var relativePath = ContentModelHelpers.ToRelativePath(siteRoot, filePath);
        var integrity = await ValidateIntegrityAsync(
            "presentations",
            relativePath,
            slug,
            frontmatter,
            null,
            cancellationToken);
        ThrowWhenBlocked(integrity);

        await markdownService.WriteAsync(filePath, new MarkdownDocument(frontmatter, request.MarkdownBody ?? string.Empty), cancellationToken);

        return await MapDetailAsync(filePath, cancellationToken);
    }

    public async Task<PresentationDetailDto?> UpdateAsync(string slug, PresentationUpsertRequest request, CancellationToken cancellationToken = default)
    {
        Validate(request);

        var sourceFilePath = await FindFileBySlugAsync(slug, cancellationToken);
        if (sourceFilePath is null)
        {
            return null;
        }

        var sourceDocument = await markdownService.ReadAsync(sourceFilePath, cancellationToken);
        var sourceFrontmatterSlug = ContentModelHelpers.GetString(sourceDocument.Frontmatter, "slug") ?? Path.GetFileNameWithoutExtension(sourceFilePath);
        var nextSlug = ContentModelHelpers.NormalizeSlug(request.Slug, request.Title);
        var targetFilePath = Path.Combine(Path.GetDirectoryName(sourceFilePath)!, $"{nextSlug}.md");

        if (!string.Equals(sourceFilePath, targetFilePath, StringComparison.OrdinalIgnoreCase) && File.Exists(targetFilePath))
        {
            throw new InvalidOperationException($"A presentation with slug '{nextSlug}' already exists.");
        }

        var mergedFrontmatter = sourceDocument.Frontmatter;
        foreach (var (key, value) in BuildFrontmatter(request, nextSlug))
        {
            mergedFrontmatter[key] = value;
        }

        var siteRoot = ResolveSiteRoot();
        var relativePath = ContentModelHelpers.ToRelativePath(siteRoot, targetFilePath);
        var slugMutation = string.Equals(sourceFrontmatterSlug, nextSlug, StringComparison.OrdinalIgnoreCase)
            ? null
            : new SlugMutationRequest("presentations", sourceFrontmatterSlug, nextSlug, AutoCascadeReferences: true);
        var integrity = await ValidateIntegrityAsync(
            "presentations",
            relativePath,
            nextSlug,
            mergedFrontmatter,
            slugMutation,
            cancellationToken);
        ThrowWhenBlocked(integrity);

        await markdownService.EnsureNotModifiedSinceAsync(sourceFilePath, request.ExpectedLastModifiedUtc, cancellationToken);
        await markdownService.WriteAsync(targetFilePath, new MarkdownDocument(mergedFrontmatter, request.MarkdownBody ?? string.Empty), cancellationToken);
        if (!string.Equals(sourceFilePath, targetFilePath, StringComparison.OrdinalIgnoreCase))
        {
            File.Delete(sourceFilePath);
        }

        return await MapDetailAsync(targetFilePath, cancellationToken);
    }

    public async Task<PresentationDetailDto?> SetFeaturedAsync(string slug, bool featured, CancellationToken cancellationToken = default)
    {
        var detail = await GetBySlugAsync(slug, cancellationToken);
        if (detail is null)
        {
            return null;
        }

        var request = MapUpsertRequest(detail) with { Featured = featured };
        return await UpdateAsync(slug, request, cancellationToken);
    }

    public async Task<PresentationDetailDto?> SetStatusAsync(string slug, string status, CancellationToken cancellationToken = default)
    {
        var detail = await GetBySlugAsync(slug, cancellationToken);
        if (detail is null)
        {
            return null;
        }

        var request = MapUpsertRequest(detail) with { Status = NormalizeStatus(status) };
        return await UpdateAsync(slug, request, cancellationToken);
    }

    public async Task<ArchiveOperationResponse> ArchiveAsync(string slug, CancellationToken cancellationToken = default)
    {
        var filePath = await FindFileBySlugAsync(slug, cancellationToken);
        if (filePath is null)
        {
            return new ArchiveOperationResponse(ArchiveOperationType.Archive, false, "Presentation not found.", slug);
        }

        var relativePath = ContentModelHelpers.ToRelativePath(ResolveSiteRoot(), filePath);
        return await archiveService.ArchiveAsync(relativePath, cancellationToken);
    }

    private static PresentationUpsertRequest MapUpsertRequest(PresentationDetailDto detail)
        => new(
            detail.Title,
            detail.Slug,
            detail.Description,
            detail.Type,
            detail.Durations,
            detail.Level,
            detail.LearningObjectives,
            detail.Tags,
            detail.RelatedPresentations,
            detail.Resources,
            detail.HeroImage,
            detail.Status,
            detail.Featured,
            detail.Validated,
            detail.MarkdownBody,
            detail.LastModifiedUtc);

    private async Task<PresentationDetailDto> MapDetailAsync(string filePath, CancellationToken cancellationToken)
    {
        var siteRoot = ResolveSiteRoot();
        var document = await markdownService.ReadAsync(filePath, cancellationToken);
        var frontmatter = document.Frontmatter;
        var slug = ContentModelHelpers.GetString(frontmatter, "slug") ?? Path.GetFileNameWithoutExtension(filePath);
        var resources = ParseResources(frontmatter);

        return new PresentationDetailDto(
            slug,
            ContentModelHelpers.GetString(frontmatter, "title") ?? slug,
            ContentModelHelpers.GetString(frontmatter, "description") ?? string.Empty,
            NormalizeType(ContentModelHelpers.GetString(frontmatter, "type")),
            ContentModelHelpers.GetIntList(frontmatter, "durations"),
            NormalizeLevel(ContentModelHelpers.GetString(frontmatter, "level")),
            ContentModelHelpers.GetStringList(frontmatter, "learningObjectives"),
            ContentModelHelpers.GetStringList(frontmatter, "tags"),
            ContentModelHelpers.GetStringList(frontmatter, "relatedPresentations"),
            resources,
            ContentModelHelpers.GetString(frontmatter, "heroImage"),
            NormalizeStatus(ContentModelHelpers.GetString(frontmatter, "status")),
            ContentModelHelpers.GetBool(frontmatter, "featured"),
            ContentModelHelpers.GetBool(frontmatter, "validated"),
            document.Body,
            ContentModelHelpers.ToRelativePath(siteRoot, filePath),
            File.GetLastWriteTimeUtc(filePath));
    }

    private static List<PresentationResourceDto> ParseResources(Dictionary<string, object?> frontmatter)
    {
        var resources = new List<PresentationResourceDto>();
        if (!frontmatter.TryGetValue("resources", out var value) || value is null)
        {
            return resources;
        }

        foreach (var item in ContentModelHelpers.AsList(value))
        {
            var dictionary = ContentModelHelpers.AsDictionary(item);
            var resourceType = dictionary.TryGetValue("type", out var rawType) ? rawType?.ToString() ?? "other" : "other";
            resources.Add(new PresentationResourceDto(
                NormalizeResourceType(resourceType),
                dictionary.TryGetValue("title", out var title) ? title?.ToString() ?? string.Empty : string.Empty,
                dictionary.TryGetValue("url", out var url) ? url?.ToString() ?? string.Empty : string.Empty,
                dictionary.TryGetValue("description", out var description) ? description?.ToString() : null));
        }

        return resources.Where(resource => !string.IsNullOrWhiteSpace(resource.Title) && !string.IsNullOrWhiteSpace(resource.Url)).ToList();
    }

    private static Dictionary<string, object?> BuildFrontmatter(PresentationUpsertRequest request, string slug)
    {
        return new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
        {
            ["title"] = request.Title.Trim(),
            ["slug"] = slug,
            ["description"] = request.Description.Trim(),
            ["type"] = NormalizeType(request.Type),
            ["durations"] = request.Durations.Distinct().OrderBy(value => value).ToList(),
            ["level"] = NormalizeLevel(request.Level),
            ["learningObjectives"] = request.LearningObjectives.Where(value => !string.IsNullOrWhiteSpace(value)).Select(value => value.Trim()).ToList(),
            ["tags"] = request.Tags.Where(value => !string.IsNullOrWhiteSpace(value)).Select(value => value.Trim()).ToList(),
            ["relatedPresentations"] = request.RelatedPresentations.Where(value => !string.IsNullOrWhiteSpace(value)).Select(value => value.Trim()).ToList(),
            ["resources"] = request.Resources
                .Where(resource => !string.IsNullOrWhiteSpace(resource.Title) && !string.IsNullOrWhiteSpace(resource.Url))
                .Select(resource => new Dictionary<string, object?>
                {
                    ["type"] = NormalizeResourceType(resource.Type),
                    ["title"] = resource.Title.Trim(),
                    ["url"] = resource.Url.Trim(),
                    ["description"] = string.IsNullOrWhiteSpace(resource.Description) ? null : resource.Description.Trim()
                })
                .ToList(),
            ["heroImage"] = string.IsNullOrWhiteSpace(request.HeroImage) ? null : request.HeroImage.Trim(),
            ["status"] = NormalizeStatus(request.Status),
            ["featured"] = request.Featured,
            ["validated"] = request.Validated
        };
    }

    private async Task<string?> FindFileBySlugAsync(string slug, CancellationToken cancellationToken)
    {
        var siteRoot = ResolveSiteRoot();
        var normalizedSlug = ContentModelHelpers.NormalizeSlug(slug, slug);
        var fileByName = Path.Combine(GetPresentationsRoot(siteRoot), $"{normalizedSlug}.md");
        if (File.Exists(fileByName))
        {
            return fileByName;
        }

        if (!Directory.Exists(GetPresentationsRoot(siteRoot)))
        {
            return null;
        }

        foreach (var filePath in Directory.EnumerateFiles(GetPresentationsRoot(siteRoot), "*.md", SearchOption.AllDirectories))
        {
            var document = await markdownService.ReadAsync(filePath, cancellationToken);
            var frontmatterSlug = ContentModelHelpers.GetString(document.Frontmatter, "slug") ?? Path.GetFileNameWithoutExtension(filePath);
            if (string.Equals(ContentModelHelpers.NormalizeSlug(frontmatterSlug, frontmatterSlug), normalizedSlug, StringComparison.OrdinalIgnoreCase))
            {
                return filePath;
            }
        }

        return null;
    }

    private static string NormalizeStatus(string? status)
    {
        var normalized = (status ?? "active").Trim().ToLowerInvariant();
        return AllowedStatuses.Contains(normalized) ? normalized : "active";
    }

    private static string NormalizeType(string? type)
    {
        var normalized = (type ?? "session").Trim().ToLowerInvariant();
        return AllowedTypes.Contains(normalized) ? normalized : "session";
    }

    private static string NormalizeLevel(string? level)
    {
        var normalized = (level ?? "all").Trim().ToLowerInvariant();
        return AllowedLevels.Contains(normalized) ? normalized : "all";
    }

    private static string NormalizeResourceType(string? type)
    {
        var normalized = (type ?? "other").Trim().ToLowerInvariant();
        return ResourceTypes.Contains(normalized) ? normalized : "other";
    }

    private static void Validate(PresentationUpsertRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            throw new InvalidOperationException("Title is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Description))
        {
            throw new InvalidOperationException("Description is required.");
        }
    }

    private string ResolveSiteRoot()
        => SiteRootResolver.Resolve(_options.SiteRoot, AppContext.BaseDirectory);

    private static string GetPresentationsRoot(string siteRoot)
        => Path.Combine(siteRoot, "src", "content", "presentations");

    private async Task<IntegrityValidationResponse> ValidateIntegrityAsync(
        string collection,
        string relativePath,
        string entryId,
        Dictionary<string, object?> frontmatter,
        SlugMutationRequest? slugMutation,
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
            SlugMutation: slugMutation,
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
