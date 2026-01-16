using BIMformative.DynamoExtension.Models;
using BIMformative.DynamoExtension.Services.Auth;
using BIMformative.DynamoExtension.Services.Exceptions;
using BIMformative.DynamoExtension.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reflection.Metadata;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http.Headers;

namespace BIMformative.DynamoExtension.Services
{
    public class ScriptApiClient : IScriptApiClient
    {
        private readonly HttpClient _http;
        private readonly IAuthService _auth;

        public ScriptApiClient(HttpClient http, IAuthService auth)
        {
            _http = http ?? throw new ArgumentNullException(nameof(http));
            _auth = auth;
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

            var url = $"public/v1/scripts?{string.Join("&", query)}";

            return await GetAsync<PagedResponse<ScriptDto>>(url, cancellationToken);
        }

        public async Task<ScriptDto> GetScriptBySlugAsync(
            string slug,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(slug))
                throw new ArgumentException("Slug cannot be empty.", nameof(slug));

            return await GetAsync<ScriptDto>($"public/v1/scripts/{slug}", cancellationToken);
        }

        private async Task<T> GetAsync<T>(string url, CancellationToken ct)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);

            if (_auth.IsAuthenticated)
            {
                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", _auth.AccessToken);
            }

            try
            {
                using var response = await _http.GetAsync(url, ct);

                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException(
                        $"API returned {(int)response.StatusCode} - {response.ReasonPhrase}");
                }

                var result = await response.Content.ReadFromJsonAsync<T>(cancellationToken: ct);

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
                throw new ApiUnavailableException(
                    "Unexpected error while contacting BIMformative API.",
                    ex);
            }
            
        }
    }
}
