using BIMformative.DynamoExtension.UI.ViewModels.Scripts;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace BIMformative.DynamoExtension.UI.Views
{
    /// <summary>
    /// Interaction logic for EditScriptDialog.xaml
    /// </summary>
    public partial class EditScriptDialog : Window
    {
        public EditScriptDialog()
        {
            InitializeComponent();
        }

        private void OnCloseClicked(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OnTitleBarMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private void ScrollToMetadata(object sender, RoutedEventArgs e)
        {
            MetadataSection.BringIntoView();
        }

        private void ScrollToTags(object sender, RoutedEventArgs e)
        {
            TagsSection.BringIntoView();
        }

        private void ScrollToVisibility(object sender, RoutedEventArgs e)
        {
            VisibilitySection.BringIntoView();
        }

        private void ScrollToVersions(object sender, RoutedEventArgs e)
        {
            VersionsSection.BringIntoView();
        }

        private void EditorScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            HighlightActiveSection();
        }

        private void HighlightActiveSection()
        {
            double scrollPos = EditorScrollViewer.VerticalOffset;

            double metadataPos = MetadataSection.TransformToAncestor(EditorScrollViewer).Transform(new Point(0, 0)).Y;
            double tagsPos = TagsSection.TransformToAncestor(EditorScrollViewer).Transform(new Point(0, 0)).Y;
            double visibilityPos = VisibilitySection.TransformToAncestor(EditorScrollViewer).Transform(new Point(0, 0)).Y;
            double versionsPos = VersionsSection.TransformToAncestor(EditorScrollViewer).Transform(new Point(0, 0)).Y;

            ResetSidebar();

            if (scrollPos >= versionsPos - 50)
                Highlight(VersionsButton);
            else if (scrollPos >= visibilityPos - 50)
                Highlight(VisibilityButton);
            else if (scrollPos >= tagsPos - 50)
                Highlight(TagsButton);
            else
                Highlight(MetadataButton);
        }

        private void Highlight(Button btn)
        {
            btn.Background = new SolidColorBrush(Color.FromRgb(59,130,246));
        }

        private void ResetSidebar()
        {
            MetadataButton.ClearValue(Button.BackgroundProperty);
            TagsButton.ClearValue(Button.BackgroundProperty);
            VisibilityButton.ClearValue(Button.BackgroundProperty);
            VersionsButton.ClearValue(Button.BackgroundProperty);
        }

        private void TagInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (DataContext is EditScriptViewModel vm)
                    vm.AddTagCommand.Execute(null);
            }
        }

        private void VersionsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is EditScriptViewModel vm)
            {
                var selected = VersionsGrid.SelectedItems
                    .Cast<ScriptVersionRowViewModel>()
                    .ToList();

                vm.UpdateSelectedVersions(selected);                   
            }
        }
    }
}
