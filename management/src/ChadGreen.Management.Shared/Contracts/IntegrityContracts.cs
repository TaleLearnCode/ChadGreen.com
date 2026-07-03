using System.Text.Json;

namespace ChadGreen.Management.Shared.Contracts;

// Keep these contracts aligned with src/lib/integrity/{contracts,dtos,rules}.ts
// so Rusty's management API/UI can consume the same rule IDs and blocking semantics.

public enum IntegritySeverity
{
    Error,
    Warning,
    Info
}

public enum IntegrityScope
{
    SchemaFrontmatter,
    InternalReference,
    ExternalLink,
    MediaPath
}

public record IntegrityFindingLocation(
    string Collection,
    string FilePath,
    string? EntryId = null,
    string? Field = null,
    string? Path = null,
    int? Line = null,
    int? Column = null);

public record IntegrityRemediation(
    string Summary,
    string? SuggestedChange = null,
    bool AutoFixAvailable = false,
    string? DocsPath = null);

public record IntegrityFinding(
    string RuleId,
    IntegritySeverity Severity,
    IntegrityScope Scope,
    string Message,
    IntegrityFindingLocation Location,
    IntegrityRemediation Remediation,
    bool BlocksSave)
{
    public string Code => RuleId;

    public string File => Location.FilePath;

    public string? FieldOrPath
        => !string.IsNullOrWhiteSpace(Location.Field)
            ? Location.Field
            : Location.Path;
}

public record IntegrityRuleDefinition(
    string Id,
    string Name,
    string Description,
    IntegrityScope Scope,
    IntegritySeverity DefaultSeverity,
    bool BlocksSave,
    bool EnabledByDefault);

public record SlugMutationRequest(
    string Collection,
    string From,
    string To,
    bool AutoCascadeReferences);

public record IntegrityValidationRequest(
    string Source,
    string Collection,
    string FilePath,
    string? EntryId = null,
    JsonElement? Frontmatter = null,
    string? Body = null,
    IReadOnlyList<string>? ChangedFields = null,
    SlugMutationRequest? SlugMutation = null,
    bool IncludeExternalLinkChecks = false);

public record ReferenceCascadeUpdate(
    string Collection,
    string FilePath,
    string? EntryId,
    string FieldPath,
    string OldValue,
    string NewValue,
    bool AutoApplied);

public record IntegrityValidationSummary(
    int Errors,
    int Warnings,
    int Infos,
    int BlockingFindings,
    int WarningFindings,
    bool BlocksSave);

public record IntegrityValidationResponse(
    string RequestId,
    DateTimeOffset EvaluatedAt,
    IntegrityValidationSummary Summary,
    IReadOnlyList<IntegrityFinding> Findings,
    IReadOnlyList<IntegrityRuleDefinition> RulesEvaluated,
    IReadOnlyList<ReferenceCascadeUpdate> CascadeUpdates);

public record IntegritySaveBlockedResponse(
    string Message,
    IntegrityValidationResponse Integrity);
