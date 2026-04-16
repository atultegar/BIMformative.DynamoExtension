using BIMformative.Core.Interfaces;
using BIMformative.Core.Models;
using BIMformative.DynamoExtension.Services.Interfaces;
using BIMformative.DynamoExtension.UI.ViewModels.Scripts;
using BIMformative.DynamoExtension.UI.Views.Controls;
using Dynamo.Wpf.Utilities;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BIMformative.DynamoExtension.UI.ViewModels.Tabs
{
    public class MyScriptsTabViewModel : TabItemViewModel, IAsyncInitializable, IDisposable
    {
        private readonly IAuthService _auth;
        private readonly IScriptService _scriptService;
        private readonly IScriptLoadService _loader;
        private readonly IScriptCompareService _compareService;
        private readonly IDialogService _dialogService;

        private bool _initialized;
        private CancellationTokenSource? _searchCts;

        public ScriptsListViewModel Scripts { get; }
        
        public event Action? RequestClose;

        public ICommand CloseDetailsCommand => Scripts.CloseDetailsCommand;

        public MyScriptsTabViewModel(IScriptService scriptService, IAuthService auth, IScriptLoadService loader, IScriptCompareService compareService, IDialogService dialogService)
            : base (
                  header: "My Scripts",
                  contentFactory: () => new MyScriptsControl())                  
        {
            if (Content is MyScriptsControl control)
                control.DataContext = this;

            _auth = auth ?? throw new ArgumentNullException(nameof(auth));
            _scriptService = scriptService ?? throw new ArgumentNullException(nameof(scriptService));
            _loader = loader ?? throw new ArgumentNullException(nameof(loader));
            _compareService = compareService ?? throw new ArgumentNullException(nameof(compareService));
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));

            Scripts = new ScriptsListViewModel(scriptService, OnLoadScriptAsync, OnViewDetails, loader, _compareService, _dialogService);

            _auth.AuthStateChanged += OnAuthStateChanged;
        }

        public async Task InitializeAsync()
        {
            if (!_auth.IsAuthenticated)
            {
                Scripts.ClearMyScripts();
                Scripts.MyCurrentState = ViewState.NotAuthenticated;
                _initialized = false;
                return;
            }

            await Scripts.LoadMyScriptsAsync(CancellationToken.None);

            _initialized = true;
        }

        private async void OnAuthStateChanged(object? sender, EventArgs e)
        {
            try
            {
                if (!_auth.IsAuthenticated)
                {
                    Scripts.ClearMyScripts();
                    Scripts.MyCurrentState = ViewState.NotAuthenticated;
                    _initialized = false;
                    return;
                }

                // Force reload even if already initialized
                _initialized = false;

                await InitializeAsync();
            }
            catch (Exception ex)
            {
                MessageBoxService.Show(
                    $"Auth refresh failed:\n{ex.Message}",
                    "Error",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
            }
        }

        private string? _searchText;
        public string? SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                    DebounceSearchAsync(value);
            }
        }

        private async void DebounceSearchAsync(string? text)
        {
            _searchCts?.Cancel();
            _searchCts = new CancellationTokenSource();
            var token = _searchCts.Token;

            try
            {
                await Task.Delay(350, token);

                Scripts.SearchText = text;
                await Scripts.LoadMyScriptsAsync(token);
            }
            catch (TaskCanceledException)
            {

            }
        }

        private void OnViewDetails(ScriptRowViewModel script)
        {
            // Hook point
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

                // Close ScriptManagerWindow
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

        public void Dispose()
        {
            _auth.AuthStateChanged -= OnAuthStateChanged;

            _searchCts?.Cancel();
            _searchCts?.Dispose();
        }
    }
}
