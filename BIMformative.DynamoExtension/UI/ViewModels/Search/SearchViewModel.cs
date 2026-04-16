using BIMformative.Core.Interfaces;
using BIMformative.Core.Models;
using BIMformative.DynamoExtension.Infrastructure;
using BIMformative.DynamoExtension.Services.Interfaces;
using BIMformative.DynamoExtension.UI.ViewModels.Base;
using BIMformative.DynamoExtension.UI.ViewModels.Scripts;
using Dynamo.Wpf.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace BIMformative.DynamoExtension.UI.ViewModels.Search
{
    public class SearchViewModel : ViewModelBase, IDisposable
    {
        private readonly IScriptService _scriptService;
        private readonly IScriptLoadService _loader;
        private readonly IScriptCompareService _compareService;
        private readonly IDialogService _dialogService;

        public ScriptsListViewModel Scripts { get; }

        private CancellationTokenSource? _searchCts;
        public event Action? RequestClose;

        public ICommand CloseDetailsCommand => Scripts.CloseDetailsCommand;

        public ObservableCollection<FilterItemViewModel> Filters { get; } = new();

        public IEnumerable<string> SelectedFilters =>
            Filters.Where(x => x.IsSelected)
            .Select(x => x.Name);

        public SearchViewModel(IScriptService scriptService, IScriptLoadService loader, IScriptCompareService compareService, IDialogService dialogService)
        {
            _scriptService = scriptService ?? throw new ArgumentNullException(nameof(scriptService));
            _loader = loader ?? throw new ArgumentNullException(nameof(loader));
            _compareService = compareService ?? throw new ArgumentNullException(nameof(compareService));
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            
            Filters.Add(new FilterItemViewModel("revit", "Revit"));
            Filters.Add(new FilterItemViewModel("civil3d", "Civil 3D"));

            Scripts = new ScriptsListViewModel(_scriptService, OnLoadScriptAsync, OnViewDetails, loader, _compareService, _dialogService);

            Scripts.LoadFirstPageCommand.Execute(null);
            SortByCommand = new RelayCommand<string>(SetSortBy);
            SortOrderCommand = new RelayCommand<string>(SetSortOrder);

            ClearFiltersCommand = new RelayCommand(ClearFilters);
            ClearSortCommand = new RelayCommand(ClearSort);
            ToggleFilterCommand = new RelayCommand<FilterItemViewModel>(ToggleFilter);

            foreach (var f in Filters)
            {
                f.FilterChanged += OnFilterChanged;
            }
        }

        private string? _searchText;
        public string? SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    DebounceSearchAsync(value);
                }
            }
        }

        private async void DebounceSearchAsync(string? text)
        {
            _searchCts?.Cancel();
            _searchCts = new CancellationTokenSource();
            var token = _searchCts.Token;

            try
            {
                await Task.Delay(350, token); // debounce delay

                await ReloadScriptsAsync();
            }
            catch (TaskCanceledException)
            {

            }
        }

        private ScriptSortField _sortBy = ScriptSortField.updated_at;
        public ScriptSortField SortBy
        {
            get => _sortBy;
            set
            {
                if (SetProperty(ref _sortBy, value))
                    _ = ReloadScriptsAsync();
            }
        }

        private SortOrder _sortOrder = SortOrder.desc;
        public SortOrder SortOrder
        {
            get => _sortOrder;
            set 
            {
                if (SetProperty(ref _sortOrder, value))
                    _ = ReloadScriptsAsync();
            }
        }

        private async Task ReloadScriptsAsync()
        {
            _searchCts?.Cancel();
            _searchCts = new CancellationTokenSource();

            try
            {
                Scripts.SearchText = SearchText;
                Scripts.SortField = SortBy;
                Scripts.SortOrder = SortOrder;

                // Apply selected filters
                var selected = SelectedFilters?.ToList();

                if (selected != null && selected.Any())
                    Scripts.ScriptType = selected.First();

                await Scripts.LoadFirstPageAsync(_searchCts.Token);
            }
            catch (TaskCanceledException) 
            {
                // expected when user types quickly
            }
        }

        private void SetSortBy(string sort)
        {
            SortBy = (ScriptSortField)Enum.Parse(typeof(ScriptSortField), sort);
        }

        private void SetSortOrder(string order)
        {
            SortOrder = (SortOrder)Enum.Parse(typeof(SortOrder), order);
        }

        public ICommand SortByLikesCommand =>
            new RelayCommand<object>(_ => Scripts.ChangeSortCommand.Execute(ScriptSortField.likes_count));

        public ICommand SortByDateCommand =>
            new RelayCommand(() => Scripts.ChangeSortCommand.Execute(ScriptSortField.updated_at));

        public ICommand SortByDownloadsCommand =>
            new RelayCommand(() => Scripts.ChangeSortCommand.Execute(ScriptSortField.downloads_count));

        public ICommand SortByCommand { get;}
        public ICommand SortOrderCommand { get;}
        public ICommand ClearFiltersCommand { get;}
        public ICommand ClearSortCommand { get;}
        public ICommand ToggleFilterCommand { get;}
               

        private void OnViewDetails(ScriptRowViewModel script)
        {
            // Hook point for analytics / logging later
            // UI state already handled by ScriptsListViewModel
        }
        
        private async Task OnLoadScriptAsync(ScriptRowViewModel script)
        {
            if (script == null) return;

            try
            {
                var success = await _loader.LoadScriptAsync(script.GetDto(), CancellationToken.None);

                // User cancelled save dialog -> do nothing
                if (!success) return;

                // Update row UI state
                script.MarkAsLoaded();

                // Close ScriptManager window
                RequestClose?.Invoke();
            }
            catch (Exception ex)
            {
                MessageBoxService.Show(
                    $"Failed to load script:\n{ex.Message}",
                    "Load Error",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
            }
        }

        public void Initialize()
        {
            Scripts.LoadFirstPageCommand.Execute(null);
        }

        public void Dispose()
        {
            _searchCts?.Cancel();
            _searchCts?.Dispose();            
        }

        private void ClearFilters()
        {
            foreach (var f in Filters)
                f.IsSelected = false;

            _ = ReloadScriptsAsync();
        }

        private void ClearSort()
        {
            SortBy = ScriptSortField.updated_at;
            SortOrder = SortOrder.desc;

            _ = ReloadScriptsAsync();
        }

        private void ToggleFilter(FilterItemViewModel filter)
        {
            if (filter == null)
                return;

            filter.IsSelected = false;

            UpdateScriptFilters();

            _ = ReloadScriptsAsync();
        }

        private void UpdateScriptFilters()
        {
            var selected = Filters
                .Where(f => f.IsSelected)
                .Select(f => f.Header)
                .ToList();

            if (!selected.Any())
            {
                Scripts.ScriptType = null;
                return;
            }

            Scripts.ScriptType = selected.First();
        }

        private void OnFilterChanged(FilterItemViewModel filter)
        {
            UpdateScriptFilters();

            _ = ReloadScriptsAsync();
        }
    }
}
