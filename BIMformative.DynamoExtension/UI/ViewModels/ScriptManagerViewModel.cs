using BIMformative.Core.Interfaces;
using BIMformative.DynamoExtension.Infrastructure;
using BIMformative.DynamoExtension.Services.Interfaces;
using BIMformative.DynamoExtension.UI.ViewModels.Base;
using BIMformative.DynamoExtension.UI.ViewModels.Tabs;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BIMformative.DynamoExtension.UI.ViewModels
{
    public sealed class ScriptManagerViewModel : ViewModelBase, IDisposable
    {
        // ----------------------------------------------------------------
        // Fields
        // ----------------------------------------------------------------

        private readonly IAuthService _auth;     
        private readonly IScriptLoadService _loader;
        private readonly ISettingsService _settings;
        private readonly IScriptService _scriptService;
        private readonly IDownloadedScriptsService _downloadedScriptsService;
        private readonly IScriptCompareService _scriptCompareService;
        private readonly IDialogService _dialogService;

        // ----------------------------------------------------------------
        // Commands
        // ----------------------------------------------------------------
        public ICommand SignInCommand { get; }
        public ICommand SignOutCommand { get; }
        public ICommand CloseCommand { get; }

        // ----------------------------------------------------------------
        // Authentication-bound properties
        // ----------------------------------------------------------------
        private bool _isAuthenticated;
        public bool IsAuthenticated
        {
            get => _isAuthenticated;
            private set => SetProperty(ref _isAuthenticated, value);
        }

        public string UserName =>
            _auth.CurrentUser?.FullName ?? "Unknown User";

        public string AvatarUrl =>
            _auth.CurrentUser?.Avatarurl 
            ?? "pack://application:,,,/BIMformative.DynamoExtension;component/UI/Assets/avatar-placeholder.png";

        public string Email =>
            _auth.CurrentUser?.Email ?? "user@bimformative.com";

        // ----------------------------------------------------------------
        // Tabs
        // ----------------------------------------------------------------

        public ObservableCollection<TabItemViewModel> Tabs { get; }

        private TabItemViewModel _selectedTab;
        public TabItemViewModel SelectedTab
        {
            get => _selectedTab;
            set
            {
                if (SetProperty(ref _selectedTab, value))
                {
                    if (value is IAsyncInitializable asyncVm)
                        _ = asyncVm.InitializeAsync();
                }
            }
        }

        // ----------------------------------------------------------------
        // Window close signalling
        // ----------------------------------------------------------------
        public enum WindowCloseReason
        {
            None,
            ScriptLoaded
        }

        public event Action<WindowCloseReason>? RequestClose;

        // ----------------------------------------------------------------
        // Constructor
        // ----------------------------------------------------------------
        public ScriptManagerViewModel(
            IAuthService auth,
            IScriptLoadService loader,
            ISettingsService settings,
            IScriptService scriptService,
            IDownloadedScriptsService downloadedScriptService,
            IScriptCompareService scriptCompareService,
            IDialogService dialogService)
        {
            _auth = auth ?? throw new ArgumentNullException(nameof(auth));
            _loader = loader ?? throw new ArgumentNullException(nameof(loader));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _scriptService = scriptService ?? throw new ArgumentNullException(nameof(scriptService));
            _downloadedScriptsService = downloadedScriptService ?? throw new ArgumentNullException(nameof(downloadedScriptService));
            _scriptCompareService = scriptCompareService ?? throw new ArgumentNullException(nameof(scriptCompareService));
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));

            // Commands
            SignInCommand = new AsyncRelayCommand(SignInAsync);
            SignOutCommand = new AsyncRelayCommand(SignOut);
            CloseCommand = new RelayCommand(OnClose);
                        
            _auth.AuthStateChanged += OnAuthStateChanged;
            
            // Tabs
            var searchTab = new SearchTabViewModel(_scriptService, _loader, _scriptCompareService, _dialogService);
            searchTab.RequestClose += OnScriptLoaded;

            var installedTab = new InstalledTabViewModel(_downloadedScriptsService, _loader, _scriptCompareService, _dialogService);
            installedTab.RequestClose += OnScriptLoaded;


            Tabs = new ObservableCollection<TabItemViewModel>
            {
                searchTab,
                new PublishTabViewModel(_auth, _scriptService, _loader),
                installedTab,
                new MyScriptsTabViewModel(_scriptService, _auth, _loader, _scriptCompareService, _dialogService),
                new SettingsTabViewModel(_settings)
            };

            SelectedTab = Tabs[0];

            // Initial UI sync (CRITICAL)
            SyncAuthStateToUI();
            
        }

        // ----------------------------------------------------------------
        // Initialization
        // ----------------------------------------------------------------
        public async Task InitializeAsync()
        {
            await _auth.RestoreSessionAsync();
            
            SyncAuthStateToUI();
        }

        // ----------------------------------------------------------------
        // Auth handling
        // ----------------------------------------------------------------
        private void OnAuthStateChanged(object? sender, EventArgs e)
        {
            SyncAuthStateToUI();
        }

        private void SyncAuthStateToUI()
        {
            IsAuthenticated = _auth.IsAuthenticated;

            RaisePropertyChanged(nameof(IsAuthenticated));
            RaisePropertyChanged(nameof(UserName));
            RaisePropertyChanged(nameof(AvatarUrl));
            RaisePropertyChanged(nameof(Email));
        }

        

        private async Task SignInAsync()
        {
            await _auth.EnsureAuthenticatedAsync();
            SyncAuthStateToUI();
        }

        private async Task SignOut()
        {
            await _auth.LogoutAsync();
            SyncAuthStateToUI();
        }

        // ----------------------------------------------------------------
        // Window control
        // ----------------------------------------------------------------
        private void OnScriptLoaded()
        {
            RequestClose?.Invoke(WindowCloseReason.ScriptLoaded);
        }

        private void OnClose()
        {
            RequestClose?.Invoke(WindowCloseReason.None);
        }

        // ----------------------------------------------------------------
        // Cleanup
        // ----------------------------------------------------------------
        public void Dispose()
        {
            foreach (var tab in Tabs)
            {
                if (tab is IDisposable disposable)
                    disposable.Dispose();
            }

            _auth.AuthStateChanged -= OnAuthStateChanged;
        }
    }
}
