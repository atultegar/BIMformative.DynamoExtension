using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIMformative.DynamoExtension.Services.Settings
{
    public enum OverwriteDecision
    {
        Overwrite,
        SaveAs,
        Cancel
    }
    public interface IFileOverwritePrompt
    {
        OverwriteDecision Ask(string exisitngFilePath);
        public string? ShowSaveAs(string defaultPath);
    }
}
