using BIMformative.DynamoExtension.UI.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIMformative.DynamoExtension.UI.ViewModels.Tabs
{
    public class InstalledTabViewModel : TabItemViewModel
    {
        public InstalledTabViewModel()
            : base (
                  header: "Installed Scripts",
                  contentFactory: () => new InstalledScriptsControl())
        {            
        }
    }
}
