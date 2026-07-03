using ChadGreen.Management.Api.Options;
using ChadGreen.Management.Shared.Contracts;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace ChadGreen.Management.Api.Services;

public interface IMeetupEventManagementService
{
    Task<IReadOnlyList<MeetupEventListItemDto>> ListAsync(CancellationToken cancellationToken = default);

    Task<MeetupEventDetailDto?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);

    Task<MeetupEventDetailDto> CreateAsync(MeetupEventUpsertRequest request, CancellationToken cancellationToken = default);

    Task<MeetupEventDetailDto?> UpdateAsync(string slug, MeetupEventUpsertRequest request, CancellationToken cancellationToken = default);

    Task<ArchiveOperationResponse> ArchiveAsync(string slug, CancellationToken cancellationToken = default);
}

public sealed class MeetupEventManagementService(
    IOptions<ManagementOptions> options,
    IMarkdownFrontmatterFileService markdownService,
    IArchiveService archiveService,
    IIntegrityValidationService integrityValidationService) : IMeetupEventManagementService
{
    private static readonly HashSet<string> AllowedStatuses = ["upcoming", "past"];
    private static readonly HashSet<string> ResourceTypes = ["slides", "video", "github", "code", "demo", "blog", "download", "documentation", "other"];
    private readonly ManagementOptions _options = options.Value;

    public async Task<IReadOnlyList<MeetupEventListItemDto>> ListAsync(CancellationToken cancellationToken = default)
    {
        var siteRoot = ResolveSiteRoot();
        var root = GetMeetupEventsRoot(siteRoot);
        if (!Directory.Exists(root))
        {
            return [];
        }

        var items = new List<MeetupEventListItemDto>();
        foreach (var filePath in Directory.EnumerateFiles(root, "*.md", SearchOption.AllDirectories))
        {
            var document = await markdownService.ReadAsync(filePath, cancellationToken);
            var frontmatter = document.Frontmatter;
            var slug = ContentModelHelpers.GetString(frontmatter, "slug") ?? Path.GetFileNameWithoutExtension(filePath);
            var date = ParseDate(frontmatter, "date") ?? DateOnly.FromDateTime(DateTime.Today);

            items.Add(new MeetupEventListItemDto(
                slug,
                ContentModelHelpers.GetString(frontmatter, "title") ?? slug,
                ContentModelHelpers.GetString(frontmatter, "meetupGroup") ?? string.Empty,
                date,
                NormalizeStatus(ContentModelHelpers.GetString(frontmatter, "status"), date),
                ContentModelHelpers.ToRelativePath(siteRoot, filePath),
                File.GetLastWriteTimeUtc(filePath)));
        }

        return items.OrderByDescending(item => item.Date).ThenBy(item => item.Title, StringComparer.OrdinalIgnoreCase).ToList();
    }

    public async Task<MeetupEventDetailDto?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        var filePath = await FindFileBySlugAsync(slug, cancellationToken);
        if (filePath is null)
        {
            return null;
        }

        return await MapDetailAsync(filePath, cancellationToken);
    }

    public async Task<MeetupEventDetailDto> CreateAsync(MeetupEventUpsertRequest request, CancellationToken cancellationToken = default)
    {
        request = NormalizeRequest(request);
        Validate(request);

        var siteRoot = ResolveSiteRoot();
        var root = GetMeetupEventsRoot(siteRoot);
        Directory.CreateDirectory(root);

        var slug = ContentModelHelpers.NormalizeSlug(request.Slug, request.Title);
        if (await FindFileBySlugAsync(slug, cancellationToken) is not null)
        {
            throw new InvalidOperationException($"A meetup event with slug '{slug}' already exists.");
        }

        var filePath = Path.Combine(root, $"{slug}.md");
        var frontmatter = BuildFrontmatter(request, slug);
        var relativePath = ContentModelHelpers.ToRelativePath(siteRoot, filePath);
        var integrity = await ValidateIntegrityAsync(
            "meetupEvents",
            relativePath,
            slug,
            frontmatter,
            null,
            cancellationToken);
        ThrowWhenBlocked(integrity);

        await markdownService.WriteAsync(filePath, new MarkdownDocument(frontmatter, request.MarkdownBody ?? string.Empty), cancellationToken);
        return await MapDetailAsync(filePath, cancellationToken);
    }

    public async Task<MeetupEventDetailDto?> UpdateAsync(string slug, MeetupEventUpsertRequest request, CancellationToken cancellationToken = default)
    {
        request = NormalizeRequest(request);
        Validate(request);

        var sourcePath = await FindFileBySlugAsync(slug, cancellationToken);
        if (sourcePath is null)
        {
            return null;
        }

        var sourceDocument = await markdownService.ReadAsync(sourcePath, cancellationToken);
        var sourceFrontmatterSlug = ContentModelHelpers.GetString(sourceDocument.Frontmatter, "slug") ?? Path.GetFileNameWithoutExtension(sourcePath);
        var nextSlug = ContentModelHelpers.NormalizeSlug(request.Slug, request.Title);
        var targetPath = Path.Combine(Path.GetDirectoryName(sourcePath)!, $"{nextSlug}.md");

        if (!string.Equals(sourcePath, targetPath, StringComparison.OrdinalIgnoreCase) && File.Exists(targetPath))
        {
            throw new InvalidOperationException($"A meetup event with slug '{nextSlug}' already exists.");
        }

        var mergedFrontmatter = BuildFrontmatter(request, nextSlug);

        var siteRoot = ResolveSiteRoot();
        var relativePath = ContentModelHelpers.ToRelativePath(siteRoot, targetPath);
        var slugMutation = string.Equals(sourceFrontmatterSlug, nextSlug, StringComparison.OrdinalIgnoreCase)
            ? null
            : new SlugMutationRequest("meetupEvents", sourceFrontmatterSlug, nextSlug, AutoCascadeReferences: true);
        var integrity = await ValidateIntegrityAsync(
            "meetupEvents",
            relativePath,
            nextSlug,
            mergedFrontmatter,
            slugMutation,
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

    public async Task<ArchiveOperationResponse> ArchiveAsync(string slug, CancellationToken cancellationToken = default)
    {
        var filePath = await FindFileBySlugAsync(slug, cancellationToken);
        if (filePath is null)
        {
            return new ArchiveOperationResponse(ArchiveOperationType.Archive, false, "Meetup event not found.", slug);
        }

        var relativePath = ContentModelHelpers.ToRelativePath(ResolveSiteRoot(), filePath);
        return await archiveService.ArchiveAsync(relativePath, cancellationToken);
    }

    private async Task<MeetupEventDetailDto> MapDetailAsync(string filePath, CancellationToken cancellationToken)
    {
        var siteRoot = ResolveSiteRoot();
        var document = await markdownService.ReadAsync(filePath, cancellationToken);
        var frontmatter = document.Frontmatter;
        var slug = ContentModelHelpers.GetString(frontmatter, "slug") ?? Path.GetFileNameWithoutExtension(filePath);
        var date = ParseDate(frontmatter, "date") ?? DateOnly.FromDateTime(DateTime.Today);

        return new MeetupEventDetailDto(
            slug,
            ContentModelHelpers.GetString(frontmatter, "title") ?? slug,
            ContentModelHelpers.GetString(frontmatter, "description") ?? string.Empty,
            ContentModelHelpers.GetString(frontmatter, "shortDescription"),
            ContentModelHelpers.GetString(frontmatter, "meetupGroup") ?? string.Empty,
            date,
            ContentModelHelpers.GetString(frontmatter, "time"),
            ContentModelHelpers.GetString(frontmatter, "eventUrl"),
            ParseSpeaker(frontmatter),
            ParseResources(frontmatter),
            ContentModelHelpers.GetString(frontmatter, "thumbnail"),
            ContentModelHelpers.GetString(frontmatter, "heroImage"),
            ParseLocation(frontmatter),
            NormalizeStatus(ContentModelHelpers.GetString(frontmatter, "status"), date),
            document.Body,
            ContentModelHelpers.ToRelativePath(siteRoot, filePath),
            File.GetLastWriteTimeUtc(filePath));
    }

    private static MeetupSpeakerDto? ParseSpeaker(Dictionary<string, object?> frontmatter)
    {
        if (!frontmatter.TryGetValue("speaker", out var value) || value is null)
        {
            return null;
        }

        var speaker = ContentModelHelpers.AsDictionary(value);
        var name = speaker.TryGetValue("name", out var nameValue) ? nameValue?.ToString() : null;
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        MeetupSpeakerSocialDto? social = null;
        if (speaker.TryGetValue("social", out var socialValue) && socialValue is not null)
        {
            var socialMap = ContentModelHelpers.AsDictionary(socialValue);
            social = new MeetupSpeakerSocialDto(
                socialMap.TryGetValue("website", out var website) ? website?.ToString() : null,
                socialMap.TryGetValue("linkedin", out var linkedin) ? linkedin?.ToString() : null,
                socialMap.TryGetValue("twitter", out var twitter) ? twitter?.ToString() : null,
                socialMap.TryGetValue("github", out var github) ? github?.ToString() : null,
                socialMap.TryGetValue("youtube", out var youtube) ? youtube?.ToString() : null,
                socialMap.TryGetValue("sessionize", out var sessionize) ? sessionize?.ToString() : null,
                socialMap.TryGetValue("bluesky", out var bluesky) ? bluesky?.ToString() : null);
        }

        return new MeetupSpeakerDto(
            name.Trim(),
            speaker.TryGetValue("bio", out var bio) ? bio?.ToString() : null,
            speaker.TryGetValue("title", out var title) ? title?.ToString() : null,
            speaker.TryGetValue("company", out var company) ? company?.ToString() : null,
            speaker.TryGetValue("photo", out var photo) ? photo?.ToString() : null,
            social);
    }

    private static MeetupLocationDto? ParseLocation(Dictionary<string, object?> frontmatter)
    {
        if (!frontmatter.TryGetValue("location", out var locationValue) || locationValue is null)
        {
            return null;
        }

        var location = ContentModelHelpers.AsDictionary(locationValue);
        return new MeetupLocationDto(
            location.TryGetValue("venue", out var venue) ? venue?.ToString() : null,
            location.TryGetValue("address", out var address) ? address?.ToString() : null,
            location.TryGetValue("city", out var city) ? city?.ToString() : null,
            location.TryGetValue("state", out var state) ? state?.ToString() : null);
    }

    private static List<MeetupResourceDto> ParseResources(Dictionary<string, object?> frontmatter)
    {
        var resources = new List<MeetupResourceDto>();
        if (!frontmatter.TryGetValue("resources", out var value) || value is null)
        {
            return resources;
        }

        foreach (var item in ContentModelHelpers.AsList(value))
        {
            var dictionary = ContentModelHelpers.AsDictionary(item);
            var title = dictionary.TryGetValue("title", out var titleValue) ? titleValue?.ToString() ?? string.Empty : string.Empty;
            var url = dictionary.TryGetValue("url", out var urlValue) ? urlValue?.ToString() ?? string.Empty : string.Empty;
            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(url))
            {
                continue;
            }

            var type = dictionary.TryGetValue("type", out var typeValue) ? typeValue?.ToString() : null;
            resources.Add(new MeetupResourceDto(
                NormalizeResourceType(type),
                title.Trim(),
                url.Trim(),
                dictionary.TryGetValue("description", out var description) ? description?.ToString() : null));
        }

        return resources;
    }

    private static Dictionary<string, object?> BuildFrontmatter(MeetupEventUpsertRequest request, string slug)
    {
        var frontmatter = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
        {
            ["title"] = request.Title.Trim(),
            ["slug"] = slug,
            ["description"] = request.Description.Trim(),
            ["meetupGroup"] = request.MeetupGroup.Trim(),
            ["date"] = request.Date.ToString("yyyy-MM-dd"),
            ["status"] = NormalizeStatus(request.Status, request.Date)
        };

        SetOptionalStringField(frontmatter, "shortDescription", request.ShortDescription);
        SetOptionalStringField(frontmatter, "time", request.Time);
        SetOptionalStringField(frontmatter, "eventUrl", request.EventUrl);
        SetOptionalStringField(frontmatter, "thumbnail", request.Thumbnail);
        SetOptionalStringField(frontmatter, "heroImage", request.HeroImage);

        if (request.Speaker is not null && !string.IsNullOrWhiteSpace(request.Speaker.Name))
        {
            frontmatter["speaker"] = BuildSpeakerFrontmatter(request.Speaker);
        }

        if (request.Location is not null)
        {
            var location = BuildLocationFrontmatter(request.Location);
            if (location.Count > 0)
            {
                frontmatter["location"] = location;
            }
        }

        var resources = request.Resources ?? [];
        if (resources.Count > 0)
        {
            frontmatter["resources"] = resources
                .Where(resource => !string.IsNullOrWhiteSpace(resource.Title) && !string.IsNullOrWhiteSpace(resource.Url))
                .Select(resource => new Dictionary<string, object?>
                {
                    ["type"] = NormalizeResourceType(resource.Type),
                    ["title"] = resource.Title.Trim(),
                    ["url"] = resource.Url.Trim(),
                    ["description"] = string.IsNullOrWhiteSpace(resource.Description) ? null : resource.Description.Trim()
                })
                .ToList();
        }

        return frontmatter;
    }

    private static Dictionary<string, object?> BuildSpeakerFrontmatter(MeetupSpeakerDto speaker)
    {
        var payload = new Dictionary<string, object?>
        {
            ["name"] = speaker.Name.Trim()
        };

        SetOptionalStringField(payload, "bio", speaker.Bio);
        SetOptionalStringField(payload, "title", speaker.Title);
        SetOptionalStringField(payload, "company", speaker.Company);
        SetOptionalStringField(payload, "photo", speaker.Photo);

        if (speaker.Social is not null)
        {
            var socialPayload = new Dictionary<string, object?>();
            SetOptionalStringField(socialPayload, "website", speaker.Social.Website);
            SetOptionalStringField(socialPayload, "linkedin", speaker.Social.Linkedin);
            SetOptionalStringField(socialPayload, "twitter", speaker.Social.Twitter);
            SetOptionalStringField(socialPayload, "github", speaker.Social.Github);
            SetOptionalStringField(socialPayload, "youtube", speaker.Social.Youtube);
            SetOptionalStringField(socialPayload, "sessionize", speaker.Social.Sessionize);
            SetOptionalStringField(socialPayload, "bluesky", speaker.Social.Bluesky);

            if (socialPayload.Count > 0)
            {
                payload["social"] = socialPayload;
            }
        }

        return payload;
    }

    private static Dictionary<string, object?> BuildLocationFrontmatter(MeetupLocationDto location)
    {
        var payload = new Dictionary<string, object?>();
        SetOptionalStringField(payload, "venue", location.Venue);
        SetOptionalStringField(payload, "address", location.Address);
        SetOptionalStringField(payload, "city", location.City);
        SetOptionalStringField(payload, "state", location.State);
        return payload;
    }

    private async Task<string?> FindFileBySlugAsync(string slug, CancellationToken cancellationToken)
    {
        var siteRoot = ResolveSiteRoot();
        var normalizedSlug = ContentModelHelpers.NormalizeSlug(slug, slug);
        var fileByName = Path.Combine(GetMeetupEventsRoot(siteRoot), $"{normalizedSlug}.md");
        if (File.Exists(fileByName))
        {
            return fileByName;
        }

        if (!Directory.Exists(GetMeetupEventsRoot(siteRoot)))
        {
            return null;
        }

        foreach (var filePath in Directory.EnumerateFiles(GetMeetupEventsRoot(siteRoot), "*.md", SearchOption.AllDirectories))
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
        if (AllowedStatuses.Contains(normalized))
        {
            return normalized;
        }

        return date >= DateOnly.FromDateTime(DateTime.Today) ? "upcoming" : "past";
    }

    private static string NormalizeResourceType(string? type)
    {
        var normalized = (type ?? "other").Trim().ToLowerInvariant();
        return ResourceTypes.Contains(normalized) ? normalized : "other";
    }

    private static MeetupEventUpsertRequest NormalizeRequest(MeetupEventUpsertRequest request)
    {
        var normalizedResources = (request.Resources ?? [])
            .Where(resource => !string.IsNullOrWhiteSpace(resource.Title) && !string.IsNullOrWhiteSpace(resource.Url))
            .Select(resource => resource with
            {
                Type = NormalizeResourceType(resource.Type),
                Title = resource.Title.Trim(),
                Url = resource.Url.Trim(),
                Description = string.IsNullOrWhiteSpace(resource.Description) ? null : resource.Description.Trim()
            })
            .ToList();

        return request with
        {
            Resources = normalizedResources
        };
    }

    private static void Validate(MeetupEventUpsertRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            throw new InvalidOperationException("Title is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Description))
        {
            throw new InvalidOperationException("Description is required.");
        }

        if (string.IsNullOrWhiteSpace(request.MeetupGroup))
        {
            throw new InvalidOperationException("Meetup group is required.");
        }

        if (request.Speaker is not null && string.IsNullOrWhiteSpace(request.Speaker.Name))
        {
            throw new InvalidOperationException("Speaker name is required when speaker details are provided.");
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
