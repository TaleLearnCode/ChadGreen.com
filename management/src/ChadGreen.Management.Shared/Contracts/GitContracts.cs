namespace ChadGreen.Management.Shared.Contracts;

public enum ConventionalCommitType
{
    Feat,
    Fix,
    Docs,
    Style,
    Refactor,
    Perf,
    Test,
    Build,
    Ci,
    Chore,
    Revert
}

public record CommitRequest(
    ConventionalCommitType Type,
    string Description,
    string? Scope = null,
    string? Body = null,
    bool IncludeAllChanges = true);

public record GitCapabilityResponse(
    bool Enabled,
    bool IsRepository,
    bool GitCliAvailable,
    bool SupportsLocalCommitsOnly,
    bool ConventionalCommitsRequired,
    string Message);

public record CommitResponse(
    bool Success,
    string Message,
    string? CommitSha = null,
    string? CommitMessage = null);
