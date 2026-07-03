
using ChadGreen.Management.Api.Options;
using ChadGreen.Management.Api.Services;
using ChadGreen.Management.Shared.Contracts;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;

namespace ChadGreen.Management.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var managementSection = builder.Configuration.GetSection("Management");
        var managementOptions = managementSection.Get<ManagementOptions>() ?? new ManagementOptions();

        builder.Services.Configure<ManagementOptions>(managementSection);
        builder.Services.AddScoped<IArchiveService, ArchiveService>();
        builder.Services.AddScoped<IGitCapabilityService, GitCapabilityService>();
        builder.Services.AddScoped<IIntegrityValidationService, IntegrityValidationService>();
        builder.Services.AddScoped<IGitCommitService, GitCommitService>();
        builder.Services.AddScoped<IMarkdownFrontmatterFileService, MarkdownFrontmatterFileService>();
        builder.Services.AddScoped<IPresentationManagementService, PresentationManagementService>();
        builder.Services.AddScoped<IEngagementManagementService, EngagementManagementService>();
        builder.Services.AddScoped<IMeetupGroupManagementService, MeetupGroupManagementService>();
        builder.Services.AddScoped<IMeetupEventManagementService, MeetupEventManagementService>();
        builder.Services.AddScoped<IBlogManagementService, BlogManagementService>();
        builder.Services.AddScoped<IAboutManagementService, AboutManagementService>();
        builder.Services.AddScoped<IMediaManagementService, MediaManagementService>();
        builder.Services.AddScoped<ISystemUtilityScanService, SystemUtilityScanService>();

        builder.Services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
            options.SerializerOptions.Converters.Add(new EngagementPresentationAssignmentDtoJsonConverter());
        });

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("ManagementClient", policy =>
            {
                policy.WithOrigins(managementOptions.ClientOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });

        var app = builder.Build();

        app.UseHttpsRedirection();
        app.UseCors("ManagementClient");

        app.MapGet("/api/health", (IConfiguration configuration) =>
        {
            var siteRoot = SiteRootResolver.Resolve(configuration["Management:SiteRoot"], app.Environment.ContentRootPath);

            return Results.Ok(new
            {
                status = "ok",
                timestampUtc = DateTimeOffset.UtcNow,
                siteRoot,
                archiveRetentionDays = configuration.GetValue("Management:ArchiveRetentionDays", 90)
            });
        });

        app.MapPost("/api/archive", async (ArchiveItemRequest request, IArchiveService archiveService, CancellationToken cancellationToken) =>
        {
            var response = await archiveService.ArchiveAsync(request.RelativePath, cancellationToken);
            return response.Success ? Results.Ok(response) : Results.BadRequest(response);
        });

        app.MapPost("/api/archive/restore", async (ArchiveItemRequest request, IArchiveService archiveService, CancellationToken cancellationToken) =>
        {
            var response = await archiveService.RestoreAsync(request.RelativePath, cancellationToken);
            return response.Success ? Results.Ok(response) : Results.BadRequest(response);
        });

        app.MapGet("/api/git/capability", async (IGitCapabilityService gitCapabilityService, CancellationToken cancellationToken) =>
        {
            var response = await gitCapabilityService.GetCapabilityAsync(cancellationToken);
            return Results.Ok(response);
        });

        // Mirrors src/lib/integrity contract semantics so the management UI can distinguish
        // blocking findings from warning-only findings during save workflows.
        app.MapPost("/api/integrity/validate", async (IntegrityValidationRequest request, IIntegrityValidationService integrityValidationService, CancellationToken cancellationToken) =>
        {
            var response = await integrityValidationService.ValidateAsync(request, cancellationToken);
            return Results.Ok(response);
        });

        app.MapGet("/api/utilities/dashboard", (ISystemUtilityScanService utilityScanService) =>
        {
            return Results.Ok(utilityScanService.GetDashboard());
        });

        app.MapGet("/api/utilities/scans/{scanType}/latest", (string scanType, ISystemUtilityScanService utilityScanService) =>
        {
            if (!TryParseScanType(scanType, out var parsedScanType))
            {
                return Results.BadRequest(new ApiErrorResponse(
                    $"Unsupported scan type '{scanType}'.",
                    Code: "invalid_scan_type",
                    Guidance: "Use 'dead-links' or 'missing-images'."));
            }

            var result = utilityScanService.GetLatestResult(parsedScanType);
            return result is null ? Results.NotFound() : Results.Ok(result);
        });

        app.MapPost("/api/utilities/scans/{scanType}/run", async (string scanType, ISystemUtilityScanService utilityScanService, CancellationToken cancellationToken) =>
        {
            if (!TryParseScanType(scanType, out var parsedScanType))
            {
                return Results.BadRequest(new ApiErrorResponse(
                    $"Unsupported scan type '{scanType}'.",
                    Code: "invalid_scan_type",
                    Guidance: "Use 'dead-links' or 'missing-images'."));
            }

            try
            {
                var result = await utilityScanService.RunScanAsync(parsedScanType, cancellationToken);
                return Results.Ok(result);
            }
            catch (UtilityScanExecutionException exception)
            {
                return Results.Json(
                    new ApiErrorResponse(
                        exception.Message,
                        Code: "utility_scan_failed",
                        Guidance: exception.InnerException?.Message),
                    statusCode: StatusCodes.Status500InternalServerError);
            }
        });

        app.MapPost("/api/git/commit", async (CommitRequest request, IGitCommitService gitCommitService, CancellationToken cancellationToken) =>
        {
            var response = await gitCommitService.CommitAsync(request, cancellationToken);
            return response.Success ? Results.Ok(response) : Results.BadRequest(response);
        });

        app.MapGet("/api/presentations", async (IPresentationManagementService service, CancellationToken cancellationToken) =>
        {
            var response = await service.ListAsync(cancellationToken);
            return Results.Ok(response);
        });

        app.MapGet("/api/presentations/options", async (IPresentationManagementService service, CancellationToken cancellationToken) =>
        {
            var response = await service.ListOptionsAsync(cancellationToken);
            return Results.Ok(response);
        });

        app.MapGet("/api/presentations/{slug}", async (string slug, IPresentationManagementService service, CancellationToken cancellationToken) =>
        {
            var response = await service.GetBySlugAsync(slug, cancellationToken);
            return response is null ? Results.NotFound() : Results.Ok(response);
        });

        app.MapPost("/api/presentations", async (PresentationUpsertRequest request, IPresentationManagementService service, CancellationToken cancellationToken) =>
        {
            try
            {
                var response = await service.CreateAsync(request, cancellationToken);
                return Results.Ok(response);
            }
            catch (IntegrityGateException exception)
            {
                return Results.BadRequest(new IntegritySaveBlockedResponse(
                    "Save blocked by integrity validation findings.",
                    exception.Result));
            }
            catch (SaveConflictException exception)
            {
                return ToConflictResult(exception);
            }
            catch (InvalidOperationException exception)
            {
                return ToValidationErrorResult(exception);
            }
        });

        app.MapPut("/api/presentations/{slug}", async (string slug, PresentationUpsertRequest request, IPresentationManagementService service, CancellationToken cancellationToken) =>
        {
            try
            {
                var response = await service.UpdateAsync(slug, request, cancellationToken);
                return response is null ? Results.NotFound() : Results.Ok(response);
            }
            catch (IntegrityGateException exception)
            {
                return Results.BadRequest(new IntegritySaveBlockedResponse(
                    "Save blocked by integrity validation findings.",
                    exception.Result));
            }
            catch (SaveConflictException exception)
            {
                return ToConflictResult(exception);
            }
            catch (InvalidOperationException exception)
            {
                return ToValidationErrorResult(exception);
            }
        });

        app.MapPut("/api/presentations/{slug}/status", async (string slug, PresentationStatusUpdateRequest request, IPresentationManagementService service, CancellationToken cancellationToken) =>
        {
            var response = await service.SetStatusAsync(slug, request.Status, cancellationToken);
            return response is null ? Results.NotFound() : Results.Ok(response);
        });

        app.MapPut("/api/presentations/{slug}/featured", async (string slug, PresentationFeaturedUpdateRequest request, IPresentationManagementService service, CancellationToken cancellationToken) =>
        {
            var response = await service.SetFeaturedAsync(slug, request.Featured, cancellationToken);
            return response is null ? Results.NotFound() : Results.Ok(response);
        });

        app.MapPost("/api/presentations/{slug}/archive", async (string slug, IPresentationManagementService service, CancellationToken cancellationToken) =>
        {
            var response = await service.ArchiveAsync(slug, cancellationToken);
            return response.Success ? Results.Ok(response) : Results.BadRequest(response);
        });

        app.MapGet("/api/engagements", async (IEngagementManagementService service, CancellationToken cancellationToken) =>
        {
            var response = await service.ListAsync(cancellationToken);
            return Results.Ok(response);
        });

        app.MapGet("/api/engagements/{slug}", async (string slug, IEngagementManagementService service, CancellationToken cancellationToken) =>
        {
            var response = await service.GetBySlugAsync(slug, cancellationToken);
            return response is null ? Results.NotFound() : Results.Ok(response);
        });

        app.MapPost("/api/engagements", async (EngagementUpsertRequest request, IEngagementManagementService service, CancellationToken cancellationToken) =>
        {
            try
            {
                var response = await service.CreateAsync(request, cancellationToken);
                return Results.Ok(response);
            }
            catch (IntegrityGateException exception)
            {
                return Results.BadRequest(new IntegritySaveBlockedResponse(
                    "Save blocked by integrity validation findings.",
                    exception.Result));
            }
            catch (SaveConflictException exception)
            {
                return ToConflictResult(exception);
            }
            catch (InvalidOperationException exception)
            {
                return ToValidationErrorResult(exception);
            }
        });

        app.MapPut("/api/engagements/{slug}", async (string slug, EngagementUpsertRequest request, IEngagementManagementService service, CancellationToken cancellationToken) =>
        {
            try
            {
                var response = await service.UpdateAsync(slug, request, cancellationToken);
                return response is null ? Results.NotFound() : Results.Ok(response);
            }
            catch (IntegrityGateException exception)
            {
                return Results.BadRequest(new IntegritySaveBlockedResponse(
                    "Save blocked by integrity validation findings.",
                    exception.Result));
            }
            catch (SaveConflictException exception)
            {
                return ToConflictResult(exception);
            }
            catch (InvalidOperationException exception)
            {
                return ToValidationErrorResult(exception);
            }
        });

        app.MapPut("/api/engagements/{slug}/featured", async (string slug, EngagementFeaturedUpdateRequest request, IEngagementManagementService service, CancellationToken cancellationToken) =>
        {
            var response = await service.SetFeaturedAsync(slug, request.Featured, cancellationToken);
            return response is null ? Results.NotFound() : Results.Ok(response);
        });

        app.MapPost("/api/engagements/{slug}/archive", async (string slug, IEngagementManagementService service, CancellationToken cancellationToken) =>
        {
            var response = await service.ArchiveAsync(slug, cancellationToken);
            return response.Success ? Results.Ok(response) : Results.BadRequest(response);
        });

        app.MapGet("/api/meetup-groups", async (IMeetupGroupManagementService service, CancellationToken cancellationToken) =>
        {
            var response = await service.ListAsync(cancellationToken);
            return Results.Ok(response);
        });

        app.MapGet("/api/meetup-groups/{slug}", async (string slug, IMeetupGroupManagementService service, CancellationToken cancellationToken) =>
        {
            var response = await service.GetBySlugAsync(slug, cancellationToken);
            return response is null ? Results.NotFound() : Results.Ok(response);
        });

        app.MapPost("/api/meetup-groups", async (MeetupGroupUpsertRequest request, IMeetupGroupManagementService service, CancellationToken cancellationToken) =>
        {
            try
            {
                var response = await service.CreateAsync(request, cancellationToken);
                return Results.Ok(response);
            }
            catch (IntegrityGateException exception)
            {
                return Results.BadRequest(new IntegritySaveBlockedResponse(
                    "Save blocked by integrity validation findings.",
                    exception.Result));
            }
            catch (SaveConflictException exception)
            {
                return ToConflictResult(exception);
            }
            catch (InvalidOperationException exception)
            {
                return ToValidationErrorResult(exception);
            }
        });

        app.MapPut("/api/meetup-groups/{slug}", async (string slug, MeetupGroupUpsertRequest request, IMeetupGroupManagementService service, CancellationToken cancellationToken) =>
        {
            try
            {
                var response = await service.UpdateAsync(slug, request, cancellationToken);
                return response is null ? Results.NotFound() : Results.Ok(response);
            }
            catch (IntegrityGateException exception)
            {
                return Results.BadRequest(new IntegritySaveBlockedResponse(
                    "Save blocked by integrity validation findings.",
                    exception.Result));
            }
            catch (SaveConflictException exception)
            {
                return ToConflictResult(exception);
            }
            catch (InvalidOperationException exception)
            {
                return ToValidationErrorResult(exception);
            }
        });

        app.MapPost("/api/meetup-groups/{slug}/archive", async (string slug, IMeetupGroupManagementService service, CancellationToken cancellationToken) =>
        {
            var response = await service.ArchiveAsync(slug, cancellationToken);
            return response.Success ? Results.Ok(response) : Results.BadRequest(response);
        });

        app.MapGet("/api/meetup-events", async (IMeetupEventManagementService service, CancellationToken cancellationToken) =>
        {
            var response = await service.ListAsync(cancellationToken);
            return Results.Ok(response);
        });

        app.MapGet("/api/meetup-events/{slug}", async (string slug, IMeetupEventManagementService service, CancellationToken cancellationToken) =>
        {
            var response = await service.GetBySlugAsync(slug, cancellationToken);
            return response is null ? Results.NotFound() : Results.Ok(response);
        });

        app.MapPost("/api/meetup-events", async (MeetupEventUpsertRequest request, IMeetupEventManagementService service, CancellationToken cancellationToken) =>
        {
            try
            {
                var response = await service.CreateAsync(request, cancellationToken);
                return Results.Ok(response);
            }
            catch (IntegrityGateException exception)
            {
                return Results.BadRequest(new IntegritySaveBlockedResponse(
                    "Save blocked by integrity validation findings.",
                    exception.Result));
            }
            catch (SaveConflictException exception)
            {
                return ToConflictResult(exception);
            }
            catch (InvalidOperationException exception)
            {
                return ToValidationErrorResult(exception);
            }
        });

        app.MapPut("/api/meetup-events/{slug}", async (string slug, MeetupEventUpsertRequest request, IMeetupEventManagementService service, CancellationToken cancellationToken) =>
        {
            try
            {
                var response = await service.UpdateAsync(slug, request, cancellationToken);
                return response is null ? Results.NotFound() : Results.Ok(response);
            }
            catch (IntegrityGateException exception)
            {
                return Results.BadRequest(new IntegritySaveBlockedResponse(
                    "Save blocked by integrity validation findings.",
                    exception.Result));
            }
            catch (SaveConflictException exception)
            {
                return ToConflictResult(exception);
            }
            catch (InvalidOperationException exception)
            {
                return ToValidationErrorResult(exception);
            }
        });

        app.MapPost("/api/meetup-events/{slug}/archive", async (string slug, IMeetupEventManagementService service, CancellationToken cancellationToken) =>
        {
            var response = await service.ArchiveAsync(slug, cancellationToken);
            return response.Success ? Results.Ok(response) : Results.BadRequest(response);
        });

        app.MapGet("/api/blog", async (IBlogManagementService service, CancellationToken cancellationToken) =>
        {
            var response = await service.ListAsync(cancellationToken);
            return Results.Ok(response);
        });

        app.MapGet("/api/blog/{slug}", async (string slug, IBlogManagementService service, CancellationToken cancellationToken) =>
        {
            var response = await service.GetBySlugAsync(slug, cancellationToken);
            return response is null ? Results.NotFound() : Results.Ok(response);
        });

        app.MapPost("/api/blog", async (BlogUpsertRequest request, IBlogManagementService service, CancellationToken cancellationToken) =>
        {
            try
            {
                var response = await service.CreateAsync(request, cancellationToken);
                return Results.Ok(response);
            }
            catch (IntegrityGateException exception)
            {
                return Results.BadRequest(new IntegritySaveBlockedResponse(
                    "Save blocked by integrity validation findings.",
                    exception.Result));
            }
            catch (SaveConflictException exception)
            {
                return ToConflictResult(exception);
            }
            catch (InvalidOperationException exception)
            {
                return ToValidationErrorResult(exception);
            }
        });

        app.MapPut("/api/blog/{slug}", async (string slug, BlogUpsertRequest request, IBlogManagementService service, CancellationToken cancellationToken) =>
        {
            try
            {
                var response = await service.UpdateAsync(slug, request, cancellationToken);
                return response is null ? Results.NotFound() : Results.Ok(response);
            }
            catch (IntegrityGateException exception)
            {
                return Results.BadRequest(new IntegritySaveBlockedResponse(
                    "Save blocked by integrity validation findings.",
                    exception.Result));
            }
            catch (SaveConflictException exception)
            {
                return ToConflictResult(exception);
            }
            catch (InvalidOperationException exception)
            {
                return ToValidationErrorResult(exception);
            }
        });

        app.MapGet("/api/about", async (IAboutManagementService service, CancellationToken cancellationToken) =>
        {
            var response = await service.GetAsync(cancellationToken);
            return Results.Ok(response);
        });

        app.MapPut("/api/about", async (AboutProfileUpsertRequest request, IAboutManagementService service, CancellationToken cancellationToken) =>
        {
            try
            {
                var response = await service.UpdateAsync(request, cancellationToken);
                return Results.Ok(response);
            }
            catch (IntegrityGateException exception)
            {
                return Results.BadRequest(new IntegritySaveBlockedResponse(
                    "Save blocked by integrity validation findings.",
                    exception.Result));
            }
            catch (SaveConflictException exception)
            {
                return ToConflictResult(exception);
            }
            catch (InvalidOperationException exception)
            {
                return ToValidationErrorResult(exception);
            }
        });

        app.MapGet("/api/media", async (string? folder, IMediaManagementService service, CancellationToken cancellationToken) =>
        {
            try
            {
                var response = await service.ListAsync(folder, cancellationToken);
                return Results.Ok(response);
            }
            catch (SaveConflictException exception)
            {
                return ToConflictResult(exception);
            }
            catch (InvalidOperationException exception)
            {
                return ToValidationErrorResult(exception);
            }
        });

        app.MapPost("/api/media/upload", async ([FromForm] IFormFile? file, [FromForm] string? folder, [FromForm] bool? overwrite, IMediaManagementService service, CancellationToken cancellationToken) =>
        {
            if (file is null)
            {
                return Results.BadRequest(new ApiErrorResponse("A media file is required.", Code: "missing_media_file"));
            }

            try
            {
                var response = await service.UploadAsync(file, folder, overwrite == true, cancellationToken);
                return response.Success ? Results.Ok(response) : Results.BadRequest(response);
            }
            catch (SaveConflictException exception)
            {
                return ToConflictResult(exception);
            }
            catch (InvalidOperationException exception)
            {
                return ToValidationErrorResult(exception);
            }
        }).DisableAntiforgery();

        app.MapPost("/api/media/replace", async ([FromForm] IFormFile? file, [FromForm] string relativePath, IMediaManagementService service, CancellationToken cancellationToken) =>
        {
            if (file is null)
            {
                return Results.BadRequest(new ApiErrorResponse("A media file is required.", Code: "missing_media_file"));
            }

            try
            {
                var response = await service.ReplaceAsync(relativePath, file, cancellationToken);
                return response.Success ? Results.Ok(response) : Results.BadRequest(response);
            }
            catch (SaveConflictException exception)
            {
                return ToConflictResult(exception);
            }
            catch (InvalidOperationException exception)
            {
                return ToValidationErrorResult(exception);
            }
        }).DisableAntiforgery();

        app.MapPost("/api/media/archive", async (MediaArchiveRequest request, IMediaManagementService service, CancellationToken cancellationToken) =>
        {
            var response = await service.ArchiveAsync(request.RelativePath, cancellationToken);
            return response.Success ? Results.Ok(response) : Results.BadRequest(response);
        });

        app.Run();
    }

    private static bool TryParseScanType(string value, out UtilityScanType scanType)
    {
        switch (value.Trim().ToLowerInvariant())
        {
            case "dead-links":
            case "deadlinks":
            case "dead_links":
                scanType = UtilityScanType.DeadLinks;
                return true;
            case "missing-images":
            case "missingimages":
            case "missing_images":
                scanType = UtilityScanType.MissingImages;
                return true;
            default:
                scanType = default;
                return false;
        }
    }

    private static IResult ToValidationErrorResult(InvalidOperationException exception)
        => Results.BadRequest(new ApiErrorResponse(exception.Message, Code: "validation_error"));

    private static IResult ToConflictResult(SaveConflictException exception)
        => Results.Conflict(new SaveConflictResponse(
            Message: "Save conflict detected. The file changed since you loaded it.",
            Code: "changed-since-load",
            FilePath: exception.FilePath.Replace('\\', '/'),
            ExpectedLastModifiedUtc: exception.ExpectedLastModifiedUtc,
            CurrentLastModifiedUtc: exception.CurrentLastModifiedUtc,
            Guidance: "Reload the editor to get the latest content, then reapply your changes and save again."));
}
