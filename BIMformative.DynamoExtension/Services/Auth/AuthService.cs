using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIMformative.DynamoExtension.Services.Auth
{
    public class AuthService : IAuthService
    {
        public bool IsAuthenticated => false;

        public string? AccessToken => null;

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
