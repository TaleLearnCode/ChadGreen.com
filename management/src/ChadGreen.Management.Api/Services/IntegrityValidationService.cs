using ChadGreen.Management.Api.Options;
using ChadGreen.Management.Shared.Contracts;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;

namespace ChadGreen.Management.Api.Services;

public interface IIntegrityValidationService
{
    Task<IntegrityValidationResponse> ValidateAsync(IntegrityValidationRequest request, CancellationToken cancellationToken = default);
}

public sealed partial class IntegrityValidationService(IOptions<ManagementOptions> options) : IIntegrityValidationService
{
    private const string SchemaRuleId = "schema-frontmatter-compliance";
    private const string InternalReferenceRuleId = "internal-reference-check";
    private const string ExternalLinkRuleId = "external-link-warning";
    private const string MediaPathRuleId = "media-path-existence";
    private static readonly HashSet<string> BlogCategories =
    [
        "technical",
        "speaking",
        "community",
        "career",
        "tutorial",
        "announcement",
        "personal",
        "other"
    ];

    private static readonly IntegrityRuleDefinition[] RuleDefinitions =
    [
        new(SchemaRuleId, "Schema and frontmatter compliance", "Validates frontmatter against content schema and required field rules.", IntegrityScope.SchemaFrontmatter, IntegritySeverity.Error, true, true),
        new(InternalReferenceRuleId, "Internal reference checks", "Validates local references and cross-collection slug/id links resolve correctly.", IntegrityScope.InternalReference, IntegritySeverity.Error, true, true),
        new(ExternalLinkRuleId, "External link warning checks", "Validates external links and emits warnings for dead or unreachable targets.", IntegrityScope.ExternalLink, IntegritySeverity.Warning, false, true),
        new(MediaPathRuleId, "Media path existence checks", "Validates local media paths exist in the repository and are resolvable at runtime.", IntegrityScope.MediaPath, IntegritySeverity.Error, true, true)
    ];
    private static readonly EnumerationOptions RecursiveEnumeration = new()
    {
        RecurseSubdirectories = true,
        IgnoreInaccessible = true
    };

    private static readonly HttpClient ExternalLinkClient = CreateExternalLinkClient();
    private readonly ManagementOptions _options = options.Value;

    public async Task<IntegrityValidationResponse> ValidateAsync(IntegrityValidationRequest request, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var requestId = Guid.NewGuid().ToString("n");
        var findings = new List<IntegrityFinding>();
        var cascadeUpdates = new List<ReferenceCascadeUpdate>();
        var siteRoot = SiteRootResolver.Resolve(_options.SiteRoot, AppContext.BaseDirectory);

        var presentationSlugs = LoadSlugs(siteRoot, "presentations");
        var eventSlugs = LoadSlugs(siteRoot, "events");
        var meetupGroupSlugs = LoadSlugs(siteRoot, "meetupGroups");
        var location = BuildLocation(request);
        var collection = request.Collection.Trim().ToLowerInvariant();

        if (!request.Frontmatter.HasValue || request.Frontmatter.Value.ValueKind != System.Text.Json.JsonValueKind.Object)
        {
            findings.Add(new IntegrityFinding(
                SchemaRuleId,
                IntegritySeverity.Error,
                IntegrityScope.SchemaFrontmatter,
                "Frontmatter payload is required for integrity validation.",
                location with { Field = "frontmatter" },
                new IntegrityRemediation("Include the parsed frontmatter object in the integrity request."),
                true));
        }
        else
        {
            if (collection == "presentations")
            {
                await ValidatePresentationAsync(request, findings, location, siteRoot, presentationSlugs, cancellationToken).ConfigureAwait(false);
            }
            else if (collection == "events")
            {
                await ValidateEventAsync(request, findings, location, siteRoot, presentationSlugs, eventSlugs, cancellationToken).ConfigureAwait(false);
            }
            else if (collection == "meetupgroups")
            {
                await ValidateMeetupGroupAsync(request, findings, location, siteRoot, cancellationToken).ConfigureAwait(false);
            }
            else if (collection == "meetupevents")
            {
                await ValidateMeetupEventAsync(request, findings, location, siteRoot, meetupGroupSlugs, cancellationToken).ConfigureAwait(false);
            }
            else if (collection == "blog")
            {
                await ValidateBlogAsync(request, findings, location, siteRoot, cancellationToken).ConfigureAwait(false);
            }
            else if (collection is "authors" or "sitedata" or "about" or "profile")
            {
                await ValidateProfileAsync(request, findings, location, siteRoot, collection, cancellationToken).ConfigureAwait(false);
            }
        }

        var errors = findings.Count(f => f.Severity == IntegritySeverity.Error);
        var warnings = findings.Count(f => f.Severity == IntegritySeverity.Warning);
        var infos = findings.Count(f => f.Severity == IntegritySeverity.Info);
        var blockingFindings = findings.Count(f => f.BlocksSave);
        var blocksSave = blockingFindings > 0;

        if (request.SlugMutation is not null)
        {
            ApplySlugCascade(
                siteRoot,
                request.SlugMutation with { AutoCascadeReferences = request.SlugMutation.AutoCascadeReferences && !blocksSave },
                cascadeUpdates);
        }

        return new IntegrityValidationResponse(
            requestId,
            DateTimeOffset.UtcNow,
            new IntegrityValidationSummary(
                Errors: errors,
                Warnings: warnings,
                Infos: infos,
                BlockingFindings: blockingFindings,
                WarningFindings: warnings,
                BlocksSave: blocksSave),
            findings,
            RuleDefinitions,
            cascadeUpdates);
    }

