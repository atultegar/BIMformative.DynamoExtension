using BIMformative.DynamoExtension.Models.Scripts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIMformative.DynamoExtension.Db
{
    public class BimformativeDbContext : DbContext
    {
        public DbSet<DownloadedScript> DownloadedScripts => Set<DownloadedScript>();

        public BimformativeDbContext(DbContextOptions<BimformativeDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DownloadedScript>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Title).IsRequired();
                entity.Property(x => x.Slug).IsRequired();
                entity.Property(x => x.ScriptType).IsRequired();
                entity.Property(x => x.DownloadedVersion).IsRequired();
                entity.Property(x => x.LocalPath).IsRequired();
                entity.Property(X => X.SyncStatus).HasConversion<int>();
            });
        }
    }
}
