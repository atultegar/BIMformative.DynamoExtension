using BIMformative.DynamoExtension.Models;
using BIMformative.DynamoExtension.Services.Auth;
using BIMformative.DynamoExtension.Services.Interfaces;
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
        private readonly IAuthService _auth;
        private readonly IScriptDownloadService _downloader;

        public ScriptLoadService(
            IDynamoContext dynamo,
            IAuthService auth,
            IScriptDownloadService downloader)
        {
            _dynamo = dynamo ?? throw new ArgumentNullException(nameof(dynamo));
            _auth = auth ?? throw new ArgumentNullException(nameof(auth));
            _downloader = downloader ?? throw new ArgumentNullException(nameof(downloader));
        }

        public async Task<bool> LoadScriptAsync(ScriptDto script, CancellationToken ct = default)
        {
            if (!await EnsureWorkspaceCanCloseAsync())
                return false;

            string filePath = await _downloader.DownloadAsync(script, _auth.AccessToken, ct);

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
