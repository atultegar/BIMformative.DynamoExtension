using BIMformative.DynamoExtension.Infrastructure;
using BIMformative.DynamoExtension.Models;
using BIMformative.DynamoExtension.Services.Interfaces;
using BIMformative.DynamoExtension.UI.ViewModels.Base;
using BIMformative.DynamoExtension.UI.ViewModels.Scripts;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Input;

namespace BIMformative.DynamoExtension.UI.ViewModels.Search
{
    public class SearchViewModel : ViewModelBase, IDisposable
    {
        private readonly IScriptApiClient _api;
        private readonly IScriptLoadService _loader;
        public ScriptsListViewModel Scripts { get; }

        private CancellationTokenSource? _searchCts;

        public SearchViewModel(IScriptApiClient api, IScriptLoadService loader)
        {
            _api = api ?? throw new ArgumentNullException(nameof(api));
            _loader = loader ?? throw new ArgumentNullException(nameof(loader));

            Scripts = new ScriptsListViewModel(_api, OnLoadScriptAsync);

            Scripts.LoadFirstPageCommand.Execute(null);
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

                Scripts.SearchText = text;
                await Scripts.LoadFirstPageAsync(token);
            }
            catch (TaskCanceledException)
            {

            }
        }

        public ICommand SortByLikesCommand =>
            new RelayCommand<object>(_ => Scripts.ChangeSortCommand.Execute(ScriptSortField.likes_count));

        public ICommand SortByDateCommand =>
            new RelayCommand(() => Scripts.ChangeSortCommand.Execute(ScriptSortField.updated_at));

        public ICommand SortByDownloadsCommand =>
            new RelayCommand(() => Scripts.ChangeSortCommand.Execute(ScriptSortField.downloads_count));

        public event Action? RequestClose;

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
                System.Windows.MessageBox.Show(
                    $"Failed to load script:\n{ex.Message}",
                    "BIMformative",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
            }

            await _loader.LoadScriptAsync(script.GetDto(), CancellationToken.None);
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

    }
}
