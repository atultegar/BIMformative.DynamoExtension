using BIMformative.DynamoExtension.UI.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIMformative.DynamoExtension.UI.ViewModels.Tabs
{
    public class SettingsTabViewModel : TabItemViewModel
    {
        public SettingsTabViewModel()
            : base (
                  header: "Settings",
                  contentFactory: () => new SettingsControl())
        {            
        }
    }
}
