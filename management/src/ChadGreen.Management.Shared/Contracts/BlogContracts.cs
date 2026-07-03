namespace ChadGreen.Management.Shared.Contracts;

public record BlogListItemDto(
    string Slug,
    string Title,
    string Description,
    string Category,
    DateOnly PubDate,
    bool Draft,
    bool Featured,
    string RelativePath,
    DateTimeOffset? LastModifiedUtc = null);

public record BlogDetailDto(
    string Slug,
    string Title,
    string Description,
    string Author,
    DateOnly PubDate,
    DateOnly? UpdatedDate,
    string Category,
    List<string> Tags,
    List<string> RelatedPresentations,
    string HeroImage,
    string? HeroImageAlt,
    bool Draft,
    bool Featured,
    string? CanonicalUrl,
    string MarkdownBody,
    string RelativePath,
    DateTimeOffset? LastModifiedUtc = null);

public record BlogUpsertRequest(
    string Title,
    string? Slug,
    string Description,
    string Author,
    DateOnly PubDate,
    DateOnly? UpdatedDate,
    string Category,
    List<string> Tags,
    List<string> RelatedPresentations,
    string? HeroImage,
    string? HeroImageAlt,
    bool Draft,
    bool Featured,
    string? CanonicalUrl,
    string MarkdownBody,
    DateTimeOffset? ExpectedLastModifiedUtc = null);
