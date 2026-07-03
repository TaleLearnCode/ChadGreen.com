using ChadGreen.Management.Shared.Contracts;
using Microsoft.AspNetCore.Components.Forms;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ChadGreen.Management.Client.Services;

public sealed class ManagementApiHttpClient(HttpClient httpClient)
{
    private static readonly JsonSerializerOptions ApiSerializerOptions = CreateApiSerializerOptions();

    public HttpClient HttpClient { get; } = httpClient;

    public async Task<IReadOnlyList<PresentationListItemDto>> GetPresentationsAsync(CancellationToken cancellationToken = default)
        => await HttpClient.GetFromJsonAsync<List<PresentationListItemDto>>("/api/presentations", cancellationToken) ?? [];

    public async Task<IReadOnlyList<PresentationOptionDto>> GetPresentationOptionsAsync(CancellationToken cancellationToken = default)
        => await HttpClient.GetFromJsonAsync<List<PresentationOptionDto>>("/api/presentations/options", cancellationToken) ?? [];

    public Task<PresentationDetailDto?> GetPresentationAsync(string slug, CancellationToken cancellationToken = default)
        => HttpClient.GetFromJsonAsync<PresentationDetailDto>($"/api/presentations/{Uri.EscapeDataString(slug)}", cancellationToken);

    public Task<PresentationDetailDto> CreatePresentationAsync(PresentationUpsertRequest request, CancellationToken cancellationToken = default)
        => SendAsync<PresentationUpsertRequest, PresentationDetailDto>(HttpMethod.Post, "/api/presentations", request, cancellationToken);

    public Task<PresentationDetailDto> UpdatePresentationAsync(string slug, PresentationUpsertRequest request, CancellationToken cancellationToken = default)
        => SendAsync<PresentationUpsertRequest, PresentationDetailDto>(HttpMethod.Put, $"/api/presentations/{Uri.EscapeDataString(slug)}", request, cancellationToken);

    public Task<PresentationDetailDto> UpdatePresentationStatusAsync(string slug, string status, CancellationToken cancellationToken = default)
        => SendAsync<PresentationStatusUpdateRequest, PresentationDetailDto>(HttpMethod.Put, $"/api/presentations/{Uri.EscapeDataString(slug)}/status", new PresentationStatusUpdateRequest(status), cancellationToken);

    public Task<PresentationDetailDto> UpdatePresentationFeaturedAsync(string slug, bool featured, CancellationToken cancellationToken = default)
        => SendAsync<PresentationFeaturedUpdateRequest, PresentationDetailDto>(HttpMethod.Put, $"/api/presentations/{Uri.EscapeDataString(slug)}/featured", new PresentationFeaturedUpdateRequest(featured), cancellationToken);

    public Task<ArchiveOperationResponse> ArchivePresentationAsync(string slug, CancellationToken cancellationToken = default)
        => SendAsync<object, ArchiveOperationResponse>(HttpMethod.Post, $"/api/presentations/{Uri.EscapeDataString(slug)}/archive", new { }, cancellationToken);

    public async Task<IReadOnlyList<EngagementListItemDto>> GetEngagementsAsync(CancellationToken cancellationToken = default)
        => await HttpClient.GetFromJsonAsync<List<EngagementListItemDto>>("/api/engagements", cancellationToken) ?? [];

    public Task<EngagementDetailDto?> GetEngagementAsync(string slug, CancellationToken cancellationToken = default)
        => HttpClient.GetFromJsonAsync<EngagementDetailDto>($"/api/engagements/{Uri.EscapeDataString(slug)}", cancellationToken);

    public Task<EngagementDetailDto> CreateEngagementAsync(EngagementUpsertRequest request, CancellationToken cancellationToken = default)
        => SendAsync<EngagementUpsertRequest, EngagementDetailDto>(HttpMethod.Post, "/api/engagements", request, cancellationToken);

