
using BIMformative.Core.Models.Scripts;
using System.Threading;
using System.Threading.Tasks;

namespace BIMformative.Core.Interfaces
{
    public interface IScriptLoadService
    {
        bool HasOpenWorkspace();
        Task<bool> LoadScriptAsync(
            ScriptDto script,
            CancellationToken ct = default);

        Task<bool> LoadScriptFileAsync(DownloadedScript model);

        Task<bool> DownloadLatestFileAsync(DownloadedScript model, CancellationToken ct = default);

        Task<ScriptAnalyzeResponseDto> AnalyzeWorkspaceAsync(CancellationToken ct = default);

        Task<ScriptVersionDto> UploadVersionFromWorkspaceAsync(string slug, string changeLog = "", CancellationToken ct = default);

    }
}
