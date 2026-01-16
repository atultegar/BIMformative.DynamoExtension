using BIMformative.DynamoExtension.UI.ViewModels.Scripts;
using System;
using System.Windows.Controls;
using Dynamo.Utilities;
using System.Windows;
using System.Windows.Media;
using BIMformative.DynamoExtension.UI.Views.Controls;

namespace BIMformative.DynamoExtension.UI.Views
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
        }

        private void ExecuteOpenScriptDetails()
        {
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
                ScrollViewerBehavior.SetLoadMoreCommand(sv, ((ScriptsListViewModel)listBox.DataContext).LoadNextPageCommand);
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
    }
}
