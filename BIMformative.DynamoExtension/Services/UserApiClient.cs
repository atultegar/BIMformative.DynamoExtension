using BIMformative.DynamoExtension.Models;
using BIMformative.DynamoExtension.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using System.Text.Json;

namespace BIMformative.DynamoExtension.Services
{
    public class UserApiClient : IUserApiClient
    {
        private readonly HttpClient _http;

        public UserApiClient(HttpClient http)
        {
            _http = http;
        }

        public async Task<UserProfileDto> GetMeAsync(string accessToken, CancellationToken ct)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/auth/me");
            request.Headers.Authorization = 
                new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _http.SendAsync(request, ct);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(ct);

            return JsonSerializer.Deserialize<UserProfileDto>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                )!;
        }
    }
}
