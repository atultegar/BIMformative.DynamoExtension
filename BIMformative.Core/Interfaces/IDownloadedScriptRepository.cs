using BIMformative.Core.Models.Scripts;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BIMformative.Core.Interfaces
{
    public interface IDownloadedScriptRepository
    {
        Task<List<DownloadedScript>> GetAllAsync(CancellationToken ct = default);
        Task<DownloadedScript> GetByIdAsync(string id, CancellationToken ct = default);
        Task AddAsync(DownloadedScript script, CancellationToken ct = default);
        Task UpdateAsync(DownloadedScript script, CancellationToken ct = default);
        Task DeleteAsync(string id, CancellationToken ct = default);
        Task<bool> ExistsAsync(string id, CancellationToken ct = default);
        Task SaveChangesAsync(CancellationToken ct = default);
    }
}
