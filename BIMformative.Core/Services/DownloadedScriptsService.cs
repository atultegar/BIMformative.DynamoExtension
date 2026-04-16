using BIMformative.Core.Interfaces;
using BIMformative.Core.Models;
using BIMformative.Core.Models.Scripts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace BIMformative.Core.Services
{
    public class DownloadedScriptsService : IDownloadedScriptsService
    {
        private readonly IDownloadedScriptRepository _repo;
        private readonly IScriptService _scriptService;

        public DownloadedScriptsService(IDownloadedScriptRepository repo, IScriptService scriptService)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _scriptService = scriptService ?? throw new ArgumentNullException(nameof(scriptService));            
        }

        public async Task<List<DownloadedScript>> GetAllAsync()
            => await _repo.GetAllAsync();

        public Task<bool> ExistsAsync(string scriptId)
            => _repo.ExistsAsync(scriptId);

        public async Task AddOrUpdateAsync(DownloadedScript script)
        {
            if (script == null) throw new ArgumentNullException(nameof(script));
            
            var existing = await _repo.GetByIdAsync(script.Id);

            if (existing == null)
            {
                script.DownloadedAt = DateTime.UtcNow;
                script.LastCheckedAt = DateTime.UtcNow;

                await _repo.AddAsync(script);
            }
            else
            {
                UpdateExisting(existing, script);
            }

            await _repo.SaveChangesAsync();
        }

        private void UpdateExisting(DownloadedScript existing, DownloadedScript incoming)
        {
            existing.Title = incoming.Title;
            existing.LocalPath = incoming.LocalPath;
            existing.DownloadedVersion = incoming.DownloadedVersion;
            existing.LatestVersion = incoming.LatestVersion;
            existing.LastCheckedAt = DateTime.UtcNow;
            existing.DownloadedHash = incoming.DownloadedHash;
            existing.CurrentLocalHash = incoming.CurrentLocalHash;
            existing.SyncStatus = incoming.SyncStatus;
            existing.LastLocalFileWriteTime = incoming.LastLocalFileWriteTime;
        }

        public async Task DeleteAsync(DownloadedScript script)
        {
            if (script == null) throw new ArgumentNullException(nameof(script));

            try
            {
                if (File.Exists(script.LocalPath))
                    File.Delete(script.LocalPath);
            }
            catch
            {
                // optional: log
            }
            await _repo.DeleteAsync(script.Id);
            await _repo.SaveChangesAsync();
        }

        public async Task CheckForUpdateAsync(CancellationToken ct = default)
        {
            var scripts = await _repo.GetAllAsync();

            foreach (var script in scripts)
            {
                ct.ThrowIfCancellationRequested();

                try
                {
                    await ProcessScriptAsync(script, ct);
                    await _repo.UpdateAsync(script);
                }
                catch
                {
                    // TODO: log
                    continue;
                }                
            }
            await _repo.SaveChangesAsync(ct);
        }
        
        public async Task ProcessScriptAsync(DownloadedScript script, CancellationToken ct)
        {
            if (!File.Exists(script.LocalPath))
                return;

            await UpdateLocalStateAsync(script, ct);
            await UpdateRemoteStateAsync(script, ct);
            
        }

        private async Task UpdateLocalStateAsync(DownloadedScript script, CancellationToken ct)
        {
            var lastWriteTime = File.GetLastWriteTimeUtc(script.LocalPath);

            if (script.DownloadedAt != lastWriteTime)
            {
                var json = File.ReadAllText(script.LocalPath);
                var hash = await _scriptService.GetHashAsync(json, ct);

                script.CurrentLocalHash = hash;
                script.LastLocalFileWriteTime = lastWriteTime;
            }
        }

        private async Task UpdateRemoteStateAsync(DownloadedScript script, CancellationToken ct)
        {
            var latestInfo = await _scriptService.GetLatestInfoAsync(script.Slug, ct);

            if (latestInfo == null)
                throw new InvalidOperationException($"Script not found: {script.Slug}");

            var remoteHash = latestInfo.Hash ?? string.Empty;

            // Determine script status
            script.SyncStatus = DetermineStatus(script.DownloadedHash, script.CurrentLocalHash, remoteHash);

            script.LatestVersion = $"V{latestInfo?.Current_Version_Number}";
            script.LastCheckedAt = DateTime.UtcNow;            
        }

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
