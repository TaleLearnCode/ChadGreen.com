namespace ChadGreen.Management.Api.Services;

public sealed class SaveConflictException(
    string filePath,
    DateTimeOffset? expectedLastModifiedUtc,
    DateTimeOffset? currentLastModifiedUtc) : InvalidOperationException("The file changed on disk after it was loaded.")
{
    public string FilePath { get; } = filePath;
    public DateTimeOffset? ExpectedLastModifiedUtc { get; } = expectedLastModifiedUtc;
    public DateTimeOffset? CurrentLastModifiedUtc { get; } = currentLastModifiedUtc;
}
