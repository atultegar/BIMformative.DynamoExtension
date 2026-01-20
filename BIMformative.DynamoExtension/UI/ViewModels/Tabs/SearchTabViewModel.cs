using BIMformative.DynamoExtension.Services.Interfaces;
using BIMformative.DynamoExtension.UI.ViewModels.Search;
using BIMformative.DynamoExtension.UI.Views;
using System;

namespace BIMformative.DynamoExtension.UI.ViewModels.Tabs
{
    public class SearchTabViewModel : TabItemViewModel
    {
        public SearchViewModel ViewModel { get; }

        public event Action? RequestClose;

        public SearchTabViewModel(
            IScriptApiClient api, 
            IScriptLoadService loader)
            : this(new SearchViewModel(api, loader))
        {            
        }

        private SearchTabViewModel(SearchViewModel vm)
            : base(
                  header: "Search Scripts",
                  contentFactory: () => new SearchControl
                  {
                      DataContext = vm
                  })
        {
            ViewModel = vm;

            ViewModel.RequestClose += () =>
            {
                RequestClose?.Invoke();
            };
        }
    }
}
