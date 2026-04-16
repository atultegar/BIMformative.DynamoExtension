using System.Windows;
using System.Windows.Controls;

namespace BIMformative.DynamoExtension.UI.Views.Controls
{
    /// <summary>
    /// Interaction logic for InstalledScriptsControl.xaml
    /// </summary>
    public partial class InstalledScriptsControl : UserControl
    {
        public InstalledScriptsControl()
        {
            InitializeComponent();
        }

        private void MoreButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.ContextMenu != null)
            {
                button.ContextMenu.DataContext = button.DataContext;
                button.ContextMenu.IsOpen = true;
            }
        }
    }
}
