using ChadGreen.Management.Api.Options;
using ChadGreen.Management.Shared.Contracts;
using Microsoft.Extensions.Options;

namespace ChadGreen.Management.Api.Services;

public interface IMediaManagementService
{
    Task<MediaListResponse> ListAsync(string? folder, CancellationToken cancellationToken = default);

    Task<MediaUploadResponse> UploadAsync(IFormFile file, string? folder, bool overwrite, CancellationToken cancellationToken = default);

    Task<MediaUploadResponse> ReplaceAsync(string relativePath, IFormFile file, CancellationToken cancellationToken = default);

    Task<ArchiveOperationResponse> ArchiveAsync(string relativePath, CancellationToken cancellationToken = default);
}

public sealed class MediaManagementService(
    IOptions<ManagementOptions> options,
    IArchiveService archiveService) : IMediaManagementService
{
    private static readonly HashSet<string> AllowedExtensions = [".png", ".jpg", ".jpeg", ".webp", ".svg"];
    private static readonly EnumerationOptions RecursiveEnumeration = new()
    {
        RecurseSubdirectories = true,
        IgnoreInaccessible = true
    };
    private const long MaxFileSizeBytes = 5 * 1024 * 1024;
    private readonly ManagementOptions _options = options.Value;

    public Task<MediaListResponse> ListAsync(string? folder, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var siteRoot = ResolveSiteRoot();
        var imagesRoot = GetImagesRoot(siteRoot);
        Directory.CreateDirectory(imagesRoot);

        if (!TryNormalizeFolder(folder, out var normalizedFolder, out var validationError))
        {
            throw new InvalidOperationException(validationError);
        }

        var searchRoot = string.IsNullOrWhiteSpace(normalizedFolder)
            ? imagesRoot
            : Path.Combine(imagesRoot, normalizedFolder!);

        if (!Directory.Exists(searchRoot))
        {
            return Task.FromResult(new MediaListResponse([]));
        }

        var items = new List<MediaItemDto>();
        foreach (var path in Directory.EnumerateFiles(searchRoot, "*", RecursiveEnumeration))
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!AllowedExtensions.Contains(Path.GetExtension(path)))
            {
                continue;
            }

            try
            {
                items.Add(MapItem(siteRoot, imagesRoot, path));
            }
            catch (IOException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
        }

        items.Sort(static (left, right) => string.Compare(left.RelativePath, right.RelativePath, StringComparison.OrdinalIgnoreCase));

        return Task.FromResult(new MediaListResponse(items));
    }

    public async Task<MediaUploadResponse> UploadAsync(IFormFile file, string? folder, bool overwrite, CancellationToken cancellationToken = default)
    {
        ValidateFile(file);

        if (!TryNormalizeFolder(folder, out var normalizedFolder, out var validationError))
        {
            return new MediaUploadResponse(false, validationError);
        }

        var siteRoot = ResolveSiteRoot();
        var imagesRoot = GetImagesRoot(siteRoot);
        Directory.CreateDirectory(imagesRoot);

        var destinationDirectory = string.IsNullOrWhiteSpace(normalizedFolder)
            ? imagesRoot
            : Path.Combine(imagesRoot, normalizedFolder!);
        Directory.CreateDirectory(destinationDirectory);

        var destinationPath = Path.Combine(destinationDirectory, Path.GetFileName(file.FileName));
        if (File.Exists(destinationPath) && !overwrite)
        {
            return new MediaUploadResponse(false, "A file with the same name already exists.");
        }

        await using (var destinationStream = File.Create(destinationPath))
        await using (var sourceStream = file.OpenReadStream())
        {
            await sourceStream.CopyToAsync(destinationStream, cancellationToken);
        }

        return new MediaUploadResponse(true, "Upload completed.", MapItem(siteRoot, imagesRoot, destinationPath));
    }

    public async Task<MediaUploadResponse> ReplaceAsync(string relativePath, IFormFile file, CancellationToken cancellationToken = default)
    {
        ValidateFile(file);

        if (!TryNormalizeImageRelativePath(relativePath, out var normalizedRelativePath, out var validationError))
        {
            return new MediaUploadResponse(false, validationError);
        }

        var siteRoot = ResolveSiteRoot();
        var imagesRoot = GetImagesRoot(siteRoot);
        Directory.CreateDirectory(imagesRoot);

        var targetPath = Path.GetFullPath(Path.Combine(siteRoot, normalizedRelativePath));
        if (!targetPath.StartsWith(imagesRoot, StringComparison.OrdinalIgnoreCase))
        {
            return new MediaUploadResponse(false, "Path escapes public/images.");
        }

        if (!File.Exists(targetPath))
        {
            return new MediaUploadResponse(false, "Target image was not found.");
        }

        var targetExtension = Path.GetExtension(targetPath);
        var incomingExtension = Path.GetExtension(file.FileName);
        if (!string.Equals(targetExtension, incomingExtension, StringComparison.OrdinalIgnoreCase))
        {
            return new MediaUploadResponse(false, $"Replacement extension must match existing file extension '{targetExtension}'.");
        }

        var replacementPath = $"{targetPath}.uploading";
        try
        {
            await using (var destinationStream = File.Create(replacementPath))
            await using (var sourceStream = file.OpenReadStream())
            {
                await sourceStream.CopyToAsync(destinationStream, cancellationToken);
            }

            File.Copy(replacementPath, targetPath, overwrite: true);
            File.SetLastWriteTimeUtc(targetPath, DateTime.UtcNow);
        }
        finally
        {
            if (File.Exists(replacementPath))
            {
                File.Delete(replacementPath);
            }
        }

        return new MediaUploadResponse(true, "Replacement completed.", MapItem(siteRoot, imagesRoot, targetPath));
    }

    public async Task<ArchiveOperationResponse> ArchiveAsync(string relativePath, CancellationToken cancellationToken = default)
    {
        if (!TryNormalizeImageRelativePath(relativePath, out var normalizedRelativePath, out var validationError))
        {
            return new ArchiveOperationResponse(ArchiveOperationType.Archive, false, validationError, relativePath);
        }

        return await archiveService.ArchiveAsync(normalizedRelativePath, cancellationToken);
    }

    private string ResolveSiteRoot()
        => SiteRootResolver.Resolve(_options.SiteRoot, AppContext.BaseDirectory);

    private static string GetImagesRoot(string siteRoot)
        => Path.Combine(siteRoot, "public", "images");

    private static MediaItemDto MapItem(string siteRoot, string imagesRoot, string absolutePath)
    {
        var relativeFromRoot = Path.GetRelativePath(siteRoot, absolutePath).Replace('\\', '/');
        var relativeFromImages = Path.GetRelativePath(imagesRoot, absolutePath).Replace('\\', '/');
        var directory = Path.GetDirectoryName(relativeFromImages)?.Replace('\\', '/') ?? string.Empty;
        var publicUrlPath = relativeFromRoot.StartsWith("public/", StringComparison.OrdinalIgnoreCase)
            ? relativeFromRoot["public/".Length..]
            : relativeFromRoot;
        var publicUrl = "/" + publicUrlPath.Replace('\\', '/');
        var fileInfo = new FileInfo(absolutePath);

        return new MediaItemDto(
            RelativePath: relativeFromRoot,
            PublicUrl: publicUrl,
            FileName: Path.GetFileName(absolutePath),
            Directory: directory,
            SizeBytes: fileInfo.Length,
            LastModifiedUtc: fileInfo.LastWriteTimeUtc);
    }

    private static bool TryNormalizeFolder(string? folder, out string? normalizedFolder, out string error)
    {
        normalizedFolder = null;
        error = string.Empty;

        if (string.IsNullOrWhiteSpace(folder))
        {
            return true;
        }

        var candidate = folder.Replace('\\', '/').Trim();
        while (candidate.StartsWith("/", StringComparison.Ordinal))
        {
            candidate = candidate[1..];
        }

        if (candidate.StartsWith("images/", StringComparison.OrdinalIgnoreCase))
        {
            candidate = candidate["images/".Length..];
        }

        var segments = candidate.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (segments.Length == 0)
        {
            return true;
        }

        if (segments.Any(segment => segment == ".."))
        {
            error = "Path traversal segments are not allowed.";
            return false;
        }

        normalizedFolder = Path.Combine(segments);
        return true;
    }

    private static bool TryNormalizeImageRelativePath(string? relativePath, out string normalizedPath, out string error)
    {
        normalizedPath = string.Empty;
        error = string.Empty;

        if (string.IsNullOrWhiteSpace(relativePath))
        {
            error = "A relative path is required.";
            return false;
        }

        var candidate = relativePath.Replace('\\', '/').Trim();
        while (candidate.StartsWith("/", StringComparison.Ordinal))
        {
            candidate = candidate[1..];
        }

        if (!candidate.StartsWith("public/images/", StringComparison.OrdinalIgnoreCase))
        {
            if (candidate.StartsWith("images/", StringComparison.OrdinalIgnoreCase))
            {
                candidate = $"public/{candidate}";
            }
            else
            {
                error = "Only files under public/images are allowed.";
                return false;
            }
        }

        var segments = candidate.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (segments.Length < 3 || segments.Any(segment => segment == ".."))
        {
            error = "Invalid image path.";
            return false;
        }

        if (!segments[0].Equals("public", StringComparison.OrdinalIgnoreCase) ||
            !segments[1].Equals("images", StringComparison.OrdinalIgnoreCase))
        {
            error = "Only files under public/images are allowed.";
            return false;
        }

        var extension = Path.GetExtension(candidate);
        if (!AllowedExtensions.Contains(extension))
        {
            error = $"Unsupported media type '{extension}'.";
            return false;
        }

        normalizedPath = Path.Combine(segments);
        return true;
    }

    private static void ValidateFile(IFormFile? file)
    {
        if (file is null || file.Length == 0)
        {
            throw new InvalidOperationException("A media file is required.");
        }

        if (file.Length > MaxFileSizeBytes)
        {
            throw new InvalidOperationException($"Media files must be 5 MB or smaller. Received {file.Length} bytes.");
        }

        var extension = Path.GetExtension(file.FileName);
        if (!AllowedExtensions.Contains(extension))
        {
            throw new InvalidOperationException("Only png, jpg, jpeg, webp, and svg files are supported.");
        }
    }
}
