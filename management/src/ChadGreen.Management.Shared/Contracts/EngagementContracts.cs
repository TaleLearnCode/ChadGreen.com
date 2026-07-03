namespace ChadGreen.Management.Shared.Contracts;

public record EngagementLocationDto(
    string? Venue,
    string City,
    string? State,
    string Country);

public record EngagementPresentationAssignmentDto(
    string Id,
    int DisplayOrder,
    string? SessionName = null,
    string? Date = null,
    string? Time = null,
    string? TimeZone = null,
    string? Room = null,
    string? SessionUrl = null);

public record EngagementListItemDto(
    string Slug,
    string Title,
    string EventType,
    DateOnly StartDate,
    DateOnly? EndDate,
    bool Featured,
    int PresentationCount,
    string RelativePath,
    DateTimeOffset? LastModifiedUtc = null);

public record EngagementDetailDto(
    string Slug,
    string Title,
    string EventType,
    string? Description,
    DateOnly StartDate,
    DateOnly? EndDate,
    EngagementLocationDto Location,
    string? Website,
    bool Featured,
    bool Validated,
    string? HeroImage,
    List<EngagementPresentationAssignmentDto> Presentations,
    string MarkdownBody,
    string RelativePath,
    DateTimeOffset? LastModifiedUtc = null);

public record EngagementUpsertRequest(
    string Title,
    string? Slug,
    string EventType,
    string? Description,
    DateOnly StartDate,
    DateOnly? EndDate,
    EngagementLocationDto Location,
    string? Website,
    bool Featured,
    bool Validated,
    string? HeroImage,
    List<EngagementPresentationAssignmentDto> Presentations,
    string MarkdownBody,
    DateTimeOffset? ExpectedLastModifiedUtc = null);

public record EngagementFeaturedUpdateRequest(bool Featured);