    private async Task ValidatePresentationAsync(
        IntegrityValidationRequest request,
        List<IntegrityFinding> findings,
        IntegrityFindingLocation location,
        string siteRoot,
        HashSet<string> presentationSlugs,
        CancellationToken cancellationToken)
    {
        var frontmatter = request.Frontmatter!.Value;
        var relatedPresentations = GetStringArrayProperty(frontmatter, "relatedPresentations");
        var duplicateRelated = relatedPresentations
            .Where(static value => !string.IsNullOrWhiteSpace(value))
            .GroupBy(static value => value, StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToList();

        foreach (var duplicate in duplicateRelated)
        {
            findings.Add(new IntegrityFinding(
                InternalReferenceRuleId,
                IntegritySeverity.Error,
                IntegrityScope.InternalReference,
                $"Duplicate related presentation reference '{duplicate}' detected.",
                location with { Field = "relatedPresentations" },
                new IntegrityRemediation("Keep each related presentation slug only once."),
                true));
        }

        foreach (var related in relatedPresentations.Where(static value => !string.IsNullOrWhiteSpace(value)))
        {
            if (!presentationSlugs.Contains(related))
            {
                findings.Add(new IntegrityFinding(
                    InternalReferenceRuleId,
                    IntegritySeverity.Error,
                    IntegrityScope.InternalReference,
                    $"Related presentation '{related}' does not resolve to an existing presentation slug.",
                    location with { Field = "relatedPresentations" },
                    new IntegrityRemediation("Reference an existing slug from src/content/presentations."),
                    true));
            }
        }

        ValidateLocalMediaPath(frontmatter, "heroImage", request.FilePath, siteRoot, findings, location);
        ValidateLocalMediaPath(frontmatter, "thumbnail", request.FilePath, siteRoot, findings, location);

        if (TryGetProperty(frontmatter, "resources", out var resources) && resources.ValueKind == System.Text.Json.JsonValueKind.Array)
        {
            foreach (var resource in resources.EnumerateArray())
            {
                if (resource.ValueKind != System.Text.Json.JsonValueKind.Object || !TryGetProperty(resource, "url", out var urlElement))
                {
                    continue;
                }

                var url = urlElement.GetString();
                await ValidateUrlReferenceAsync(url, "resources[].url", request, findings, location, siteRoot, cancellationToken).ConfigureAwait(false);
            }
        }
    }

    private async Task ValidateEventAsync(
        IntegrityValidationRequest request,
        List<IntegrityFinding> findings,
        IntegrityFindingLocation location,
        string siteRoot,
        HashSet<string> presentationSlugs,
        HashSet<string> eventSlugs,
        CancellationToken cancellationToken)
    {
        _ = eventSlugs;
        var frontmatter = request.Frontmatter!.Value;

        ValidateLocalMediaPath(frontmatter, "heroImage", request.FilePath, siteRoot, findings, location);
        ValidateLocalMediaPath(frontmatter, "thumbnail", request.FilePath, siteRoot, findings, location);

        if (TryGetProperty(frontmatter, "website", out var website))
        {
            await ValidateUrlReferenceAsync(website.GetString(), "website", request, findings, location, siteRoot, cancellationToken).ConfigureAwait(false);
        }

        var eventPresentationIds = new List<string>();
        if (TryGetProperty(frontmatter, "presentations", out var presentations) && presentations.ValueKind == System.Text.Json.JsonValueKind.Array)
        {
            var index = 0;
            foreach (var presentationEntry in presentations.EnumerateArray())
            {
                index++;
                var presentationId = GetEventPresentationId(presentationEntry);
                if (string.IsNullOrWhiteSpace(presentationId))
                {
                    findings.Add(new IntegrityFinding(
                        InternalReferenceRuleId,
                        IntegritySeverity.Error,
                        IntegrityScope.InternalReference,
                        $"Event presentation entry at index {index} is missing a presentation id.",
                        location with { Field = "presentations" },
                        new IntegrityRemediation("Use a presentation slug string or object with a non-empty id."),
                        true));
                    continue;
                }

                eventPresentationIds.Add(presentationId);

                if (!presentationSlugs.Contains(presentationId))
                {
                    findings.Add(new IntegrityFinding(
                        InternalReferenceRuleId,
                        IntegritySeverity.Error,
                        IntegrityScope.InternalReference,
                        $"Event presentation id '{presentationId}' does not resolve to an existing presentation slug.",
                        location with { Field = "presentations" },
                        new IntegrityRemediation("Reference an existing slug from src/content/presentations."),
                        true));
                }

                if (presentationEntry.ValueKind == System.Text.Json.JsonValueKind.Object &&
                    TryGetProperty(presentationEntry, "sessionUrl", out var sessionUrl))
                {
                    await ValidateUrlReferenceAsync(sessionUrl.GetString(), "presentations[].sessionUrl", request, findings, location, siteRoot, cancellationToken).ConfigureAwait(false);
                }
            }
        }

        var duplicates = eventPresentationIds
            .GroupBy(static id => id, StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToList();

        foreach (var duplicate in duplicates)
        {
            findings.Add(new IntegrityFinding(
                InternalReferenceRuleId,
                IntegritySeverity.Error,
                IntegrityScope.InternalReference,
                $"Duplicate event presentation reference '{duplicate}' detected.",
                location with { Field = "presentations" },
                new IntegrityRemediation("Keep each presentation id unique within the event."),
                true));
        }

    }

    private async Task ValidateMeetupGroupAsync(
        IntegrityValidationRequest request,
        List<IntegrityFinding> findings,
        IntegrityFindingLocation location,
        string siteRoot,
        CancellationToken cancellationToken)
    {
        var frontmatter = request.Frontmatter!.Value;

        RequireString(frontmatter, "title", location, findings);
        RequireString(frontmatter, "slug", location, findings);
        RequireString(frontmatter, "description", location, findings);
        RequireString(frontmatter, "city", location, findings);
        RequireString(frontmatter, "country", location, findings);

        ValidateLocalMediaPath(frontmatter, "heroImage", request.FilePath, siteRoot, findings, location);

        if (TryGetProperty(frontmatter, "website", out var website))
        {
            await ValidateUrlReferenceAsync(GetElementString(website), "website", request, findings, location, siteRoot, cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task ValidateMeetupEventAsync(
        IntegrityValidationRequest request,
        List<IntegrityFinding> findings,
        IntegrityFindingLocation location,
        string siteRoot,
        HashSet<string> meetupGroupSlugs,
        CancellationToken cancellationToken)
    {
        var frontmatter = request.Frontmatter!.Value;

        RequireString(frontmatter, "title", location, findings);
        RequireString(frontmatter, "description", location, findings);
        RequireString(frontmatter, "meetupGroup", location, findings);
        RequireDate(frontmatter, "date", location, findings);

        ValidateLocalMediaPath(frontmatter, "heroImage", request.FilePath, siteRoot, findings, location);
        ValidateLocalMediaPath(frontmatter, "thumbnail", request.FilePath, siteRoot, findings, location);
        ValidateLocalMediaPath(frontmatter, "speaker.photo", request.FilePath, siteRoot, findings, location, "speaker", "photo");

        if (TryGetProperty(frontmatter, "meetupGroup", out var meetupGroupElement) &&
            TryGetNormalizedString(meetupGroupElement, out var meetupGroupSlug) &&
            !meetupGroupSlugs.Contains(meetupGroupSlug))
        {
            findings.Add(new IntegrityFinding(
                InternalReferenceRuleId,
                IntegritySeverity.Error,
                IntegrityScope.InternalReference,
                $"Meetup group '{meetupGroupSlug}' does not resolve to an existing meetup group slug.",
                location with { Field = "meetupGroup" },
                new IntegrityRemediation("Reference an existing slug from src/content/meetupGroups."),
                true));
        }

        if (TryGetProperty(frontmatter, "eventUrl", out var eventUrl))
        {
            await ValidateUrlReferenceAsync(GetElementString(eventUrl), "eventUrl", request, findings, location, siteRoot, cancellationToken).ConfigureAwait(false);
        }

        if (TryGetProperty(frontmatter, "resources", out var resources) && resources.ValueKind == System.Text.Json.JsonValueKind.Array)
        {
            foreach (var resource in resources.EnumerateArray())
            {
                if (resource.ValueKind != System.Text.Json.JsonValueKind.Object || !TryGetProperty(resource, "url", out var urlElement))
                {
                    continue;
                }

                await ValidateUrlReferenceAsync(GetElementString(urlElement), "resources[].url", request, findings, location, siteRoot, cancellationToken).ConfigureAwait(false);
            }
        }

        if (TryGetProperty(frontmatter, "speaker", out var speaker) && speaker.ValueKind == System.Text.Json.JsonValueKind.Object)
        {
            RequireString(speaker, "name", location, findings, "speaker.name");

            if (TryGetProperty(speaker, "social", out var social) && social.ValueKind == System.Text.Json.JsonValueKind.Object)
            {
                foreach (var platform in new[] { "website", "linkedin", "twitter", "github", "youtube", "sessionize", "bluesky" })
                {
                    if (TryGetProperty(social, platform, out var socialUrl))
                    {
                        var rawSocialUrl = GetElementString(socialUrl);
                        if (platform == "twitter" && !IsAbsoluteHttpUrl(rawSocialUrl))
                        {
                            continue;
                        }

                        await ValidateUrlReferenceAsync(rawSocialUrl, $"speaker.social.{platform}", request, findings, location, siteRoot, cancellationToken).ConfigureAwait(false);
                    }
                }
            }
        }
    }

    private async Task ValidateBlogAsync(
        IntegrityValidationRequest request,
        List<IntegrityFinding> findings,
        IntegrityFindingLocation location,
        string siteRoot,
        CancellationToken cancellationToken)
    {
        var frontmatter = request.Frontmatter!.Value;

        RequireString(frontmatter, "title", location, findings);
        RequireString(frontmatter, "description", location, findings);
        RequireDate(frontmatter, "pubDate", location, findings);

        ValidateLocalMediaPath(frontmatter, "heroImage", request.FilePath, siteRoot, findings, location);

        if (TryGetProperty(frontmatter, "category", out var categoryElement) &&
            TryGetNormalizedString(categoryElement, out var category) &&
            !BlogCategories.Contains(category))
        {
            findings.Add(new IntegrityFinding(
                SchemaRuleId,
                IntegritySeverity.Error,
                IntegrityScope.SchemaFrontmatter,
                $"Blog category '{category}' is not in the allowed enum set.",
                location with { Field = "category" },
                new IntegrityRemediation("Use one of: Technical, Speaking, Community, Career, Tutorial, Announcement, Personal, Other."),
                true));
        }

        if (TryGetProperty(frontmatter, "canonicalUrl", out var canonicalUrl))
        {
            await ValidateUrlReferenceAsync(GetElementString(canonicalUrl), "canonicalUrl", request, findings, location, siteRoot, cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task ValidateProfileAsync(
        IntegrityValidationRequest request,
        List<IntegrityFinding> findings,
        IntegrityFindingLocation location,
        string siteRoot,
        string collection,
        CancellationToken cancellationToken)
    {
        var frontmatter = request.Frontmatter!.Value;

        if (collection is "authors" or "about" or "profile")
        {
            RequireString(frontmatter, "name", location, findings);
            ValidateLocalMediaPath(frontmatter, "avatar", request.FilePath, siteRoot, findings, location);
        }
        else
        {
            RequireString(frontmatter, "name", location, findings);
            RequireString(frontmatter, "tagline", location, findings);
            RequireString(frontmatter, "description", location, findings);
        }

        if (TryGetProperty(frontmatter, "social", out var social) && social.ValueKind == System.Text.Json.JsonValueKind.Object)
        {
            foreach (var field in new[] { "twitter", "linkedin", "github", "youtube", "website", "sessionize", "bluesky" })
            {
                if (TryGetProperty(social, field, out var socialUrl))
                {
                    var rawSocialUrl = GetElementString(socialUrl);
                    if (field == "twitter" && !IsAbsoluteHttpUrl(rawSocialUrl))
                    {
                        continue;
                    }

                    await ValidateUrlReferenceAsync(rawSocialUrl, $"social.{field}", request, findings, location, siteRoot, cancellationToken).ConfigureAwait(false);
                }
            }
        }

        if (TryGetProperty(frontmatter, "socialLinks", out var socialLinks) && socialLinks.ValueKind == System.Text.Json.JsonValueKind.Array)
        {
            foreach (var socialLink in socialLinks.EnumerateArray())
            {
                if (socialLink.ValueKind != System.Text.Json.JsonValueKind.Object || !TryGetProperty(socialLink, "url", out var url))
                {
                    continue;
                }

                await ValidateUrlReferenceAsync(GetElementString(url), "socialLinks[].url", request, findings, location, siteRoot, cancellationToken).ConfigureAwait(false);
            }
        }
    }

    private void ApplySlugCascade(string siteRoot, SlugMutationRequest slugMutation, List<ReferenceCascadeUpdate> updates)
    {
        if (string.IsNullOrWhiteSpace(slugMutation.From) ||
            string.IsNullOrWhiteSpace(slugMutation.To) ||
            string.Equals(slugMutation.From, slugMutation.To, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var mutationCollection = slugMutation.Collection.Trim().ToLowerInvariant();
        if (mutationCollection == "events")
        {
            CascadeLineKey(
                siteRoot,
                "engagementPresentations",
                "eventSlug",
                slugMutation.From,
                slugMutation.To,
                slugMutation.AutoCascadeReferences,
                updates);
        }
        else if (mutationCollection == "presentations")
        {
            CascadeLineKey(
                siteRoot,
                "engagementPresentations",
                "presentationSlug",
                slugMutation.From,
                slugMutation.To,
                slugMutation.AutoCascadeReferences,
                updates);
            CascadeListBlock(
                siteRoot,
                "presentations",
                "relatedPresentations",
                slugMutation.From,
                slugMutation.To,
                slugMutation.AutoCascadeReferences,
                updates);
            CascadeEventPresentationsBlock(
                siteRoot,
                slugMutation.From,
                slugMutation.To,
                slugMutation.AutoCascadeReferences,
                updates);
        }
        else if (mutationCollection == "meetupgroups")
        {
            CascadeLineKey(
                siteRoot,
                "meetupEvents",
                "meetupGroup",
                slugMutation.From,
                slugMutation.To,
                slugMutation.AutoCascadeReferences,
                updates);
        }
    }

    private void CascadeLineKey(
        string siteRoot,
        string collection,
        string key,
        string from,
        string to,
        bool autoApply,
        List<ReferenceCascadeUpdate> updates)
    {
        foreach (var filePath in EnumerateCollectionMarkdownFiles(siteRoot, collection))
        {
            var source = File.ReadAllText(filePath);
            if (!TryUpdateFrontmatter(source, lines => ReplaceKeyLine(lines, key, from, to), out var updated, out var replacedCount))
            {
                continue;
            }

            if (autoApply && updated is not null)
            {
                File.WriteAllText(filePath, updated);
            }

            for (var i = 0; i < replacedCount; i++)
            {
                updates.Add(new ReferenceCascadeUpdate(
                    collection,
                    ToRepoRelativePath(siteRoot, filePath),
                    Path.GetFileNameWithoutExtension(filePath),
                    $"frontmatter.{key}",
                    from,
                    to,
                    autoApply));
            }
        }
    }

    private void CascadeListBlock(
        string siteRoot,
        string collection,
        string listName,
        string from,
        string to,
        bool autoApply,
        List<ReferenceCascadeUpdate> updates)
    {
        foreach (var filePath in EnumerateCollectionMarkdownFiles(siteRoot, collection))
        {
            var source = File.ReadAllText(filePath);
            if (!TryUpdateFrontmatter(source, lines => ReplaceListBlockValues(lines, listName, from, to), out var updated, out var replacedCount))
            {
                continue;
            }

            if (autoApply && updated is not null)
            {
                File.WriteAllText(filePath, updated);
            }

            for (var i = 0; i < replacedCount; i++)
            {
                updates.Add(new ReferenceCascadeUpdate(
                    collection,
                    ToRepoRelativePath(siteRoot, filePath),
                    Path.GetFileNameWithoutExtension(filePath),
                    $"frontmatter.{listName}[]",
                    from,
                    to,
                    autoApply));
            }
        }
    }

    private void CascadeEventPresentationsBlock(
        string siteRoot,
        string from,
        string to,
        bool autoApply,
        List<ReferenceCascadeUpdate> updates)
    {
        const string collection = "events";
        foreach (var filePath in EnumerateCollectionMarkdownFiles(siteRoot, collection))
        {
            var source = File.ReadAllText(filePath);
            if (!TryUpdateFrontmatter(source, lines => ReplaceEventPresentationValues(lines, from, to), out var updated, out var replacedCount))
            {
                continue;
            }

            if (autoApply && updated is not null)
            {
                File.WriteAllText(filePath, updated);
            }

            for (var i = 0; i < replacedCount; i++)
            {
                updates.Add(new ReferenceCascadeUpdate(
                    collection,
                    ToRepoRelativePath(siteRoot, filePath),
                    Path.GetFileNameWithoutExtension(filePath),
                    "frontmatter.presentations[].id",
                    from,
                    to,
                    autoApply));
            }
        }
    }

    private async Task ValidateUrlReferenceAsync(
        string? rawUrl,
        string field,
        IntegrityValidationRequest request,
        List<IntegrityFinding> findings,
        IntegrityFindingLocation location,
        string siteRoot,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(rawUrl))
        {
            return;
        }

        if (Uri.TryCreate(rawUrl, UriKind.Absolute, out var absoluteUri))
        {
            if (!string.Equals(absoluteUri.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(absoluteUri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
            {
                findings.Add(new IntegrityFinding(
                    ExternalLinkRuleId,
                    IntegritySeverity.Warning,
                    IntegrityScope.ExternalLink,
                    $"Unsupported external URL scheme for '{rawUrl}'.",
                    location with { Field = field },
                    new IntegrityRemediation("Use an http or https URL."),
                    false));
                return;
            }

            if (!request.IncludeExternalLinkChecks)
            {
                return;
            }

            using var requestMessage = new HttpRequestMessage(HttpMethod.Head, absoluteUri);
            try
            {
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cts.CancelAfter(TimeSpan.FromSeconds(2));
                using var response = await ExternalLinkClient.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead, cts.Token).ConfigureAwait(false);
                if ((int)response.StatusCode >= 400)
                {
                    findings.Add(new IntegrityFinding(
                        ExternalLinkRuleId,
                        IntegritySeverity.Warning,
                        IntegrityScope.ExternalLink,
                        $"External URL '{rawUrl}' returned HTTP {(int)response.StatusCode}.",
                        location with { Field = field },
                        new IntegrityRemediation("Verify the external URL is reachable; warnings do not block save."),
                        false));
                }
            }
            catch
            {
                findings.Add(new IntegrityFinding(
                    ExternalLinkRuleId,
                    IntegritySeverity.Warning,
                    IntegrityScope.ExternalLink,
                    $"External URL '{rawUrl}' could not be verified.",
                    location with { Field = field },
                    new IntegrityRemediation("Review the URL manually; external link issues remain warning-only."),
                    false));
            }

            return;
        }

        ValidateLocalFilePath(rawUrl, field, request.FilePath, siteRoot, findings, location);
    }

    private static IntegrityFindingLocation BuildLocation(IntegrityValidationRequest request)
        => new(
            Collection: request.Collection,
            FilePath: request.FilePath,
            EntryId: string.IsNullOrWhiteSpace(request.EntryId) ? Path.GetFileNameWithoutExtension(request.FilePath) : request.EntryId);

    private static string GetEventPresentationId(System.Text.Json.JsonElement presentationEntry)
    {
        if (presentationEntry.ValueKind == System.Text.Json.JsonValueKind.String)
        {
            return presentationEntry.GetString() ?? string.Empty;
        }

        if (presentationEntry.ValueKind == System.Text.Json.JsonValueKind.Object &&
            TryGetProperty(presentationEntry, "id", out var idElement))
        {
            return idElement.GetString() ?? string.Empty;
        }

        return string.Empty;
    }

    private static List<string> GetStringArrayProperty(System.Text.Json.JsonElement frontmatter, string propertyName)
    {
        var values = new List<string>();
        if (!TryGetProperty(frontmatter, propertyName, out var property) || property.ValueKind != System.Text.Json.JsonValueKind.Array)
        {
            return values;
        }

        foreach (var item in property.EnumerateArray())
        {
            if (item.ValueKind == System.Text.Json.JsonValueKind.String)
            {
                values.Add(item.GetString() ?? string.Empty);
            }
        }

        return values;
    }

    private static void RequireString(
        System.Text.Json.JsonElement frontmatter,
        string propertyName,
        IntegrityFindingLocation location,
        List<IntegrityFinding> findings,
        string? fieldOverride = null)
    {
        if (!TryGetProperty(frontmatter, propertyName, out var property) || !TryGetNormalizedString(property, out _))
        {
            findings.Add(new IntegrityFinding(
                SchemaRuleId,
                IntegritySeverity.Error,
                IntegrityScope.SchemaFrontmatter,
                $"Required field '{propertyName}' must be a non-empty string.",
                location with { Field = fieldOverride ?? propertyName },
                new IntegrityRemediation($"Provide a non-empty value for '{propertyName}' to satisfy collection schema requirements."),
                true));
        }
    }

    private static void RequireDate(
        System.Text.Json.JsonElement frontmatter,
        string propertyName,
        IntegrityFindingLocation location,
        List<IntegrityFinding> findings)
    {
        if (!TryGetProperty(frontmatter, propertyName, out var property) || !TryGetDate(property, out _))
        {
            findings.Add(new IntegrityFinding(
                SchemaRuleId,
                IntegritySeverity.Error,
                IntegrityScope.SchemaFrontmatter,
                $"Required field '{propertyName}' must be a valid date value.",
                location with { Field = propertyName },
                new IntegrityRemediation($"Provide a valid date for '{propertyName}' (for example 2026-07-03)."),
                true));
        }
    }

    private static bool TryGetDate(System.Text.Json.JsonElement element, out DateTimeOffset parsedDate)
    {
        parsedDate = default;
        if (element.ValueKind == System.Text.Json.JsonValueKind.String &&
            DateTimeOffset.TryParse(element.GetString(), out var parsedFromString))
        {
            parsedDate = parsedFromString;
            return true;
        }

        return false;
    }

    private static bool TryGetNormalizedString(System.Text.Json.JsonElement element, out string normalized)
    {
        if (element.ValueKind != System.Text.Json.JsonValueKind.String)
        {
            normalized = string.Empty;
            return false;
        }

        normalized = element.GetString() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(normalized))
        {
            normalized = string.Empty;
            return false;
        }

        normalized = normalized.Trim();
        return true;
    }

    private static string GetElementString(System.Text.Json.JsonElement element)
    {
        return element.ValueKind switch
        {
            System.Text.Json.JsonValueKind.String => element.GetString() ?? string.Empty,
            System.Text.Json.JsonValueKind.Number => element.ToString(),
            System.Text.Json.JsonValueKind.True => bool.TrueString,
            System.Text.Json.JsonValueKind.False => bool.FalseString,
            _ => string.Empty
        };
    }

    private static bool IsAbsoluteHttpUrl(string? value)
        => !string.IsNullOrWhiteSpace(value) &&
           Uri.TryCreate(value, UriKind.Absolute, out var uri) &&
           (string.Equals(uri.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase));

    private static bool TryGetProperty(System.Text.Json.JsonElement element, string propertyName, out System.Text.Json.JsonElement value)
    {
        if (element.ValueKind == System.Text.Json.JsonValueKind.Object)
        {
            foreach (var property in element.EnumerateObject())
            {
                if (string.Equals(property.Name, propertyName, StringComparison.OrdinalIgnoreCase))
                {
                    value = property.Value;
                    return true;
                }
            }
        }

        value = default;
        return false;
    }

    private static void ValidateLocalMediaPath(
        System.Text.Json.JsonElement frontmatter,
        string field,
        string filePath,
        string siteRoot,
        List<IntegrityFinding> findings,
        IntegrityFindingLocation location)
    {
        if (TryGetProperty(frontmatter, field, out var pathElement) && pathElement.ValueKind == System.Text.Json.JsonValueKind.String)
        {
            ValidateLocalFilePath(pathElement.GetString(), field, filePath, siteRoot, findings, location);
        }
    }

    private static void ValidateLocalMediaPath(
        System.Text.Json.JsonElement frontmatter,
        string field,
        string filePath,
        string siteRoot,
        List<IntegrityFinding> findings,
        IntegrityFindingLocation location,
        string parentPropertyName,
        string nestedFieldName)
    {
        if (!TryGetProperty(frontmatter, parentPropertyName, out var parent) ||
            parent.ValueKind != System.Text.Json.JsonValueKind.Object ||
            !TryGetProperty(parent, nestedFieldName, out var pathElement) ||
            pathElement.ValueKind != System.Text.Json.JsonValueKind.String)
        {
            return;
        }

        ValidateLocalFilePath(pathElement.GetString(), field, filePath, siteRoot, findings, location);
    }

    private static void ValidateLocalFilePath(
        string? rawPath,
        string field,
        string requestFilePath,
        string siteRoot,
        List<IntegrityFinding> findings,
        IntegrityFindingLocation location)
    {
        if (string.IsNullOrWhiteSpace(rawPath) ||
            rawPath.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            rawPath.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var normalized = rawPath.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
        string candidate;
        if (rawPath.StartsWith('/'))
        {
            candidate = Path.Combine(siteRoot, "public", normalized.TrimStart(Path.DirectorySeparatorChar));
        }
        else
        {
            var sourcePath = requestFilePath.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
            var sourceDirectory = Path.GetDirectoryName(Path.Combine(siteRoot, sourcePath)) ?? siteRoot;
            candidate = Path.GetFullPath(Path.Combine(sourceDirectory, normalized));
        }

        if (!File.Exists(candidate) && !Directory.Exists(candidate))
        {
            findings.Add(new IntegrityFinding(
                MediaPathRuleId,
                IntegritySeverity.Error,
                IntegrityScope.MediaPath,
                $"Local path '{rawPath}' does not exist.",
                location with { Field = field },
                new IntegrityRemediation("Update the path to an existing local file or image asset."),
                true));
        }
    }

    private static HashSet<string> LoadSlugs(string siteRoot, string collection)
    {
        var collectionPath = Path.Combine(siteRoot, "src", "content", collection);
        if (!Directory.Exists(collectionPath))
        {
            return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        return Directory.EnumerateFiles(collectionPath, "*.md", RecursiveEnumeration)
            .Select(filePath => Path.GetFileNameWithoutExtension(filePath) ?? string.Empty)
            .Where(static slug => !string.IsNullOrWhiteSpace(slug))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    private static IEnumerable<string> EnumerateCollectionMarkdownFiles(string siteRoot, string collection)
    {
        var collectionPath = Path.Combine(siteRoot, "src", "content", collection);
        return Directory.Exists(collectionPath)
            ? Directory.EnumerateFiles(collectionPath, "*.md", RecursiveEnumeration)
            : [];
    }

    private static string ToRepoRelativePath(string siteRoot, string absolutePath)
        => Path.GetRelativePath(siteRoot, absolutePath).Replace('\\', '/');

    private static bool TryUpdateFrontmatter(
        string source,
        Func<List<string>, int> mutation,
        out string? updatedContent,
        out int replacements)
    {
        updatedContent = null;
        replacements = 0;
        var newline = source.Contains("\r\n", StringComparison.Ordinal) ? "\r\n" : "\n";
        var lines = source.Split(["\r\n", "\n"], StringSplitOptions.None).ToList();
        if (lines.Count < 3 || lines[0].Trim() != "---")
        {
            return false;
        }

        var closingIndex = lines.FindIndex(1, line => line.Trim() == "---");
        if (closingIndex <= 0)
        {
            return false;
        }

        var frontmatterLines = lines.GetRange(1, closingIndex - 1);
        replacements = mutation(frontmatterLines);
        if (replacements <= 0)
        {
            return false;
        }

        lines.RemoveRange(1, closingIndex - 1);
        lines.InsertRange(1, frontmatterLines);
        updatedContent = string.Join(newline, lines);
        return true;
    }

    private static int ReplaceKeyLine(List<string> frontmatterLines, string key, string from, string to)
    {
        var replacements = 0;
        for (var i = 0; i < frontmatterLines.Count; i++)
        {
            var line = frontmatterLines[i];
            var match = KeyLineRegex().Match(line);
            if (!match.Success || !string.Equals(match.Groups["key"].Value, key, StringComparison.Ordinal))
            {
                continue;
            }

            if (!string.Equals(match.Groups["value"].Value.Trim(), from, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            frontmatterLines[i] = $"{match.Groups["prefix"].Value}{to}{match.Groups["suffix"].Value}";
            replacements++;
        }

        return replacements;
    }

    private static int ReplaceListBlockValues(List<string> frontmatterLines, string listName, string from, string to)
    {
        var replacements = 0;
        var inBlock = false;

        for (var i = 0; i < frontmatterLines.Count; i++)
        {
            var line = frontmatterLines[i];
            var trimmed = line.TrimStart();
            var indent = line.Length - trimmed.Length;

            if (!inBlock && indent == 0 && trimmed.StartsWith($"{listName}:", StringComparison.Ordinal))
            {
                inBlock = true;
                continue;
            }

            if (inBlock && indent == 0 && trimmed.Contains(':', StringComparison.Ordinal))
            {
                inBlock = false;
            }

            if (!inBlock)
            {
                continue;
            }

            var match = ListItemRegex().Match(line);
            if (!match.Success)
            {
                continue;
            }

            if (!string.Equals(match.Groups["value"].Value.Trim(), from, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            frontmatterLines[i] = $"{match.Groups["prefix"].Value}{to}{match.Groups["suffix"].Value}";
            replacements++;
        }

        return replacements;
    }

    private static int ReplaceEventPresentationValues(List<string> frontmatterLines, string from, string to)
    {
        var replacements = 0;
        var inPresentationsBlock = false;

        for (var i = 0; i < frontmatterLines.Count; i++)
        {
            var line = frontmatterLines[i];
            var trimmed = line.TrimStart();
            var indent = line.Length - trimmed.Length;

            if (!inPresentationsBlock && indent == 0 && trimmed.StartsWith("presentations:", StringComparison.Ordinal))
            {
                inPresentationsBlock = true;
                continue;
            }

            if (inPresentationsBlock && indent == 0 && trimmed.Contains(':', StringComparison.Ordinal))
            {
                inPresentationsBlock = false;
            }

            if (!inPresentationsBlock)
            {
                continue;
            }

            var listItemMatch = ListItemRegex().Match(line);
            if (listItemMatch.Success &&
                string.Equals(listItemMatch.Groups["value"].Value.Trim(), from, StringComparison.OrdinalIgnoreCase))
            {
                frontmatterLines[i] = $"{listItemMatch.Groups["prefix"].Value}{to}{listItemMatch.Groups["suffix"].Value}";
                replacements++;
                continue;
            }

            var idMatch = EventPresentationIdRegex().Match(line);
            if (idMatch.Success &&
                string.Equals(idMatch.Groups["value"].Value.Trim(), from, StringComparison.OrdinalIgnoreCase))
            {
                frontmatterLines[i] = $"{idMatch.Groups["prefix"].Value}{to}{idMatch.Groups["suffix"].Value}";
                replacements++;
            }
        }

        return replacements;
    }

    private static HttpClient CreateExternalLinkClient()
    {
        var client = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(2)
        };

        client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("ChadGreenManagementIntegrity", "1.0"));
        return client;
    }

    [GeneratedRegex(@"^(?<prefix>\s*(?<key>[A-Za-z0-9_-]+)\s*:\s*['""]?)(?<value>[^'""]+?)(?<suffix>['""]?\s*(#.*)?)$")]
    private static partial Regex KeyLineRegex();

    [GeneratedRegex(@"^(?<prefix>\s*-\s*['""]?)(?<value>[^'""]+?)(?<suffix>['""]?\s*(#.*)?)$")]
    private static partial Regex ListItemRegex();

    [GeneratedRegex(@"^(?<prefix>\s*id\s*:\s*['""]?)(?<value>[^'""]+?)(?<suffix>['""]?\s*(#.*)?)$")]
    private static partial Regex EventPresentationIdRegex();
}
