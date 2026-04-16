using BIMformative.Core.Models;

namespace BIMformative.Core.Interfaces
{    
    public interface IFileOverwritePrompt
    {
        OverwriteDecision Ask(string exisitngFilePath);
        string ShowSaveAs(string defaultPath);
    }
}
