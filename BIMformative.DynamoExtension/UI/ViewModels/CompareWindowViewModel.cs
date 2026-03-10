using BIMformative.DynamoExtension.Infrastructure;
using BIMformative.DynamoExtension.Models;
using BIMformative.DynamoExtension.Services.Auth;
using BIMformative.DynamoExtension.Services.Interfaces;
using BIMformative.DynamoExtension.UI.ViewModels.Base;
using BIMformative.DynamoExtension.UI.ViewModels.Scripts;
using BIMformative.DynamoExtension.UI.Views.Controls;
using Dynamo.Wpf.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace BIMformative.DynamoExtension.UI.ViewModels
{
    public class CompareWindowViewModel : ViewModelBase
    {
        private readonly DownloadedScriptItemViewModel _item;
        private readonly IScriptLoadService _loader;
        private readonly Func<Task> _refreshParentList;
        private readonly Action _closeAction;

        public ICommand UpdateCommand { get; }

        public ScriptSyncStatus syncStatus => _item.SyncStatus;
        public string ScriptName => _item.Title;

        public CompareWindowViewModel(
            DownloadedScriptItemViewModel item,
            IScriptLoadService loader,
            Func<Task> refreshParentList,
            Action closeAction)
        {
            _item = item;
            _loader = loader;
            _refreshParentList = refreshParentList;
            _closeAction = closeAction;

            UpdateCommand = new AsyncRelayCommand(OnUpdateAsync, CanUpdate);
        }

        public bool CanUpdate()
        {
            return _item.SyncStatus == ScriptSyncStatus.UpdateAvailable ||
                _item.SyncStatus == ScriptSyncStatus.Conflict;
        }

        public async Task OnUpdateAsync()
        {
            if (_item.Model == null)
                return;

            try
            {
                var success = await _loader.DownloadLatestFileAsync(_item.Model);

                if (!success)
                    return;

                // Refresh Installed tab
                if (_refreshParentList != null)
                    await _refreshParentList();

                // Close Compare Window
                _closeAction?.Invoke();
            }
            catch (Exception ex)
            {
                MessageBoxService.Show(
                    $"Failed to download script:\n{ex.Message}",
                    "BIMformative",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}
