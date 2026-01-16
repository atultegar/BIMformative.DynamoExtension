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
        public ScriptsListViewModel Scripts { get; }

        private CancellationTokenSource? _searchCts;

        public SearchViewModel(IScriptApiClient api)
        {
            _api = api;
            Scripts = new ScriptsListViewModel(_api);

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

        public void Initialize()
        {
            Scripts.LoadFirstPageCommand.Execute(null);
        }

        public void Dispose()
        {
            
        }

    }
}
