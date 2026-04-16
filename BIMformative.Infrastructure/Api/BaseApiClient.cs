using BIMformative.Core.Models.Api;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace BIMformative.Infrastructure.Api
{
    public abstract class BaseApiClient
    {
        protected readonly HttpClient _http;

        protected BaseApiClient(HttpClient http)
        {
            _http = http ?? throw new ArgumentNullException(nameof(http));
        }

        protected virtual void AddHeaders(HttpRequestMessage request) { }

        public async Task<T> SendAsync<T>(
            HttpMethod method,
            string url,
            CancellationToken ct,
            HttpContent content = null)
        {
            var request = new HttpRequestMessage(method, url)
            {
                Content = content
            };

            AddHeaders(request);

            try
            {
                var response = await _http.SendAsync(request, ct);
                var json = await response.Content.ReadAsStringAsync();

                // Handle HTTP-level errors first
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    throw new UnauthorizedAccessException("Unauthorized: " + json);

                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    throw new UnauthorizedAccessException("Forbidden: " + json);

                // Deserialize into API wrapper
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<T>>(json);

                if (apiResponse == null)
                    throw new InvalidOperationException("Invalid API response.");

                // Handle API-level errors
                if (!apiResponse.Success)
                {
                    var code = apiResponse.Error.Code ?? "UNKNOWN_ERROR";
                    var message = apiResponse.Error.Message ?? "Unknown error";

                    throw new ApiException(code, message);
                }

                if (apiResponse.Data == null)
                    throw new InvalidOperationException("API returned empty data.");               


                return apiResponse.Data;
            }
            catch (ApiException)
            {
                throw;
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (HttpRequestException ex)
            {
                throw new ApiUnavailableException("BIMformative API unreachable", ex);
            }
            catch (Exception ex)
            {
                throw new ApiUnavailableException("Unexpected API error.", ex);
            }
        }

        public async Task<HttpResponseMessage> SendRawAsync(
            HttpMethod method,
            string relativeUrl,
            CancellationToken ct,
            HttpContent content = null)
        {
            var request = new HttpRequestMessage(method, relativeUrl)
            {
                Content = content
            };

            AddHeaders(request);

            try
            {
                var response = await _http.SendAsync(
                    request,
                    HttpCompletionOption.ResponseHeadersRead,
                    ct);

                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    response.Dispose();

                    throw new ApiException("DOWNLOAD_FAILED", $"Download failed: {errorBody}", 500);
                }

                return response;
            }
            catch (ApiException)
            {
                throw;
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (HttpRequestException ex)
            {
                request.Dispose();
                throw new ApiUnavailableException("BIMformative API unreachable", ex);
            }
            catch (Exception ex)
            {
                throw new ApiUnavailableException("Unexpected API error.", ex);
            }
        }
    }
}
