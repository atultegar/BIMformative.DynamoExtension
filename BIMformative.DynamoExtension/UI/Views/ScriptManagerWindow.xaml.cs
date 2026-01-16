using BIMformative.DynamoExtension.UI.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BIMformative.DynamoExtension.UI.Views
{
    /// <summary>
    /// Interaction logic for ScriptManagerWindow.xaml
    /// </summary>
    public partial class ScriptManagerWindow : Window
    {
        public ScriptManagerWindow()
        {
            InitializeComponent();
        }
        
        private void OnTitleBarMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private void OnCloseClicked(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
