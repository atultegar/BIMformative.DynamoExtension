using BIMformative.DynamoExtension.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BIMformative.DynamoExtension.Services
{
    public class ScriptCatalogService
    {
        private static readonly HttpClient _httpClient = new()
        {
            Timeout = TimeSpan.FromSeconds(10)
        };

        private const string ScriptsEndpoint = "https://www.bimformative.com/api/public/v1/scripts";

        public async Task<IReadOnlyList<ScriptDto>> GetScriptsAsync(
            CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ApiResponse<ScriptDto>>(
                    ScriptsEndpoint,
                    cancellationToken);

                return response?.Data ?? [];
            }
            catch
            {
                return [];
            }
        }
    }
}
