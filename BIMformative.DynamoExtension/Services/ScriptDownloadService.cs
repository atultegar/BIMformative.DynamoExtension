using BIMformative.DynamoExtension.Models.Scripts;
using BIMformative.DynamoExtension.Services.Interfaces;
using BIMformative.DynamoExtension.Services.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Printing;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BIMformative.DynamoExtension.Services
{
    public class ScriptDownloadService : IScriptDownloadService
    {
        private readonly HttpClient _http;
        private readonly ISettingsService _settings;
        private readonly IFileOverwritePrompt _overwritePrompt;

        public ScriptDownloadService(HttpClient http, ISettingsService settings, IFileOverwritePrompt overwritePrompt)
        {
            _http = http ?? throw new ArgumentNullException(nameof(http));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _overwritePrompt = overwritePrompt;
        }

        public async Task<string> DownloadAsync(
            ScriptDto script, 
            string accessToken, 
            CancellationToken ct)
        {
            var url = $"/api/v1/scripts/{script.Slug}/download";

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            using var response = await _http.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                ct);

            response.EnsureSuccessStatusCode();

            // 1. Resolve filename
            var filename = ResolveFileName(response, script);

            // 2. Resolve target directory
            var targetDir = ResolveTargetDirectory();

            // 3. Combine full path
            var filePath = Path.Combine(targetDir, filename);

            // File Exists?
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

            // 4. Write file
            await using var stream = await response.Content.ReadAsStreamAsync(ct);
            await using var file = File.Create(filePath);
            await stream.CopyToAsync(file, ct);

            return filePath;            
        }

        private static string ResolveFileName(HttpResponseMessage response, ScriptDto script)
        {
            // Try Content-Disposition first
            var contentDisposition = response.Content.Headers.ContentDisposition;

            var fileName = contentDisposition?.FileNameStar ??
                contentDisposition?.FileName;

            if (!string.IsNullOrWhiteSpace(fileName))
            {
                fileName = fileName.Trim('"');
            }
            else
            {
                // Fallback
                fileName = $"{script.Slug}.dyn";
            }

            // Ensure .dyn extension
            if (!fileName.EndsWith(".dyn", StringComparison.OrdinalIgnoreCase))
            {
                fileName += ".dyn";
            }

            // Sanitize filename (Windows-safe)
            foreach (var c in Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(c, '_');
            }

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

            // Fallback (safe + predictable)
            var fallback = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "BIMformative");

            Directory.CreateDirectory(fallback);
            return fallback;
        }

        public async Task<string> GetScriptCurrentHash(ScriptDto script)
        {
            throw new NotImplementedException();
        }
    }
}
