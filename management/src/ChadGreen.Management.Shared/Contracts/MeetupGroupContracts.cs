namespace ChadGreen.Management.Shared.Contracts;

public record MeetupGroupEventSummaryDto(
    string Slug,
    string Title,
    DateOnly Date,
    string Status);

public record MeetupGroupListItemDto(
    string Slug,
    string Title,
    string City,
    string? State,
    string Country,
    bool Featured,
    int EventCount,
    string RelativePath,
    DateTimeOffset? LastModifiedUtc = null);

public record MeetupGroupDetailDto(
    string Slug,
    string Title,
    string Description,
    string City,
    string? State,
    string Country,
    string? Website,
    string? Role,
    bool Featured,
    string HeroImage,
    string MarkdownBody,
    List<MeetupGroupEventSummaryDto> RelatedEvents,
    string RelativePath,
    DateTimeOffset? LastModifiedUtc = null);

public record MeetupGroupUpsertRequest(
    string Title,
    string? Slug,
    string Description,
    string City,
    string? State,
    string Country,
    string? Website,
    string? Role,
    bool Featured,
    string? HeroImage,
    string MarkdownBody,
    DateTimeOffset? ExpectedLastModifiedUtc = null);
