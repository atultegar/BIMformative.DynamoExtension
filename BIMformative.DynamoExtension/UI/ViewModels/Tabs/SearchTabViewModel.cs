using BIMformative.DynamoExtension.Services.Interfaces;
using BIMformative.DynamoExtension.Services.Script;
using BIMformative.DynamoExtension.UI.ViewModels.Search;
using BIMformative.DynamoExtension.UI.Views.Controls;
using System;

namespace BIMformative.DynamoExtension.UI.ViewModels.Tabs
{
    public class SearchTabViewModel : TabItemViewModel
    {
        public SearchViewModel ViewModel { get; }

        public event Action? RequestClose;

        public SearchTabViewModel(
            IScriptService scriptService, 
            IScriptLoadService loader,
            IScriptCompareService scriptCompareService)
            : this(new SearchViewModel(scriptService, loader, scriptCompareService))
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
