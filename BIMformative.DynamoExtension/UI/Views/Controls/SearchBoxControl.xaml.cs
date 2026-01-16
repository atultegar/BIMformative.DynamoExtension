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

namespace BIMformative.DynamoExtension.UI.Views
{
    /// <summary>
    /// Interaction logic for SearchBoxControl.xaml
    /// </summary>
    public partial class SearchBoxControl : UserControl
    {
        public static readonly DependencyProperty SearchTextProperty =
        DependencyProperty.Register(
            nameof(SearchText),
            typeof(string),
            typeof(SearchBoxControl),
            new FrameworkPropertyMetadata(
                string.Empty,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public string SearchText
        {
            get => (string)GetValue(SearchTextProperty);
            set => SetValue(SearchTextProperty, value);
        }

        public SearchBoxControl()
        {
            InitializeComponent();
        }

        private void SearchBox_OnKeyboardFocusWithinChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            UpdateWatermark();
        }

        private void OnSearchClearButtonClicked(object sender, MouseButtonEventArgs e)
        {
            this.SearchTextbox.Clear();
        }
        private void UpdateWatermark()
        {
            SearchTextBoxWatermark.Visibility = 
                string.IsNullOrEmpty(SearchText)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }
    }
}
