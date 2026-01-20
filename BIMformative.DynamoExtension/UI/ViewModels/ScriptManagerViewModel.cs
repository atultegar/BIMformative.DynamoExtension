using BIMformative.DynamoExtension.Infrastructure;
using BIMformative.DynamoExtension.Services;
using BIMformative.DynamoExtension.Services.Auth;
using BIMformative.DynamoExtension.Services.Interfaces;
using BIMformative.DynamoExtension.UI.ViewModels.Base;
using BIMformative.DynamoExtension.UI.ViewModels.Tabs;
using BIMformative.DynamoExtension.UI.Views;
using Dynamo.Extensions;
using Dynamo.Wpf.Extensions;
using System;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Windows.Input;

namespace BIMformative.DynamoExtension.UI.ViewModels
{
    public class ScriptManagerViewModel : ViewModelBase, IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly IScriptApiClient _api;
        private readonly IAuthService _auth;
        private readonly IScriptLoadService _loader;

        public enum WindowCloseReason
        {
            None,
            ScriptLoaded
        }

        public ObservableCollection<TabItemViewModel> Tabs { get; }

        private TabItemViewModel _selectedTab;
        public TabItemViewModel SelectedTab
        {
            get => _selectedTab;
            set => SetProperty(ref _selectedTab, value);
        }

        public ICommand CloseCommand { get; }

        public event Action<WindowCloseReason>? RequestClose;

        public ScriptManagerViewModel(IDynamoContext dynamoContext)
        {
            if (dynamoContext == null)
                throw new ArgumentNullException(nameof(dynamoContext));

            // Initialize services
            _httpClient = new HttpClient
            {
                
                BaseAddress = new Uri("http://localhost:3000/")
                // Production:
                //BaseAddress = new Uri("https://www.bimformative.com/")
            };

            _auth = new AuthService();

            // Single API client
            _api = new ScriptApiClient(_httpClient, _auth);
            _loader = new ScriptLoadService(dynamoContext, _auth, new ScriptDownloadService(_httpClient));

            var searchTab = new SearchTabViewModel(_api, _loader);

            searchTab.RequestClose += OnTabRequestClose;

            // Initialize tabs
            Tabs =
            [
                searchTab,
                new PublishTabViewModel(_api, _auth),
                new InstalledTabViewModel(),
                new MyScriptsTabViewModel(_api, _auth),
                new SettingsTabViewModel()
            ];

            SelectedTab = Tabs[0];
            CloseCommand = new RelayCommand(OnClose);
        }

        private void OnTabRequestClose()
        {
            RequestClose?.Invoke(WindowCloseReason.ScriptLoaded);
        }

        private void OnClose()
        {
            RequestClose?.Invoke(WindowCloseReason.None);
        }

        public void Dispose()
        {
            foreach (var tab in Tabs)
            {
                if (tab is IDisposable disposable)
                    disposable.Dispose();
            }

            _httpClient.Dispose();
        }
    }
}
