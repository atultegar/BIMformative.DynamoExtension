using BIMformative.Core.Interfaces;
using BIMformative.DynamoExtension.Infrastructure;
using BIMformative.DynamoExtension.Services.Interfaces;
using BIMformative.DynamoExtension.UI.ViewModels.Scripts;
using BIMformative.DynamoExtension.UI.Views.Controls;
using Dynamo.Wpf.Utilities;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace BIMformative.DynamoExtension.UI.ViewModels.Tabs
{
    public class InstalledTabViewModel : TabItemViewModel
    {
        private readonly IDownloadedScriptsService _service;
        private readonly IScriptLoadService _loader;
        private readonly IScriptCompareService _scriptCompareService;
        private bool _isBusy;
        public event Action? RequestClose;
        private readonly IDialogService _dialogService;

        private CancellationTokenSource? _searchCts;

        public ObservableCollection<DownloadedScriptItemViewModel> Scripts { get; } = new();

        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        private string _searchText;

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    DebounceSearch();
                }
            }
        }

        private async void DebounceSearch()
        {
            _searchCts?.Cancel();
            _searchCts = new CancellationTokenSource();
            var token = _searchCts.Token;

            try
            {
                await Task.Delay(350, token);

                if (!token.IsCancellationRequested)
                {
                    ScriptsView.Refresh();
                }
            }
            catch (TaskCanceledException)
            {
                // Ignore
            }
        }

        public ICollectionView ScriptsView { get; }


        public ICommand RefreshCommand { get; }
        public ICommand CheckForUpdatesCommand { get; }
        public ICommand DeleteCommand { get; }

        public InstalledTabViewModel(IDownloadedScriptsService service, IScriptLoadService loader, IScriptCompareService scriptCompareService, IDialogService dialogService)
            : base (
                  header: "Downloaded Scripts",
                  contentFactory: () => new InstalledScriptsControl())
        {
            if (Content is InstalledScriptsControl control)
                control.DataContext = this;

            _service = service;
            _loader = loader;
            _scriptCompareService = scriptCompareService;
            _dialogService = dialogService;

            ScriptsView = CollectionViewSource.GetDefaultView(Scripts);
            ScriptsView.Filter = FilterScripts;

            RefreshCommand = new AsyncRelayCommand(LoadAsync);
            CheckForUpdatesCommand = new AsyncRelayCommand(CheckUpdatesAsync);

            RefreshCommand.Execute(this);
        }

        private bool FilterScripts(object obj)
        {
            if (obj is not DownloadedScriptItemViewModel item)
                return false;

            if (string.IsNullOrWhiteSpace(SearchText))
                return true;

            return item.Title.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
                || item.Slug.Contains(SearchText, StringComparison.OrdinalIgnoreCase);
        }

        private async Task LoadAsync()
        {
            IsBusy = true;
            Scripts.Clear();

            var items = await _service.GetAllAsync();

            foreach ( var item in items )
            {
                Scripts.Add(new DownloadedScriptItemViewModel(
                    item, 
                    OnOpenDirectory, 
                    OnLoad, 
                    OnCompare, 
                    OnDelete));
            }

            IsBusy = false;

            ScriptsView.Refresh();
        }

        private async void OnDelete(DownloadedScriptItemViewModel item)
        {
            if (MessageBoxService.Show(
                $"Delete '{item.Title}'?",
                "Confirm",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                await _service.DeleteAsync(item.Model);
                Scripts.Remove(item);
            }
        }

        private async void OnCompare(DownloadedScriptItemViewModel item)
        {
            await _scriptCompareService.OpenCompareAsync(item, LoadAsync);            
        }

        private async void OnLoad(DownloadedScriptItemViewModel item)
        {
            if (item.Model == null) return;
            try
            {
                var success = await _loader.LoadScriptFileAsync(item.Model);

                if (!success) return;

                RequestClose?.Invoke();
            }
            catch (Exception ex)
            {
                MessageBoxService.Show(                    
                    $"Failed to load script:\n{ex.Message}",
                    "BIMformative",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }            
        }

        private void OnOpenDirectory(DownloadedScriptItemViewModel item)
        {
            var folder = System.IO.Path.GetDirectoryName(item.FilePath);
            if (!string.IsNullOrEmpty(folder))
                System.Diagnostics.Process.Start("explorer.exe", folder);
        }

        private async Task CheckUpdatesAsync()
        {
            IsBusy = true;
            await _service.CheckForUpdateAsync();
            await LoadAsync();
            IsBusy = false;
        }

    }
}
