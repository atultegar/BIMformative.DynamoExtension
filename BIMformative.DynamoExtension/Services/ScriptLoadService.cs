using BIMformative.DynamoExtension.Db;
using BIMformative.DynamoExtension.Models;
using BIMformative.DynamoExtension.Models.Scripts;
using BIMformative.DynamoExtension.Services.Auth;
using BIMformative.DynamoExtension.Services.Interfaces;
using BIMformative.DynamoExtension.Services.Script;
using Dynamo.Wpf.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace BIMformative.DynamoExtension.Services
{
    public class ScriptLoadService : IScriptLoadService
    {
        private readonly IDynamoContext _dynamo;
        private readonly IDownloadedScriptsService _downloadedScriptsService;
        private readonly IScriptService _scriptService;

        public ScriptLoadService(
            IDynamoContext dynamo,
            IScriptService scriptService,
            IDownloadedScriptsService downloadedScriptsService)
        {
            _dynamo = dynamo ?? throw new ArgumentNullException(nameof(dynamo));
            _scriptService = scriptService ?? throw new ArgumentNullException(nameof(scriptService));
            _downloadedScriptsService = downloadedScriptsService ?? throw new ArgumentNullException(nameof(downloadedScriptsService));
        }

        public async Task<bool> LoadScriptAsync(ScriptDto script, CancellationToken ct = default)
        {
            if (!await EnsureWorkspaceCanCloseAsync())
                return false;

            string filePath = await _scriptService.DownloadAsync(script, ct);

            var latestInfo = await _scriptService.GetLatestInfoAsync(script.Slug, ct);

            await _downloadedScriptsService.AddOrUpdateAsync(
                new DownloadedScript
                {
                    Id = script.Id.ToString(),
                    Slug = script.Slug,
                    Title = script.Title,
                    ScriptType = script.Script_Type,
                    DownloadedVersion = $"V{script.Current_Version_Number.ToString()}",
                    LocalPath = filePath,
                    DownloadedAt = DateTime.UtcNow,
                    LatestVersion = $"V{script.Current_Version_Number.ToString()}",
                    LastCheckedAt = DateTime.UtcNow,
                    DownloadedHash = latestInfo?.Hash ?? "",
                    CurrentLocalHash = latestInfo?.Hash ?? "",
                    SyncStatus = ScriptSyncStatus.Downloaded,
                    LastLocalFileWriteTime = DateTime.UtcNow
                });
                        

            if (!File.Exists(filePath))
                throw new FileNotFoundException("Download script not found", filePath);


            // 5. Load script
            await _dynamo.Window.Dispatcher.InvokeAsync(() =>
            {
                _dynamo.Model.ClearCurrentWorkspace();
                _dynamo.ViewModel.OpenCommand.Execute(filePath);
            });

            return true;
        }

        public async Task<bool> LoadScriptFileAsync(DownloadedScript model)
        {
            if (!await EnsureWorkspaceCanCloseAsync()) return false;

            var filePath = model.LocalPath;

            if (!File.Exists(filePath)) 
                throw new FileNotFoundException("Downloaded script not found", filePath);

            await _dynamo.Window.Dispatcher.InvokeAsync(() =>
            {
                _dynamo.Model.ClearCurrentWorkspace();
                _dynamo.ViewModel.OpenCommand.Execute(filePath);
            });

            return true;
        }

        public async Task<bool> DownloadLatestFileAsync(DownloadedScript script, CancellationToken ct = default)
        {
            string filePath = await _scriptService.DownloadLatestAsync(script);

            var latestInfo = await _scriptService.GetLatestInfoAsync(script.Slug, ct);

            await _downloadedScriptsService.AddOrUpdateAsync(
                new DownloadedScript
                {
                    Id = script.Id,
                    Slug = script.Slug,
                    Title = script.Title,
                    ScriptType = script.ScriptType,
                    DownloadedVersion = $"V{latestInfo?.Current_Version_Number.ToString()}",
                    LocalPath = filePath,
                    DownloadedAt = DateTime.UtcNow,
                    LatestVersion = $"V{latestInfo?.Current_Version_Number.ToString()}",
                    LastCheckedAt = DateTime.UtcNow,
                    DownloadedHash = latestInfo?.Hash ?? "",
                    CurrentLocalHash = latestInfo?.Hash ?? "",
                    SyncStatus = ScriptSyncStatus.Downloaded,
                    LastLocalFileWriteTime = DateTime.UtcNow
                });

            return true;
        }

        private async Task<bool> EnsureWorkspaceCanCloseAsync()
        {
            var model = _dynamo.Model;
            var vm = _dynamo.ViewModel;

            if (!model.CurrentWorkspace.HasUnsavedChanges)
                return true;

            bool canContinue = false;

            // Ensure we run on UI thread (STA)
            await _dynamo.Window.Dispatcher.InvokeAsync(() =>
            {
                canContinue = vm.AskUserToSaveWorkspaceOrCancel(model.CurrentWorkspace);

            });

            return canContinue;
        }

        

    }
}
