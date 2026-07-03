namespace ChadGreen.Management.Shared.Contracts;

public record MeetupSpeakerSocialDto(
    string? Website,
    string? Linkedin,
    string? Twitter,
    string? Github,
    string? Youtube,
    string? Sessionize,
    string? Bluesky);

public record MeetupSpeakerDto(
    string Name,
    string? Bio,
    string? Title,
    string? Company,
    string? Photo,
    MeetupSpeakerSocialDto? Social);

public record MeetupLocationDto(
    string? Venue,
    string? Address,
    string? City,
    string? State);

public record MeetupResourceDto(
    string Type,
    string Title,
    string Url,
    string? Description = null);

public record MeetupEventListItemDto(
    string Slug,
    string Title,
    string MeetupGroup,
    DateOnly Date,
    string Status,
    string RelativePath,
    DateTimeOffset? LastModifiedUtc = null);

public record MeetupEventDetailDto(
    string Slug,
    string Title,
    string Description,
    string? ShortDescription,
    string MeetupGroup,
    DateOnly Date,
    string? Time,
    string? EventUrl,
    MeetupSpeakerDto? Speaker,
    List<MeetupResourceDto> Resources,
    string? Thumbnail,
    string? HeroImage,
    MeetupLocationDto? Location,
    string Status,
    string MarkdownBody,
    string RelativePath,
    DateTimeOffset? LastModifiedUtc = null);

public record MeetupEventUpsertRequest(
    string Title,
    string? Slug,
    string Description,
    string? ShortDescription,
    string MeetupGroup,
    DateOnly Date,
    string? Time,
    string? EventUrl,
    MeetupSpeakerDto? Speaker,
    List<MeetupResourceDto> Resources,
    string? Thumbnail,
    string? HeroImage,
    MeetupLocationDto? Location,
    string? Status,
    string MarkdownBody,
    DateTimeOffset? ExpectedLastModifiedUtc = null);
