using BIMformative.DynamoExtension.Db;
using BIMformative.DynamoExtension.Models.Scripts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIMformative.DynamoExtension.Services
{
    public class DownloadedScriptRepository
    {
        private readonly BimformativeDbContext _db;

        public DownloadedScriptRepository(BimformativeDbContext db)
        {
            _db = db;
        }

        public Task<List<DownloadedScript>> GetAllAsync()
            => _db.DownloadedScripts
                .OrderBy(x => x.Title)
                .ToListAsync();

        public async Task UpdateAsync(DownloadedScript entity)
        {
            _db.DownloadedScripts.Update(entity);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(string id)
        {
            var entity = await _db.DownloadedScripts.FindAsync(id);
            if (entity == null) return;

            _db.DownloadedScripts.Remove(entity);
            await _db.SaveChangesAsync();
        }

    }
}
