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
    /// Interaction logic for PublishControl.xaml
    /// </summary>
    public partial class PublishControl : UserControl
    {
        public PublishControl()
        {
            InitializeComponent();
        }

        private void Enter(object sender, KeyEventArgs e)
        {

        }

        private void TagTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (DataContext is PublishTabViewModel vm)
                {
                    vm.AddTagCommand.Execute(null);
                }
            }
        }

        private void UploadZone_Drop(object sender, DragEventArgs e)
        {

        }
    }
}
