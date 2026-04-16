using BIMformative.Infrastructure.Db;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;

namespace BIMformative.Infrastructure
{
    public static class DatabaseBootstrapper
    {
        private static BIMformativeDbContext _db;
        public static BIMformativeDbContext Initialize()
        {
            if (_db != null)
                return _db;

            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var folder = Path.Combine(appData, "BIMformative", "data");
            Directory.CreateDirectory(folder);

            var dbPath = Path.Combine(folder, "bimformative.db");

            var options = new DbContextOptionsBuilder<BIMformativeDbContext>()
                .UseSqlite($"Data Source={dbPath}")
                .Options;

            _db = new BIMformativeDbContext(options);
            _db.Database.Migrate();

            _db.Database.ExecuteSqlRaw("PRAGMA foreign_keys=ON;");
            _db.Database.ExecuteSqlRaw("PRAGMA journal_mode=WAL;");
            _db.Database.ExecuteSqlRawAsync("PRAGMA synchronous=NORMAL;");

            return _db;
        }
    }
}
