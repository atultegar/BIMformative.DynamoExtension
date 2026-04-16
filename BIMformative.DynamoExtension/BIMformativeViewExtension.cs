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
using BIMformative.DynamoExtension.Services.Script;
using BIMformative.Core.Interfaces;
using BIMformative.Infrastructure;
using BIMformative.Infrastructure.Repositories;
using BIMformative.Core.Services;
using BIMformative.Infrastructure.Api;

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

        // Services
        private IAuthService? _auth;
        private IScriptService? _scriptService;
        private IScriptLoadService? _scriptLoader;
        private ISettingsService? _settingsService;
        private IDownloadedScriptsService? _downloadedScriptsService;
        private IScriptCompareService? _scriptCompareService;
        private IDialogService? _dialogService;

        // Infra
        private HttpClient? _authHttp;
        private HttpClient? _publicHttp;

        public void Loaded(ViewLoadedParams vlp)
        {            
            _dynamoWindow = vlp.DynamoWindow 
                ?? throw new InvalidOperationException("Dynamo window is null.");

            var dynamoViewModel = _dynamoWindow.DataContext as DynamoViewModel 
                ?? throw new InvalidOperationException("Invalid Dynamo DataContext");

            _dynamoContext = new DynamoContext(dynamoViewModel, _dynamoWindow);
                        
            InitializeServices();
            
            CreateMenu(vlp);
        }

        private void InitializeServices()
        {
            if (_dynamoContext == null)
                throw new InvalidOperationException("DynamoContext not initialized");
            
            // Environment
#if DEBUG_REVIT2025
            var env = Environments.Local;
#else
            var env = Environments.Production;
#endif
            // Http Clients
            _authHttp = new HttpClient { BaseAddress = env.ApiBaseAddress };
            _publicHttp = new HttpClient { BaseAddress = env.ApiPublicBaseAddress };

            // Database
            SQLitePCL.Batteries_V2.Init();
            var dbContext = DatabaseBootstrapper.Initialize();
            var downloadedRepo = new DownloadedScriptRepository(dbContext);

            
            

            // Local Services
            var authStore = new LocalAuthStore();

            _settingsService = new FileSettingsService();
            _settingsService.Load();

            _dialogService = new DialogService(_dynamoWindow);

            var overwritePrompt = new FileOverwritePrompt();

            // User API
            var userApiClient = new UserApiClient(_authHttp);

            // Auth Service
            _auth = new AuthService(_authHttp, userApiClient, authStore);

            // Infrastructure - API Clients
            var authApiClient = new AuthApiClient(_authHttp, _auth);
            var publicApiClient = new PublicApiClient(_publicHttp);
            var scriptApiClient = new ScriptApiClient(authApiClient, publicApiClient);

            // Core Services
            _scriptService = new ScriptService(
                scriptApiClient, 
                _settingsService, 
                overwritePrompt
            );

            _downloadedScriptsService = new DownloadedScriptsService(
                downloadedRepo,
                _scriptService
            );

            _scriptLoader = new ScriptLoadService(
                _dynamoContext, 
                _scriptService, 
                _downloadedScriptsService
            );

            var baseUrl = env.BaseApiUrl;

            _scriptCompareService = new ScriptCompareService(
                _auth, 
                _scriptLoader, 
                baseUrl, 
                _dialogService
            );
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
                if (_auth == null || _scriptService == null || _scriptLoader == null || _settingsService == null ||
                    _downloadedScriptsService == null || _scriptCompareService == null || _dialogService == null)
                {
                    throw new InvalidOperationException("Services are not initialized");
                }
                    

                _managerWindow = new ScriptManagerWindow
                {
                    Owner = _dynamoWindow,
                    DataContext = new UI.ViewModels.ScriptManagerViewModel(
                        _auth,
                        _scriptLoader,
                        _settingsService,
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
