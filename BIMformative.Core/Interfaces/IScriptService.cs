using BIMformative.Core.Models;
using BIMformative.Core.Models.Api;
using BIMformative.Core.Models.Scripts;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BIMformative.Core.Interfaces
{
    public interface IScriptService
    {
        // QUERIES
        Task<PagedResponse<ScriptDto>> GetPublicAsync(
            int page,
            int limit,
            string search = null,
            string scriptType = null,
            ScriptSortField sortField = ScriptSortField.updated_at,
            SortOrder sortOrder = SortOrder.desc,
            CancellationToken cancellationToken = default);

        Task<PagedResponse<MyScriptDto>> GetMyScriptsAsync(
            string search = null,
            string scriptType = null,
            CancellationToken cancellationToken = default);

        Task<ScriptDetailsDto> GetBySlugAsync(
            string slug,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<ScriptVersionDto>> GetVersionsAsync(
            string slug,
            CancellationToken ct = default);

        // COMMANDS
        Task<ScriptAnalyzeResponseDto> AnalyzeAsync(
            string filePath,
            CancellationToken ct = default);
               
        Task<ScriptPublishResponse> PublishAsync(
            ScriptPublishRequestDto request,
            CancellationToken ct = default);

        Task<ScriptVersionDto> PublishVersionAsync(
            string slug,
            ScriptAnalyzeResponseDto parsed,
            string changeLog, 
            CancellationToken ct = default);
                

        Task<ScriptDetailsDto> UpdateScriptMetadataAsync(
            string slug,
            ScriptUpdateRequest scriptUpdateRequest,
            CancellationToken ct = default);

        Task<UpdateScriptVisibilityResponse> UpdateScriptVisibilityAsync(string slug, bool isPublic, CancellationToken ct = default);

        Task<string> DeleteAsync(
            string slug, 
            CancellationToken ct = default);


        // UTILITIES
        Task<string> DownloadAsync(
            ScriptDto script,
            CancellationToken ct);

        Task<string> DownloadLatestAsync(DownloadedScript model, CancellationToken ct = default);

        Task<string> GetLatestVersionAsync(
            string slug,
            CancellationToken ct = default);

        Task<RemoteScriptInfo> GetLatestInfoAsync(
            string slug,
            CancellationToken ct = default);

        Task<string> GetHashAsync(
            string scriptJson, 
            CancellationToken ct = default);

        Task<bool> HasLiked(string slug, CancellationToken ct = default);

        Task<ScriptLikesDto> LikeAsync(string slug, CancellationToken ct = default);

        Task<ScriptLikesDto> UnlikeAsync(string slug, CancellationToken ct = default);

        Task<SetCurrentVersionResponse> SetCurrentVersionAsync(string slug, int versionNumber, CancellationToken ct = default);

        Task<string> DeleteVersionAsync(string slug, int versionNumber, CancellationToken ct = default);                
    }
}
