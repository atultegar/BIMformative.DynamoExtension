using BIMformative.Core.Models.Auth;
using System.Threading.Tasks;

namespace BIMformative.Core.Interfaces
{
    public interface ILocalAuthStore
    {
        Task SaveAsync(AuthCache cache);
        Task<AuthCache> LoadAsync();
        Task ClearAsync();
    }
}
