using BIMformative.DynamoExtension.Services.Auth;
using BIMformative.DynamoExtension.Services.Interfaces;
using BIMformative.DynamoExtension.UI.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIMformative.DynamoExtension.UI.ViewModels.Tabs
{
    public class MyScriptsTabViewModel : TabItemViewModel
    {
        public MyScriptsTabViewModel(IScriptApiClient api, IAuthService auth)
            : base (
                  header: "My Scripts",
                  contentFactory: () => new MyScriptsControl())
        {            
        }
    }
}
