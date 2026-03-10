using BIMformative.DynamoExtension.Models;
using BIMformative.DynamoExtension.Models.Scripts;
using BIMformative.DynamoExtension.Services.Auth;
using BIMformative.DynamoExtension.Services.Exceptions;
using BIMformative.DynamoExtension.Services.Interfaces;
using BIMformative.DynamoExtension.Services.Settings;
using Dynamo.Wpf.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace BIMformative.DynamoExtension.Services.Script
{
    public sealed class ScriptService : IScriptService
    {
        private readonly HttpClient _authHttp;
        private readonly HttpClient _publicHttp;
        private readonly IAuthService _auth;
        private readonly ISettingsService _settings;
        private readonly IFileOverwritePrompt _overwritePrompt;
        private readonly IDynamoContext _dynamo;

        public ScriptService(
            IDynamoContext dynamo,
            HttpClient authHttp, 
            HttpClient publicHttp, 
            IAuthService auth, 
            ISettingsService settings, 
            IFileOverwritePrompt overwritePrompt)
        {
            _dynamo = dynamo;
            _authHttp = authHttp;
            _publicHttp = publicHttp;
            _auth = auth;
            _settings = settings;
            _overwritePrompt = overwritePrompt;
        }

        public async Task<ScriptAnalyzeResponseDto> AnalyzeAsync(string filePath, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException("File path is required", nameof(filePath));

            if (!File.Exists(filePath))
                throw new FileNotFoundException("Script file not found", filePath);

            using var form = new MultipartFormDataContent();

            await using var fileStream = File.OpenRead(filePath);
            var fileContent = new StreamContent(fileStream);
            fileContent.Headers.ContentType =
                new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");

            fileContent.Headers.ContentDisposition =
                new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data")
                {
                    Name = "\"file\"",
                    FileName = $"\"{Path.GetFileName(filePath)}\""
                };

            form.Add(fileContent);

            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                "scripts/analyze")
            {
                Content = form
            };

            if (_auth.IsAuthenticated)
            {
                request.Headers.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue(
                        "Bearer",
                        _auth.AccessToken);
            }

            request.Headers.ExpectContinue = false;
            using var response = await _authHttp.SendAsync(request, ct);

            var json = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"Analyze failed ({response.StatusCode}): {json}");
            }

            var options = new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var result = JsonSerializer.Deserialize<ScriptAnalyzeResponseDto>(json, options);

            return result!;
        }

        public async Task<ScriptAnalyzeResponseDto> AnalyzeWorkspaceAsync(CancellationToken ct = default)
        {
            var model = _dynamo.Model;
            var vm = _dynamo.ViewModel;

            var workspace = model.CurrentWorkspace
                ?? throw new InvalidOperationException("No active workspace");

            ct.ThrowIfCancellationRequested();


            // Handle unsaved changes on UI thread
            if (workspace.HasUnsavedChanges)
            {
                bool canContinue = false;

                await _dynamo.Window.Dispatcher.InvokeAsync(() =>
                {
                    canContinue = vm.AskUserToSaveWorkspaceOrCancel(model.CurrentWorkspace);

                });

                if (!canContinue)
                    throw new OperationCanceledException("User cancelled save operation.");
            }

            // Ensure file is saved
            if (string.IsNullOrWhiteSpace(workspace.FileName))
                throw new InvalidOperationException("Workspace must be saved before analyzing.");

            ct.ThrowIfCancellationRequested();

            // Delegate to AnalyzeAsync
            return await AnalyzeAsync(workspace.FileName, ct);
        }

        public Task<string> DeleteAsync(string slug, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public async Task<string> DownloadAsync(
            ScriptDto script, 
            CancellationToken ct = default)
        {
            if (script == null) 
                throw new ArgumentNullException(nameof(script));

            if (!_auth.IsAuthenticated || string.IsNullOrEmpty(_auth.AccessToken))
                throw new InvalidOperationException("User must be authenticated to download.");

            var url = $"scripts/{script.Slug}/download";

            using var request = new HttpRequestMessage(HttpMethod.Get, url);

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", _auth.AccessToken);

            using var response = await _authHttp.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                ct);

            response.EnsureSuccessStatusCode();

            var filename = ResolveFileName(response, script);
            var targetDir = ResolveTargetDirectory();
            var filePath = Path.Combine(targetDir, filename);

            // Handle overwrite logic
            if (File.Exists(filePath))
            {
                if (_settings.Current.AskBeforeOverwrite)
                {
                    var decision = _overwritePrompt.Ask(filePath);

                    switch (decision)
                    {
                        case OverwriteDecision.Overwrite:
                            break;

                        case OverwriteDecision.SaveAs:
                            var newPath = _overwritePrompt.ShowSaveAs(filePath);
                            if (string.IsNullOrEmpty(newPath))
                                throw new OperationCanceledException();

                            filePath = newPath;
                            break;

                        case OverwriteDecision.Cancel:
                            throw new OperationCanceledException();
                    }
                }
            }

            await using var stream = await response.Content.ReadAsStreamAsync(ct);
            await using var file = File.Create(filePath);
            await stream.CopyToAsync(file, ct);

            return filePath;
        }

        public async Task<string> DownloadLatestAsync(DownloadedScript script, CancellationToken ct = default)
        {
            if (script == null)
                throw new ArgumentNullException(nameof(script));

            if (!_auth.IsAuthenticated || string.IsNullOrEmpty(_auth.AccessToken))
                throw new InvalidOperationException("User must be authenticated to download.");

            var url = $"scripts/{script.Slug}/download";

            using var request = new HttpRequestMessage(HttpMethod.Get, url);

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", _auth.AccessToken);

            using var response = await _authHttp.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                ct);

            response.EnsureSuccessStatusCode();
                        
            var filePath = script.LocalPath;

            // Handle overwrite logic
            if (File.Exists(filePath))
            {
                if (_settings.Current.AskBeforeOverwrite)
                {
                    var decision = _overwritePrompt.Ask(filePath);

                    switch (decision)
                    {
                        case OverwriteDecision.Overwrite:
                            break;

                        case OverwriteDecision.SaveAs:
                            var newPath = _overwritePrompt.ShowSaveAs(filePath);
                            if (string.IsNullOrEmpty(newPath))
                                throw new OperationCanceledException();

                            filePath = newPath;
                            break;

                        case OverwriteDecision.Cancel:
                            throw new OperationCanceledException();
                    }
                }
            }

            await using var stream = await response.Content.ReadAsStreamAsync(ct);
            await using var file = File.Create(filePath);
            await stream.CopyToAsync(file, ct);

            return filePath;
        }

        public async Task<ScriptDetailsDto> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(slug))
                throw new ArgumentException("Slug cannot be empty.", nameof(slug));

            var url = $"scripts/{slug}";

            // Use the generic GET helper
            var response = await GetAsync<ApiResponse<ScriptDetailsDto>>(_authHttp, url, cancellationToken);

            return response.Data
                ?? throw new InvalidOperationException($"Script not found for slug '{slug}'");
        }

        public async Task<string> GetHashAsync(string scriptJson, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(scriptJson))
                throw new ArgumentException("Slug cannot be empty.", nameof(scriptJson));

            var payload = new
            {
                scriptContent = scriptJson,
            };

            var response = await PostJsonAsync<HashResponseDto>(_authHttp, "hash", payload, ct);

            if (string.IsNullOrWhiteSpace(response.Hash))
                throw new InvalidOperationException("Hash calculation error for the script.");

            return response.Hash;
        }

        public async Task<RemoteScriptInfo?> GetLatestInfoAsync(string slug, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(slug))
                throw new ArgumentException("Slug cannot be empty.", nameof(slug));

            var url = $"scripts/{slug}/latest-info";

            var response = await GetAsync<ApiResponse<RemoteScriptInfo>>(_authHttp, url, ct);

            return response.Data ?? throw new InvalidOperationException($"Script not found for slug '{slug}'");
        }

        public async Task<string> GetLatestVersionAsync(string slug, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(slug))
                throw new ArgumentException("Slug cannot be empty.", nameof(slug));

            var url = $"scripts/{slug}";

            // Use the generic GET helper
            var response = await GetAsync<ApiResponse<ScriptDetailsDto>>(_authHttp, url, cancellationToken);

            var currentVersion = response?.Data?.Current_Version_Number;

            if (currentVersion == null) throw new InvalidOperationException($"Script not found for slug '{slug}'");

            return $"V{currentVersion.ToString()}"; 
        }

        public async Task<IReadOnlyList<MyScriptDto>> GetMyScriptsAsync(string? search = null, string? scriptType = null, CancellationToken cancellationToken = default)
        {
            var query = new List<string>();

            if (!string.IsNullOrWhiteSpace(search))
                query.Add($"search={Uri.EscapeDataString(search)}");

            if (!string.IsNullOrWhiteSpace(scriptType))
                query.Add($"type={scriptType}");

            //var url = $"scripts?{string.Join("&", query)}";
            var url = $"scripts";

            var wrapper = await GetAsync<IReadOnlyList<MyScriptDto>>(_authHttp, url, cancellationToken);

            if (wrapper == null) throw new InvalidOperationException($"MyScripts not found");

            return wrapper;
        }

        public async Task<PagedResponse<ScriptDto>> GetPublicAsync(
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


        public async Task<IReadOnlyList<ScriptVersionDto>> GetVersionsAsync(string slug, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(slug)) throw new ArgumentException("Slug cannot be empty.", nameof(slug));

            var url = $"scripts/{slug}/versions";

            // Use the generic GET helper
            var wrapper = await GetAsync<ApiListResponse<ScriptVersionDto>>(_authHttp, url, ct);

            if (wrapper?.Data == null)
                throw new InvalidOperationException($"Versions not found");

            return wrapper.Data;
        }

        public async Task<bool> PublishAsync(ScriptPublishRequestDto request, IProgress<double>? progress = null, CancellationToken ct = default)
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
                : JsonSerializer.Deserialize<object>(request.ParsedJson)
            };

            var json = JsonSerializer.Serialize(
                payload,
                new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    DefaultIgnoreCondition =
                    System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                });

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "scripts")
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            if (_auth.IsAuthenticated)
            {
                httpRequest.Headers.Authorization =
                    new AuthenticationHeaderValue(
                        "Bearer",
                        _auth.AccessToken);
            }

            progress?.Report(0.1);

            using var response = await _authHttp.SendAsync(httpRequest, ct);

            progress?.Report(0.8);

            var responseBody = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Publish failed ({response.StatusCode}): {responseBody}");
            }

            response.EnsureSuccessStatusCode();
            progress?.Report(1.0);

            return true;
        }

        public Task<string> PublishVersionAsync(string slug, string version, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<string> UpdateMetadataAsync(string slug, string metadata, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public async Task<ScriptLikesDto> LikeAsync(string slug, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(slug))
                throw new ArgumentException("Slug cannot be empty.", nameof(slug));

            var url = $"scripts/{slug}/likes";

            var response = await PostAsync<ApiResponse<ScriptLikesDto>>(_authHttp, url, ct);

            if (response?.Data == null)
                throw new InvalidOperationException("Error liking script");

            return response.Data;
        }

        public async Task<ScriptLikesDto> UnlikeAsync(string slug, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(slug))
                throw new ArgumentException("Slug cannot be empty.", nameof(slug));

            var url = $"scripts/{slug}/likes";

            var response = await DeleteAsync<ApiResponse<ScriptLikesDto>>(_authHttp, url, ct);

            if (response?.Data == null)
                throw new InvalidOperationException("Error liking script");

            return response.Data;
        }

        public async Task<bool> HasLiked(string slug, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(slug))
                throw new ArgumentException("Slug cannot be empty.", nameof(slug));

            var url = $"scripts/{slug}/likes/me";

            // Use the generic GET helper
            var wrapper = await GetAsync<ApiResponse<bool>>(_authHttp, url, ct);

            if (wrapper?.Data == null)
                throw new InvalidOperationException($"Like not found");

            return wrapper.Data;
        }

        #region GENERIC METHODS
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
                using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);

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
        

        private async Task<T> PostAsync<T>(HttpClient client, string relativeUrl, CancellationToken ct, HttpContent? content = null )
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, relativeUrl)
            {
                Content = content
            };

            if (_auth.IsAuthenticated && !string.IsNullOrEmpty(_auth.AccessToken))
            {
                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", _auth.AccessToken);
            }

            try
            {
                using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);

                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync(ct);

                    throw new HttpRequestException(
                        $"API returned {(int)response.StatusCode} - {response.ReasonPhrase}. Content: {errorBody}");
                }

                await using var stream = await response.Content.ReadAsStreamAsync(ct);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var result = await JsonSerializer.DeserializeAsync<T>(stream, options, ct);

                if (result is null)
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

        private async Task<T> PostJsonAsync<T>(HttpClient client, string relativeUrl, object payload, CancellationToken ct)
        {
            var json = JsonSerializer.Serialize(payload);

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            return await PostAsync<T>(client, relativeUrl, ct, content);
        }

        private async Task<T> DeleteAsync<T>(HttpClient client, string relativeUrl, CancellationToken ct, HttpContent? content = null)
        {
            using var request = new HttpRequestMessage(HttpMethod.Delete, relativeUrl)
            {
                Content = content
            };

            if (_auth.IsAuthenticated && !string.IsNullOrEmpty(_auth.AccessToken))
            {
                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", _auth.AccessToken);
            }

            try
            {
                using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);

                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync(ct);

                    throw new HttpRequestException(
                        $"API returned { (int)response.StatusCode } - { response.ReasonPhrase}. Content: { errorBody}");
                }

                await using var stream = await response.Content.ReadAsStreamAsync(ct);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var result = await JsonSerializer.DeserializeAsync<T>(stream, options, ct);

                if (result is null)
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

        private string ResolveFileName(HttpResponseMessage response, ScriptDto script)
        {
            var contentDisposition = response.Content.Headers.ContentDisposition;

            var fileName = contentDisposition?.FileNameStar ??
                           contentDisposition?.FileName;

            if (!string.IsNullOrWhiteSpace(fileName))
                fileName = fileName.Trim('"');
            else
                fileName = $"{script.Slug}.dyn";

            if (!fileName.EndsWith(".dyn", StringComparison.OrdinalIgnoreCase))
                fileName += ".dyn";

            foreach (var c in Path.GetInvalidFileNameChars())
                fileName = fileName.Replace(c, '_');

            return fileName;
        }

        private string ResolveTargetDirectory()
        {
            var configuredPath = _settings.Current.DefaultScriptSavePath;

            if (!string.IsNullOrWhiteSpace(configuredPath) &&
                Directory.Exists(configuredPath))
            {
                return configuredPath;
            }

            var fallback = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "BIMformative");

            Directory.CreateDirectory(fallback);
            return fallback;
        }

        

        #endregion

        public async Task<bool> HasOpenScript()
        {
            var model = _dynamo.Model;

            var workspace = model.CurrentWorkspace != null;

            return workspace;           
        }
    }
}
