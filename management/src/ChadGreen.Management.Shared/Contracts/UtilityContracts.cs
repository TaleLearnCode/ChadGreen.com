using System.Text.Json.Serialization;

namespace ChadGreen.Management.Shared.Contracts;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum UtilityScanType
{
    DeadLinks,
    MissingImages
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum UtilityScanSeverity
{
    Error,
    Warning,
    Info
}

public record UtilityScanIssue(
    UtilityScanSeverity Severity,
    string SourceFile,
    string? FieldPath,
    string Issue,
    string? Target = null,
    string? IssueCode = null,
    bool BlocksSave = false,
    string? Remediation = null)
{
    public string Code => string.IsNullOrWhiteSpace(IssueCode) ? "unspecified" : IssueCode;
    public string File => SourceFile;
    public string Message => Issue;
    public string? FieldOrPath => !string.IsNullOrWhiteSpace(FieldPath) ? FieldPath : Target;
}

public record UtilityScanSummary(
    int TotalChecked,
    int Issues,
    int Errors,
    int Warnings,
    int Infos,
    int BlockingIssues = 0,
    int WarningIssues = 0);

public record UtilityScanResult(
    UtilityScanType ScanType,
    DateTimeOffset StartedAtUtc,
    DateTimeOffset CompletedAtUtc,
    bool Success,
    string? ErrorMessage,
    UtilityScanSummary Summary,
    IReadOnlyList<UtilityScanIssue> Issues);

public record UtilityDashboardItem(
    UtilityScanType ScanType,
    DateTimeOffset? LastRunAtUtc,
    bool LastRunSucceeded,
    UtilityScanSummary? Summary);

public record UtilityDashboardResponse(
    DateTimeOffset GeneratedAtUtc,
    IReadOnlyList<UtilityDashboardItem> Scans);

public record ApiErrorResponse(
    string Message,
    string Code,
    string? Guidance = null);

public record SaveConflictResponse(
    string Message,
    string Code,
    string FilePath,
    DateTimeOffset? ExpectedLastModifiedUtc,
    DateTimeOffset? CurrentLastModifiedUtc,
    string Guidance);
