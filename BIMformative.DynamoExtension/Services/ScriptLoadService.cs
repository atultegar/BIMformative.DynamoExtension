using BIMformative.Core.Interfaces;
using BIMformative.Core.Models;
using BIMformative.Core.Models.Scripts;
using BIMformative.DynamoExtension.Services.Interfaces;
using Microsoft.Web.WebView2.Core;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

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

        public async Task<ScriptAnalyzeResponseDto> AnalyzeWorkspaceAsync(CancellationToken ct = default)
        {
            var model = _dynamo.Model;
            var vm = _dynamo.ViewModel;

            var workspace = model.CurrentWorkspace
                ?? throw new InvalidOperationException("No active workspace");

            ct.ThrowIfCancellationRequested();

            // Handle unsaved changes on UI thread
            if (workspace.HasUnsavedChanges)
            {
                bool canContinue = false;

                await _dynamo.Window.Dispatcher.InvokeAsync(() =>
                {
                    canContinue = vm.AskUserToSaveWorkspaceOrCancel(model.CurrentWorkspace);
                });

                if (!canContinue)
                    throw new OperationCanceledException("User cancelled save operation.");
            }

            // Ensure file is saved
            if (string.IsNullOrWhiteSpace(workspace.FileName))
                throw new InvalidOperationException("Workspace must ne saved before anlyzing.");

            ct.ThrowIfCancellationRequested();

            // Delegate to AnalyzeAsync
            return await _scriptService.AnalyzeAsync(workspace.FileName, ct);
        }

        public async Task<ScriptVersionDto> UploadVersionFromWorkspaceAsync(string slug, string changeLog = "", CancellationToken ct = default)
        {
            var parsed = await AnalyzeWorkspaceAsync();

            return await _scriptService.PublishVersionAsync(slug, parsed, changeLog, ct);
        }

        public bool HasOpenWorkspace()
        {
            return _dynamo.HasOpenWorkspace;
        }
    }
}
