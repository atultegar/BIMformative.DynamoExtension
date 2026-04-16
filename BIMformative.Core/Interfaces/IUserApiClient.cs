using BIMformative.Core.Models;
using System.Threading;
using System.Threading.Tasks;

namespace BIMformative.Core.Interfaces
{
    public interface IUserApiClient
    {
        Task<UserProfileDto> GetMeAsync(string accessToken, CancellationToken ct);
    }
}
