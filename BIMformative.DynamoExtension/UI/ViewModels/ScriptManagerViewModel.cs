using BIMformative.DynamoExtension.Infrastructure;
using BIMformative.DynamoExtension.Services;
using BIMformative.DynamoExtension.Services.Auth;
using BIMformative.DynamoExtension.Services.Interfaces;
using BIMformative.DynamoExtension.UI.ViewModels.Base;
using BIMformative.DynamoExtension.UI.ViewModels.Tabs;
using BIMformative.DynamoExtension.UI.Views;
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
        public ObservableCollection<TabItemViewModel> Tabs { get; }

        private TabItemViewModel _selectedTab;
        public TabItemViewModel SelectedTab
        {
            get => _selectedTab;
            set => SetProperty(ref _selectedTab, value);
        }

        public ICommand CloseCommand { get; }

        public event Action? RequestClose;

        public ScriptManagerViewModel()
        {
            // Single HttpClient
            _httpClient = new HttpClient
            {
                
                BaseAddress = new Uri("http://localhost:3000/api/")
                // Production:
                //BaseAddress = new Uri("https://www.bimformative.com/api/")
            };

            _auth = new AuthService();

            // Single API client
            _api = new ScriptApiClient(_httpClient, _auth);

            Tabs =
            [
                new SearchTabViewModel(_api),
                new PublishTabViewModel(_api, _auth),
                new InstalledTabViewModel(),
                new MyScriptsTabViewModel(_api, _auth),
                new SettingsTabViewModel()
            ];

            SelectedTab = Tabs[0];
            CloseCommand = new RelayCommand(() => RequestClose?.Invoke());
        }

        private void OnClose()
        {
            RequestClose?.Invoke();
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
