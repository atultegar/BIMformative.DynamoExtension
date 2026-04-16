using BIMformative.Infrastructure.Db;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIMformative.DynamoExtension.Db
{
    public class BIMformativeDbContextFactory : IDesignTimeDbContextFactory<BIMformativeDbContext>
    {
        public BIMformativeDbContext CreateDbContext(string[] args)
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var folder = Path.Combine(appData, "BIMformative", "data");
            Directory.CreateDirectory(folder);

            var dbPath = Path.Combine(folder, "bimformative.db");

            var options = new DbContextOptionsBuilder<BIMformativeDbContext>()
                .UseSqlite($"Data Source={dbPath}")
                .Options;

            return new BIMformativeDbContext(options);
        }
    }
}
