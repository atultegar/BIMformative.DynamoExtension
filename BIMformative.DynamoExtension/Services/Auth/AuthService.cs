using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIMformative.DynamoExtension.Services.Auth
{
    public class AuthService : IAuthService
    {
        public string _accessToken = "eyJhbGciOiJSUzI1NiIsImNhdCI6ImNsX0I3ZDRQRDIyMkFBQSIsImtpZCI6Imluc18ydWpxNlJvbW9Kb1RjcjZjcHZxVVN0MEFhN0wiLCJ0eXAiOiJKV1QifQ.eyJhcHAiOiJkZXNrdG9wIiwiYXpwIjoiaHR0cHM6Ly93d3cuYmltZm9ybWF0aXZlLmNvbSIsImVtYWlsIjoiYXR1bC50ZWdhckBnbWFpbC5jb20iLCJleHAiOjE3Njg5MDgzNjIsImZpcnN0X25hbWUiOiJBdHVsIiwiaWF0IjoxNzY4OTA0NzYyLCJpc3MiOiJodHRwczovL2NsZXJrLmJpbWZvcm1hdGl2ZS5jb20iLCJqdGkiOiIyOTkyYjU2ZmU0MzU0MmEyMDA4MSIsImxhc3RfbmFtZSI6IlRlZ2FyIiwibmJmIjoxNzY4OTA0NzAyLCJzb3VyY2UiOiJiaW1mb3JtYXRpdmUiLCJzdWIiOiJ1c2VyXzJ1bDBURUIyU09Qc3ZYMDlFd0RqSEV4dFRsTSJ9.ghrDCHzlrWfS0PKL_5miNp7ssmPkL6O2w0CEinwExyMV7I3pJHRQMYkRMdE4iOYF8wrQJx-FB9ZuLRMz0HVT_Zkp2bniS4KThgorXLX43Io8qIJr3t31cxmwI16IpvOI7huZKrLNsD9-R0MN-9-ilTgMLsPRu4lbyWvP3EIfO4Mkwn_S6WUejpaXiC9JLJ1RaJ1njUssjabxifgb_GuLO1iH6xScfcH8J5ErCvWGmf-7I2DkPT45o2oKUMBoDktdL1fFRxSWX3nFCwtAD2fV-lhUo0X-NELdx5JExwC_kYCMaTb1-Q2Hc6oM0PlQ-3Mlp3lnWyspOLZcHoMNs6EAmg";
        private DateTime _expiresAt;

        public bool IsAuthenticated => !string.IsNullOrEmpty(_accessToken) && !IsTokenExpired;
        public bool IsTokenExpired => DateTime.Now >= _expiresAt;
        public string? AccessToken => _accessToken;

        public event EventHandler? AuthStateChanged;

        public async Task<bool> EnsureAuthenticatedAsync()
        {
            if (IsAuthenticated)
                return true;

            return await LoginAsync();
        }

        public Task<bool> LoginAsync()
        {
            // TODO: OAuth / browser login
            return Task.FromResult(false);
        }

        public Task LogoutAsync()
        {
            return Task.CompletedTask;
        }
    }

}
