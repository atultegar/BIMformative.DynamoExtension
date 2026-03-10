using BIMformative.DynamoExtension.Services.Exceptions;
using Clerk.BackendAPI.Models.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace BIMformative.DynamoExtension.Services.Auth
{
    public class DesktopTicketService : IDesktopTicketService
    {
        private readonly HttpClient _publicHttp;
        private readonly IAuthService _authService;

        private const string TicketEndpoint = "desktop-auth/ticket";

        public DesktopTicketService(HttpClient publicHttp, IAuthService authService)
        {
            _publicHttp = publicHttp ?? throw new ArgumentNullException(nameof(publicHttp));
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        }

        public async Task<string> CreateTicketAsync(CancellationToken ct = default)
        {
            if (!_authService.IsAuthenticated || string.IsNullOrEmpty(_authService.AccessToken))
                throw new InvalidOperationException("User is not authenticated.");

            using var request = new HttpRequestMessage(HttpMethod.Post, TicketEndpoint);

            request.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _authService.AccessToken);

            try
            {
                using var response = await _publicHttp.SendAsync(request, ct);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync(ct);
                    throw new HttpRequestException(
                        $"Failed to create desktop ticket. Status: {response.StatusCode}. Response: {error}");
                }


                var result = await response.Content.ReadFromJsonAsync<TicketResponse>(ct);

                if (result == null || string.IsNullOrWhiteSpace(result.Ticket))
                    throw new InvalidOperationException("Ticket response was invalid.");

                return result.Ticket;
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
    }

    public class TicketResponse
    {
        public string Ticket { get; set; } = string.Empty;
    }
}
