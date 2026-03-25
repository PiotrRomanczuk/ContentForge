using System.Net.Http.Headers;
using System.Text.Json;
using ContentForge.Domain.Entities;
using ContentForge.Domain.Enums;
using ContentForge.Domain.Interfaces.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ContentForge.Infrastructure.Services.Publishing;

// Implements IPlatformAdapter for Facebook using the Meta Graph API.
// Uses IHttpClientFactory (injected by DI) for connection pooling and Polly retry policies.
// Like an Axios instance configured with retry middleware in Node.js.
public class FacebookPlatformAdapter : IPlatformAdapter
{
    public Platform Platform => Platform.Facebook;

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly FacebookOptions _options;
    private readonly ILogger<FacebookPlatformAdapter> _logger;

    public FacebookPlatformAdapter(
        IHttpClientFactory httpClientFactory,
        IOptions<FacebookOptions> options,
        ILogger<FacebookPlatformAdapter> logger)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<PublishRecord> PublishAsync(
        ContentItem content, SocialAccount account,
        CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("Facebook");
        var pageId = GetPageId(account);
        var record = new PublishRecord
        {
            ContentItemId = content.Id,
            SocialAccountId = account.Id,
            Platform = Platform.Facebook,
            AttemptedAt = DateTime.UtcNow
        };

        try
        {
            HttpResponseMessage response;

            if (!string.IsNullOrEmpty(content.MediaPath) && File.Exists(content.MediaPath))
            {
                // Photo post: upload image + caption via multipart form data.
                // POST /{page-id}/photos — Graph API endpoint for photo publishing.
                response = await PublishPhotoPostAsync(
                    client, pageId, account.AccessToken,
                    content.MediaPath, content.TextContent, cancellationToken);
            }
            else
            {
                // Text-only post: POST /{page-id}/feed with message parameter.
                response = await PublishTextPostAsync(
                    client, pageId, account.AccessToken,
                    content.TextContent, cancellationToken);
            }

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            using var doc = JsonDocument.Parse(json);

            if (response.IsSuccessStatusCode)
            {
                record.ExternalPostId = doc.RootElement.GetProperty("id").GetString();
                record.IsSuccess = true;
                _logger.LogInformation(
                    "Published to Facebook: {PostId}", record.ExternalPostId);
            }
            else
            {
                var error = doc.RootElement.TryGetProperty("error", out var errObj)
                    ? errObj.GetProperty("message").GetString()
                    : json;
                record.ErrorMessage = error;
                _logger.LogWarning("Facebook publish failed: {Error}", error);
            }
        }
        catch (Exception ex)
        {
            record.ErrorMessage = ex.Message;
            _logger.LogError(ex, "Facebook publish exception for content {Id}", content.Id);
        }

        return record;
    }

    public async Task<ContentMetric?> FetchMetricsAsync(
        string externalPostId, SocialAccount account,
        CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("Facebook");
        // SECURITY: Pass token via Authorization header, never in URL query params.
        // URLs are logged by proxies, CDNs, and browsers — headers are not.
        var url = $"{_options.GraphApiBaseUrl}/{externalPostId}/insights"
            + "?metric=post_impressions,post_reach,post_reactions_like_total";

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", account.AccessToken);
            var response = await client.SendAsync(request, cancellationToken);
            var json = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to fetch metrics for {PostId}: {Json}",
                    externalPostId, json);
                return null;
            }

            using var doc = JsonDocument.Parse(json);
            var data = doc.RootElement.GetProperty("data");

            var metric = new ContentMetric
            {
                Platform = Platform.Facebook,
                ExternalPostId = externalPostId,
                CollectedAt = DateTime.UtcNow,
                RawData = new Dictionary<string, string> { ["response"] = json }
            };

            // Parse each metric from the insights response array
            foreach (var item in data.EnumerateArray())
            {
                var name = item.GetProperty("name").GetString();
                var value = item.GetProperty("values")[0].GetProperty("value").GetInt32();

                switch (name)
                {
                    case "post_impressions": metric.Impressions = value; break;
                    case "post_reach": metric.Reach = value; break;
                    case "post_reactions_like_total": metric.Likes = value; break;
                }
            }

            return metric;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching metrics for {PostId}", externalPostId);
            return null;
        }
    }

    public async Task<bool> ValidateAccountAsync(
        SocialAccount account, CancellationToken cancellationToken = default)
    {
        // Check token expiry first
        if (account.TokenExpiresAt.HasValue && account.TokenExpiresAt.Value < DateTime.UtcNow)
        {
            _logger.LogWarning("Facebook token expired for account '{Name}'", account.Name);
            return false;
        }

        // Validate token with Graph API: GET /me returns page info if token is valid.
        // SECURITY: Token sent via header, not URL query param.
        var client = _httpClientFactory.CreateClient("Facebook");
        var url = $"{_options.GraphApiBaseUrl}/me";

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", account.AccessToken);
            var response = await client.SendAsync(request, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    private async Task<HttpResponseMessage> PublishPhotoPostAsync(
        HttpClient client, string pageId, string accessToken,
        string imagePath, string caption, CancellationToken ct)
    {
        var url = $"{_options.GraphApiBaseUrl}/{pageId}/photos";

        // Multipart form: like FormData in JS — sends file + text fields in one request.
        using var form = new MultipartFormDataContent();
        var imageBytes = await File.ReadAllBytesAsync(imagePath, ct);
        var imageContent = new ByteArrayContent(imageBytes);
        imageContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        form.Add(imageContent, "source", Path.GetFileName(imagePath));
        form.Add(new StringContent(caption), "message");
        form.Add(new StringContent(accessToken), "access_token");

        return await client.PostAsync(url, form, ct);
    }

    private async Task<HttpResponseMessage> PublishTextPostAsync(
        HttpClient client, string pageId, string accessToken,
        string message, CancellationToken ct)
    {
        var url = $"{_options.GraphApiBaseUrl}/{pageId}/feed";

        // FormUrlEncodedContent = sends as application/x-www-form-urlencoded (like fetch with URLSearchParams).
        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["message"] = message,
            ["access_token"] = accessToken
        });

        return await client.PostAsync(url, content, ct);
    }

    // Gets the Facebook Page ID from the SocialAccount.Metadata dictionary.
    // The Page ID is stored during account setup (separate from the user's personal ID).
    private static string GetPageId(SocialAccount account)
    {
        if (account.Metadata.TryGetValue("page_id", out var pageId))
            return pageId;

        // Fallback: use ExternalId (which should be the page ID)
        return account.ExternalId;
    }
}
