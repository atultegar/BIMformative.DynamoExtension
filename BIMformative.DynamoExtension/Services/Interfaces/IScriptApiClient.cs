using BIMformative.DynamoExtension.Models;
using BIMformative.DynamoExtension.Models.Scripts;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BIMformative.DynamoExtension.Services.Interfaces
{
    public interface IScriptApiClient
    {
        // PUBLIC SEARCH
        Task<PagedResponse<ScriptDto>> GetPublicScriptsAsync(
            int page,
            int limit,
            string? search = null,
            string? scriptType = null,
            ScriptSortField sortField = ScriptSortField.updated_at,
            SortOrder sortOrder = SortOrder.desc,
            CancellationToken cancellationToken = default);

        // SCRIPT DETAILS
        Task<ScriptDetailsDto> GetScriptBySlugAsync(
            string slug,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<ScriptVersionDto>> GetScriptVersionsAsync(
            string slug,
            CancellationToken ct = default);


        //Task<PagedResponse<ScriptVersionDto>> GetScriptVersionsAsync(
        //    string slug,
        //    int page = 1,
        //    int limit = 20,
        //    CancellationToken cancellationToken= default);

        //Task<ScriptDownloadDto> GetDownloadAsync(
        //    string slug,
        //    string version,
        //    CancellationToken cancellationToken = default);

        //Task<AnalyzeScriptResultDto> AnalyzeScriptAsync(
        //    AnalyzeScriptRequestDto request,
        //    CancellationToken cancellationToken = default);

        //Task PublishScriptAsync(
        //    PublishScriptRequestDto request,
        //    CancellationToken cancellationToken = default);
    }
}
