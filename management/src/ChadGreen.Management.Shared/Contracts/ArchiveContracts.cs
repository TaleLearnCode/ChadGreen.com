namespace ChadGreen.Management.Shared.Contracts;

public enum ArchiveOperationType
{
    Archive,
    Restore
}

public record ArchiveItemRequest(string RelativePath);

public record ArchiveOperationResponse(
    ArchiveOperationType Operation,
    bool Success,
    string Message,
    string RelativePath,
    string? SourcePath = null,
    string? DestinationPath = null,
    int PurgedEntries = 0,
    DateTimeOffset? ProcessedAtUtc = null);
