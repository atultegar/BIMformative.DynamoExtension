using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BIMformative.DynamoExtension.UI.Views.Controls
{
    public static class ScrollViewerBehavior
    {
        public static readonly DependencyProperty LoadMoreCommandProperty =
            DependencyProperty.RegisterAttached(
                "LoadMoreCommand",
                typeof(ICommand),
                typeof(ScrollViewerBehavior),
                new PropertyMetadata(null, OnLoadMoreCommandChanged));

        public static void SetLoadMoreCommand(DependencyObject obj, ICommand value)
            => obj.SetValue(LoadMoreCommandProperty, value);

        public static ICommand GetLoadMoreCommand(DependencyObject obj)
            => (ICommand)obj.GetValue(LoadMoreCommandProperty);

        private static void OnLoadMoreCommandChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            if (d is ScrollViewer sv)
            {
                sv.ScrollChanged -= OnScrollChanged;
                sv.ScrollChanged += OnScrollChanged;
            }
        }

        private static void OnScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            var sv = (ScrollViewer)sender;

            if (sv.VerticalOffset >= sv.ScrollableHeight - 50)
            {
                var command = GetLoadMoreCommand(sv);
                if (command?.CanExecute(null) == true)
                    command.Execute(null);
            }
        }
    }
}
