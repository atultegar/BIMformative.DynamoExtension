using BIMformative.DynamoExtension.UI.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using static BIMformative.DynamoExtension.UI.ViewModels.ScriptManagerViewModel;

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
            Loaded += OnLoaded;
        }


        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is ScriptManagerViewModel vm)
            {
                vm.RequestClose += OnRequestClose;
            }
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

        private void OnRequestClose(WindowCloseReason reason)
        {
            if (reason == WindowCloseReason.ScriptLoaded)
            {
                ShowToastAndClose();
            }
            else
            {
                Close();
            }
        }

        private void ShowToastAndClose()
        {
            var toast = (Storyboard)FindResource("ToastStoryboard");
            var close = (Storyboard)FindResource("CloseStoryboard");

            toast.Begin();

            close.Completed += (_, _) => Close();
            close.Begin(this);
        }
    }
}