    public Task<EngagementDetailDto> UpdateEngagementAsync(string slug, EngagementUpsertRequest request, CancellationToken cancellationToken = default)
        => SendAsync<EngagementUpsertRequest, EngagementDetailDto>(HttpMethod.Put, $"/api/engagements/{Uri.EscapeDataString(slug)}", request, cancellationToken);

    public Task<EngagementDetailDto> UpdateEngagementFeaturedAsync(string slug, bool featured, CancellationToken cancellationToken = default)
        => SendAsync<EngagementFeaturedUpdateRequest, EngagementDetailDto>(HttpMethod.Put, $"/api/engagements/{Uri.EscapeDataString(slug)}/featured", new EngagementFeaturedUpdateRequest(featured), cancellationToken);

    public Task<ArchiveOperationResponse> ArchiveEngagementAsync(string slug, CancellationToken cancellationToken = default)
        => SendAsync<object, ArchiveOperationResponse>(HttpMethod.Post, $"/api/engagements/{Uri.EscapeDataString(slug)}/archive", new { }, cancellationToken);

    public async Task<IReadOnlyList<MeetupGroupListItemDto>> GetMeetupGroupsAsync(CancellationToken cancellationToken = default)
        => await HttpClient.GetFromJsonAsync<List<MeetupGroupListItemDto>>("/api/meetup-groups", cancellationToken) ?? [];

    public Task<MeetupGroupDetailDto?> GetMeetupGroupAsync(string slug, CancellationToken cancellationToken = default)
        => HttpClient.GetFromJsonAsync<MeetupGroupDetailDto>($"/api/meetup-groups/{Uri.EscapeDataString(slug)}", cancellationToken);

    public Task<MeetupGroupDetailDto> CreateMeetupGroupAsync(MeetupGroupUpsertRequest request, CancellationToken cancellationToken = default)
        => SendAsync<MeetupGroupUpsertRequest, MeetupGroupDetailDto>(HttpMethod.Post, "/api/meetup-groups", request, cancellationToken);

    public Task<MeetupGroupDetailDto> UpdateMeetupGroupAsync(string slug, MeetupGroupUpsertRequest request, CancellationToken cancellationToken = default)
        => SendAsync<MeetupGroupUpsertRequest, MeetupGroupDetailDto>(HttpMethod.Put, $"/api/meetup-groups/{Uri.EscapeDataString(slug)}", request, cancellationToken);

    public Task<ArchiveOperationResponse> ArchiveMeetupGroupAsync(string slug, CancellationToken cancellationToken = default)
        => SendAsync<object, ArchiveOperationResponse>(HttpMethod.Post, $"/api/meetup-groups/{Uri.EscapeDataString(slug)}/archive", new { }, cancellationToken);

    public async Task<IReadOnlyList<MeetupEventListItemDto>> GetMeetupEventsAsync(CancellationToken cancellationToken = default)
        => await HttpClient.GetFromJsonAsync<List<MeetupEventListItemDto>>("/api/meetup-events", cancellationToken) ?? [];

    public Task<MeetupEventDetailDto?> GetMeetupEventAsync(string slug, CancellationToken cancellationToken = default)
        => HttpClient.GetFromJsonAsync<MeetupEventDetailDto>($"/api/meetup-events/{Uri.EscapeDataString(slug)}", cancellationToken);

    public Task<MeetupEventDetailDto> CreateMeetupEventAsync(MeetupEventUpsertRequest request, CancellationToken cancellationToken = default)
        => SendAsync<MeetupEventUpsertRequest, MeetupEventDetailDto>(HttpMethod.Post, "/api/meetup-events", request, cancellationToken);

    public Task<MeetupEventDetailDto> UpdateMeetupEventAsync(string slug, MeetupEventUpsertRequest request, CancellationToken cancellationToken = default)
        => SendAsync<MeetupEventUpsertRequest, MeetupEventDetailDto>(HttpMethod.Put, $"/api/meetup-events/{Uri.EscapeDataString(slug)}", request, cancellationToken);

