using BIMformative.DynamoExtension.Services.Interfaces;
using BIMformative.DynamoExtension.UI.ViewModels.Search;
using BIMformative.DynamoExtension.UI.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIMformative.DynamoExtension.UI.ViewModels.Tabs
{
    public class SearchTabViewModel : TabItemViewModel
    {
        public SearchTabViewModel(IScriptApiClient api)
            : base( 
                  header: "Search Scripts", 
                  contentFactory: () => new SearchControl
                  {
                      DataContext = new SearchViewModel(api)
                  })
        {            
        }
    }
}
