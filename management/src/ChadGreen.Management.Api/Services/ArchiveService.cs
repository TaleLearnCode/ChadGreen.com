using ChadGreen.Management.Api.Options;
using ChadGreen.Management.Shared.Contracts;
using Microsoft.Extensions.Options;

namespace ChadGreen.Management.Api.Services;

public interface IArchiveService
{
    Task<ArchiveOperationResponse> ArchiveAsync(string relativePath, CancellationToken cancellationToken = default);

    Task<ArchiveOperationResponse> RestoreAsync(string relativePath, CancellationToken cancellationToken = default);
}

public sealed class ArchiveService(IOptions<ManagementOptions> options) : IArchiveService
{
    private readonly ManagementOptions _options = options.Value;

    public Task<ArchiveOperationResponse> ArchiveAsync(string relativePath, CancellationToken cancellationToken = default)
        => MoveAsync(relativePath, ArchiveOperationType.Archive, cancellationToken);

    public Task<ArchiveOperationResponse> RestoreAsync(string relativePath, CancellationToken cancellationToken = default)
        => MoveAsync(relativePath, ArchiveOperationType.Restore, cancellationToken);

    private Task<ArchiveOperationResponse> MoveAsync(string relativePath, ArchiveOperationType operationType, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!TryNormalizeRelativePath(relativePath, out var normalizedPath, out var validationError))
        {
            return Task.FromResult(new ArchiveOperationResponse(operationType, false, validationError, relativePath));
        }

        var siteRoot = ResolveSiteRoot();
        var archiveRoot = Path.Combine(siteRoot, _options.ArchiveFolderName);

        var sourceRoot = operationType == ArchiveOperationType.Archive ? siteRoot : archiveRoot;
        var destinationRoot = operationType == ArchiveOperationType.Archive ? archiveRoot : siteRoot;

        var sourcePath = Path.GetFullPath(Path.Combine(sourceRoot, normalizedPath));
        var destinationPath = Path.GetFullPath(Path.Combine(destinationRoot, normalizedPath));

        if (!sourcePath.StartsWith(sourceRoot, StringComparison.OrdinalIgnoreCase) ||
            !destinationPath.StartsWith(destinationRoot, StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(new ArchiveOperationResponse(operationType, false, "Path escapes allowed workspace.", normalizedPath));
        }

        if (operationType == ArchiveOperationType.Archive && sourcePath.StartsWith(archiveRoot, StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(new ArchiveOperationResponse(operationType, false, "Cannot archive content that is already in the archive root.", normalizedPath));
        }

        var sourceExists = File.Exists(sourcePath) || Directory.Exists(sourcePath);
        if (!sourceExists)
        {
            return Task.FromResult(new ArchiveOperationResponse(operationType, false, "Source path was not found.", normalizedPath));
        }

        if (File.Exists(destinationPath) || Directory.Exists(destinationPath))
        {
            return Task.FromResult(new ArchiveOperationResponse(operationType, false, "Destination already exists.", normalizedPath));
        }

        Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);

        try
        {
            if (File.Exists(sourcePath))
            {
                File.Move(sourcePath, destinationPath);
                File.SetLastWriteTimeUtc(destinationPath, DateTime.UtcNow);
            }
            else
            {
                Directory.Move(sourcePath, destinationPath);
                Directory.SetLastWriteTimeUtc(destinationPath, DateTime.UtcNow);
            }
        }
        catch (IOException exception)
        {
            return Task.FromResult(new ArchiveOperationResponse(operationType, false, $"Archive operation failed: {exception.Message}", normalizedPath));
        }
        catch (UnauthorizedAccessException exception)
        {
            return Task.FromResult(new ArchiveOperationResponse(operationType, false, $"Archive operation failed: {exception.Message}", normalizedPath));
        }

        var purgedEntries = PurgeExpiredArchiveEntries(
            archiveRoot,
            DateTime.UtcNow.AddDays(-Math.Abs(_options.ArchiveRetentionDays)),
            cancellationToken);

        return Task.FromResult(new ArchiveOperationResponse(
            operationType,
            true,
            "Operation completed.",
            normalizedPath,
            sourcePath,
            destinationPath,
            purgedEntries,
            DateTimeOffset.UtcNow));
    }

    private string ResolveSiteRoot()
    {
        var siteRoot = SiteRootResolver.Resolve(_options.SiteRoot, AppContext.BaseDirectory);
        Directory.CreateDirectory(Path.Combine(siteRoot, _options.ArchiveFolderName));
        return siteRoot;
    }

    private static bool TryNormalizeRelativePath(string? candidatePath, out string normalizedPath, out string validationError)
    {
        normalizedPath = string.Empty;
        validationError = string.Empty;

        if (string.IsNullOrWhiteSpace(candidatePath))
        {
            validationError = "A relative path is required.";
            return false;
        }

        var replaced = candidatePath.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar).Trim();
        if (Path.IsPathRooted(replaced))
        {
            validationError = "Only relative paths are allowed.";
            return false;
        }

        var segments = replaced.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length == 0 || segments.Any(segment => segment == ".."))
        {
            validationError = "Path traversal segments are not allowed.";
            return false;
        }

        normalizedPath = Path.Combine(segments);
        return true;
    }

    private static int PurgeExpiredArchiveEntries(string archiveRoot, DateTime thresholdUtc, CancellationToken cancellationToken)
    {
        if (!Directory.Exists(archiveRoot))
        {
            return 0;
        }

        var purgedCount = 0;

        foreach (var file in Directory.EnumerateFiles(archiveRoot, "*", SearchOption.AllDirectories))
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                if (File.GetLastWriteTimeUtc(file) < thresholdUtc)
                {
                    File.Delete(file);
                    purgedCount++;
                }
            }
            catch (IOException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
        }

        foreach (var directory in Directory.EnumerateDirectories(archiveRoot, "*", SearchOption.AllDirectories)
                     .OrderByDescending(path => path.Length))
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                if (!Directory.EnumerateFileSystemEntries(directory).Any() && Directory.GetLastWriteTimeUtc(directory) < thresholdUtc)
                {
                    Directory.Delete(directory, false);
                    purgedCount++;
                }
            }
            catch (IOException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
        }

        return purgedCount;
    }
}
