using BIMformative.DynamoExtension.Db;
using BIMformative.DynamoExtension.Models;
using BIMformative.DynamoExtension.Models.Scripts;
using BIMformative.DynamoExtension.Services.Interfaces;
using BIMformative.DynamoExtension.Services.Script;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BIMformative.DynamoExtension.Services
{
    public class DownloadedScriptsService : IDownloadedScriptsService
    {
        private readonly DownloadedScriptRepository _repo;
        private readonly IScriptService _scriptService;
        private readonly BimformativeDbContext _db;

        public DownloadedScriptsService(BimformativeDbContext db, IScriptService scriptService)
        {
            _db = db;
            _scriptService = scriptService;            
        }

        public async Task CheckForUpdateAsync(CancellationToken ct = default)
        {
            var scripts = await GetAllAsync();

            foreach (var script in scripts)
            {
                if (!File.Exists(script.LocalPath))
                    continue;

                // Detect local file change via LastWriteTime
                var lastWriteTime = File.GetLastWriteTimeUtc(script.LocalPath);

                if (script.DownloadedAt != lastWriteTime)
                {
                    var json = await File.ReadAllTextAsync(script.LocalPath);

                    var localHash = await _scriptService.GetHashAsync(json, ct);

                    script.CurrentLocalHash = localHash;
                    script.LastLocalFileWriteTime = lastWriteTime;
                }

                // Get remote hash
                var latestInfo = await _scriptService.GetLatestInfoAsync(script.Slug, ct);

                // Determine script status
                script.SyncStatus = DetermineStatus(script.DownloadedHash, script.CurrentLocalHash, latestInfo?.Hash ?? "");
                
                script.LatestVersion = $"V{latestInfo?.Current_Version_Number.ToString()}";

                script.LastCheckedAt = DateTime.UtcNow;                
            }
            await _db.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(DownloadedScript script)
        {
            if (File.Exists(script.LocalPath))
                File.Delete(script.LocalPath);

            var entity = await _db.DownloadedScripts.FindAsync(script.Id);
            if (entity == null) return;

            _db.DownloadedScripts.Remove(entity);
            await _db.SaveChangesAsync();
        }

        public async Task<List<DownloadedScript>> GetAllAsync()
            => await _db.DownloadedScripts
                .OrderByDescending(x => x.DownloadedAt)
                .ToListAsync();

        public async Task AddOrUpdateAsync(DownloadedScript script)
        {
            var existing = await _db.DownloadedScripts
                .FirstOrDefaultAsync(x => x.Id == script.Id);

            if (existing == null)
            {
                script.DownloadedAt = DateTime.UtcNow;
                _db.DownloadedScripts.Add(script);
            }
            else
            {
                existing.Title = script.Title;
                existing.LocalPath = script.LocalPath;
                existing.DownloadedVersion = script.DownloadedVersion;
                existing.DownloadedAt = script.DownloadedAt;
                existing.LatestVersion = script.LatestVersion;
                existing.LastCheckedAt = DateTime.UtcNow;
                existing.DownloadedHash = script.DownloadedHash;
                existing.CurrentLocalHash = script.CurrentLocalHash;
                existing.SyncStatus = script.SyncStatus;
                existing.LastLocalFileWriteTime = script.LastLocalFileWriteTime;
            }
            await _db.SaveChangesAsync();
        }

        public Task<bool> ExistsAsync(string scriptId)
            => _db.DownloadedScripts.AnyAsync(x => x.Id == scriptId);

        public Task MarkUpdateAvailableAsync(string scriptId, string latestVersion)
        {
            throw new NotImplementedException();
        }

        public ScriptSyncStatus DetermineStatus(
            string downloadedHash,
            string currentLocalHash,
            string remoteHash)
        {
            if (currentLocalHash == downloadedHash &&
                downloadedHash == remoteHash)
                return ScriptSyncStatus.UpToDate;

            if (currentLocalHash != downloadedHash &&
                downloadedHash == remoteHash)
                return ScriptSyncStatus.ModifiedLocally;

            if (currentLocalHash == downloadedHash &&
                downloadedHash != remoteHash)
                return ScriptSyncStatus.UpdateAvailable;

            return ScriptSyncStatus.Conflict;
        }
    }
}
