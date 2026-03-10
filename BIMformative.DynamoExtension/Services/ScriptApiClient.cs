using BIMformative.DynamoExtension.Models;
using BIMformative.DynamoExtension.Services.Auth;
using BIMformative.DynamoExtension.Services.Exceptions;
using BIMformative.DynamoExtension.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using BIMformative.DynamoExtension.Models.Scripts;

namespace BIMformative.DynamoExtension.Services
{
    public class ScriptApiClient : IScriptApiClient
    {
        private const string PublicApiRoot = "/api/public/v1";
        private const string AuthApiRoot = "/api/v1";
        
        private readonly HttpClient _authHttp;
        private readonly HttpClient _publicHttp;
        private readonly IAuthService _auth;

        public ScriptApiClient(HttpClient authHttp, HttpClient publicHttp, IAuthService auth)
        {
            _authHttp = authHttp ?? throw new ArgumentNullException(nameof(authHttp));
            _publicHttp = publicHttp ?? throw new ArgumentNullException(nameof(publicHttp));
            _auth = auth ?? throw new ArgumentNullException(nameof(auth));
        }

        // PUBLIC SEARCH
        public async Task<PagedResponse<ScriptDto>> GetPublicScriptsAsync(
            int page, 
            int limit, 
            string? search = null, 
            string? scriptType = null, 
            ScriptSortField sortField = ScriptSortField.updated_at, 
            SortOrder sortOrder = SortOrder.desc,
            CancellationToken cancellationToken = default)
        {
            var query = new List<string>
            {
                $"page={page}",
                $"limit={limit}",
                $"sort={sortField}",
                $"order={sortOrder}",
            };

            if (!string.IsNullOrWhiteSpace(search))
                query.Add($"search={Uri.EscapeDataString(search)}");

            if (!string.IsNullOrWhiteSpace(scriptType))
                query.Add($"type={scriptType}");

            var url = $"scripts?{string.Join("&", query)}";

            return await GetAsync<PagedResponse<ScriptDto>>(_publicHttp, url, cancellationToken);
        }

        public async Task<ScriptDetailsDto> GetScriptBySlugAsync(
            string slug,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(slug))
                throw new ArgumentException("Slug cannot be empty.", nameof(slug));

            var url = $"scripts/{slug}";

            // Use the generic GET helper
            var response = await GetAsync<ApiResponse<ScriptDetailsDto>>(_authHttp, url, cancellationToken);

            return response.Data
                ?? throw new InvalidOperationException($"Script not found for slug '{slug}'");
        }

        public async Task<IReadOnlyList<ScriptVersionDto>> GetScriptVersionsAsync(
            string slug,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(slug)) throw new ArgumentException("Slug cannot be empty.", nameof(slug));

            var url = $"scripts/{slug}/versions";

            // Use the generic GET helper
            var wrapper = await GetAsync<ApiListResponse<ScriptVersionDto>>(_authHttp, url, ct);

            if (wrapper?.Data == null)
                throw new InvalidOperationException($"Versions not found");

            return wrapper.Data;
        }

        private async Task<T> GetAsync<T>(HttpClient client, string relativeUrl, CancellationToken ct)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, relativeUrl);

            if (_auth.IsAuthenticated && !string.IsNullOrEmpty(_auth.AccessToken))
            {
                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", _auth.AccessToken);
            }

            try
            {
                using var response = await client.SendAsync(request, ct);

                if (!response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync(ct);
                    throw new HttpRequestException(
                        $"API returned {(int)response.StatusCode} - {response.ReasonPhrase}. Content: {content}");
                }

                var options = new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var result = await response.Content.ReadFromJsonAsync<T>(options, ct);

                if (result == null)
                    throw new InvalidOperationException("Empty response form API");

                return result;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (HttpRequestException ex)
            {
                throw new ApiUnavailableException("BIMformative server is unreachable.", ex);
            }
            catch (Exception ex)
            {
                throw new ApiUnavailableException("Unexpected error while contacting BIMformative API.", ex);
            }            
        }

        
    }
}
