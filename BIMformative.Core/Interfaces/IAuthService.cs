using BIMformative.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIMformative.Core.Interfaces
{
    public interface IAuthService
    {
        bool IsAuthenticated { get; }
        string AccessToken { get; }
        UserProfileDto CurrentUser { get; }

        event EventHandler AuthStateChanged;

        /// <summary>
        /// Ensures authentication
        /// Show login UI only if required.
        /// </summary>
        Task<bool> EnsureAuthenticatedAsync();

        /// <summary>
        /// Silent restore from local storage (NO UI).
        /// Call once on app startup.
        /// </summary>
        /// <returns></returns>
        Task RestoreSessionAsync();
       
        Task LogoutAsync();

        Task<string> CreateWebViewSignInUrlAsync(string redirectPath);
    }
}
