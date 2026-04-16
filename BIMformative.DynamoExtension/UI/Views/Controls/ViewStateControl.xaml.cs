using BIMformative.Core.Models;
using System;
using System.Windows;
using System.Windows.Controls;

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
