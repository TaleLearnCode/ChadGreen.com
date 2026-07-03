namespace ChadGreen.Management.Shared.Contracts;

public enum ChangeKind
{
    Added,
    Modified,
    Deleted,
    Renamed,
    Unchanged,
    Unknown
}

public record ChangeSummaryItem(
    string RelativePath,
    ChangeKind Kind,
    string? Details = null,
    long? SizeInBytes = null,
    DateTimeOffset? LastModifiedUtc = null);

public record ChangeSummaryResponse(IReadOnlyList<ChangeSummaryItem> Items);
