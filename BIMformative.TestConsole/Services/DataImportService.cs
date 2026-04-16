using BIMformative.Core.Models.Scripts;
using BIMformative.Infrastructure.Db;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIMformative.TestConsole.Services
{
    public class DataImportService
    {
        private readonly BIMformativeDbContext _db;

        public DataImportService(BIMformativeDbContext db)
        {
            _db = db;
        }

        public async Task ImportDownloadedScriptsAsync(string path)
        {
            var json = await File.ReadAllTextAsync(path);
            var data = JsonConvert.DeserializeObject<List<DownloadedScript>>(json);

            if (data == null) return;

            await _db.DownloadedScripts.AddRangeAsync(data);
            await _db.SaveChangesAsync();
        }
    }
}
