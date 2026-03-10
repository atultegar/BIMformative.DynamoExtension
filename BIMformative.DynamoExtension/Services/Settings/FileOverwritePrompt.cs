using Dynamo.Wpf.Utilities;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace BIMformative.DynamoExtension.Services.Settings
{
    public sealed class FileOverwritePrompt : IFileOverwritePrompt
    {
        public OverwriteDecision Ask(string existingFilePath)
        {
            var result = MessageBoxService.Show(
                $"The file already exists:\n\n{existingFilePath}\n\n" +
                "Yes → Overwrite\n" + "No → Save with a different name\n" + "Cancel → Abort download",
                "File already exists",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Warning
                );
            return result switch 
            {
                MessageBoxResult.Yes => OverwriteDecision.Overwrite,
                MessageBoxResult.No => OverwriteDecision.SaveAs,
                _ => OverwriteDecision.Cancel
            };
        }

        public string? ShowSaveAs(string defaultPath)
        {
            var dialog = new SaveFileDialog
            {
                InitialDirectory = Path.GetDirectoryName(defaultPath),
                FileName = Path.GetFileName(defaultPath),
                Filter = "Dynamo Script (*.dyn)|*.dyn",
                AddExtension = true,
                OverwritePrompt = false
            };

            return dialog.ShowDialog() == true
                ? dialog.FileName 
                : null;
        }
    }
}
