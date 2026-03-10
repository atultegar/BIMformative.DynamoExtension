using BIMformative.DynamoExtension.UI.ViewModels.Scripts;
using System;
using System.Windows.Controls;
using Dynamo.Utilities;
using System.Windows;
using System.Windows.Media;
using BIMformative.DynamoExtension.UI.Views.Controls;
using System.Windows.Data;
using System.Net;

namespace BIMformative.DynamoExtension.UI.Views.Controls
{
    /// <summary>
    /// Interaction logic for ScriptManagerScriptsControl.xaml
    /// </summary>
    public partial class ScriptManagerScriptsControl : UserControl
    {
        public ScriptManagerScriptsControl()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {

        }

        private void MoreDetailsButton_OnClick(object sender, System.Windows.RoutedEventArgs e)
        {
            if (!(sender is Button button)) return;
            if (!(button.DataContext is ScriptRowViewModel scriptRowViewModel)) return;

            var scriptListVM = this.DataContext as ScriptsListViewModel;

            //ExecuteOpenScriptDetails(scriptRowViewModel, scriptListVM);
        }

        private void ExecuteOpenScriptDetails(ScriptRowViewModel scriptRowViewModel, ScriptsListViewModel scriptsListViewModel)
        {
            var name = this.Name;
            var parent = this.Parent as SearchControl;
            if (parent == null) return;

            var mainWindow = parent.Parent as ScriptManagerWindow;
            if (mainWindow == null) return;

            var width = mainWindow.contentColumn.ActualWidth * 0.5;
            mainWindow.detailsColumn.Width = new GridLength(width, GridUnitType.Pixel);
        }

        private void ScriptsListBox_Loaded(object sender, RoutedEventArgs e)
        {
            var listBox = sender as ListBox;
            if (listBox == null) return;

            var sv = FindScrollViewer(listBox);
            if (sv != null)
            {
                try
                {
                    ScrollViewerBehavior.SetLoadMoreCommand(sv, ((ScriptsListViewModel)listBox.DataContext).LoadNextPageCommand);
                }
                catch
                {

                }
            }
        }

        private static ScrollViewer? FindScrollViewer(DependencyObject d)
        {
            if (d is ScrollViewer sv) return sv;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(d); i++)
            {
                var child = VisualTreeHelper.GetChild(d, i);
                var result = FindScrollViewer(child);
                if (result != null) return result;
            }

            return null;
        }

        private void DropDownLoadButton_Loaded(object sender, RoutedEventArgs e)
        {
            var loadButton = sender as Button;
            if (loadButton != null)
            {
                var dropDownLoadButton = loadButton.Template.FindName("dropDownLoadButton", loadButton) as Button;
                if (dropDownLoadButton != null)
                {
                    dropDownLoadButton.Click -= DropDownLoadButton_OnClick;

                    dropDownLoadButton.Click += DropDownLoadButton_OnClick;
                }
            }
        }

        private void DropDownLoadButton_Unloaded(object sender, RoutedEventArgs e)
        {
            var loadButton = sender as Button;
            if (loadButton != null)
            {
                var dropDownLoadButton = loadButton.Template.FindName("dropDownLoadButton", loadButton) as Button;
                if (dropDownLoadButton != null)
                {
                    dropDownLoadButton.Click -= DropDownLoadButton_OnClick;
                }
            }
        }

        private void DropDownLoadButton_OnClick(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button == null || button.DataContext == null)
            {
                return;
            }

            var contextMenu = new ContextMenu();
            var commandBinding = new Binding("DownloadLatestToCustomPathCommand");
            commandBinding.Source = button.DataContext;
            var commandParameterBinding = new Binding("LatestVersion");
            commandParameterBinding.Source = button.DataContext;

            //var contextMenuStyle = new Style(typeof(ContextMenu));
            //contextMenuStyle.BasedOn = (Style)SharedDictionaryManager.ContextMenuDictionary["DarkContextMenuItemStyle"];

            //contextMenu.Style = contextMenuStyle;
            contextMenu.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(60, 60, 60));

            // Create and add menu item to the ContextMenu
            var menuItem = new MenuItem
            {
                Header = "Download to folder...",
                MinWidth = 60,
                MinHeight = 30,
                Cursor = System.Windows.Input.Cursors.Hand
            };

            contextMenu.Items.Add(menuItem);
            BindingOperations.SetBinding(menuItem, MenuItem.CommandProperty, commandBinding);
            BindingOperations.SetBinding(menuItem, MenuItem.CommandParameterProperty, commandParameterBinding);

            // Attach the context menu to the button
            button.ContextMenu = contextMenu;

            contextMenu.IsOpen = true;
        }
    }
}
