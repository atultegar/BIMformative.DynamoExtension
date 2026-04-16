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
        Task<List<DownloadedScript>> GetAllAsync();
        Task<DownloadedScript> GetByIdAsync(string id);
        Task AddAsync(DownloadedScript script);
        Task UpdateAsync(DownloadedScript script);
        Task DeleteAsync(string id);
        Task<bool> ExistsAsync(string id);
        Task SaveChangesAsync(CancellationToken ct = default);
    }
}
