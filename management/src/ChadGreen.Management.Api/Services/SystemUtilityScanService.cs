using ChadGreen.Management.Api.Options;
using ChadGreen.Management.Shared.Contracts;
using Microsoft.Extensions.Options;
using System.Collections;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace ChadGreen.Management.Api.Services;

public interface ISystemUtilityScanService
{
    Task<UtilityScanResult> RunScanAsync(UtilityScanType scanType, CancellationToken cancellationToken = default);
    UtilityDashboardResponse GetDashboard();
    UtilityScanResult? GetLatestResult(UtilityScanType scanType);
}

public sealed class UtilityScanExecutionException(string message, Exception? innerException = null) : InvalidOperationException(message, innerException);

public sealed class SystemUtilityScanService(
    IOptions<ManagementOptions> options,
    IMarkdownFrontmatterFileService markdownService) : ISystemUtilityScanService
{
    private static readonly Regex MarkdownLinkRegex = new(@"(?<!!)\[[^\]]*\]\((?<target>[^)\s]+)", RegexOptions.Compiled);
    private static readonly Regex MarkdownImageRegex = new(@"!\[[^\]]*\]\((?<target>[^)\s]+)", RegexOptions.Compiled);
    private static readonly string[] ImageExtensions = [".png", ".jpg", ".jpeg", ".gif", ".webp", ".svg", ".avif"];
    private static readonly HttpClient ExternalLinkClient = new(new HttpClientHandler { AllowAutoRedirect = true })
    {
        Timeout = TimeSpan.FromSeconds(5)
    };

    private readonly ManagementOptions _options = options.Value;
    private readonly ConcurrentDictionary<UtilityScanType, UtilityScanResult> _latestResults = new();

    static SystemUtilityScanService()
    {
        ExternalLinkClient.DefaultRequestHeaders.UserAgent.ParseAdd("ChadGreenManagementUtilityScan/1.0");
    }

    public async Task<UtilityScanResult> RunScanAsync(UtilityScanType scanType, CancellationToken cancellationToken = default)
    {
        var startedAtUtc = DateTimeOffset.UtcNow;
        var siteRoot = ResolveSiteRoot();
        var contentRoot = Path.Combine(siteRoot, "src", "content");
        if (!Directory.Exists(contentRoot))
        {
            throw new UtilityScanExecutionException($"Content root '{contentRoot}' was not found.");
        }

        UtilityScanResult result;
        try
        {
            result = scanType switch
            {
                UtilityScanType.DeadLinks => await RunDeadLinkScanAsync(siteRoot, contentRoot, startedAtUtc, cancellationToken),
                UtilityScanType.MissingImages => await RunMissingImageScanAsync(siteRoot, contentRoot, startedAtUtc, cancellationToken),
                _ => throw new UtilityScanExecutionException($"Unsupported scan type '{scanType}'.")
            };
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception) when (exception is not UtilityScanExecutionException)
        {
            throw new UtilityScanExecutionException($"The {scanType} utility scan failed.", exception);
        }

        _latestResults[scanType] = result;
        return result;
    }

    public UtilityDashboardResponse GetDashboard()
    {
        var scans = Enum.GetValues<UtilityScanType>()
            .Select(scanType =>
            {
                if (_latestResults.TryGetValue(scanType, out var result))
                {
                    return new UtilityDashboardItem(scanType, result.CompletedAtUtc, result.Success, result.Summary);
                }

                return new UtilityDashboardItem(scanType, null, false, null);
            })
            .ToList();

        return new UtilityDashboardResponse(DateTimeOffset.UtcNow, scans);
    }

    public UtilityScanResult? GetLatestResult(UtilityScanType scanType)
        => _latestResults.TryGetValue(scanType, out var result) ? result : null;

    private async Task<UtilityScanResult> RunDeadLinkScanAsync(string siteRoot, string contentRoot, DateTimeOffset startedAtUtc, CancellationToken cancellationToken)
    {
        var issues = new List<UtilityScanIssue>();
        var checkedLinks = 0;
        var seenCandidates = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var filePath in Directory.EnumerateFiles(contentRoot, "*.md", SearchOption.AllDirectories))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var document = await markdownService.ReadAsync(filePath, cancellationToken);
            var sourceFile = ContentModelHelpers.ToRelativePath(siteRoot, filePath);

            foreach (var link in ExtractLinkCandidates(document.Frontmatter, document.Body))
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (!TryNormalizeTarget(link.Target, out var normalizedTarget))
                {
                    continue;
                }

                if (!LooksLikeDeadLinkCandidate(normalizedTarget))
                {
                    continue;
                }

                var candidateKey = $"{sourceFile}|{link.FieldPath}|{normalizedTarget}";
                if (!seenCandidates.Add(candidateKey))
                {
                    continue;
                }

                checkedLinks++;
                var check = await CheckLinkAsync(siteRoot, filePath, normalizedTarget, cancellationToken);
                if (!check.IsBroken)
                {
                    continue;
                }

                issues.Add(new UtilityScanIssue(
                    check.Severity,
                    sourceFile,
                    link.FieldPath,
                    check.Message,
                    normalizedTarget,
                    check.Code,
                    check.BlocksSave,
                    check.Remediation));
            }
        }

        var normalizedIssues = NormalizeIssues(issues);
        var summary = BuildSummary(checkedLinks, normalizedIssues);
        return new UtilityScanResult(
            UtilityScanType.DeadLinks,
            startedAtUtc,
            DateTimeOffset.UtcNow,
            Success: true,
            ErrorMessage: null,
            summary,
            normalizedIssues);
    }

    private async Task<UtilityScanResult> RunMissingImageScanAsync(string siteRoot, string contentRoot, DateTimeOffset startedAtUtc, CancellationToken cancellationToken)
    {
        var issues = new List<UtilityScanIssue>();
        var checkedImages = 0;
        var seenCandidates = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var filePath in Directory.EnumerateFiles(contentRoot, "*.md", SearchOption.AllDirectories))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var document = await markdownService.ReadAsync(filePath, cancellationToken);
            var sourceFile = ContentModelHelpers.ToRelativePath(siteRoot, filePath);

            foreach (var image in ExtractImageCandidates(document.Frontmatter, document.Body))
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (!TryNormalizeTarget(image.Target, out var normalizedTarget))
                {
                    continue;
                }

                if (!IsLocalPath(normalizedTarget))
                {
                    continue;
                }

                var candidateKey = $"{sourceFile}|{image.FieldPath}|{normalizedTarget}";
                if (!seenCandidates.Add(candidateKey))
                {
                    continue;
                }

                checkedImages++;
                var resolvedPath = ResolveLocalPath(siteRoot, filePath, normalizedTarget);
                if (resolvedPath is not null && File.Exists(resolvedPath))
                {
                    continue;
                }

                issues.Add(new UtilityScanIssue(
                    UtilityScanSeverity.Error,
                    sourceFile,
                    image.FieldPath,
                    "Image file does not exist.",
                    normalizedTarget,
                    IssueCode: "missing_image",
                    BlocksSave: true,
                    Remediation: "Point the image field to an existing file under /public/images or update the markdown reference."));
            }
        }

        var normalizedIssues = NormalizeIssues(issues);
        var summary = BuildSummary(checkedImages, normalizedIssues);
        return new UtilityScanResult(
            UtilityScanType.MissingImages,
            startedAtUtc,
            DateTimeOffset.UtcNow,
            Success: true,
            ErrorMessage: null,
            summary,
            normalizedIssues);
    }

    private static async Task<LinkCheckResult> CheckLinkAsync(string siteRoot, string sourceFilePath, string target, CancellationToken cancellationToken)
    {
        if (Uri.TryCreate(target, UriKind.Absolute, out var absoluteUri) && (absoluteUri.Scheme == Uri.UriSchemeHttp || absoluteUri.Scheme == Uri.UriSchemeHttps))
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Head, absoluteUri);
                using var response = await ExternalLinkClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                var statusCode = (int)response.StatusCode;
                if (statusCode is 405 or 501)
                {
                    using var fallbackRequest = new HttpRequestMessage(HttpMethod.Get, absoluteUri);
                    using var fallbackResponse = await ExternalLinkClient.SendAsync(fallbackRequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                    statusCode = (int)fallbackResponse.StatusCode;
                }

                if (statusCode == 429 || statusCode >= 500)
                {
                    return new LinkCheckResult(
                        IsBroken: true,
                        Severity: UtilityScanSeverity.Warning,
                        Message: $"External URL returned transient HTTP {statusCode}; verification is inconclusive.",
                        Code: "external_link_unverified",
                        BlocksSave: false,
                        Remediation: "Retry the scan later or verify the URL manually before publishing.");
                }

                if (statusCode >= 400)
                {
                    return new LinkCheckResult(
                        IsBroken: true,
                        Severity: UtilityScanSeverity.Warning,
                        Message: $"External URL returned HTTP {statusCode}.",
                        Code: "external_dead_link",
                        BlocksSave: false,
                        Remediation: "Fix or remove the URL. External dead links are warning-only and do not block save.");
                }

                return LinkCheckResult.Ok;
            }
            catch (HttpRequestException exception)
            {
                return new LinkCheckResult(
                    IsBroken: true,
                    Severity: UtilityScanSeverity.Warning,
                    Message: $"External URL could not be verified: {exception.GetType().Name}.",
                    Code: "external_link_unverified",
                    BlocksSave: false,
                    Remediation: "Retry the scan later or verify the URL manually.");
            }
            catch (TaskCanceledException)
            {
                return new LinkCheckResult(
                    IsBroken: true,
                    Severity: UtilityScanSeverity.Warning,
                    Message: "External URL check timed out.",
                    Code: "external_link_unverified",
                    BlocksSave: false,
                    Remediation: "Retry the scan later or verify the URL manually.");
            }
        }

        if (!IsLocalPath(target) || !LooksLikeResolvableLocalPath(target))
        {
            return LinkCheckResult.Ok;
        }

        var path = ResolveLocalPath(siteRoot, sourceFilePath, target);
        if (path is not null && File.Exists(path))
        {
            return LinkCheckResult.Ok;
        }

        return new LinkCheckResult(
            IsBroken: true,
            Severity: UtilityScanSeverity.Error,
            Message: "Local link target does not exist.",
            Code: "local_dead_link",
            BlocksSave: true,
            Remediation: "Update the local link to an existing file or route-backed markdown source.");
    }

    private static UtilityScanSummary BuildSummary(int totalChecked, IReadOnlyList<UtilityScanIssue> issues)
    {
        var errors = issues.Count(issue => issue.Severity == UtilityScanSeverity.Error);
        var warnings = issues.Count(issue => issue.Severity == UtilityScanSeverity.Warning);
        var infos = issues.Count(issue => issue.Severity == UtilityScanSeverity.Info);
        var blockingIssues = issues.Count(issue => issue.BlocksSave);
        return new UtilityScanSummary(totalChecked, issues.Count, errors, warnings, infos, blockingIssues, warnings);
    }

    private static bool LooksLikeDeadLinkCandidate(string target)
    {
        if (target.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase) ||
            target.StartsWith("tel:", StringComparison.OrdinalIgnoreCase) ||
            (target.Length > 0 && target[0] == '#'))
        {
            return false;
        }

        if (Uri.TryCreate(target, UriKind.Absolute, out var absoluteUri))
        {
            return absoluteUri.Scheme == Uri.UriSchemeHttp || absoluteUri.Scheme == Uri.UriSchemeHttps;
        }

        return (target.Length > 0 && target[0] == '/') ||
               target.StartsWith("./", StringComparison.Ordinal) ||
               target.StartsWith("../", StringComparison.Ordinal) ||
               target.EndsWith(".md", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsLocalPath(string target)
        => !Uri.TryCreate(target, UriKind.Absolute, out _)
            && !target.StartsWith("data:", StringComparison.OrdinalIgnoreCase)
            && !target.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase)
            && !target.StartsWith("tel:", StringComparison.OrdinalIgnoreCase);

    private static bool TryNormalizeTarget(string? rawTarget, out string normalizedTarget)
    {
        normalizedTarget = string.Empty;
        if (string.IsNullOrWhiteSpace(rawTarget))
        {
            return false;
        }

        normalizedTarget = rawTarget.Trim().Trim('<', '>').Trim('"', '\'');
        var hashIndex = normalizedTarget.IndexOf('#');
        if (hashIndex >= 0)
        {
            normalizedTarget = normalizedTarget[..hashIndex];
        }

        if (string.IsNullOrWhiteSpace(normalizedTarget))
        {
            return false;
        }

        var queryIndex = normalizedTarget.IndexOf('?');
        if (queryIndex >= 0 && !normalizedTarget.StartsWith("http://", StringComparison.OrdinalIgnoreCase) && !normalizedTarget.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            normalizedTarget = normalizedTarget[..queryIndex];
        }

        return !string.IsNullOrWhiteSpace(normalizedTarget);
    }

    private static string? ResolveLocalPath(string siteRoot, string sourceFilePath, string target)
    {
        if (target.Length > 0 && target[0] == '/')
        {
            var rootedTarget = target.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var underPublic = Path.Combine(siteRoot, "public", rootedTarget);
            if (File.Exists(underPublic))
            {
                return underPublic;
            }

            var direct = Path.Combine(siteRoot, rootedTarget);
            return direct;
        }

        var sourceDirectory = Path.GetDirectoryName(sourceFilePath) ?? siteRoot;
        var combined = Path.GetFullPath(Path.Combine(sourceDirectory, target.Replace('/', Path.DirectorySeparatorChar)));
        if (!combined.StartsWith(siteRoot, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return combined;
    }

    private static IEnumerable<LinkCandidate> ExtractLinkCandidates(Dictionary<string, object?> frontmatter, string body)
    {
        foreach (var field in ExtractStringFields(frontmatter, prefix: null))
        {
            yield return new LinkCandidate(field.Path, field.Value);
        }

        foreach (Match match in MarkdownLinkRegex.Matches(body))
        {
            var value = match.Groups["target"].Value;
            yield return new LinkCandidate("body.links[]", value);
        }
    }

    private static IEnumerable<LinkCandidate> ExtractImageCandidates(Dictionary<string, object?> frontmatter, string body)
    {
        foreach (var field in ExtractStringFields(frontmatter, prefix: null))
        {
            if (IsImageField(field.Path, field.Value))
            {
                yield return new LinkCandidate(field.Path, field.Value);
            }
        }

        foreach (Match match in MarkdownImageRegex.Matches(body))
        {
            var value = match.Groups["target"].Value;
            yield return new LinkCandidate("body.images[]", value);
        }
    }

    private static bool IsImageField(string path, string value)
    {
        var pathHintsImage = path.Contains("image", StringComparison.OrdinalIgnoreCase) ||
                             path.Contains("thumbnail", StringComparison.OrdinalIgnoreCase) ||
                             path.Contains("avatar", StringComparison.OrdinalIgnoreCase) ||
                             path.Contains("photo", StringComparison.OrdinalIgnoreCase);
        var valueLooksLikeImagePath = ImageExtensions.Any(extension => value.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
                                      || value.StartsWith("/images/", StringComparison.OrdinalIgnoreCase);

        return valueLooksLikeImagePath && pathHintsImage;
    }

    private static IReadOnlyList<UtilityScanIssue> NormalizeIssues(IEnumerable<UtilityScanIssue> issues)
    {
        return issues
            .GroupBy(
                issue => $"{issue.Code}|{issue.File}|{issue.FieldPath}|{issue.Target}|{issue.Issue}",
                StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .OrderByDescending(issue => issue.BlocksSave)
            .ThenBy(issue => SeveritySortKey(issue.Severity))
            .ThenBy(issue => issue.File, StringComparer.OrdinalIgnoreCase)
            .ThenBy(issue => issue.FieldPath, StringComparer.OrdinalIgnoreCase)
            .ThenBy(issue => issue.Target, StringComparer.OrdinalIgnoreCase)
            .ThenBy(issue => issue.Code, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static int SeveritySortKey(UtilityScanSeverity severity)
        => severity switch
        {
            UtilityScanSeverity.Error => 0,
            UtilityScanSeverity.Warning => 1,
            _ => 2
        };

    private static bool LooksLikeResolvableLocalPath(string target)
    {
        if (target.EndsWith(".md", StringComparison.OrdinalIgnoreCase) ||
            target.StartsWith("/images/", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return Path.HasExtension(target);
    }

    private static IEnumerable<StringFieldValue> ExtractStringFields(object? value, string? prefix)
    {
        if (value is null)
        {
            yield break;
        }

        if (value is string text)
        {
            yield return new StringFieldValue(prefix ?? "$", text);
            yield break;
        }

        if (value is IEnumerable list and not string)
        {
            var index = 0;
            foreach (var item in list)
            {
                var path = $"{prefix}[{index}]";
                foreach (var nested in ExtractStringFields(item, path))
                {
                    yield return nested;
                }

                index++;
            }

            yield break;
        }

        if (value is Dictionary<string, object?> dictionary)
        {
            foreach (var (key, item) in dictionary)
            {
                var path = string.IsNullOrWhiteSpace(prefix) ? key : $"{prefix}.{key}";
                foreach (var nested in ExtractStringFields(item, path))
                {
                    yield return nested;
                }
            }

            yield break;
        }

        if (value is IDictionary<string, object?> genericDictionary)
        {
            foreach (var (key, item) in genericDictionary)
            {
                var path = string.IsNullOrWhiteSpace(prefix) ? key : $"{prefix}.{key}";
                foreach (var nested in ExtractStringFields(item, path))
                {
                    yield return nested;
                }
            }
        }
    }

    private string ResolveSiteRoot()
        => SiteRootResolver.Resolve(_options.SiteRoot, AppContext.BaseDirectory);

    private readonly record struct LinkCandidate(string FieldPath, string Target);
    private readonly record struct StringFieldValue(string Path, string Value);
    private readonly record struct LinkCheckResult(
        bool IsBroken,
        UtilityScanSeverity Severity,
        string Message,
        string Code,
        bool BlocksSave,
        string Remediation)
    {
        public static LinkCheckResult Ok => new(false, UtilityScanSeverity.Info, string.Empty, "ok", false, string.Empty);
    }
}
