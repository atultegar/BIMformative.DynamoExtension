using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIMformative.DynamoExtension.Services.Auth
{
    public interface ILocalAuthStore
    {
        Task SaveAsync(AuthCache cache);
        Task<AuthCache?> LoadAsync();
        Task ClearAsync();
    }
}
