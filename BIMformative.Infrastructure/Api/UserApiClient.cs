using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using BIMformative.Core.Interfaces;
using BIMformative.Core.Models;
using BIMformative.Core.Models.Api;
using Newtonsoft.Json;

namespace BIMformative.Infrastructure.Api
{
    public class UserApiClient : IUserApiClient
    {
        private readonly HttpClient _http;
        public UserApiClient(HttpClient http)
        {
            _http = http ?? throw new ArgumentNullException(nameof(http));
        }

        public async Task<UserProfileDto> GetMeAsync(string accessToken, CancellationToken ct)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/auth/me");

            request.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _http.SendAsync(request, ct);

            var json = await response.Content.ReadAsStringAsync();

            var apiResponse = JsonConvert.DeserializeObject<ApiResponse<UserProfileDto>>(json);

            if (apiResponse == null)
                throw new InvalidOperationException("Invalid API response");

            if (!apiResponse.Success)
                throw new ApiException(apiResponse.Error?.Code, apiResponse.Error?.Message);

            return apiResponse.Data
                ?? throw new InvalidOperationException("User data is null");
        }
    }
}
