using BIMformative.Core.Interfaces;
using BIMformative.Core.Models;
using BIMformative.DynamoExtension.Services.Interfaces;
using BIMformative.DynamoExtension.UI.ViewModels;
using BIMformative.DynamoExtension.UI.ViewModels.Scripts;
using BIMformative.DynamoExtension.UI.Views;
using Dynamo.Wpf.Utilities;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;


namespace BIMformative.DynamoExtension.Services.Script
{
    public class ScriptCompareService : IScriptCompareService
    {
        private readonly IAuthService _authService;
        private readonly IScriptLoadService _loader;
        private readonly Uri _baseAddress;
        private readonly IDialogService _dialogService;

        public ScriptCompareService(IAuthService authService, IScriptLoadService loader, Uri baseAddress, IDialogService dialogService)
        {
            _authService = authService;
            _loader = loader;
            _baseAddress = baseAddress;
            _dialogService = dialogService;
        }

        public async Task OpenCompareAsync(DownloadedScriptItemViewModel item, Func<Task> refreshParentList)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            // Only allow compare for supported status
            if (item.SyncStatus != ScriptSyncStatus.ModifiedLocally &&
                item.SyncStatus != ScriptSyncStatus.UpdateAvailable &&
                item.SyncStatus != ScriptSyncStatus.Conflict)
            {
                return; 
            }

            if (!await _authService.EnsureAuthenticatedAsync()) 
                return;

            var comparePath = $"/resources/dynamo-scripts/{item.Slug}/compare";
            var url = new Uri(_baseAddress, comparePath);

            var payload = await BuildComparePayload(item);

            var window = new CompareWindow(url.AbsoluteUri, payload);

            var vm = new CompareWindowViewModel(
                item, 
                _loader, 
                refreshParentList,
                closeAction: () => window.Close()
            );
            window.DataContext = vm;

            _dialogService.ShowDialog(window);
        }

        public async Task OpenVersionCompareAsync(string slug, int leftVersion, int rightVersion)
        {
            if (string.IsNullOrWhiteSpace(slug))
                throw new ArgumentException("Slug cannot be null.", nameof(slug));

            if (leftVersion == rightVersion)
                return;

            try
            {
                if (!await _authService.EnsureAuthenticatedAsync())
                    return;

                var comparePath = $"/resources/dynamo-scripts/{slug}/compare";
                var url = new Uri(_baseAddress, comparePath);

                var payload = new
                {
                    title = slug,
                    left = new { type = "versionNo", value = leftVersion.ToString() },
                    right = new { type = "versionNo", value = rightVersion.ToString() }
                };

                var window = new CompareWindow(url.AbsoluteUri, payload);
                window.UpdateButton.Visibility = Visibility.Collapsed;

                _dialogService.ShowDialog(window);
            }
            catch (Exception ex)
            {
                MessageBoxService.Show(
                    $"Unable to open version compare. \n\n{ex.Message}",
                    "Compare Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private async Task<object> BuildComparePayload(DownloadedScriptItemViewModel item)
        {
            var scriptJson = File.ReadAllText(item.FilePath);
            switch (item.SyncStatus)
            {
                case ScriptSyncStatus.ModifiedLocally:
                    
                    return new
                    {
                        title = item.Title,                        
                        left = new { type = "versionNo", value = item.DownloadedVersion.TrimStart('V') },
                        right = new { type = "local", payload = scriptJson },
                    };

                case ScriptSyncStatus.UpdateAvailable:
                    return new
                    {
                        title = item.Title,
                        left = new { type = "versionNo", value = item.DownloadedVersion.TrimStart('V') },
                        right = new { type = "latest" }
                    };

                case ScriptSyncStatus.Conflict:
                    return new
                    {
                        title = item.Title,
                        left = new { type = "local", payload = scriptJson },
                        right = new { type = "latest" }
                    };

                default:
                    throw new InvalidOperationException(
                        $"Compare not supported for SyncStatus {item.SyncStatus}");
            }
        }
    }
}
