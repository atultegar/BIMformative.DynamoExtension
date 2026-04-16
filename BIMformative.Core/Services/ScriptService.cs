using BIMformative.Core.Interfaces;
using BIMformative.Core.Models;
using BIMformative.Core.Models.Api;
using BIMformative.Core.Models.Scripts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace BIMformative.Core.Services
{
    public sealed class ScriptService : IScriptService
    {
        private readonly IScriptApiClient _api;
        private readonly ISettingsService _settings;
        private readonly IFileOverwritePrompt _overwritePrompt;

        public ScriptService(
            IScriptApiClient api, 
            ISettingsService settings, 
            IFileOverwritePrompt overwritePrompt)
        {
            _api = api;
            _settings = settings;
            _overwritePrompt = overwritePrompt;
        }

        #region ANALYZE

        public async Task<ScriptAnalyzeResponseDto> AnalyzeAsync(string filePath, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException("File path is required", nameof(filePath));

            if (!File.Exists(filePath))
                throw new FileNotFoundException("Script file not found", filePath);

            using (var form = new MultipartFormDataContent())
            using (var fileStream = File.OpenRead(filePath))
            using (var fileContent = new StreamContent(fileStream))
            {
                fileContent.Headers.ContentType = 
                    new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");

                fileContent.Headers.ContentDisposition =
                    new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data")
                    {
                        Name = "\"file\"",
                        FileName = $"\"{Path.GetFileName(filePath)}\""
                    };

                form.Add(fileContent);

                return await _api.AnalyzeAsync(form, ct);
            }
        }

        #endregion

        #region DOWNLOAD

        public async Task<string> DownloadAsync(ScriptDto script, CancellationToken ct = default)
        {
            var response = await _api.DownloadAsync(script.Slug, ct);

            var fileName = ResolveFileName(response, script.Slug);            
            var filePath = PrepareFilePath(fileName);

            await SaveToFileAsync(response, filePath, ct);

            return filePath;
        }

        public async Task<string> DownloadLatestAsync(DownloadedScript script, CancellationToken ct = default)
        {
            var response = await _api.DownloadAsync(script.Slug, ct);

            var filePath = PrepareFilePath(script.LocalPath);

            await SaveToFileAsync(response, filePath, ct);

            return filePath;
        }

        #endregion

        #region API PASS_THROUGH

        public async Task<string> DeleteAsync(string slug, CancellationToken ct = default)
        {
            return await _api.DeleteScriptAsync(slug, ct);
        }
        public async Task<ScriptDetailsDto> GetBySlugAsync(string slug, CancellationToken ct = default)
        {
            return await _api.GetBySlugAsync(slug, ct);
        }

        public async Task<string> GetHashAsync(string scriptJson, CancellationToken ct = default)
        {
            return await _api.GetHashAsync(scriptJson, ct);
        }

        public async Task<RemoteScriptInfo> GetLatestInfoAsync(string slug, CancellationToken ct = default)
        {
            return await _api.GetLatestInfoAsync(slug, ct);
        }

        public async Task<string> GetLatestVersionAsync(string slug, CancellationToken ct = default)
        {
            var script = await GetBySlugAsync(slug, ct);

            var currentVersion = script.Current_Version_Number;

            return $"V{currentVersion.ToString()}"; 
        }

        public async Task<PagedResponse<MyScriptDto>> GetMyScriptsAsync(string search = null, string scriptType = null, CancellationToken ct = default)
        {
            return await _api.GetMyScriptsAsync(search, scriptType, ct);
        }

        public async Task<PagedResponse<ScriptDto>> GetPublicAsync(
            int page, 
            int limit, 
            string search = null, 
            string scriptType = null, 
            ScriptSortField sortField = ScriptSortField.updated_at, 
            SortOrder sortOrder = SortOrder.desc, 
            CancellationToken ct = default)
        {

            return await _api.GetPublicAsync(page, limit, search, scriptType, sortField, sortOrder, ct);
        }


        public async Task<IReadOnlyList<ScriptVersionDto>> GetVersionsAsync(string slug, CancellationToken ct = default)
        {
            return await _api.GetVersionsAsync(slug, ct);
        }

        public async Task<ScriptPublishResponse> PublishAsync(ScriptPublishRequestDto request, CancellationToken ct = default)
        {
            return await _api.PublishAsync(request, ct);
        }

        public async Task<ScriptVersionDto> PublishVersionAsync(string slug, ScriptAnalyzeResponseDto parsed, string changeLog, CancellationToken ct = default)
        {
            return await _api.PublishVersionAsync(slug, parsed, changeLog, ct);
        }

        public async Task<ScriptLikesDto> LikeAsync(string slug, CancellationToken ct = default)
        {
            return await _api.LikeAsync(slug, ct);
        }

        public async Task<ScriptLikesDto> UnlikeAsync(string slug, CancellationToken ct = default)
        {
            return await _api.UnlikeAsync(slug, ct);
        }

        public async Task<bool> HasLiked(string slug, CancellationToken ct = default)
        {
            return await _api.HasLikedAsync(slug, ct);
        }

        public async Task<ScriptDetailsDto> UpdateScriptMetadataAsync(string slug, ScriptUpdateRequest scriptUpdateRequest, CancellationToken ct = default)
        {
            return await _api.UpdateScriptMetadataAsync(slug, scriptUpdateRequest, ct);
        }

        public async Task<SetCurrentVersionResponse> SetCurrentVersionAsync(string slug, int versionNumber, CancellationToken ct = default)
        {
            return await _api.SetCurrentVersionAsync(slug, versionNumber, ct);
        }

        public async Task<string> DeleteVersionAsync(string slug, int versionNumber, CancellationToken ct = default)
        {
            return await _api.DeleteVersionAsync(slug, versionNumber, ct);
        }        

        public async Task<UpdateScriptVisibilityResponse> UpdateScriptVisibilityAsync(string slug, bool isPublic, CancellationToken ct = default)
        {
            return await _api.UpdateScriptVisibilityAsync(slug, isPublic, ct);
        }

        #endregion

        #region HELPERS

        private async Task SaveToFileAsync(HttpResponseMessage response, string filePath, CancellationToken ct)
        {
            if (File.Exists(filePath) && _settings.Current.AskBeforeOverwrite)
            {
                var decision = _overwritePrompt.Ask(filePath);

                if (decision == OverwriteDecision.Cancel)
                    throw new OperationCanceledException();

                if (decision == OverwriteDecision.SaveAs)
                {
                    var newPath = _overwritePrompt.ShowSaveAs(filePath);
                    if (string.IsNullOrEmpty(newPath))
                        throw new OperationCanceledException();

                    filePath = newPath;
                }
            }

            using (var stream = await response.Content.ReadAsStreamAsync())
            {
                using (var file = File.Create(filePath))
                {
                    await stream.CopyToAsync(file);
                }
            }
        }

        private string ResolveFileName(HttpResponseMessage response, string slug)
        {
            var contentDisposition = response.Content.Headers.ContentDisposition;

            var fileName = contentDisposition?.FileNameStar ??
                           contentDisposition?.FileName;

            if (!string.IsNullOrWhiteSpace(fileName))
                fileName = fileName.Trim('"');
            else
                fileName = $"{slug}.dyn";

            if (!fileName.EndsWith(".dyn", StringComparison.OrdinalIgnoreCase))
                fileName += ".dyn";

            foreach (var c in Path.GetInvalidFileNameChars())
                fileName = fileName.Replace(c, '_');

            return fileName;
        }

        private string PrepareFilePath(string fileNameOrPath)
        {
            if (Path.IsPathRooted(fileNameOrPath))
                return fileNameOrPath;

            var dir = ResolveTargetDirectory();
            return Path.Combine(dir, fileNameOrPath);
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
    }
}
