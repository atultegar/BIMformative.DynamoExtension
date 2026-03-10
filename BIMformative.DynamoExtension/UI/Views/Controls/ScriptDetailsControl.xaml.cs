using BIMformative.DynamoExtension.UI.ViewModels.Scripts;
using BIMformative.DynamoExtension.UI.ViewModels.Search;
using BIMformative.DynamoExtension.UI.ViewModels.Tabs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BIMformative.DynamoExtension.UI.Views.Controls
{
    /// <summary>
    /// Interaction logic for ScriptDetailsControl.xaml
    /// </summary>
    public partial class ScriptDetailsControl : UserControl
    {
        public ScriptDetailsControl()
        {
            InitializeComponent();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            var parent = VisualTreeHelper.GetParent(this) as Border;

            var searchVM = parent?.DataContext as SearchViewModel;

            searchVM?.Scripts.IsDetailOpen = false;
            searchVM?.Scripts.SelectedScript = null;
            searchVM?.Scripts.SelectedDetails = null;

            var myScriptsVm = parent?.DataContext as MyScriptsTabViewModel;

            myScriptsVm?.Scripts.IsDetailOpen = false;
            myScriptsVm?.Scripts.SelectedScript = null;
            myScriptsVm?.Scripts.SelectedDetails = null;
        }

        private void VersionsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is ScriptDetailsViewModel vm)
            {
                var selected = VersionsGrid.SelectedItems
                    .Cast<ScriptVersionRowViewModel>()
                    .ToList();

                vm.UpdateSelectedVersions(selected);
            }
        }
    }
}
