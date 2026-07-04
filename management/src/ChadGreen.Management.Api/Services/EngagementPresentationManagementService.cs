using ChadGreen.Management.Api.Options;
using ChadGreen.Management.Shared.Contracts;
using Microsoft.Extensions.Options;

namespace ChadGreen.Management.Api.Services;

public interface IEngagementPresentationManagementService
{
    Task<IReadOnlyList<EngagementPresentationListItemDto>> ListByEngagementAsync(string eventSlug, CancellationToken cancellationToken = default);

    Task<EngagementPresentationDetailDto?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);

    Task<EngagementPresentationDetailDto> EnsureCreatedAsync(string eventSlug, string presentationSlug, CancellationToken cancellationToken = default);

    Task<EngagementPresentationDetailDto?> UpdateAsync(string slug, EngagementPresentationUpsertRequest request, CancellationToken cancellationToken = default);

    Task<ArchiveOperationResponse> ArchiveAsync(string slug, CancellationToken cancellationToken = default);
}

public sealed class EngagementPresentationManagementService(
    IOptions<ManagementOptions> options,
    IMarkdownFrontmatterFileService markdownService,
    IPresentationManagementService presentationService,
    IArchiveService archiveService) : IEngagementPresentationManagementService
{
    private static readonly HashSet<string> AllowedCanonicalPaths = ["presentation-event", "speaking-session"];
    private static readonly HashSet<string> AllowedLinkTypes = ["slides", "video", "github", "code", "demo", "blog", "download", "documentation", "lab", "other"];
    private readonly ManagementOptions _options = options.Value;

    public async Task<IReadOnlyList<EngagementPresentationListItemDto>> ListByEngagementAsync(string eventSlug, CancellationToken cancellationToken = default)
    {
        var siteRoot = ResolveSiteRoot();
        var root = GetEngagementPresentationsRoot(siteRoot);
        if (!Directory.Exists(root))
        {
            return [];
        }

        var items = new List<EngagementPresentationListItemDto>();
        var normalizedSlug = eventSlug.Trim().ToLowerInvariant();

        foreach (var filePath in Directory.EnumerateFiles(root, "*.md", SearchOption.AllDirectories))
        {
            var document = await markdownService.ReadAsync(filePath, cancellationToken);
            var frontmatter = document.Frontmatter;
            var fileEventSlug = ContentModelHelpers.GetString(frontmatter, "eventSlug") ?? string.Empty;

            if (!string.Equals(fileEventSlug, normalizedSlug, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var slug = Path.GetFileNameWithoutExtension(filePath);
            items.Add(new EngagementPresentationListItemDto(
                slug,
                ContentModelHelpers.GetString(frontmatter, "title") ?? slug,
                fileEventSlug,
                ContentModelHelpers.GetString(frontmatter, "presentationSlug") ?? string.Empty,
                ContentModelHelpers.ToRelativePath(siteRoot, filePath),
                File.GetLastWriteTimeUtc(filePath)));
        }

        return items.OrderBy(item => item.PresentationSlug, StringComparer.OrdinalIgnoreCase).ToList();
    }

    public async Task<EngagementPresentationDetailDto?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        var filePath = FindFile(slug);
        return filePath is null ? null : await MapDetailAsync(filePath, cancellationToken);
    }

    public async Task<EngagementPresentationDetailDto> EnsureCreatedAsync(string eventSlug, string presentationSlug, CancellationToken cancellationToken = default)
    {
        var slug = BuildSlug(eventSlug, presentationSlug);
        var siteRoot = ResolveSiteRoot();
        var root = GetEngagementPresentationsRoot(siteRoot);
        var filePath = Path.Combine(root, $"{slug}.md");

        if (File.Exists(filePath))
        {
            return await MapDetailAsync(filePath, cancellationToken);
        }

        var basePresentation = await presentationService.GetBySlugAsync(presentationSlug, cancellationToken)
            ?? throw new InvalidOperationException($"Base presentation '{presentationSlug}' not found.");

        Directory.CreateDirectory(root);

        var body = BuildInitialBody(basePresentation, eventSlug);
        var frontmatter = BuildFrontmatter(
            eventSlug,
            presentationSlug,
            sessionSlug: presentationSlug,
            title: basePresentation.Title,
            description: BuildInitialDescription(basePresentation, eventSlug),
            sessionTitle: null,
            date: null,
            time: null,
            timeZone: null,
            room: null,
            sessionUrl: null,
            thumbnail: null,
            heroImage: basePresentation.HeroImage,
            links: [],
            canonicalPath: "speaking-session");

        await markdownService.WriteAsync(filePath, new MarkdownDocument(frontmatter, body), cancellationToken);
        return await MapDetailAsync(filePath, cancellationToken);
    }

    public async Task<EngagementPresentationDetailDto?> UpdateAsync(string slug, EngagementPresentationUpsertRequest request, CancellationToken cancellationToken = default)
    {
        Validate(request);

        var filePath = FindFile(slug);
        if (filePath is null)
        {
            return null;
        }

        var sourceDocument = await markdownService.ReadAsync(filePath, cancellationToken);
        var mergedFrontmatter = sourceDocument.Frontmatter;
        foreach (var (key, value) in BuildFrontmatter(
            request.EventSlug,
            request.PresentationSlug,
            request.SessionSlug,
            request.Title,
            request.Description,
            request.SessionTitle,
            request.Date,
            request.Time,
            request.TimeZone,
            request.Room,
            request.SessionUrl,
            request.Thumbnail,
            request.HeroImage,
            request.Links,
            request.CanonicalPath))
        {
            mergedFrontmatter[key] = value;
        }

        SynchronizeOptionalStringField(mergedFrontmatter, "sessionTitle", request.SessionTitle);
        SynchronizeOptionalDateField(mergedFrontmatter, "date", request.Date);
        SynchronizeOptionalStringField(mergedFrontmatter, "time", request.Time);
        SynchronizeOptionalStringField(mergedFrontmatter, "timeZone", request.TimeZone);
        SynchronizeOptionalStringField(mergedFrontmatter, "room", request.Room);
        SynchronizeOptionalStringField(mergedFrontmatter, "sessionUrl", request.SessionUrl);
        SynchronizeOptionalStringField(mergedFrontmatter, "thumbnail", request.Thumbnail);
        SynchronizeOptionalStringField(mergedFrontmatter, "heroImage", request.HeroImage);

        await markdownService.EnsureNotModifiedSinceAsync(filePath, request.ExpectedLastModifiedUtc, cancellationToken);
        await markdownService.WriteAsync(filePath, new MarkdownDocument(mergedFrontmatter, request.MarkdownBody ?? string.Empty), cancellationToken);

        return await MapDetailAsync(filePath, cancellationToken);
    }

    public async Task<ArchiveOperationResponse> ArchiveAsync(string slug, CancellationToken cancellationToken = default)
    {
        var filePath = FindFile(slug);
        if (filePath is null)
        {
            return new ArchiveOperationResponse(ArchiveOperationType.Archive, false, "Engagement presentation not found.", slug);
        }

        var relativePath = ContentModelHelpers.ToRelativePath(ResolveSiteRoot(), filePath);
        return await archiveService.ArchiveAsync(relativePath, cancellationToken);
    }

    private async Task<EngagementPresentationDetailDto> MapDetailAsync(string filePath, CancellationToken cancellationToken)
    {
        var siteRoot = ResolveSiteRoot();
        var document = await markdownService.ReadAsync(filePath, cancellationToken);
        var frontmatter = document.Frontmatter;
        var slug = Path.GetFileNameWithoutExtension(filePath);

        return new EngagementPresentationDetailDto(
            slug,
            ContentModelHelpers.GetString(frontmatter, "title") ?? slug,
            ContentModelHelpers.GetString(frontmatter, "description") ?? string.Empty,
            ContentModelHelpers.GetString(frontmatter, "eventSlug") ?? string.Empty,
            ContentModelHelpers.GetString(frontmatter, "presentationSlug") ?? string.Empty,
            ContentModelHelpers.GetString(frontmatter, "sessionSlug") ?? string.Empty,
            ContentModelHelpers.GetString(frontmatter, "sessionTitle"),
            ParseDate(frontmatter, "date"),
            ContentModelHelpers.GetString(frontmatter, "time"),
            ContentModelHelpers.GetString(frontmatter, "timeZone"),
            ContentModelHelpers.GetString(frontmatter, "room"),
            ContentModelHelpers.GetString(frontmatter, "sessionUrl"),
            ContentModelHelpers.GetString(frontmatter, "thumbnail"),
            ContentModelHelpers.GetString(frontmatter, "heroImage"),
            ParseLinks(frontmatter),
            NormalizeCanonicalPath(ContentModelHelpers.GetString(frontmatter, "canonicalPath")),
            document.Body,
            ContentModelHelpers.ToRelativePath(siteRoot, filePath),
            File.GetLastWriteTimeUtc(filePath));
    }

    private string? FindFile(string slug)
    {
        var siteRoot = ResolveSiteRoot();
        var root = GetEngagementPresentationsRoot(siteRoot);
        var normalized = ContentModelHelpers.NormalizeSlug(slug, slug);
        var candidate = Path.Combine(root, $"{normalized}.md");
        return File.Exists(candidate) ? candidate : null;
    }

    private static Dictionary<string, object?> BuildFrontmatter(
        string eventSlug,
        string presentationSlug,
        string sessionSlug,
        string title,
        string description,
        string? sessionTitle,
        DateOnly? date,
        string? time,
        string? timeZone,
        string? room,
        string? sessionUrl,
        string? thumbnail,
        string? heroImage,
        List<EngagementPresentationLinkDto> links,
        string canonicalPath)
    {
        var frontmatter = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
        {
            ["title"] = title.Trim(),
            ["description"] = description.Trim(),
            ["eventSlug"] = eventSlug.Trim(),
            ["presentationSlug"] = presentationSlug.Trim(),
            ["sessionSlug"] = sessionSlug.Trim(),
            ["canonicalPath"] = NormalizeCanonicalPath(canonicalPath),
        };

        SetOptionalStringField(frontmatter, "sessionTitle", sessionTitle);
        SetOptionalDateField(frontmatter, "date", date);
        SetOptionalStringField(frontmatter, "time", time);
        SetOptionalStringField(frontmatter, "timeZone", timeZone);
        SetOptionalStringField(frontmatter, "room", room);
        SetOptionalStringField(frontmatter, "sessionUrl", sessionUrl);
        SetOptionalStringField(frontmatter, "thumbnail", thumbnail);
        SetOptionalStringField(frontmatter, "heroImage", heroImage);

        if (links.Count > 0)
        {
            frontmatter["links"] = BuildLinkFrontmatter(links);
        }

        return frontmatter;
    }

    private static List<object> BuildLinkFrontmatter(IEnumerable<EngagementPresentationLinkDto> links)
    {
        var entries = new List<object>();
        foreach (var link in links)
        {
            var entry = new Dictionary<string, object?>
            {
                ["type"] = NormalizeLinkType(link.Type),
                ["title"] = link.Title,
                ["url"] = link.Url,
            };
            if (!string.IsNullOrWhiteSpace(link.Description))
            {
                entry["description"] = link.Description;
            }
            entries.Add(entry);
        }
        return entries;
    }

    private static List<EngagementPresentationLinkDto> ParseLinks(Dictionary<string, object?> frontmatter)
    {
        if (!frontmatter.TryGetValue("links", out var value) || value is null)
        {
            return [];
        }

        var result = new List<EngagementPresentationLinkDto>();
        foreach (var item in ContentModelHelpers.AsList(value))
        {
            var map = ContentModelHelpers.AsDictionary(item);
            var type = map.TryGetValue("type", out var t) ? t?.ToString() : null;
            var title = map.TryGetValue("title", out var ti) ? ti?.ToString() : null;
            var url = map.TryGetValue("url", out var u) ? u?.ToString() : null;
            var description = map.TryGetValue("description", out var d) ? d?.ToString() : null;

            if (!string.IsNullOrWhiteSpace(type) && !string.IsNullOrWhiteSpace(title) && !string.IsNullOrWhiteSpace(url))
            {
                result.Add(new EngagementPresentationLinkDto(NormalizeLinkType(type), title, url, description));
            }
        }

        return result;
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

    private static void Validate(EngagementPresentationUpsertRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            throw new InvalidOperationException("Title is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Description))
        {
            throw new InvalidOperationException("Description is required.");
        }

        if (string.IsNullOrWhiteSpace(request.EventSlug))
        {
            throw new InvalidOperationException("Event slug is required.");
        }

        if (string.IsNullOrWhiteSpace(request.PresentationSlug))
        {
            throw new InvalidOperationException("Presentation slug is required.");
        }

        if (string.IsNullOrWhiteSpace(request.SessionSlug))
        {
            throw new InvalidOperationException("Session slug is required.");
        }

        foreach (var link in request.Links)
        {
            if (string.IsNullOrWhiteSpace(link.Title))
            {
                throw new InvalidOperationException("Each link must have a title.");
            }

            if (string.IsNullOrWhiteSpace(link.Url))
            {
                throw new InvalidOperationException("Each link must have a URL.");
            }
        }
    }

    private static string BuildSlug(string eventSlug, string presentationSlug)
        => $"{ContentModelHelpers.NormalizeSlug(eventSlug, eventSlug)}-{ContentModelHelpers.NormalizeSlug(presentationSlug, presentationSlug)}";

    private static string BuildInitialDescription(PresentationDetailDto basePresentation, string eventSlug)
        => $"Session details and resources for {eventSlug}: {basePresentation.Title}.";

    private static string BuildInitialBody(PresentationDetailDto basePresentation, string eventSlug)
    {
        var lines = new List<string>
        {
            $"## {basePresentation.Title}",
            string.Empty,
            basePresentation.Description,
        };

        if (basePresentation.LearningObjectives.Count > 0)
        {
            lines.Add(string.Empty);
            lines.Add("### Learning Objectives");
            lines.Add(string.Empty);
            foreach (var objective in basePresentation.LearningObjectives)
            {
                lines.Add($"- {objective}");
            }
        }

        return string.Join("\n", lines);
    }

    private static string NormalizeCanonicalPath(string? value)
    {
        var normalized = (value ?? "speaking-session").Trim().ToLowerInvariant();
        return AllowedCanonicalPaths.Contains(normalized) ? normalized : "speaking-session";
    }

    private static string NormalizeLinkType(string? value)
    {
        var normalized = (value ?? "other").Trim().ToLowerInvariant();
        return AllowedLinkTypes.Contains(normalized) ? normalized : "other";
    }

    private static void SetOptionalStringField(Dictionary<string, object?> target, string key, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            target[key] = value.Trim();
        }
    }

    private static void SetOptionalDateField(Dictionary<string, object?> target, string key, DateOnly? value)
    {
        if (value is not null)
        {
            target[key] = value.Value.ToString("yyyy-MM-dd");
        }
    }

    private static void SynchronizeOptionalStringField(Dictionary<string, object?> target, string key, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            target.Remove(key);
        }
        else
        {
            target[key] = value.Trim();
        }
    }

    private static void SynchronizeOptionalDateField(Dictionary<string, object?> target, string key, DateOnly? value)
    {
        if (value is null)
        {
            target.Remove(key);
        }
        else
        {
            target[key] = value.Value.ToString("yyyy-MM-dd");
        }
    }

    private string ResolveSiteRoot()
        => SiteRootResolver.Resolve(_options.SiteRoot, AppContext.BaseDirectory);

    private static string GetEngagementPresentationsRoot(string siteRoot)
        => Path.Combine(siteRoot, "src", "content", "engagementPresentations");
}
