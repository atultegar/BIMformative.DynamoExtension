using BIMformative.DynamoExtension.UI.ViewModels.Scripts;
using System;
using System.Threading.Tasks;

namespace BIMformative.DynamoExtension.Services.Interfaces
{
    public interface IScriptCompareService
    {
        Task OpenCompareAsync(DownloadedScriptItemViewModel item, Func<Task> refreshParentList);

        Task OpenVersionCompareAsync(string slug, int leftVersion, int rightVersion);
    }
}
