using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIMformative.DynamoExtension.Services.Auth
{
    public interface IAuthService
    {
        bool IsAuthenticated { get; }
        bool IsTokenExpired { get; }
        string? AccessToken { get; }

        event EventHandler? AuthStateChanged;

        /// <summary>
        /// Ensures the use is authenticated.
        /// Show login UI only if required.
        /// </summary>
        /// <returns></returns>
        Task<bool> EnsureAuthenticatedAsync();
        Task<bool> LoginAsync();

        Task LogoutAsync();
    }
}
