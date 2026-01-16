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
        string? AccessToken { get; }

        /// <summary>
        /// Forces interactive login if required
        /// </summary>
        /// <returns></returns>
        Task<bool> LoginAsync();

        Task LogoutAsync();
    }
}
