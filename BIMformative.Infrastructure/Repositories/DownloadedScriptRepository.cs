using BIMformative.Core.Interfaces;
using BIMformative.Core.Models.Scripts;
using BIMformative.Infrastructure.Db;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BIMformative.Infrastructure.Repositories
{
    public class DownloadedScriptRepository : IDownloadedScriptRepository
    {
        private readonly BIMformativeDbContext _db;

        public DownloadedScriptRepository(BIMformativeDbContext db)
        {
            _db = db;
        }

        public async Task<List<DownloadedScript>> GetAllAsync()
            => await _db.DownloadedScripts
                .OrderByDescending(x => x.DownloadedAt)
                .ToListAsync();

        public async Task<DownloadedScript> GetByIdAsync(string id)
            => await _db.DownloadedScripts.FindAsync(id);

        public async Task AddAsync(DownloadedScript script)
            => await _db.DownloadedScripts.AddAsync(script);

        public Task UpdateAsync(DownloadedScript script)
        {
            _db.DownloadedScripts.Update(script);
            return Task.CompletedTask;
        }

        public async Task DeleteAsync(string id)
        {
            var entity = await _db.DownloadedScripts.FindAsync(id);
            if (entity != null)
                _db.DownloadedScripts.Remove(entity);
        }

        public Task<bool> ExistsAsync(string id)
            => _db.DownloadedScripts.AnyAsync(x => x.Id == id);

        public Task SaveChangesAsync(CancellationToken ct = default)
            => _db.SaveChangesAsync(ct);
    }
}
