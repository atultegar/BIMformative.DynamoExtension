using BIMformative.DynamoExtension.Models;
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
    /// Interaction logic for ViewStateControl.xaml
    /// </summary>
    public partial class ViewStateControl : UserControl
    {
        public ViewStateControl()
        {
            InitializeComponent();
        }

        public ViewState CurrentState
        {
            get => (ViewState)GetValue(CurrentStateProperty);
            set => SetValue(CurrentStateProperty, value);

        }
        public static readonly DependencyProperty CurrentStateProperty =
            DependencyProperty.Register(
                nameof(CurrentState),
                typeof(ViewState),
                typeof(ViewStateControl),
                new PropertyMetadata(ViewState.Loading));
    }
}
