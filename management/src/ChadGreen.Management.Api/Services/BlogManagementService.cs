using ChadGreen.Management.Api.Options;
using ChadGreen.Management.Shared.Contracts;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace ChadGreen.Management.Api.Services;

public interface IBlogManagementService
{
    Task<IReadOnlyList<BlogListItemDto>> ListAsync(CancellationToken cancellationToken = default);

    Task<BlogDetailDto?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);

    Task<BlogDetailDto> CreateAsync(BlogUpsertRequest request, CancellationToken cancellationToken = default);

    Task<BlogDetailDto?> UpdateAsync(string slug, BlogUpsertRequest request, CancellationToken cancellationToken = default);
}

public sealed class BlogManagementService(
    IOptions<ManagementOptions> options,
    IMarkdownFrontmatterFileService markdownService,
    IIntegrityValidationService integrityValidationService) : IBlogManagementService
{
    private static readonly HashSet<string> AllowedCategories =
    [
        "Technical",
        "Speaking",
        "Community",
        "Career",
        "Tutorial",
        "Announcement",
        "Personal",
        "Other"
    ];

    private readonly ManagementOptions _options = options.Value;
    private const string DefaultAuthor = "Chad Green";
    private const string DefaultHeroImage = "/images/blog/default-hero.svg";

    public async Task<IReadOnlyList<BlogListItemDto>> ListAsync(CancellationToken cancellationToken = default)
    {
        var siteRoot = ResolveSiteRoot();
        var root = GetBlogRoot(siteRoot);
        if (!Directory.Exists(root))
        {
            return [];
        }

        var items = new List<BlogListItemDto>();
        foreach (var filePath in Directory.EnumerateFiles(root, "*.md", SearchOption.AllDirectories))
        {
            var document = await markdownService.ReadAsync(filePath, cancellationToken);
            var frontmatter = document.Frontmatter;
            var slug = Path.GetFileNameWithoutExtension(filePath);
            var pubDate = ParseDate(frontmatter, "pubDate") ?? DateOnly.FromDateTime(DateTime.Today);

            items.Add(new BlogListItemDto(
                slug,
                ContentModelHelpers.GetString(frontmatter, "title") ?? slug,
                ContentModelHelpers.GetString(frontmatter, "description") ?? string.Empty,
                NormalizeCategory(ContentModelHelpers.GetString(frontmatter, "category")),
                pubDate,
                ContentModelHelpers.GetBool(frontmatter, "draft"),
                ContentModelHelpers.GetBool(frontmatter, "featured"),
                ContentModelHelpers.ToRelativePath(siteRoot, filePath),
                File.GetLastWriteTimeUtc(filePath)));
        }

        return items.OrderByDescending(item => item.PubDate).ThenBy(item => item.Title, StringComparer.OrdinalIgnoreCase).ToList();
    }

    public async Task<BlogDetailDto?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        var filePath = await FindFileBySlugAsync(slug, cancellationToken);
        if (filePath is null)
        {
            return null;
        }

        return await MapDetailAsync(filePath, cancellationToken);
    }

    public async Task<BlogDetailDto> CreateAsync(BlogUpsertRequest request, CancellationToken cancellationToken = default)
    {
        request = NormalizeRequest(request);
        Validate(request);

        var siteRoot = ResolveSiteRoot();
        var root = GetBlogRoot(siteRoot);
        Directory.CreateDirectory(root);

        var slug = ContentModelHelpers.NormalizeSlug(request.Slug, request.Title);
        if (await FindFileBySlugAsync(slug, cancellationToken) is not null)
        {
            throw new InvalidOperationException($"A blog post with slug '{slug}' already exists.");
        }

        var filePath = Path.Combine(root, $"{slug}.md");
        var frontmatter = BuildFrontmatter(request);
        var relativePath = ContentModelHelpers.ToRelativePath(siteRoot, filePath);
        var integrity = await ValidateIntegrityAsync(
            "blog",
            relativePath,
            slug,
            frontmatter,
            null,
            cancellationToken);
        ThrowWhenBlocked(integrity);

        await markdownService.WriteAsync(filePath, new MarkdownDocument(frontmatter, request.MarkdownBody ?? string.Empty), cancellationToken);
        return await MapDetailAsync(filePath, cancellationToken);
    }

    public async Task<BlogDetailDto?> UpdateAsync(string slug, BlogUpsertRequest request, CancellationToken cancellationToken = default)
    {
        request = NormalizeRequest(request);
        Validate(request);

        var sourcePath = await FindFileBySlugAsync(slug, cancellationToken);
        if (sourcePath is null)
        {
            return null;
        }

        var nextSlug = ContentModelHelpers.NormalizeSlug(request.Slug, request.Title);
        var targetPath = Path.Combine(Path.GetDirectoryName(sourcePath)!, $"{nextSlug}.md");
        if (!string.Equals(sourcePath, targetPath, StringComparison.OrdinalIgnoreCase) && File.Exists(targetPath))
        {
            throw new InvalidOperationException($"A blog post with slug '{nextSlug}' already exists.");
        }

        var mergedFrontmatter = BuildFrontmatter(request);

        var siteRoot = ResolveSiteRoot();
        var relativePath = ContentModelHelpers.ToRelativePath(siteRoot, targetPath);
        var integrity = await ValidateIntegrityAsync(
            "blog",
            relativePath,
            nextSlug,
            mergedFrontmatter,
            null,
            cancellationToken);
        ThrowWhenBlocked(integrity);

        await markdownService.EnsureNotModifiedSinceAsync(sourcePath, request.ExpectedLastModifiedUtc, cancellationToken);
        await markdownService.WriteAsync(targetPath, new MarkdownDocument(mergedFrontmatter, request.MarkdownBody ?? string.Empty), cancellationToken);
        if (!string.Equals(sourcePath, targetPath, StringComparison.OrdinalIgnoreCase))
        {
            File.Delete(sourcePath);
        }

        return await MapDetailAsync(targetPath, cancellationToken);
    }

    private async Task<BlogDetailDto> MapDetailAsync(string filePath, CancellationToken cancellationToken)
    {
        var siteRoot = ResolveSiteRoot();
        var document = await markdownService.ReadAsync(filePath, cancellationToken);
        var frontmatter = document.Frontmatter;

        return new BlogDetailDto(
            Path.GetFileNameWithoutExtension(filePath),
            ContentModelHelpers.GetString(frontmatter, "title") ?? string.Empty,
            ContentModelHelpers.GetString(frontmatter, "description") ?? string.Empty,
            ContentModelHelpers.GetString(frontmatter, "author") ?? DefaultAuthor,
            ParseDate(frontmatter, "pubDate") ?? DateOnly.FromDateTime(DateTime.Today),
            ParseDate(frontmatter, "updatedDate"),
            NormalizeCategory(ContentModelHelpers.GetString(frontmatter, "category")),
            ContentModelHelpers.GetStringList(frontmatter, "tags"),
            ContentModelHelpers.GetStringList(frontmatter, "relatedPresentations"),
            ContentModelHelpers.GetString(frontmatter, "heroImage") ?? DefaultHeroImage,
            ContentModelHelpers.GetString(frontmatter, "heroImageAlt"),
            ContentModelHelpers.GetBool(frontmatter, "draft"),
            ContentModelHelpers.GetBool(frontmatter, "featured"),
            ContentModelHelpers.GetString(frontmatter, "canonicalUrl"),
            document.Body,
            ContentModelHelpers.ToRelativePath(siteRoot, filePath),
            File.GetLastWriteTimeUtc(filePath));
    }

    private static Dictionary<string, object?> BuildFrontmatter(BlogUpsertRequest request)
    {
        var frontmatter = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
        {
            ["title"] = request.Title.Trim(),
            ["description"] = request.Description.Trim(),
            ["author"] = string.IsNullOrWhiteSpace(request.Author) ? DefaultAuthor : request.Author.Trim(),
            ["pubDate"] = request.PubDate.ToString("yyyy-MM-dd"),
            ["category"] = NormalizeCategory(request.Category),
            ["tags"] = request.Tags.Where(value => !string.IsNullOrWhiteSpace(value)).Select(value => value.Trim()).ToList(),
            ["relatedPresentations"] = request.RelatedPresentations.Where(value => !string.IsNullOrWhiteSpace(value)).Select(value => value.Trim()).ToList(),
            ["heroImage"] = string.IsNullOrWhiteSpace(request.HeroImage) ? DefaultHeroImage : request.HeroImage.Trim(),
            ["draft"] = request.Draft,
            ["featured"] = request.Featured
        };

        if (request.UpdatedDate is not null)
        {
            frontmatter["updatedDate"] = request.UpdatedDate.Value.ToString("yyyy-MM-dd");
        }

        SetOptionalStringField(frontmatter, "heroImageAlt", request.HeroImageAlt);
        SetOptionalStringField(frontmatter, "canonicalUrl", request.CanonicalUrl);

        return frontmatter;
    }

    private async Task<string?> FindFileBySlugAsync(string slug, CancellationToken cancellationToken)
    {
        var normalizedSlug = ContentModelHelpers.NormalizeSlug(slug, slug);
        var fileByName = Path.Combine(GetBlogRoot(ResolveSiteRoot()), $"{normalizedSlug}.md");
        if (File.Exists(fileByName))
        {
            return fileByName;
        }

        if (!Directory.Exists(GetBlogRoot(ResolveSiteRoot())))
        {
            return null;
        }

        foreach (var filePath in Directory.EnumerateFiles(GetBlogRoot(ResolveSiteRoot()), "*.md", SearchOption.AllDirectories))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var fileSlug = Path.GetFileNameWithoutExtension(filePath);
            if (string.Equals(ContentModelHelpers.NormalizeSlug(fileSlug, fileSlug), normalizedSlug, StringComparison.OrdinalIgnoreCase))
            {
                return filePath;
            }
        }

        return null;
    }

    private static DateOnly? ParseDate(Dictionary<string, object?> frontmatter, string key)
    {
        if (!frontmatter.TryGetValue(key, out var value) || value is null)
        {
            return null;
        }

        if (value is DateTime dateTime)
        {
            return DateOnly.FromDateTime(dateTime);
        }

        if (DateOnly.TryParse(value.ToString(), out var dateOnly))
        {
            return dateOnly;
        }

        if (DateTime.TryParse(value.ToString(), out var parsedDateTime))
        {
            return DateOnly.FromDateTime(parsedDateTime);
        }

        return null;
    }

    private static string NormalizeCategory(string? category)
    {
        if (string.IsNullOrWhiteSpace(category))
        {
            return "Other";
        }

        var normalized = category.Trim();
        return AllowedCategories.Contains(normalized) ? normalized : "Other";
    }

    private static BlogUpsertRequest NormalizeRequest(BlogUpsertRequest request)
    {
        return request with
        {
            Tags = request.Tags
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(value => value.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList(),
            RelatedPresentations = request.RelatedPresentations
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(value => value.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList()
        };
    }

    private static void Validate(BlogUpsertRequest request)
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

    private static void SetOptionalStringField(Dictionary<string, object?> target, string key, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            target[key] = value.Trim();
        }
    }

    private string ResolveSiteRoot()
        => SiteRootResolver.Resolve(_options.SiteRoot, AppContext.BaseDirectory);

    private static string GetBlogRoot(string siteRoot)
        => Path.Combine(siteRoot, "src", "content", "blog");

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
