using BIMformative.Core.Models.Scripts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIMformative.Infrastructure.Db
{
    public class BIMformativeDbContext : DbContext
    {
        public DbSet<DownloadedScript> DownloadedScripts => Set<DownloadedScript>();

        public BIMformativeDbContext(DbContextOptions<BIMformativeDbContext> options)
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
