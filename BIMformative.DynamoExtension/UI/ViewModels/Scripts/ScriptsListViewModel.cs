using BIMformative.DynamoExtension.Infrastructure;
using BIMformative.DynamoExtension.Models;
using BIMformative.DynamoExtension.Services.Exceptions;
using BIMformative.DynamoExtension.Services.Interfaces;
using BIMformative.DynamoExtension.UI.ViewModels.Base;
using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Reflection.Metadata;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BIMformative.DynamoExtension.UI.ViewModels.Scripts
{
    public class ScriptsListViewModel : ViewModelBase
    {
        private readonly IScriptApiClient _scriptApiClient;
        private readonly IScriptLoadService _scriptLoadService;
        private readonly Func<ScriptRowViewModel, Task> _loadScriptAsync;

        private CancellationTokenSource? _cts;

        private const int PageSize = 20;

        public ScriptsListViewModel(IScriptApiClient scriptApiClient, Func<ScriptRowViewModel, Task> loadScripAsync)
        {
            _scriptApiClient = scriptApiClient ?? throw new ArgumentNullException(nameof(scriptApiClient));
            _loadScriptAsync = loadScripAsync ?? throw new ArgumentNullException(nameof(loadScripAsync));

            Scripts = new ObservableCollection<ScriptRowViewModel>();

            LoadFirstPageCommand = new RelayCommand(async () => await LoadFirstPageAsync());
            LoadNextPageCommand = new RelayCommand(async () => await LoadNextPageAsync(), () => CanLoadNextPage);
            ChangeSortCommand = new RelayCommand<ScriptSortField>(ChangeSort);
        }

        /* ------- DATA -------*/
        public ObservableCollection<ScriptRowViewModel> Scripts { get; }

        private int _page = 1;
        private int _totalPages = 1;
        public bool CanLoadNextPage => 
            !IsLoading && _page < _totalPages;

        /* ------- FILTER / SORT -------*/        
        public string? SearchText { get; set; }
        public string? ScriptType { get; set; }

        private ScriptSortField _sortField = ScriptSortField.updated_at;
        public ScriptSortField SortField 
        {
            get => _sortField; 
            set => SetProperty(ref _sortField, value);
        }

        private SortOrder _sortOrder = SortOrder.desc;
        public SortOrder SortOrder
        {
            get => _sortOrder;
            set => SetProperty(ref _sortOrder, value);
        }

        /* ------- STATE -------*/
        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            private set
            {
                if (SetProperty(ref _isLoading, value))
                {
                    RaisePropertyChanged(nameof(CanLoadNextPage));
                    (LoadNextPageCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        /* ------- COMMANDS -------*/
        public ICommand LoadFirstPageCommand { get; }
        public ICommand LoadNextPageCommand { get; }
        public ICommand ChangeSortCommand { get; }


        /* ------- LOAD -------*/
        
        private async Task LoadFirstPageAsync()
        {
            CancelRequest();
            _page = 1;
            _totalPages = 1;
            Scripts.Clear();

            await LoadPageAsync(_page);
        }

        private async Task LoadNextPageAsync()
        {
            if (!CanLoadNextPage) return;
            await LoadPageAsync(++_page);
        }

        public async Task LoadNextPageIfNeededAsync()
        {
            if (!CanLoadNextPage || IsLoading)
                return;

            await LoadNextPageAsync();
        }

        private async Task LoadPageAsync(int page, CancellationToken externalToken = default)
        {
            CancelRequest();

            _cts = CancellationTokenSource.CreateLinkedTokenSource(externalToken);
            IsLoading = true;

            try
            {
                var result = await _scriptApiClient.GetPublicScriptsAsync(
                    page: _page,
                    limit: PageSize,
                    search: SearchText,
                    scriptType: ScriptType,
                    sortField: SortField,
                    sortOrder: SortOrder,
                    cancellationToken: _cts.Token);

                _totalPages = result.TotalPages;

                foreach (var dto in result.Data)
                {                    
                    Scripts.Add(new ScriptRowViewModel(
                        dto,
                        loadAction: _loadScriptAsync));
                }
            }
            catch (OperationCanceledException)
            {

            }
            catch (ApiUnavailableException)
            {
                Scripts.Clear();
            }
            catch (Exception ex)
            {
                
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void CancelRequest()
        {
            var cts = _cts;
            if (cts == null)
                return;

            _cts = null;

            try
            {
                if (!cts.IsCancellationRequested)
                    cts.Cancel();
            }
            finally
            {
                cts.Dispose();
            }
        }

        public async Task LoadFirstPageAsync(CancellationToken externalToken)
        {
            CancelRequest();

            _page = 1;
            _totalPages = 1;
            Scripts.Clear();

            await LoadPageAsync(_page, externalToken);
        }

        private void CancelInFlightRequest()
        {
            if (_cts != null)
            {
                _cts.Cancel();
                _cts.Dispose();
                _cts = null;
            }
        }

        /* ------- ACTIONS -------*/
        private void DownloadScript(ScriptRowViewModel script)
        {
            if (script == null || !script.CanLoad) return;

            // TODO: Add actual download logic here

            script.MarkAsLoaded();         
        }

        private void ShowVersions(ScriptRowViewModel script)
        {
            if (script == null) return;

            // TODO: Open version history dialog for this script
        }

        private void ChangeSort(ScriptSortField field)
        {
            if (SortField == field) return;

            SortField = field;
            LoadFirstPageCommand.Execute(null);
        }

        private async Task OnLoadScriptAsync(ScriptRowViewModel script)
        {
            if (script == null || !script.CanLoad || script.IsLoading) return;

            script.IsLoading = true;

            try
            {
                await _scriptLoadService.LoadScriptAsync(script.GetDto(), CancellationToken.None);
                script.MarkAsLoaded();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Failed to load script: {ex.Message}");
            }
            finally
            {
                script.IsLoading = false;
            }
        }

    }
}
