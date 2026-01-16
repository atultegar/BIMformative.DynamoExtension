using BIMformative.DynamoExtension.Infrastructure;
using BIMformative.DynamoExtension.Models;
using BIMformative.DynamoExtension.Services;
using BIMformative.DynamoExtension.UI.ViewModels.Scripts;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Printing;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BIMformative.DynamoExtension.UI.ViewModels
{
    public class ScriptBrowserViewModel : INotifyPropertyChanged
    {
        public ICommand DownloadCommand { get; }
        public ICommand VersionsCommand { get; }

        private readonly ScriptCatalogService _scriptService = new();

        public ObservableCollection<ScriptRowViewModel> Scripts { get; } = new();

        private ScriptDto? _selectedScript;
        public ScriptDto? SelectedScript
        {
            get => _selectedScript;
            set
            {
                _selectedScript = value;
                OnPropertyChanged();
            }
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            private set
            {
                _isLoading = value; 
                OnPropertyChanged(); 
            }
        }

        private string _errorMessage;
        public string ErrorMessage
        {
            get => _errorMessage;
            private set
            {
                _errorMessage = value; 
                OnPropertyChanged(); 
            }
        }

        public ScriptBrowserViewModel()
        {
            DownloadCommand = new RelayCommand<ScriptDto>(OnDownload);
            VersionsCommand = new RelayCommand<ScriptDto>(OnVersions);

            _ = LoadScriptAsync();
        }

        private async Task LoadScriptAsync()
        {
            //try
            //{
            //    IsLoading = true;
            //    ErrorMessage = null;

            //    Scripts.Clear();

            //    var scripts = await _scriptService.GetScriptsAsync();

            //    foreach (var script in scripts)
            //        Scripts.Add(new ScriptRowViewModel(script));
            //}
            //catch
            //{
            //    ErrorMessage = "Failed to load scripts";
            //}
            //finally
            //{
            //    IsLoading = false;
            //}
        }

        private void OnDownload(ScriptDto script)
        {
            // TODO: Download .dyn and load into Dynamo
        }

        private void OnVersions(ScriptDto script)
        {
            // TODO: Open version history (Webview / dialog)
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        #endregion
    }

}
