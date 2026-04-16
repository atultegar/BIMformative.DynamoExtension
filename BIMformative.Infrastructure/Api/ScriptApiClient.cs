using BIMformative.Core.Interfaces;
using BIMformative.Core.Models;
using BIMformative.Core.Models.Api;
using BIMformative.Core.Models.Scripts;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BIMformative.Infrastructure.Api
{
    public class ScriptApiClient : IScriptApiClient
    {
        private readonly AuthApiClient _authApi;
        private readonly PublicApiClient _publicApi;

        private static readonly HttpMethod PatchMethod = new HttpMethod("PATCH");

        public ScriptApiClient(AuthApiClient authApi, PublicApiClient publicApi)
        {
            _authApi = authApi ?? throw new ArgumentNullException(nameof(authApi));
            _publicApi = publicApi ?? throw new ArgumentNullException(nameof(publicApi));
        }

        #region GET METHODS

        public async Task<PagedResponse<ScriptDto>> GetPublicAsync(
            int page, 
            int limit, 
            string search = null, 
            string scriptType = null, 
            ScriptSortField sortField = ScriptSortField.updated_at, 
            SortOrder sortOrder = SortOrder.desc, 
            CancellationToken ct = default)
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

            string url = $"scripts?{string.Join("&", query)}";

            return await _publicApi.SendAsync<PagedResponse<ScriptDto>>(HttpMethod.Get, url, ct);
        }

        public async Task<PagedResponse<MyScriptDto>> GetMyScriptsAsync(string search = null, string scriptType = null, CancellationToken ct = default)
        {
            var query = new List<string>();

            if (!string.IsNullOrWhiteSpace(search))
                query.Add($"search={Uri.EscapeDataString(search)}");

            if (!string.IsNullOrWhiteSpace(scriptType))
                query.Add($"type={scriptType}");

            //var url = $"scripts?{string.Join("&", query)}";
            string url = $"scripts";

            return await _authApi.SendAsync<PagedResponse<MyScriptDto>>(HttpMethod.Get, url, ct);
        }

        public async Task<ScriptDetailsDto> GetBySlugAsync(string slug, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(slug))
                throw new ArgumentException("Slug cannot be empty.", nameof(slug));

            string url = $"scripts/{slug}";

            return await _authApi.SendAsync<ScriptDetailsDto>(HttpMethod.Get, url, ct);
        }

        public async Task<IReadOnlyList<ScriptVersionDto>> GetVersionsAsync(string slug, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(slug)) 
                throw new ArgumentException("Slug cannot be empty.", nameof(slug));

            string url = $"scripts/{slug}/versions";

            // Use the generic GET helper
            return await _authApi.SendAsync<IReadOnlyList<ScriptVersionDto>>(HttpMethod.Get, url, ct);
        }

        public async Task<RemoteScriptInfo> GetLatestInfoAsync(string slug, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(slug))
                throw new ArgumentException("Slug cannot be empty.", nameof(slug));

            string url = $"scripts/{slug}/latest-info";

            return await _authApi.SendAsync<RemoteScriptInfo>(HttpMethod.Get, url, ct);            
        }

        public async Task<bool> HasLikedAsync(string slug, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(slug))
                throw new ArgumentException("Slug cannot be empty.", nameof(slug));

            string url = $"scripts/{slug}/likes/me";

            return await _authApi.SendAsync<bool>(HttpMethod.Get, url, ct);
        }

        public async Task<HttpResponseMessage> DownloadAsync(string slug, CancellationToken ct = default) //TODO : REVISE
        {
            if (string.IsNullOrWhiteSpace(slug))
                throw new ArgumentException("Slug cannot be empty.", nameof(slug));

            string url = $"scripts/{slug}/download";

            return await _authApi.SendRawAsync(HttpMethod.Get, url, ct);
        }

        #endregion

        #region POST METHODS

        public async Task<ScriptAnalyzeResponseDto> AnalyzeAsync(MultipartFormDataContent content, CancellationToken ct = default)
        {
            if (content == null) 
                throw new ArgumentNullException(nameof(content));

            string url = "scripts/analyze";

            return await _authApi.SendAsync<ScriptAnalyzeResponseDto>(
                HttpMethod.Post, 
                url, 
                ct,
                content);
        }

        public async Task<ScriptPublishResponse> PublishAsync(ScriptPublishRequestDto request, CancellationToken ct = default)
        {
            var payload = new
            {
                storagePath = request.StoragePath,
                title = request.Title,
                description = request.Description,
                scriptType = request.ScriptType,
                demoLink = request.DemoLink,
                isPublic = request.IsPublic,
                tags = request.Tags ?? Array.Empty<string>(),
                parsedJson = string.IsNullOrWhiteSpace(request.ParsedJson)
                ? null
                : JsonConvert.DeserializeObject<object>(request.ParsedJson)
            };

            var json = JsonConvert.SerializeObject(payload);

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            string url = "scripts";

            return await _authApi.SendAsync<ScriptPublishResponse>(HttpMethod.Post, url, ct, content);
        }

        public async Task<ScriptVersionDto> PublishVersionAsync(string slug, ScriptAnalyzeResponseDto parsed, string changeLog, CancellationToken ct = default)
        {
            string url = $"scripts/{slug}/versions";
            var payload = new
            {
                storagePath = parsed.StoragePath,
                parsedJson = parsed.ScriptData,
                changeLog = changeLog
            };

            var json = JsonConvert.SerializeObject(payload);

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            return await _authApi.SendAsync<ScriptVersionDto>(HttpMethod.Post, url, ct, content);
        }

        public async Task<SetCurrentVersionResponse> SetCurrentVersionAsync(string slug, int versionNumber, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(slug))
                throw new ArgumentException("Slug cannot be empty", nameof(slug));

            if (versionNumber < 1)
                throw new ArgumentException("Version number must be greater than 0", nameof(versionNumber));

            var url = $"scripts/{slug}/versions/{versionNumber}/set-current";

            return await _authApi.SendAsync<SetCurrentVersionResponse>(HttpMethod.Post, url, ct);            
        }

        public async Task<ScriptLikesDto> LikeAsync(string slug, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(slug))
                throw new ArgumentException("Slug cannot be empty.", nameof(slug));

            var url = $"scripts/{slug}/likes";

            return await _authApi.SendAsync<ScriptLikesDto>(HttpMethod.Post, url, ct);
        }

        public async Task<string> GetHashAsync(string scriptJson, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(scriptJson))
                throw new ArgumentException("Slug cannot be empty.", nameof(scriptJson));

            var payload = new
            {
                scriptContent = scriptJson,
            };

            var json = JsonConvert.SerializeObject(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            return await _authApi.SendAsync<string>(HttpMethod.Post, "hash", ct, content);
        }

        #endregion

        #region PATCH METHODS

        public async Task<ScriptDetailsDto> UpdateScriptMetadataAsync(string slug, ScriptUpdateRequest scriptUpdateRequest, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(slug))
                throw new ArgumentException("Slug cannot be empty", nameof(slug));

            if (scriptUpdateRequest == null)
                throw new ArgumentNullException(nameof(scriptUpdateRequest));

            var url = $"scripts/{slug}";
            var json = JsonConvert.SerializeObject(scriptUpdateRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            return await _authApi.SendAsync<ScriptDetailsDto>(PatchMethod, url, ct, content);
        }

        public async Task<UpdateScriptVisibilityResponse> UpdateScriptVisibilityAsync(string slug, bool isPublic, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(slug))
                throw new ArgumentNullException("Slug cannot be empty", nameof(slug));

            var url = $"scripts/{slug}/visibility";

            var payload = new
            {
                isPublic
            };

            var json = JsonConvert.SerializeObject(payload);

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            return await _authApi.SendAsync<UpdateScriptVisibilityResponse>(PatchMethod, url, ct, content);
        }

        #endregion

        #region DELETE METHODS

        public async Task<string> DeleteScriptAsync(string slug, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(slug))
                throw new ArgumentException("Slug cannot be empty.", nameof(slug));

            var url = $"scripts/{slug}";

            return await _authApi.SendAsync<string>(HttpMethod.Delete, url, ct);
        }

        public async Task<string> DeleteVersionAsync(string slug, int versionNumber, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(slug))
                throw new ArgumentException("Slug cannot be empty", nameof(slug));

            if (versionNumber < 1)
                throw new ArgumentException("Version number must be greater than 0", nameof(versionNumber));

            var url = $"scripts/{slug}/versions/{versionNumber}";

            return await _authApi.SendAsync<string>(HttpMethod.Delete, url, ct);
        }

        public async Task<ScriptLikesDto> UnlikeAsync(string slug, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(slug))
                throw new ArgumentException("Slug cannot be empty.", nameof(slug));

            var url = $"scripts/{slug}/likes";

            return await _authApi.SendAsync<ScriptLikesDto>(HttpMethod.Delete, url, ct);
        }

        #endregion
    }
}
