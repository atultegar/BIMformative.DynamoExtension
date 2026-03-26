using Dynamo.Wpf.Extensions;
using BIMformative.DynamoExtension.UI.Views;
using System.Windows.Controls;
using System.Windows;
using Dynamo.ViewModels;
using BIMformative.DynamoExtension.Services;
using System;
using System.Net.Http;
using BIMformative.DynamoExtension.Services.Auth;
using BIMformative.DynamoExtension.Services.Interfaces;
using BIMformative.DynamoExtension.Services.Settings;
using BIMformative.DynamoExtension.Infrastructure.Environment;
using System.Printing;
using BIMformative.DynamoExtension.Services.Script;
using BIMformative.DynamoExtension.Db;

namespace BIMformative.DynamoExtension
{
    public class BIMformativeViewExtension : IViewExtension
    {
        public string UniqueId => "bimformative.extension.view";
        public string Name => "BIMformative";

        private MenuItem? _extensionMenu;
        private ScriptManagerWindow? _managerWindow;
        private Window? _dynamoWindow;
        private DynamoContext? _dynamoContext;

        // Shared services
        private HttpClient? _authHttp;
        private HttpClient? _publicHttp;
        private IAuthService? _auth;
        private IScriptApiClient? _scriptApi;
        private IScriptLoadService? _scriptLoader;
        private ISettingsService? _settingsService;
        private IScriptAnalyzeService? _scriptAnalyzeService;
        private IScriptService? _scriptService;
        private BimformativeDbContext? _dbContext;
        private IDownloadedScriptsService? _downloadedScriptsService;
        private IScriptCompareService? _scriptCompareService;
        private IDialogService? _dialogService;


        public void Loaded(ViewLoadedParams vlp)
        {            
            _dynamoWindow = vlp.DynamoWindow 
                ?? throw new InvalidOperationException("Dynamo window is null.");

            var dynamoViewModel = _dynamoWindow.DataContext as DynamoViewModel 
                ?? throw new InvalidOperationException("Dynamo DataContext is not a DynamoViewModel");

            _dynamoContext = new DynamoContext(dynamoViewModel, _dynamoWindow);

            // Create shared services
            InitializeServices();
            
            CreateMenu(vlp);
        }

        private void InitializeServices()
        {
            // HTTP Client
#if DEBUG
            var env = Environments.Local;
#else
            var env = Environments.Production;
#endif

            _authHttp = new HttpClient
            {
                BaseAddress = env.ApiBaseAddress
            };

            _publicHttp = new HttpClient
            {
                BaseAddress = env.ApiPublicBaseAddress
            };

            var baseUrl = env.BaseApiUrl;

            // Dialog service
            _dialogService = new DialogService(_dynamoWindow);

            // Auth service
            var authStore = new FileLocalAuthStore();
            var userApi = new UserApiClient(_authHttp);
            _auth = new AuthService(_authHttp, userApi, authStore);

            // Settings service
            _settingsService = new FileSettingsService();
            _settingsService.Load();

            // Script API & Loader
            _scriptApi = new ScriptApiClient(_authHttp, _publicHttp, _auth);

            var overwritePrompt = new FileOverwritePrompt();
            var downloadService = new ScriptDownloadService(_authHttp, _settingsService, overwritePrompt);

            if (_dynamoContext == null)
                throw new InvalidOperationException("DynamoContext is null during service initialization");            

            _scriptAnalyzeService = new ScriptAnalyzeService(_authHttp, _auth);

            _scriptService = new ScriptService(_dynamoContext, _authHttp, _publicHttp, _auth, _settingsService, overwritePrompt);
            _dbContext = DatabaseBootstrapper.Initialize();

            var downloadedRepo = new DownloadedScriptRepository(_dbContext);

            _downloadedScriptsService = new DownloadedScriptsService(_dbContext, _scriptService);
            _scriptLoader = new ScriptLoadService(_dynamoContext, _scriptService, _downloadedScriptsService);

            var desktopTicketService = new DesktopTicketService(_publicHttp, _auth);

            _scriptCompareService = new ScriptCompareService(_auth, _scriptLoader, baseUrl, _dialogService);
        }

        private void CreateMenu(ViewLoadedParams vlp)
        {
            _extensionMenu = new MenuItem { Header = "BIMformative" };
            var manageScripts = new MenuItem { Header = "Script Manager" };
            
            manageScripts.Click += (_, _) => ShowScriptManager();
            
            _extensionMenu.Items.Add(manageScripts);
            vlp.dynamoMenu.Items.Add(_extensionMenu);
        }        

        private void ShowScriptManager()
        {
            if (_managerWindow == null)
            {
                if (_dynamoContext == null || _auth == null || _scriptApi == null || _scriptLoader == null || _settingsService == null)
                    throw new InvalidOperationException("Services are not initialized");

                _managerWindow = new ScriptManagerWindow
                {
                    Owner = _dynamoWindow,
                    DataContext = new UI.ViewModels.ScriptManagerViewModel(
                        _auth,
                        _scriptApi,
                        _scriptLoader,
                        _settingsService,
                        _scriptAnalyzeService,
                        _scriptService,
                        _downloadedScriptsService,
                        _scriptCompareService,
                        _dialogService)
                };                

                _managerWindow.Closed += (_, _) => _managerWindow = null;
            }

            _managerWindow.Show();
            _managerWindow.Activate();
        }

        public void Shutdown()
        {
            _managerWindow?.Close();
            _managerWindow = null;
        }

        public void Dispose()
        {
            _authHttp?.Dispose();
            _publicHttp?.Dispose();
        }

        public void Startup (ViewStartupParams viewStartupParams) { }
    }
}
