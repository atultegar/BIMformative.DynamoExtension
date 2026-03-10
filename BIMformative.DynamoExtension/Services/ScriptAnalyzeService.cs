using BIMformative.DynamoExtension.Models.Scripts;
using BIMformative.DynamoExtension.Services.Auth;
using BIMformative.DynamoExtension.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace BIMformative.DynamoExtension.Services
{
    public sealed class ScriptAnalyzeService : IScriptAnalyzeService
    {
        private readonly HttpClient _http;
        private readonly IAuthService _auth;

        public ScriptAnalyzeService(HttpClient http, IAuthService auth)
        {
            _http = http ?? throw new ArgumentNullException(nameof(http));
            _auth = auth ?? throw new ArgumentNullException(nameof(auth));
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
            using var response = await _http.SendAsync(request, ct);

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
    }
}
