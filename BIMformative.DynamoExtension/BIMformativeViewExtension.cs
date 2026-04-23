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
using BIMformative.Infrastructure.Logging;
using System.Reflection;
using System.IO;
using System.Linq;

namespace BIMformative.DynamoExtension
{
    public class BIMformativeViewExtension : IViewExtension
    {
        public string UniqueId => "bimformative.extension.view";
        public string Name => "BIMformative";

        private static bool _assemblyResolverRegistered;

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
        private IAppLogger? _logger;
        private IDownloadedScriptRepository downloadedRepo;

        // Infra
        private HttpClient? _authHttp;
        private HttpClient? _publicHttp;

        public void Loaded(ViewLoadedParams vlp)
        {
            try
            {
                RegisterAssemblyResolver();

                _logger = new FileLogger("BIMformative", Core.Models.Logging.LogLevel.Debug);
                _logger.Info("Initializing BIMformative extension.");

                _dynamoWindow = vlp.DynamoWindow 
                    ?? throw new InvalidOperationException("Dynamo window is null.");

               
                var dynamoViewModel = _dynamoWindow.DataContext as DynamoViewModel;

                if (dynamoViewModel == null)
                {
                    var actualType = _dynamoWindow.DataContext != null
                        ? _dynamoWindow.DataContext.GetType().FullName
                        : "(null)";
                                       

                    _dynamoWindow.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        TryInitializeAfterWindowReady(vlp);
                    }));

                    return;
                }

                _dynamoContext = new DynamoContext(dynamoViewModel, _dynamoWindow);

                PreloadCriticalDependencies();
                        
            
                InitializeServices();
            }
            catch (Exception ex)
            {
                _logger.Error("InitializeServices failed: " + ex);
                throw;
            }
            
            
            CreateMenu(vlp);
            _logger.Info("Menu created.");
        }

        private void TryInitializeAfterWindowReady(ViewLoadedParams vlp)
        {
            try
            {

                var dynamoViewModel = _dynamoWindow != null
                    ? _dynamoWindow.DataContext as DynamoViewModel
                    : null;

                if (dynamoViewModel == null)
                {
                    var actualType = _dynamoWindow != null && _dynamoWindow.DataContext != null
                        ? _dynamoWindow.DataContext.GetType().FullName
                        : "(null)";
                    return;
                };

                _dynamoContext = new DynamoContext(dynamoViewModel, _dynamoWindow);

                InitializeServices();

                CreateMenu(vlp);
            }
            catch (Exception ex)
            {
                _logger?.Error("Retry initialization failed: " + ex);
            }
        }

        private static void RegisterAssemblyResolver()
        {
            if (_assemblyResolverRegistered)
                return;

            AppDomain.CurrentDomain.AssemblyResolve += ResolveFromExtensionFolder;
            _assemblyResolverRegistered = true;
        }

        private static Assembly ResolveFromExtensionFolder(object sender, ResolveEventArgs args)
        {
            try
            {
                var requestedName = new AssemblyName(args.Name).Name;
                if (string.IsNullOrWhiteSpace(requestedName))
                    return null;

                var extensionAssembly = typeof(BIMformativeViewExtension).Assembly;
                var baseDir = Path.GetDirectoryName(extensionAssembly.Location);

                if (string.IsNullOrWhiteSpace(baseDir) || !Directory.Exists(baseDir))
                    return null;

                var candidatePath = Path.Combine(baseDir, requestedName + ".dll");

                if (File.Exists(candidatePath))
                    return Assembly.LoadFrom(candidatePath);

                var nestedCandidate = Directory
                    .EnumerateFiles(baseDir, requestedName + ".dll", SearchOption.AllDirectories)
                    .FirstOrDefault();

                if (!string.IsNullOrWhiteSpace(nestedCandidate) && File.Exists(nestedCandidate))
                    return Assembly.LoadFrom(nestedCandidate);
            }
            catch
            {
                // Never throw from AssemblyResolve
            }

            return null;
        }

        private void InitializeServices()
        {
            if (_dynamoContext == null)
                throw new InvalidOperationException("DynamoContext not initialized");
                        
            
            // Environment
#if DEBUG_REVIT2023
            var env = Environments.Local;
#else
            var env = Environments.Production;
#endif            
            // Http Clients
            _authHttp = new HttpClient { BaseAddress = env.ApiBaseAddress };
            _publicHttp = new HttpClient { BaseAddress = env.ApiPublicBaseAddress };

            // Database
#if NET48

            SQLitePCL.Batteries_V2.Init();

            var connectionString = BIMformative.Infrastructure.Db.SqliteDatabaseBootstrapper.Initialize();

            downloadedRepo = new BIMformative.Infrastructure.Repositories.SqliteDownloadedScriptRepository(connectionString);

#else
            SQLitePCL.Batteries_V2.Init();

            var dbContext = DatabaseBootstrapper.Initialize();

            var downloadedRepo = new DownloadedScriptRepository(dbContext);

#endif

            // Local Services
            var authStore = new LocalAuthStore(_logger);

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

        private static bool _dependencyPreloaded;

        private void PreloadCriticalDependencies()
        {
            if (_dependencyPreloaded)
                return;

            var baseDir = Path.GetDirectoryName(typeof(BIMformativeViewExtension).Assembly.Location);
            if (string.IsNullOrWhiteSpace(baseDir) || !Directory.Exists(baseDir))
                return;

            var assemblyNames = new[]
            {
                "Microsoft.Extensions.DependencyInjection.dll",
                "Microsoft.Extensions.DependencyInjection.Abstractions.dll",
                "Microsoft.Extensions.Logging.dll",
                "Microsoft.Extensions.Logging.Abstractions.dll",
                "Microsoft.Extensions.Options.dll",
                "Microsoft.Extensions.Primitives.dll",
                "Microsoft.Extensions.Caching.Memory.dll",
                "Microsoft.Extensions.Caching.Abstractions.dll",
                "Microsoft.Extensions.Configuration.dll",
                "Microsoft.Extensions.Configuration.Abstractions.dll",
                "Microsoft.EntityFrameworkCore.dll",
                "Microsoft.EntityFrameworkCore.Relational.dll",
                "Microsoft.EntityFrameworkCore.Sqlite.dll",
                "Microsoft.Data.Sqlite.dll"
            };

            foreach (var file in assemblyNames)
            {
                try
                {
                    var path = Path.Combine(baseDir, file);
                    if (File.Exists(path))
                    {
                        Assembly.LoadFrom(path);
                    }
                    else
                    {
                        _logger?.Warning("Preload missing: " + file);
                    }
                }
                catch (Exception ex)
                {
                    _logger?.Error("Preload failed for " + file + ": " + ex.Message);
                }
            }

            _dependencyPreloaded = true;
        }


    }
}
