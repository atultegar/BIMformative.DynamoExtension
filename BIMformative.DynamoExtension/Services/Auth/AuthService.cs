using BIMformative.DynamoExtension.Models;
using BIMformative.DynamoExtension.Services.Interfaces;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace BIMformative.DynamoExtension.Services.Auth
{
    public class AuthService : IAuthService
    {
        private const string SignInPath = "/sign-in";
        private const string DesktopAuthPath = "/desktop-auth";
                
        private readonly HttpClient _http;
        private readonly IUserApiClient _userApi;
        private readonly ILocalAuthStore _authStore;

        public string? _accessToken;
        private DateTime _expiresAt;

        public UserProfileDto? CurrentUser {  get; private set; }        

        public bool IsAuthenticated => 
            !string.IsNullOrEmpty(_accessToken) && DateTime.UtcNow < _expiresAt;
            

        public string? AccessToken => _accessToken;

        public event EventHandler? AuthStateChanged;

        public AuthService(
            HttpClient http, 
            IUserApiClient userApi, 
            ILocalAuthStore authStore)
        {
            _http = http ?? throw new ArgumentNullException(nameof(http));
            _userApi = userApi ?? throw new ArgumentNullException(nameof(userApi));
            _authStore = authStore ?? throw new ArgumentNullException(nameof(authStore));
        }

        // PUBLIC API
        public async Task RestoreSessionAsync()
        {
            await TryRestoreSessionAsync();
        }

        // CALLED WHEN AUTH IS REQUIRED
        public async Task<bool> EnsureAuthenticatedAsync()
        {
            if (IsAuthenticated)
                return true;

            if (await TryRestoreSessionAsync())
                return true;

            return await LoginWithBrowserAsync();
        }

        public async Task LogoutAsync()
        {
            ClearState();
            await _authStore.ClearAsync();
            RaiseAuthChanged();
        }

        // Session restore
        private async Task<bool> TryRestoreSessionAsync()
        {
            var cache = await _authStore.LoadAsync();
            if (cache == null)
                return false;

            if (DateTime.UtcNow >= cache.ExpiresAt)
            {
                await _authStore.ClearAsync();
                return false;
            }

            _accessToken = cache.AccessToken;
            _expiresAt = cache.ExpiresAt;
            CurrentUser = cache.User;

            RaiseAuthChanged();
            return true;
        }
        
        private async Task<bool> LoginWithBrowserAsync()
        {
            var sessionId = Guid.NewGuid().ToString("N");

            var frontendBase = _http.BaseAddress?.GetLeftPart(UriPartial.Authority)
                ?? throw new InvalidOperationException("HttpClient BaseAddress not set");

            var redirectUrl =
                $"{frontendBase}{DesktopAuthPath}?session={sessionId}";

            var signInUrl =
                $"{frontendBase}{SignInPath}?redirect_url={Uri.EscapeDataString(redirectUrl)}";

            // Open browser
            Process.Start(new ProcessStartInfo
            {
                FileName = signInUrl,
                UseShellExecute = true
            });

            // Poll backend for token
            return await PollForTokenAsync(sessionId);
        }

        private async Task<bool> PollForTokenAsync(string sessionId)
        {
            // 60 seconds max
            for (int i = 0; i < 60; i++)
            {
                await Task.Delay(1000);

                HttpResponseMessage response;                               

                try
                {
                    response = await _http.GetAsync(
                        $"/api/public/v1/desktop-auth/poll?session={sessionId}");
                }
                catch
                {
                    continue;
                }

                if (!response.IsSuccessStatusCode)
                    continue;

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);

                if (doc.RootElement.GetProperty("status").GetString() != "ok")
                    continue;

                var token = doc.RootElement.GetProperty("token").GetString();
                if (string.IsNullOrEmpty(token))
                    return false;

                await CompleteLoginAsync(token);
                return true;                
            }

            return false;
        }

        private async Task CompleteLoginAsync(string token)
        {
            _accessToken = token;
            _expiresAt = ExtractExpiry(token);

            CurrentUser = await _userApi.GetMeAsync(token, CancellationToken.None);

            await _authStore.SaveAsync(new AuthCache
            {
                AccessToken = _accessToken!,
                ExpiresAt = _expiresAt,
                User = CurrentUser!
            });

            RaiseAuthChanged();
        }

        private void ClearState()
        {
            _accessToken = null;
            _expiresAt = DateTime.MinValue;
            CurrentUser = null;
        }

        private void RaiseAuthChanged()
        {
            AuthStateChanged?.Invoke(this, EventArgs.Empty);
        }


        private static DateTime ExtractExpiry(string jwt)
        {
            // JWT format: header.payload.signature
            var parts = jwt.Split('.');
            if (parts.Length != 3)
                throw new ArgumentException("Invalid JWT format");

            var payload = parts[1];

            // Base64Url -> Base64
            payload = payload
                .Replace('-', '+')
                .Replace("_", "/");

            switch (payload.Length % 4)
            {
                case 2: payload += "=="; break;
                case 3: payload += "="; break;
            }

            var jsonBytes = Convert.FromBase64String(payload);
            var json = System.Text.Encoding.UTF8.GetString(jsonBytes);

            using var doc = JsonDocument.Parse(json);

            if (!doc.RootElement.TryGetProperty("exp", out var expElement))
                throw new InvalidOperationException("JWT does not contain exp");

            var expSeconds = expElement.GetInt64();

            return DateTimeOffset
                .FromUnixTimeSeconds(expSeconds)
                .UtcDateTime;
        }

        public async Task<string?> CreateWebViewSignInUrlAsync(string redirectPath)
        {
            if (!IsAuthenticated) return null;

            var response = await _http.PostAsync("/api/public/v1/desktop-auth/ticket", null);

            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(json);

            var ticket = doc.RootElement.GetProperty("ticket").GetString();
            if(string.IsNullOrEmpty(ticket)) return null;

            var frontendBase = _http.BaseAddress?.GetLeftPart(UriPartial.Authority)
                ?? throw new InvalidOperationException("HttpClient BaseAddress not set");

            var url =
                $"{frontendBase}/desktop-auth/exchange" +
                $"?ticket={ticket}" +
                $"&redirect={Uri.EscapeUriString(redirectPath)}";

            return url;
        }
    }
}
