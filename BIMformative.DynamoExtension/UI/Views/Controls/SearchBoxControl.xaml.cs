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
    /// Interaction logic for SearchBoxControl.xaml
    /// </summary>
    public partial class SearchBoxControl : UserControl
    {
        public SearchBoxControl()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty SearchTextProperty =
        DependencyProperty.Register(
            nameof(SearchText),
            typeof(string),
            typeof(SearchBoxControl),
            new FrameworkPropertyMetadata(
                string.Empty,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnSearchTextChanged));

        public string SearchText
        {
            get => (string)GetValue(SearchTextProperty);
            set => SetValue(SearchTextProperty, value);
        }

        public static void OnSearchTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (SearchBoxControl)d;

            var text = e.NewValue as string;

            control.ClearButton.Visibility =
                string.IsNullOrEmpty(text)
                ? Visibility.Collapsed
                : Visibility.Visible;
        }
        

        private void ClearSearch_Click(object sender, RoutedEventArgs e)
        {
            SearchText = string.Empty;
        }
    }
}