    public Task<ArchiveOperationResponse> ArchiveMeetupEventAsync(string slug, CancellationToken cancellationToken = default)
        => SendAsync<object, ArchiveOperationResponse>(HttpMethod.Post, $"/api/meetup-events/{Uri.EscapeDataString(slug)}/archive", new { }, cancellationToken);

    public async Task<IReadOnlyList<BlogListItemDto>> GetBlogPostsAsync(CancellationToken cancellationToken = default)
        => await HttpClient.GetFromJsonAsync<List<BlogListItemDto>>("/api/blog", cancellationToken) ?? [];

    public Task<BlogDetailDto?> GetBlogPostAsync(string slug, CancellationToken cancellationToken = default)
        => HttpClient.GetFromJsonAsync<BlogDetailDto>($"/api/blog/{Uri.EscapeDataString(slug)}", cancellationToken);

    public Task<BlogDetailDto> CreateBlogPostAsync(BlogUpsertRequest request, CancellationToken cancellationToken = default)
        => SendAsync<BlogUpsertRequest, BlogDetailDto>(HttpMethod.Post, "/api/blog", request, cancellationToken);

    public Task<BlogDetailDto> UpdateBlogPostAsync(string slug, BlogUpsertRequest request, CancellationToken cancellationToken = default)
        => SendAsync<BlogUpsertRequest, BlogDetailDto>(HttpMethod.Put, $"/api/blog/{Uri.EscapeDataString(slug)}", request, cancellationToken);

    public Task<AboutProfileDetailDto?> GetAboutProfileAsync(CancellationToken cancellationToken = default)
        => HttpClient.GetFromJsonAsync<AboutProfileDetailDto>("/api/about", cancellationToken);

    public Task<AboutProfileDetailDto> UpdateAboutProfileAsync(AboutProfileUpsertRequest request, CancellationToken cancellationToken = default)
        => SendAsync<AboutProfileUpsertRequest, AboutProfileDetailDto>(HttpMethod.Put, "/api/about", request, cancellationToken);

    public async Task<IReadOnlyList<MediaItemDto>> GetMediaAsync(string? folder = null, CancellationToken cancellationToken = default)
    {
        var path = string.IsNullOrWhiteSpace(folder)
            ? "/api/media"
            : $"/api/media?folder={Uri.EscapeDataString(folder.Trim())}";
        var response = await HttpClient.GetFromJsonAsync<MediaListResponse>(path, ApiSerializerOptions, cancellationToken);
        return response?.Items ?? [];
    }

    public Task<MediaUploadResponse> UploadMediaAsync(IBrowserFile file, string? folder, bool overwrite = false, CancellationToken cancellationToken = default)
        => SendMultipartAsync("/api/media/upload", file, folder, relativePath: null, overwrite, cancellationToken);

    public Task<MediaUploadResponse> ReplaceMediaAsync(string relativePath, IBrowserFile file, CancellationToken cancellationToken = default)
        => SendMultipartAsync("/api/media/replace", file, folder: null, relativePath, overwrite: false, cancellationToken);

    public Task<ArchiveOperationResponse> ArchiveMediaAsync(string relativePath, CancellationToken cancellationToken = default)
        => SendAsync<MediaArchiveRequest, ArchiveOperationResponse>(HttpMethod.Post, "/api/media/archive", new MediaArchiveRequest(relativePath), cancellationToken);

    public Task<GitCapabilityResponse?> GetGitCapabilityAsync(CancellationToken cancellationToken = default)
        => HttpClient.GetFromJsonAsync<GitCapabilityResponse>("/api/git/capability", cancellationToken);

    public Task<CommitResponse> CommitAsync(CommitRequest request, CancellationToken cancellationToken = default)
        => SendAsync<CommitRequest, CommitResponse>(HttpMethod.Post, "/api/git/commit", request, cancellationToken);

