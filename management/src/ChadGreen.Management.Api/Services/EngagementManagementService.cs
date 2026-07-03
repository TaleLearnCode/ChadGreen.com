using ChadGreen.Management.Api.Options;
using ChadGreen.Management.Shared.Contracts;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace ChadGreen.Management.Api.Services;

public interface IEngagementManagementService
{
    Task<IReadOnlyList<EngagementListItemDto>> ListAsync(CancellationToken cancellationToken = default);

    Task<EngagementDetailDto?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);

    Task<EngagementDetailDto> CreateAsync(EngagementUpsertRequest request, CancellationToken cancellationToken = default);

    Task<EngagementDetailDto?> UpdateAsync(string slug, EngagementUpsertRequest request, CancellationToken cancellationToken = default);

    Task<EngagementDetailDto?> SetFeaturedAsync(string slug, bool featured, CancellationToken cancellationToken = default);

    Task<ArchiveOperationResponse> ArchiveAsync(string slug, CancellationToken cancellationToken = default);
}

public sealed class EngagementManagementService(
    IOptions<ManagementOptions> options,
    IMarkdownFrontmatterFileService markdownService,
    IArchiveService archiveService,
    IIntegrityValidationService integrityValidationService) : IEngagementManagementService
{
    private static readonly HashSet<string> AllowedEventTypes = ["conference", "meetup", "webinar", "podcast", "workshop", "user-group", "corporate", "other"];
    private readonly ManagementOptions _options = options.Value;

    public async Task<IReadOnlyList<EngagementListItemDto>> ListAsync(CancellationToken cancellationToken = default)
    {
        var siteRoot = ResolveSiteRoot();
        var root = GetEventsRoot(siteRoot);
        if (!Directory.Exists(root))
        {
            return [];
        }

        var items = new List<EngagementListItemDto>();
        foreach (var filePath in Directory.EnumerateFiles(root, "*.md", SearchOption.AllDirectories))
        {
            var document = await markdownService.ReadAsync(filePath, cancellationToken);
            var frontmatter = document.Frontmatter;
            var slug = ContentModelHelpers.GetString(frontmatter, "slug") ?? Path.GetFileNameWithoutExtension(filePath);
            var startDate = ParseDate(frontmatter, "startDate") ?? DateOnly.FromDateTime(DateTime.UtcNow);
            var endDate = ParseDate(frontmatter, "endDate");
            var presentations = ParseAssignments(frontmatter);
            var title = NormalizeTitle(frontmatter, slug);

            items.Add(new EngagementListItemDto(
                slug,
                title,
                NormalizeEventType(ContentModelHelpers.GetString(frontmatter, "eventType")),
                startDate,
                endDate,
                ContentModelHelpers.GetBool(frontmatter, "featured"),
                presentations.Count,
                ContentModelHelpers.ToRelativePath(siteRoot, filePath),
                File.GetLastWriteTimeUtc(filePath)));
        }

        return items.OrderByDescending(item => item.StartDate).ThenBy(item => item.Title, StringComparer.OrdinalIgnoreCase).ToList();
    }

    public async Task<EngagementDetailDto?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        var filePath = await FindFileBySlugAsync(slug, cancellationToken);
        return filePath is null ? null : await MapDetailAsync(filePath, cancellationToken);
    }

    public async Task<EngagementDetailDto> CreateAsync(EngagementUpsertRequest request, CancellationToken cancellationToken = default)
    {
        request = NormalizeRequest(request);
        Validate(request);

        var siteRoot = ResolveSiteRoot();
        var root = GetEventsRoot(siteRoot);
        Directory.CreateDirectory(root);

        var slug = ContentModelHelpers.NormalizeSlug(request.Slug, request.Title);
        if (await FindFileBySlugAsync(slug, cancellationToken) is not null)
        {
            throw new InvalidOperationException($"An engagement with slug '{slug}' already exists.");
        }

        var path = Path.Combine(root, $"{slug}.md");
        var frontmatter = BuildFrontmatter(request, slug);
        CoerceTitleToScalarString(frontmatter, fallback: request.Title);
        var relativePath = ContentModelHelpers.ToRelativePath(siteRoot, path);
        var integrity = await ValidateIntegrityAsync(
            "events",
            relativePath,
            slug,
            frontmatter,
            null,
            cancellationToken);
        ThrowWhenBlocked(integrity);

        await markdownService.WriteAsync(path, new MarkdownDocument(frontmatter, request.MarkdownBody ?? string.Empty), cancellationToken);
        return await MapDetailAsync(path, cancellationToken);
    }

    public async Task<EngagementDetailDto?> UpdateAsync(string slug, EngagementUpsertRequest request, CancellationToken cancellationToken = default)
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
            throw new InvalidOperationException($"An engagement with slug '{nextSlug}' already exists.");
        }

        var mergedFrontmatter = sourceDocument.Frontmatter;
        foreach (var (key, value) in BuildFrontmatter(request, nextSlug))
        {
            mergedFrontmatter[key] = value;
        }

        SynchronizeOptionalStringField(mergedFrontmatter, "description", request.Description);
        SynchronizeOptionalStringField(mergedFrontmatter, "website", request.Website);
        SynchronizeOptionalStringField(mergedFrontmatter, "heroImage", request.HeroImage);

        if (request.EndDate is null)
        {
            mergedFrontmatter.Remove("endDate");
        }
        else
        {
            mergedFrontmatter["endDate"] = request.EndDate.Value.ToString("yyyy-MM-dd");
        }

        CoerceTitleToScalarString(mergedFrontmatter, fallback: request.Title);

        var siteRoot = ResolveSiteRoot();
        var relativePath = ContentModelHelpers.ToRelativePath(siteRoot, targetPath);
        var slugMutation = string.Equals(sourceFrontmatterSlug, nextSlug, StringComparison.OrdinalIgnoreCase)
            ? null
            : new SlugMutationRequest("events", sourceFrontmatterSlug, nextSlug, AutoCascadeReferences: true);
        var integrity = await ValidateIntegrityAsync(
            "events",
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

    public async Task<EngagementDetailDto?> SetFeaturedAsync(string slug, bool featured, CancellationToken cancellationToken = default)
    {
        var detail = await GetBySlugAsync(slug, cancellationToken);
        if (detail is null)
        {
            return null;
        }

        var request = MapUpsertRequest(detail) with { Featured = featured };
        return await UpdateAsync(slug, request, cancellationToken);
    }

    public async Task<ArchiveOperationResponse> ArchiveAsync(string slug, CancellationToken cancellationToken = default)
    {
        var filePath = await FindFileBySlugAsync(slug, cancellationToken);
        if (filePath is null)
        {
            return new ArchiveOperationResponse(ArchiveOperationType.Archive, false, "Engagement not found.", slug);
        }

        var relativePath = ContentModelHelpers.ToRelativePath(ResolveSiteRoot(), filePath);
        return await archiveService.ArchiveAsync(relativePath, cancellationToken);
    }

    private static EngagementUpsertRequest MapUpsertRequest(EngagementDetailDto detail)
        => new(
            detail.Title,
            detail.Slug,
            detail.EventType,
            detail.Description,
            detail.StartDate,
            detail.EndDate,
            detail.Location,
            detail.Website,
            detail.Featured,
            detail.Validated,
            detail.HeroImage,
            detail.Presentations,
            detail.MarkdownBody,
            detail.LastModifiedUtc);

    private async Task<EngagementDetailDto> MapDetailAsync(string filePath, CancellationToken cancellationToken)
    {
        var siteRoot = ResolveSiteRoot();
        var document = await markdownService.ReadAsync(filePath, cancellationToken);
        var frontmatter = document.Frontmatter;
        var slug = ContentModelHelpers.GetString(frontmatter, "slug") ?? Path.GetFileNameWithoutExtension(filePath);
        var startDate = ParseDate(frontmatter, "startDate") ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var title = NormalizeTitle(frontmatter, slug);

        return new EngagementDetailDto(
            slug,
            title,
            NormalizeEventType(ContentModelHelpers.GetString(frontmatter, "eventType")),
            ContentModelHelpers.GetString(frontmatter, "description"),
            startDate,
            ParseDate(frontmatter, "endDate"),
            ParseLocation(frontmatter),
            ContentModelHelpers.GetString(frontmatter, "website"),
            ContentModelHelpers.GetBool(frontmatter, "featured"),
            ContentModelHelpers.GetBool(frontmatter, "validated"),
            ContentModelHelpers.GetString(frontmatter, "heroImage"),
            ParseAssignments(frontmatter),
            document.Body,
            ContentModelHelpers.ToRelativePath(siteRoot, filePath),
            File.GetLastWriteTimeUtc(filePath));
    }

    private static EngagementLocationDto ParseLocation(Dictionary<string, object?> frontmatter)
    {
        var location = frontmatter.TryGetValue("location", out var locationValue)
            ? ContentModelHelpers.AsDictionary(locationValue)
            : new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

        return new EngagementLocationDto(
            location.TryGetValue("venue", out var venue) ? venue?.ToString() : null,
            location.TryGetValue("city", out var city) ? city?.ToString() ?? string.Empty : string.Empty,
            location.TryGetValue("state", out var state) ? state?.ToString() : null,
            location.TryGetValue("country", out var country) ? country?.ToString() ?? string.Empty : string.Empty);
    }

    private static List<EngagementPresentationAssignmentDto> ParseAssignments(Dictionary<string, object?> frontmatter)
    {
        if (!frontmatter.TryGetValue("presentations", out var value) || value is null)
        {
            return [];
        }

        var result = new List<EngagementPresentationAssignmentDto>();
        var sequence = ContentModelHelpers.AsList(value);

        for (var index = 0; index < sequence.Count; index++)
        {
            var item = sequence[index];
            if (item is string stringId)
            {
                if (!string.IsNullOrWhiteSpace(stringId))
                {
                    result.Add(new EngagementPresentationAssignmentDto(stringId.Trim(), index));
                }

                continue;
            }

            var map = ContentModelHelpers.AsDictionary(item);
            var id = map.TryGetValue("id", out var idValue) ? idValue?.ToString() : null;
            if (string.IsNullOrWhiteSpace(id))
            {
                continue;
            }

            result.Add(new EngagementPresentationAssignmentDto(
                id.Trim(),
                index,
                map.TryGetValue("sessionName", out var sessionName) ? sessionName?.ToString() : null,
                map.TryGetValue("date", out var date) ? date?.ToString() : null,
                map.TryGetValue("time", out var time) ? time?.ToString() : null,
                map.TryGetValue("timeZone", out var timeZone) ? timeZone?.ToString() : null,
                map.TryGetValue("room", out var room) ? room?.ToString() : null,
                map.TryGetValue("sessionUrl", out var sessionUrl) ? sessionUrl?.ToString() : null));
        }

        return result.OrderBy(assignment => assignment.DisplayOrder).ToList();
    }

    private static Dictionary<string, object?> BuildFrontmatter(EngagementUpsertRequest request, string slug)
    {
        var orderedAssignments = request.Presentations
            .OrderBy(assignment => assignment.DisplayOrder)
            .ToList();

        var frontmatter = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
        {
            ["title"] = request.Title.Trim(),
            ["slug"] = slug,
            ["eventType"] = NormalizeEventType(request.EventType),
            ["startDate"] = request.StartDate.ToString("yyyy-MM-dd"),
            ["location"] = BuildLocationFrontmatter(request.Location),
            ["presentations"] = BuildPresentationFrontmatter(orderedAssignments),
            ["featured"] = request.Featured,
            ["validated"] = request.Validated
        };

        if (request.EndDate is not null)
        {
            frontmatter["endDate"] = request.EndDate.Value.ToString("yyyy-MM-dd");
        }

        SetOptionalStringField(frontmatter, "description", request.Description);
        SetOptionalStringField(frontmatter, "website", request.Website);
        SetOptionalStringField(frontmatter, "heroImage", request.HeroImage);

        return frontmatter;
    }

    private static string NormalizeTitle(Dictionary<string, object?> frontmatter, string fallback)
    {
        return frontmatter.TryGetValue("title", out var value)
            ? ContentModelHelpers.CoerceScalarString(value, fallback)
            : fallback;
    }

    private static void CoerceTitleToScalarString(Dictionary<string, object?> frontmatter, string fallback)
    {
        frontmatter["title"] = ContentModelHelpers.CoerceScalarString(
            frontmatter.TryGetValue("title", out var value) ? value : null,
            fallback);
    }

    private async Task<string?> FindFileBySlugAsync(string slug, CancellationToken cancellationToken)
    {
        var siteRoot = ResolveSiteRoot();
        var normalizedSlug = ContentModelHelpers.NormalizeSlug(slug, slug);
        var fileByName = Path.Combine(GetEventsRoot(siteRoot), $"{normalizedSlug}.md");
        if (File.Exists(fileByName))
        {
            return fileByName;
        }

        var root = GetEventsRoot(siteRoot);
        if (!Directory.Exists(root))
        {
            return null;
        }

        foreach (var filePath in Directory.EnumerateFiles(root, "*.md", SearchOption.AllDirectories))
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

    private static string NormalizeEventType(string? eventType)
    {
        var normalized = (eventType ?? "conference").Trim().ToLowerInvariant();
        return AllowedEventTypes.Contains(normalized) ? normalized : "other";
    }

    private static void Validate(EngagementUpsertRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            throw new InvalidOperationException("Title is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Location.City))
        {
            throw new InvalidOperationException("Location city is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Location.Country))
        {
            throw new InvalidOperationException("Location country is required.");
        }

        ValidatePresentationAssignments(request.Presentations);
    }

    private static void ValidatePresentationAssignments(IReadOnlyList<EngagementPresentationAssignmentDto> assignments)
    {
        if (assignments.Count == 0)
        {
            return;
        }

        var seenDisplayOrders = new HashSet<int>();

        foreach (var assignment in assignments)
        {
            if (string.IsNullOrWhiteSpace(assignment.Id))
            {
                throw new InvalidOperationException("Each presentation assignment must reference a presentation id.");
            }

            if (!seenDisplayOrders.Add(assignment.DisplayOrder))
            {
                throw new InvalidOperationException("Presentation display order must contain unique positions.");
            }
        }

        var expectedDisplayOrders = Enumerable.Range(0, assignments.Count).ToHashSet();
        if (!expectedDisplayOrders.SetEquals(seenDisplayOrders))
        {
            throw new InvalidOperationException("Presentation display order must be contiguous starting at 0.");
        }
    }

    private string ResolveSiteRoot()
        => SiteRootResolver.Resolve(_options.SiteRoot, AppContext.BaseDirectory);

    private static string GetEventsRoot(string siteRoot)
        => Path.Combine(siteRoot, "src", "content", "events");

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

    private static EngagementUpsertRequest NormalizeRequest(EngagementUpsertRequest request)
    {
        var normalizedAssignments = NormalizePresentationAssignments(request.Presentations);

        return request with
        {
            Presentations = normalizedAssignments
        };
    }

    private static Dictionary<string, object?> BuildLocationFrontmatter(EngagementLocationDto location)
    {
        var payload = new Dictionary<string, object?>
        {
            ["city"] = location.City.Trim(),
            ["country"] = location.Country.Trim()
        };

        SetOptionalStringField(payload, "venue", location.Venue);
        SetOptionalStringField(payload, "state", location.State);

        return payload;
    }

    private static List<object> BuildPresentationFrontmatter(IEnumerable<EngagementPresentationAssignmentDto> assignments)
    {
        var entries = new List<object>();

        foreach (var assignment in assignments)
        {
            var id = assignment.Id.Trim();
            var hasSessionMetadata =
                !string.IsNullOrWhiteSpace(assignment.SessionName) ||
                !string.IsNullOrWhiteSpace(assignment.Date) ||
                !string.IsNullOrWhiteSpace(assignment.Time) ||
                !string.IsNullOrWhiteSpace(assignment.TimeZone) ||
                !string.IsNullOrWhiteSpace(assignment.Room) ||
                !string.IsNullOrWhiteSpace(assignment.SessionUrl);

            if (!hasSessionMetadata)
            {
                entries.Add(id);
                continue;
            }

            var payload = new Dictionary<string, object?>
            {
                ["id"] = id
            };

            SetOptionalStringField(payload, "sessionName", assignment.SessionName);
            SetOptionalStringField(payload, "date", assignment.Date);
            SetOptionalStringField(payload, "time", assignment.Time);
            SetOptionalStringField(payload, "timeZone", assignment.TimeZone);
            SetOptionalStringField(payload, "room", assignment.Room);
            SetOptionalStringField(payload, "sessionUrl", assignment.SessionUrl);

            entries.Add(payload);
        }

        return entries;
    }

    private static void SetOptionalStringField(Dictionary<string, object?> target, string key, string? value)
    {
        var normalized = NormalizeOptionalString(value);
        if (normalized is not null)
        {
            target[key] = normalized;
        }
    }

    private static void SynchronizeOptionalStringField(Dictionary<string, object?> target, string key, string? value)
    {
        var normalized = NormalizeOptionalString(value);
        if (normalized is null)
        {
            target.Remove(key);
            return;
        }

        target[key] = normalized;
    }

    private static string? NormalizeOptionalString(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static List<EngagementPresentationAssignmentDto> NormalizePresentationAssignments(IReadOnlyList<EngagementPresentationAssignmentDto> assignments)
    {
        if (assignments.Count == 0)
        {
            return [];
        }

        var normalizedCandidates = assignments
            .Select((assignment, index) => new
            {
                Index = index,
                Id = assignment.Id.Trim(),
                DisplayOrder = assignment.DisplayOrder,
                SessionName = NormalizeOptionalString(assignment.SessionName),
                Date = NormalizeOptionalString(assignment.Date),
                Time = NormalizeOptionalString(assignment.Time),
                TimeZone = NormalizeOptionalString(assignment.TimeZone),
                Room = NormalizeOptionalString(assignment.Room),
                SessionUrl = NormalizeOptionalString(assignment.SessionUrl)
            })
            .ToList();

        if (normalizedCandidates.Any(candidate => string.IsNullOrWhiteSpace(candidate.Id)))
        {
            throw new InvalidOperationException("Each presentation assignment must reference a presentation id.");
        }

        var hasValidDisplayOrder =
            normalizedCandidates.All(candidate => candidate.DisplayOrder >= 0) &&
            normalizedCandidates.Select(candidate => candidate.DisplayOrder).Distinct().Count() == normalizedCandidates.Count;

        var ordered = hasValidDisplayOrder
            ? normalizedCandidates.OrderBy(candidate => candidate.DisplayOrder).ThenBy(candidate => candidate.Index)
            : normalizedCandidates.OrderBy(candidate => candidate.Index);

        return ordered
            .Select((candidate, index) => new EngagementPresentationAssignmentDto(
                candidate.Id,
                index,
                candidate.SessionName,
                candidate.Date,
                candidate.Time,
                candidate.TimeZone,
                candidate.Room,
                candidate.SessionUrl))
            .ToList();
    }
}
