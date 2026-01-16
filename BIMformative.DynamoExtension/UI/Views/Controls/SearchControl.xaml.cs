using BIMformative.DynamoExtension.Services;
using BIMformative.DynamoExtension.UI.ViewModels.Scripts;
using BIMformative.DynamoExtension.UI.ViewModels.Search;
using System;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;

namespace BIMformative.DynamoExtension.UI.Views
{
    /// <summary>
    /// Interaction logic for SearchControl.xaml
    /// </summary>
    public partial class SearchControl : UserControl
    {
        public SearchControl()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is SearchViewModel vm)
            {
                vm.Initialize();
            }
        }
    }
}
