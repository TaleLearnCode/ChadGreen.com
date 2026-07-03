namespace ChadGreen.Management.Shared.Contracts;

public record AboutSocialDto(
    string? Twitter,
    string? Linkedin,
    string? Github,
    string? Youtube,
    string? Website,
    string? Sessionize);

public record AboutProfileDetailDto(
    string Slug,
    string Name,
    string? Tagline,
    string? Bio,
    string? ShortBio,
    string Avatar,
    string? Email,
    AboutSocialDto? Social,
    string MarkdownBody,
    string RelativePath,
    DateTimeOffset? LastModifiedUtc = null);

public record AboutProfileUpsertRequest(
    string Name,
    string? Tagline,
    string? Bio,
    string? ShortBio,
    string? Avatar,
    string? Email,
    AboutSocialDto? Social,
    string MarkdownBody,
    DateTimeOffset? ExpectedLastModifiedUtc = null);
