using BIMformative.DynamoExtension.Infrastructure;
using BIMformative.DynamoExtension.Models;
using BIMformative.DynamoExtension.Services;
using BIMformative.DynamoExtension.Services.Exceptions;
using BIMformative.DynamoExtension.Services.Interfaces;
using BIMformative.DynamoExtension.Services.Script;
using BIMformative.DynamoExtension.UI.ViewModels.Base;
using BIMformative.DynamoExtension.UI.Views;
using BIMformative.DynamoExtension.UI.Views.Controls;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace BIMformative.DynamoExtension.UI.ViewModels.Scripts
{
    public class ScriptsListViewModel : ViewModelBase
    {
        private readonly IScriptService _scriptService;
        private readonly IScriptLoadService _scriptLoadService;
        private readonly IScriptCompareService _compareService;
        private readonly Func<ScriptRowViewModel, Task> _loadScriptAsync;
        private readonly Action<ScriptRowViewModel> _viewDetailsAction;
        private readonly Action<ScriptDetailsViewModel> _closeDetailsAction;
        private readonly IDialogService _dialogService;

        private readonly DispatcherTimer _timer;

        private ViewState _currentState;
        public ViewState CurrentState
        {
            get => _currentState;
            set => SetProperty(ref _currentState, value);
        }

        private ViewState _myCurrentState;
        public ViewState MyCurrentState
        {
            get => _myCurrentState;
            set => SetProperty(ref _myCurrentState, value);
        }

        private ViewState _detailState;
        public ViewState DetailState
        {
            get => _detailState;
            set => SetProperty(ref _detailState, value);
        }
        

        private ScriptRowViewModel? _selectedScript;
        public ScriptRowViewModel? SelectedScript
        {
            get => _selectedScript;
            set => SetProperty(ref _selectedScript, value);
        }

        private MyScriptRowViewModel? _mySelectedScript;
        public MyScriptRowViewModel MySelectedScript
        {
            get => _mySelectedScript;
            set => SetProperty(ref _mySelectedScript, value);
        }

        private ScriptDetailsViewModel? _details;
        public ScriptDetailsViewModel? SelectedDetails
        {
            get => _details;
            set => SetProperty(ref _details, value);
        }

        private bool _isDetailOpen;
        public bool IsDetailOpen
        {
            get => _isDetailOpen;
            set => SetProperty(ref _isDetailOpen, value);
        }

        public ICommand ViewScriptDetailsCommand { get; }
        public ICommand CloseDetailsCommand { get; }


        private CancellationTokenSource? _cts;

        private const int PageSize = 20;

        public ScriptsListViewModel(
            IScriptService scriptService, 
            Func<ScriptRowViewModel, Task> loadScriptAsync, 
            Action<ScriptRowViewModel> viewDetailsAction, 
            IScriptLoadService loader, 
            IScriptCompareService compareService,
            IDialogService dialogService)
        {
            _scriptService = scriptService ?? throw new ArgumentNullException(nameof(scriptService));
            _scriptLoadService = loader ?? throw new ArgumentNullException(nameof(loader));
            _loadScriptAsync = loadScriptAsync ?? throw new ArgumentNullException(nameof(loadScriptAsync));
            _viewDetailsAction = viewDetailsAction ?? throw new ArgumentNullException(nameof(viewDetailsAction));
            _compareService = compareService ?? throw new ArgumentNullException(nameof(compareService));
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            

            Scripts = new ObservableCollection<ScriptRowViewModel>();
            MyScripts = new ObservableCollection<MyScriptRowViewModel>();

            LoadFirstPageCommand = new RelayCommand(async () => await LoadFirstPageAsync());
            LoadNextPageCommand = new RelayCommand(async () => await LoadNextPageAsync(), () => CanLoadNextPage);
            ChangeSortCommand = new RelayCommand<ScriptSortField>(ChangeSort);
            ViewScriptDetailsCommand = new RelayCommand<ScriptRowViewModel>(OpenDetails);
            //LoadMyScriptsCommand = new RelayCommand(async () => await LoadMyScriptsAsync());

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMinutes(1)
            };

            _timer.Tick += (s, e) =>
            {
                OnPropertyChanged(nameof(MyScripts));
            };

            _timer.Start();
        }

        

        private async void OpenDetails(ScriptRowViewModel script)
        {
            IsDetailOpen = true;

            SelectedDetails = new ScriptDetailsViewModel(script.Slug, _scriptService, _scriptLoadService, _compareService);

            await SelectedDetails.InitializeAsync();            
        }

        private async void OpenDetails(MyScriptRowViewModel myScript)
        {
            MySelectedScript = myScript;
            IsDetailOpen = true;

            SelectedDetails = new ScriptDetailsViewModel(myScript.Slug, _scriptService, _scriptLoadService, _compareService);

            await SelectedDetails.InitializeAsync();
        }

        /* ------- DATA -------*/
        public ObservableCollection<ScriptRowViewModel> Scripts { get; }
        public ObservableCollection<MyScriptRowViewModel> MyScripts { get; }

        

        private int _page = 1;
        private int _totalPages = 1;

        private int _totalScripts = 0;
        public int TotalScripts
        {
            get => _totalScripts;
            set => SetProperty(ref _totalScripts, value);
        }

        private int _myScriptsTotal = 0;
        public int MyScriptsTotal
        {
            get => _myScriptsTotal;
            set => SetProperty(ref _myScriptsTotal, value);
        }

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
        public ICommand LoadMyScriptsCommand { get; }


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
                CurrentState = ViewState.Loading;

                var result = await _scriptService.GetPublicAsync(
                    page: _page,
                    limit: PageSize,
                    search: SearchText,
                    scriptType: ScriptType,
                    sortField: SortField,
                    sortOrder: SortOrder,
                    cancellationToken: _cts.Token);

                _totalPages = result.TotalPages;
                _totalScripts = result.Total;

                RaisePropertyChanged(nameof(TotalScripts));

                foreach (var dto in result.Data)
                {                    
                    Scripts.Add(new ScriptRowViewModel(
                        dto,
                        loadAction: _loadScriptAsync,
                        viewDetailsAction: OpenDetails));
                }

                CurrentState = Scripts.Any()
                    ? ViewState.Loaded
                    : ViewState.Empty;
            }
            catch (OperationCanceledException)
            {
                CurrentState = ViewState.Error;
            }
            catch (ApiUnavailableException)
            {
                Scripts.Clear();
                CurrentState = ViewState.ApiUnavailable;
            }
            catch (Exception ex)
            {
                CurrentState = ViewState.Error;
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task LoadMyScriptsAsync(CancellationToken externalToken = default)
        {
            CancelRequest();

            _cts = CancellationTokenSource.CreateLinkedTokenSource(externalToken);

            try
            {
                MyCurrentState = ViewState.Loading;
                var result = await _scriptService.GetMyScriptsAsync(
                    search: SearchText,
                    scriptType: ScriptType,
                    cancellationToken: _cts.Token);

                _myScriptsTotal = result.Count;

                RaisePropertyChanged(nameof(MyScriptsTotal));

                foreach (var dto in result)
                {
                    MyScripts.Add(new MyScriptRowViewModel(
                        dto,
                        _scriptService,
                        viewDetailsAction: OpenDetails,
                        editAction: OnEditScript,
                        loadAction: OnLoadScriptAsync,
                        deletedCallback: OnScriptDeleted,
                        uploadVersionAction: OnUploadVersion));
                }

                MyCurrentState = MyScripts.Any()
                    ? ViewState.Loaded
                    : ViewState.Empty;
            }
            catch (OperationCanceledException)
            {
                MyCurrentState = ViewState.Error;
            }
            catch (UnauthorizedAccessException)
            {
                MyCurrentState = ViewState.NotAuthenticated;
            }
            catch (ApiUnavailableException)
            {
                MyCurrentState = ViewState.ApiUnavailable;
            }
            catch (Exception ex)
            {
                MyCurrentState = ViewState.Error;
            }            
        }

        private async Task OnEditScript(MyScriptRowViewModel row)
        {
            var slug = row.Slug;

            var vm = new EditScriptViewModel(slug, _scriptService, _scriptLoadService, _compareService, _dialogService);

            vm.EditTitle = row.Title;
            vm.EditDescription = row.Description;

            await vm.InitializeEditAsync();

            var window = new EditScriptDialog
            {
                DataContext = vm
            };

            _dialogService.ShowDialog(window);
        }

        private void OnScriptDeleted(MyScriptRowViewModel row)
        {
            MyScripts.Remove(row);
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

        public async Task ReloadAsync()
        {
            Scripts.Clear();
            _page = 1;

            await LoadPageAsync(_page);
        }

        public async Task ApplySearchAsync(string search)
        {
            SearchText = search;
            await ReloadAsync();
        }

        public async Task ApplyScriptTypeFilterAsync(string scriptType)
        {
            ScriptType = scriptType;
            await ReloadAsync();
        }

        public async Task ApplySortAsync(string field, string order)
        {
            SortField = Enum.Parse<ScriptSortField>(field);
            SortOrder = Enum.Parse<SortOrder>(order);

            await ReloadAsync();
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

        private async Task OnLoadScriptAsync(MyScriptRowViewModel script)
        {
            if (script == null || !script.CanLoad || script.IsLoading) return;

            script.IsLoading = true;

            try
            {
                await _scriptLoadService.LoadScriptAsync(script.GetScriptDto(), CancellationToken.None);
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

        private void OnUploadVersion(MyScriptRowViewModel row)
        {
            var vm = new UploadVersionViewModel(row.Slug, _scriptService);

            var dialog = new UploadVersionDialog
            {
                DataContext = vm
            };

            vm.RequestClose += () => dialog.Close();

            _dialogService.ShowDialog(dialog);
        }

        public void ClearScripts()
        {
            Scripts.Clear();
            CurrentState = ViewState.Empty;
        }

        public void ClearMyScripts()
        {
            MyScripts.Clear();
            MyCurrentState = ViewState.Empty;
        }

    }
}
