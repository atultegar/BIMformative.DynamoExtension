using BIMformative.DynamoExtension.Infrastructure;
using BIMformative.DynamoExtension.Models;
using BIMformative.DynamoExtension.Models.Scripts;
using BIMformative.DynamoExtension.UI.ViewModels.Base;
using System;
using System.Windows.Input;

namespace BIMformative.DynamoExtension.UI.ViewModels.Scripts
{
    public class DownloadedScriptItemViewModel : ViewModelBase
    {
        public DownloadedScript Model { get; } = new DownloadedScript();

        // Commands
        public ICommand OpenDirectoryCommand { get; }
        public ICommand LoadCommand { get; }
        public ICommand CompareVersionCommand { get; }
        public ICommand DeleteCommand { get; }


        public DownloadedScriptItemViewModel(
            DownloadedScript model,
            Action<DownloadedScriptItemViewModel>? onOpenDirectory = null,
            Action<DownloadedScriptItemViewModel>? onLoad = null,
            Action<DownloadedScriptItemViewModel>? onCompare = null,
            Action<DownloadedScriptItemViewModel>? onDelete = null)
        {
            Model = model ?? throw new ArgumentNullException(nameof(model));

            OpenDirectoryCommand = new RelayCommand(
                () => onOpenDirectory?.Invoke(this),
                () => !string.IsNullOrWhiteSpace(Model.LocalPath));

            LoadCommand = new RelayCommand(
                () => onLoad?.Invoke(this),
                () => System.IO.File.Exists(Model.LocalPath));

            CompareVersionCommand = new RelayCommand(
                () => onCompare?.Invoke(this),
                () => Model.SyncStatus is
                    ScriptSyncStatus.UpdateAvailable or
                    ScriptSyncStatus.ModifiedLocally or
                    ScriptSyncStatus.Conflict);

            DeleteCommand = new RelayCommand(
                () => onDelete?.Invoke(this),
                () => true);
            
        }

        public string Slug => Model.Slug;
        public string Title => Model.Title;
        public string ScriptType => Model.ScriptType;
        public string DownloadedVersion => Model.DownloadedVersion;
        public string? LatestVersion => Model.LatestVersion;

        public string DownloadedAt => Model.DownloadedAt.ToString("dd MMM yyyy HH:mm");
        public string FilePath => Model.LocalPath;

        public ScriptSyncStatus SyncStatus => Model.SyncStatus;

        public string StatusText => Model.SyncStatus switch
        {
            ScriptSyncStatus.ModifiedLocally => "Modified Locally",
            ScriptSyncStatus.UpdateAvailable => "Update Available",
            ScriptSyncStatus.Conflict => "Conflict",
            ScriptSyncStatus.UpToDate => "Up To Date",
            ScriptSyncStatus.MissingFile => "Missing File",
            _ => "Downloaded"
        };
    }
}
