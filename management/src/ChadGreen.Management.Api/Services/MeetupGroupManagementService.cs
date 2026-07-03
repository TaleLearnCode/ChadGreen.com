using ChadGreen.Management.Api.Options;
using ChadGreen.Management.Shared.Contracts;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace ChadGreen.Management.Api.Services;

public interface IMeetupGroupManagementService
{
    Task<IReadOnlyList<MeetupGroupListItemDto>> ListAsync(CancellationToken cancellationToken = default);

    Task<MeetupGroupDetailDto?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);

    Task<MeetupGroupDetailDto> CreateAsync(MeetupGroupUpsertRequest request, CancellationToken cancellationToken = default);

    Task<MeetupGroupDetailDto?> UpdateAsync(string slug, MeetupGroupUpsertRequest request, CancellationToken cancellationToken = default);

    Task<ArchiveOperationResponse> ArchiveAsync(string slug, CancellationToken cancellationToken = default);
}

public sealed class MeetupGroupManagementService(
    IOptions<ManagementOptions> options,
    IMarkdownFrontmatterFileService markdownService,
    IArchiveService archiveService,
    IIntegrityValidationService integrityValidationService) : IMeetupGroupManagementService
{
    private readonly ManagementOptions _options = options.Value;
    private const string DefaultHeroImage = "/images/meetups/default-meetup.svg";

    public async Task<IReadOnlyList<MeetupGroupListItemDto>> ListAsync(CancellationToken cancellationToken = default)
    {
        var siteRoot = ResolveSiteRoot();
        var groupsRoot = GetMeetupGroupsRoot(siteRoot);
        var eventsByGroup = await GetEventCountsByGroupAsync(cancellationToken);
        if (!Directory.Exists(groupsRoot))
        {
            return [];
        }

        var items = new List<MeetupGroupListItemDto>();
        foreach (var filePath in Directory.EnumerateFiles(groupsRoot, "*.md", SearchOption.AllDirectories))
        {
            var document = await markdownService.ReadAsync(filePath, cancellationToken);
            var frontmatter = document.Frontmatter;
            var slug = ContentModelHelpers.GetString(frontmatter, "slug") ?? Path.GetFileNameWithoutExtension(filePath);

            items.Add(new MeetupGroupListItemDto(
                slug,
                ContentModelHelpers.GetString(frontmatter, "title") ?? slug,
                ContentModelHelpers.GetString(frontmatter, "city") ?? string.Empty,
                ContentModelHelpers.GetString(frontmatter, "state"),
                ContentModelHelpers.GetString(frontmatter, "country") ?? string.Empty,
                ContentModelHelpers.GetBool(frontmatter, "featured"),
                eventsByGroup.TryGetValue(slug, out var count) ? count : 0,
                ContentModelHelpers.ToRelativePath(siteRoot, filePath),
                File.GetLastWriteTimeUtc(filePath)));
        }

        return items.OrderBy(item => item.Title, StringComparer.OrdinalIgnoreCase).ToList();
    }

    public async Task<MeetupGroupDetailDto?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        var filePath = await FindFileBySlugAsync(slug, cancellationToken);
        if (filePath is null)
        {
            return null;
        }

        return await MapDetailAsync(filePath, cancellationToken);
    }

    public async Task<MeetupGroupDetailDto> CreateAsync(MeetupGroupUpsertRequest request, CancellationToken cancellationToken = default)
    {
        Validate(request);

        var siteRoot = ResolveSiteRoot();
        var groupsRoot = GetMeetupGroupsRoot(siteRoot);
        Directory.CreateDirectory(groupsRoot);

        var slug = ContentModelHelpers.NormalizeSlug(request.Slug, request.Title);
        if (await FindFileBySlugAsync(slug, cancellationToken) is not null)
        {
            throw new InvalidOperationException($"A meetup group with slug '{slug}' already exists.");
        }

        var filePath = Path.Combine(groupsRoot, $"{slug}.md");
        var frontmatter = BuildFrontmatter(request, slug);
        var relativePath = ContentModelHelpers.ToRelativePath(siteRoot, filePath);
        var integrity = await ValidateIntegrityAsync(
            "meetupGroups",
            relativePath,
            slug,
            frontmatter,
            null,
            cancellationToken);
        ThrowWhenBlocked(integrity);

        await markdownService.WriteAsync(filePath, new MarkdownDocument(frontmatter, request.MarkdownBody ?? string.Empty), cancellationToken);
        return await MapDetailAsync(filePath, cancellationToken);
    }

    public async Task<MeetupGroupDetailDto?> UpdateAsync(string slug, MeetupGroupUpsertRequest request, CancellationToken cancellationToken = default)
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
            throw new InvalidOperationException($"A meetup group with slug '{nextSlug}' already exists.");
        }

        var mergedFrontmatter = BuildFrontmatter(request, nextSlug);

        var siteRoot = ResolveSiteRoot();
        var relativePath = ContentModelHelpers.ToRelativePath(siteRoot, targetFilePath);
        var slugMutation = string.Equals(sourceFrontmatterSlug, nextSlug, StringComparison.OrdinalIgnoreCase)
            ? null
            : new SlugMutationRequest("meetupGroups", sourceFrontmatterSlug, nextSlug, AutoCascadeReferences: true);
        var integrity = await ValidateIntegrityAsync(
            "meetupGroups",
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

    public async Task<ArchiveOperationResponse> ArchiveAsync(string slug, CancellationToken cancellationToken = default)
    {
        var filePath = await FindFileBySlugAsync(slug, cancellationToken);
        if (filePath is null)
        {
            return new ArchiveOperationResponse(ArchiveOperationType.Archive, false, "Meetup group not found.", slug);
        }

        var relativePath = ContentModelHelpers.ToRelativePath(ResolveSiteRoot(), filePath);
        return await archiveService.ArchiveAsync(relativePath, cancellationToken);
    }

    private async Task<MeetupGroupDetailDto> MapDetailAsync(string filePath, CancellationToken cancellationToken)
    {
        var siteRoot = ResolveSiteRoot();
        var document = await markdownService.ReadAsync(filePath, cancellationToken);
        var frontmatter = document.Frontmatter;
        var slug = ContentModelHelpers.GetString(frontmatter, "slug") ?? Path.GetFileNameWithoutExtension(filePath);

        return new MeetupGroupDetailDto(
            slug,
            ContentModelHelpers.GetString(frontmatter, "title") ?? slug,
            ContentModelHelpers.GetString(frontmatter, "description") ?? string.Empty,
            ContentModelHelpers.GetString(frontmatter, "city") ?? string.Empty,
            ContentModelHelpers.GetString(frontmatter, "state"),
            ContentModelHelpers.GetString(frontmatter, "country") ?? string.Empty,
            ContentModelHelpers.GetString(frontmatter, "website"),
            ContentModelHelpers.GetString(frontmatter, "role"),
            ContentModelHelpers.GetBool(frontmatter, "featured"),
            ContentModelHelpers.GetString(frontmatter, "heroImage") ?? DefaultHeroImage,
            document.Body,
            await GetRelatedEventsAsync(slug, cancellationToken),
            ContentModelHelpers.ToRelativePath(siteRoot, filePath),
            File.GetLastWriteTimeUtc(filePath));
    }

    private async Task<Dictionary<string, int>> GetEventCountsByGroupAsync(CancellationToken cancellationToken)
    {
        var result = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var eventsRoot = GetMeetupEventsRoot(ResolveSiteRoot());
        if (!Directory.Exists(eventsRoot))
        {
            return result;
        }

        foreach (var filePath in Directory.EnumerateFiles(eventsRoot, "*.md", SearchOption.AllDirectories))
        {
            var document = await markdownService.ReadAsync(filePath, cancellationToken);
            var groupSlug = ContentModelHelpers.GetString(document.Frontmatter, "meetupGroup");
            if (string.IsNullOrWhiteSpace(groupSlug))
            {
                continue;
            }

            result[groupSlug] = result.GetValueOrDefault(groupSlug) + 1;
        }

        return result;
    }

    private async Task<List<MeetupGroupEventSummaryDto>> GetRelatedEventsAsync(string groupSlug, CancellationToken cancellationToken)
    {
        var eventsRoot = GetMeetupEventsRoot(ResolveSiteRoot());
        if (!Directory.Exists(eventsRoot))
        {
            return [];
        }

        var items = new List<MeetupGroupEventSummaryDto>();
        foreach (var filePath in Directory.EnumerateFiles(eventsRoot, "*.md", SearchOption.AllDirectories))
        {
            var document = await markdownService.ReadAsync(filePath, cancellationToken);
            var frontmatter = document.Frontmatter;
            var meetupGroup = ContentModelHelpers.GetString(frontmatter, "meetupGroup");
            if (!string.Equals(meetupGroup, groupSlug, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var slug = ContentModelHelpers.GetString(frontmatter, "slug") ?? Path.GetFileNameWithoutExtension(filePath);
            var date = ParseDate(frontmatter, "date") ?? DateOnly.FromDateTime(DateTime.Today);
            var status = NormalizeStatus(ContentModelHelpers.GetString(frontmatter, "status"), date);

            items.Add(new MeetupGroupEventSummaryDto(
                slug,
                ContentModelHelpers.GetString(frontmatter, "title") ?? slug,
                date,
                status));
        }

        return items.OrderByDescending(item => item.Date).ThenBy(item => item.Title, StringComparer.OrdinalIgnoreCase).ToList();
    }

    private static Dictionary<string, object?> BuildFrontmatter(MeetupGroupUpsertRequest request, string slug)
    {
        var frontmatter = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
        {
            ["title"] = request.Title.Trim(),
            ["slug"] = slug,
            ["description"] = request.Description.Trim(),
            ["city"] = request.City.Trim(),
            ["country"] = request.Country.Trim(),
            ["featured"] = request.Featured,
            ["heroImage"] = string.IsNullOrWhiteSpace(request.HeroImage) ? DefaultHeroImage : request.HeroImage.Trim()
        };

        SetOptionalStringField(frontmatter, "state", request.State);
        SetOptionalStringField(frontmatter, "website", request.Website);
        SetOptionalStringField(frontmatter, "role", request.Role);

        return frontmatter;
    }

    private async Task<string?> FindFileBySlugAsync(string slug, CancellationToken cancellationToken)
    {
        var siteRoot = ResolveSiteRoot();
        var normalizedSlug = ContentModelHelpers.NormalizeSlug(slug, slug);
        var fileByName = Path.Combine(GetMeetupGroupsRoot(siteRoot), $"{normalizedSlug}.md");
        if (File.Exists(fileByName))
        {
            return fileByName;
        }

        if (!Directory.Exists(GetMeetupGroupsRoot(siteRoot)))
        {
            return null;
        }

        foreach (var filePath in Directory.EnumerateFiles(GetMeetupGroupsRoot(siteRoot), "*.md", SearchOption.AllDirectories))
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

    private static string NormalizeStatus(string? status, DateOnly date)
    {
        var normalized = (status ?? string.Empty).Trim().ToLowerInvariant();
        if (normalized is "upcoming" or "past")
        {
            return normalized;
        }

        return date >= DateOnly.FromDateTime(DateTime.Today) ? "upcoming" : "past";
    }

    private static void Validate(MeetupGroupUpsertRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            throw new InvalidOperationException("Title is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Description))
        {
            throw new InvalidOperationException("Description is required.");
        }

        if (string.IsNullOrWhiteSpace(request.City))
        {
            throw new InvalidOperationException("City is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Country))
        {
            throw new InvalidOperationException("Country is required.");
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

    private static string GetMeetupGroupsRoot(string siteRoot)
        => Path.Combine(siteRoot, "src", "content", "meetupGroups");

    private static string GetMeetupEventsRoot(string siteRoot)
        => Path.Combine(siteRoot, "src", "content", "meetupEvents");

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
