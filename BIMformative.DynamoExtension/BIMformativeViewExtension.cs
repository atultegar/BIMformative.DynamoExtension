using Dynamo.Wpf.Extensions;
using BIMformative.DynamoExtension.UI.Views;
using System.Windows.Controls;
using System.Windows;

namespace BIMformative.DynamoExtension
{
    public class BIMformativeViewExtension : IViewExtension
    {
        public string UniqueId => "bimformative.extension.view";
        public string Name => "BIMformative";

        private MenuItem? _extensionMenu;
        private ScriptBrowserView? _browserWindow;
        private ScriptManagerWindow _managerWindow;
        private Window? _dynamoWindow;

        public void Loaded(ViewLoadedParams vlp)
        {            
            _dynamoWindow = vlp.DynamoWindow;

            CreateMenu(vlp);
        }

        private void CreateMenu(ViewLoadedParams vlp)
        {
            _extensionMenu = new MenuItem { Header = "BIMformative" };

            var browseScripts = new MenuItem { Header = "Browse Script" };
            var manageScripts = new MenuItem { Header = "Script Manager" };

            browseScripts.Click += (_, _) => ShowBrowser();
            manageScripts.Click += (_, _) => ShowScriptManager();

            _extensionMenu.Items.Add(browseScripts);
            _extensionMenu.Items.Add(manageScripts);
            vlp.dynamoMenu.Items.Add(_extensionMenu);
        }

        private void ShowBrowser()
        {
            if (_browserWindow == null )
            {
                _browserWindow = new ScriptBrowserView
                {
                    Owner = _dynamoWindow,
                    DataContext = new UI.ViewModels.ScriptBrowserViewModel()
                };

                _browserWindow.Closed += (_, _) => _browserWindow = null;
            }

            _browserWindow.Show();
            _browserWindow.Activate();
        }

        private void ShowScriptManager()
        {
            if (_managerWindow == null)
            {
                _managerWindow = new ScriptManagerWindow
                {
                    Owner = _dynamoWindow,
                    DataContext = new UI.ViewModels.ScriptManagerViewModel()
                };                

                _managerWindow.Closed += (_, _) => _managerWindow = null;
            }

            _managerWindow.Show();
            _managerWindow.Activate();
        }

        public void Shutdown()
        {
            _browserWindow?.Close();
            _browserWindow = null;
        }

        public void Dispose()
        {
        }

        public void Startup (ViewStartupParams viewStartupParams) { }
    }
}
