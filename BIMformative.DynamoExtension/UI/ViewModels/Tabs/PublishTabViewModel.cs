using BIMformative.DynamoExtension.Services.Auth;
using BIMformative.DynamoExtension.Services.Interfaces;
using BIMformative.DynamoExtension.UI.ViewModels.Base;
using BIMformative.DynamoExtension.UI.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIMformative.DynamoExtension.UI.ViewModels.Tabs
{
    public class PublishTabViewModel : TabItemViewModel
    {
        public PublishTabViewModel(IScriptApiClient api, IAuthService auth)
            : base(
                  header: "Publish Script",
                  contentFactory: () => new PublishControl())
        {            
        }
    }
}
