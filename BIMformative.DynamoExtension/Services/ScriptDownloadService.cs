using BIMformative.DynamoExtension.Models;
using BIMformative.DynamoExtension.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BIMformative.DynamoExtension.Services
{
    public class ScriptDownloadService : IScriptDownloadService
    {
        private readonly HttpClient _http;

        public ScriptDownloadService(HttpClient http)
        {
            _http = http;            
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

            using var response = await _http.SendAsync(request, ct);

            response.EnsureSuccessStatusCode();

            var bytes = await response.Content.ReadAsByteArrayAsync(ct);

            var fileName = $"{script.Slug}.dyn";
            var tempPath = Path.Combine(Path.GetTempPath(), "BIMformative", fileName);

            Directory.CreateDirectory(Path.GetDirectoryName(tempPath)!);
            await File.WriteAllBytesAsync(tempPath, bytes, ct);

            return tempPath;            
        }
    }
}
