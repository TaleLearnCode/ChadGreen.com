namespace ChadGreen.Management.Shared.Contracts;

public record PresentationResourceDto(
    string Type,
    string Title,
    string Url,
    string? Description = null);

public record PresentationListItemDto(
    string Slug,
    string Title,
    string Description,
    string Status,
    bool Featured,
    string RelativePath,
    DateTimeOffset? LastModifiedUtc = null);

public record PresentationOptionDto(
    string Slug,
    string Title,
    string Status);

public record PresentationDetailDto(
    string Slug,
    string Title,
    string Description,
    string Type,
    List<int> Durations,
    string Level,
    List<string> LearningObjectives,
    List<string> Tags,
    List<string> RelatedPresentations,
    List<PresentationResourceDto> Resources,
    string? HeroImage,
    string Status,
    bool Featured,
    bool Validated,
    string MarkdownBody,
    string RelativePath,
    DateTimeOffset? LastModifiedUtc = null);

public record PresentationUpsertRequest(
    string Title,
    string? Slug,
    string Description,
    string Type,
    List<int> Durations,
    string Level,
    List<string> LearningObjectives,
    List<string> Tags,
    List<string> RelatedPresentations,
    List<PresentationResourceDto> Resources,
    string? HeroImage,
    string Status,
    bool Featured,
    bool Validated,
    string MarkdownBody,
    DateTimeOffset? ExpectedLastModifiedUtc = null);

public record PresentationStatusUpdateRequest(string Status);

public record PresentationFeaturedUpdateRequest(bool Featured);
