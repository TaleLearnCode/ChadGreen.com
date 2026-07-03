namespace ChadGreen.Management.Shared.Contracts;

public record MediaItemDto(
    string RelativePath,
    string PublicUrl,
    string FileName,
    string Directory,
    long SizeBytes,
    DateTimeOffset LastModifiedUtc);

public record MediaListResponse(
    List<MediaItemDto> Items);

public record MediaUploadResponse(
    bool Success,
    string Message,
    MediaItemDto? Item = null);

public record MediaArchiveRequest(
    string RelativePath);

public record MediaReplaceRequest(
    string RelativePath);
