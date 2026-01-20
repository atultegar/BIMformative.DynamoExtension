using Dynamo.Wpf.Extensions;
using BIMformative.DynamoExtension.UI.Views;
using System.Windows.Controls;
using System.Windows;
using Dynamo.Models;
using Dynamo.ViewModels;
using Dynamo.Extensions;
using Dynamo.Controls;
using BIMformative.DynamoExtension.Services;
using System;

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
        private DynamoContext _dynamoContext;

        public void Loaded(ViewLoadedParams vlp)
        {            
            _dynamoWindow = vlp.DynamoWindow ?? throw new InvalidOperationException("Dynamo window is null.");

            var dynamoViewModel = _dynamoWindow.DataContext as DynamoViewModel ?? throw new InvalidOperationException("Dynamo DataContext is not a DynamoViewModel");

            _dynamoContext = new DynamoContext(dynamoViewModel, _dynamoWindow);
            
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
                if (_dynamoContext == null)
                    throw new InvalidOperationException("DynamoContext is not initialized");

                _managerWindow = new ScriptManagerWindow
                {
                    Owner = _dynamoWindow,
                    DataContext = new UI.ViewModels.ScriptManagerViewModel(_dynamoContext)
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
