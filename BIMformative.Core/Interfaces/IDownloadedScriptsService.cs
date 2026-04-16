using BIMformative.Core.Models.Scripts;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BIMformative.Core.Interfaces
{
    public interface IDownloadedScriptsService
    {
        Task<List<DownloadedScript>> GetAllAsync();
        Task AddOrUpdateAsync(DownloadedScript script);
        Task<bool> ExistsAsync(string scriptId);
        Task CheckForUpdateAsync(CancellationToken ct = default);
        Task MarkUpdateAvailableAsync(string scriptId, string latestVersion);
        Task DeleteAsync(DownloadedScript script);
    }
}
