using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIMformative.DynamoExtension.Db
{
    public static class DatabaseBootstrapper
    {
        private static BimformativeDbContext? _db;
        public static BimformativeDbContext Initialize()
        {
            if (_db != null)
                return _db;

            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var folder = Path.Combine(appData, "BIMformative", "data");
            Directory.CreateDirectory(folder);

            var dbPath = Path.Combine(folder, "bimformative.db");

            var options = new DbContextOptionsBuilder<BimformativeDbContext>()
                .UseSqlite($"Data Source={dbPath}")
                .Options;

            _db = new BimformativeDbContext(options);
            _db.Database.Migrate();

            _db.Database.ExecuteSqlRaw("PRAGMA foreign_keys=ON;");
            _db.Database.ExecuteSqlRaw("PRAGMA journal_mode=WAL;");
            _db.Database.ExecuteSqlRawAsync("PRAGMA synchronous=NORMAL;");

            return _db;
        }
    }
}