    public Task<UtilityDashboardResponse?> GetUtilityDashboardAsync(CancellationToken cancellationToken = default)
        => HttpClient.GetFromJsonAsync<UtilityDashboardResponse>("/api/utilities/dashboard", ApiSerializerOptions, cancellationToken);

    public async Task<UtilityScanResult?> GetLatestUtilityScanAsync(string scanType, CancellationToken cancellationToken = default)
    {
        var path = $"/api/utilities/scans/{Uri.EscapeDataString(scanType)}/latest";
        using var response = await HttpClient.GetAsync(path, cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        if (!response.IsSuccessStatusCode)
        {
            var message = await TryReadErrorMessageAsync(response, cancellationToken);
            throw new InvalidOperationException(message);
        }

        return await response.Content.ReadFromJsonAsync<UtilityScanResult>(ApiSerializerOptions, cancellationToken);
    }

    public Task<UtilityScanResult> RunUtilityScanAsync(string scanType, CancellationToken cancellationToken = default)
        => SendAsync<object, UtilityScanResult>(HttpMethod.Post, $"/api/utilities/scans/{Uri.EscapeDataString(scanType)}/run", new { }, cancellationToken);

    private async Task<TResponse> SendAsync<TRequest, TResponse>(HttpMethod method, string path, TRequest payload, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(method, path)
        {
            Content = JsonContent.Create(payload)
        };

        using var response = await HttpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var message = await TryReadErrorMessageAsync(response, cancellationToken);
            throw new InvalidOperationException(message);
        }

        var result = await response.Content.ReadFromJsonAsync<TResponse>(ApiSerializerOptions, cancellationToken);
        return result ?? throw new InvalidOperationException("The API returned an empty response.");
    }

    private async Task<MediaUploadResponse> SendMultipartAsync(
        string path,
        IBrowserFile file,
        string? folder,
        string? relativePath,
        bool overwrite,
        CancellationToken cancellationToken)
    {
        var content = new MultipartFormDataContent();
        await using var stream = file.OpenReadStream(maxAllowedSize: 5 * 1024 * 1024, cancellationToken);
        var streamContent = new StreamContent(stream);
        streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
        content.Add(streamContent, "file", file.Name);

        if (!string.IsNullOrWhiteSpace(folder))
        {
            content.Add(new StringContent(folder.Trim()), "folder");
        }

        if (!string.IsNullOrWhiteSpace(relativePath))
        {
            content.Add(new StringContent(relativePath.Trim()), "relativePath");
        }

        if (overwrite)
        {
            content.Add(new StringContent("true"), "overwrite");
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, path)
        {
            Content = content
        };

        using var response = await HttpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var message = await TryReadErrorMessageAsync(response, cancellationToken);
            throw new InvalidOperationException(message);
        }

        var result = await response.Content.ReadFromJsonAsync<MediaUploadResponse>(ApiSerializerOptions, cancellationToken);
        return result ?? throw new InvalidOperationException("The API returned an empty response.");
    }

    private static JsonSerializerOptions CreateApiSerializerOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }

    private static async Task<string> TryReadErrorMessageAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(content))
        {
            return $"API request failed with status {(int)response.StatusCode}.";
        }

        try
        {
            using var document = JsonDocument.Parse(content);
            if (document.RootElement.TryGetProperty("message", out var messageElement))
            {
                var message = messageElement.GetString() ?? content;
                var guidance = document.RootElement.TryGetProperty("guidance", out var guidanceElement)
                    ? guidanceElement.GetString()
                    : null;
                var code = document.RootElement.TryGetProperty("code", out var codeElement)
                    ? codeElement.GetString()
                    : null;

                if (!string.IsNullOrWhiteSpace(guidance))
                {
                    message = $"{message} {guidance}";
                }

                if (!string.IsNullOrWhiteSpace(code))
                {
                    message = $"{message} [{code}]";
                }

                return message;
            }
        }
        catch (JsonException)
        {
            return content;
        }

        return content;
    }
}
