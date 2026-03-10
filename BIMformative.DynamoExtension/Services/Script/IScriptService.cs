using BIMformative.DynamoExtension.Models;
using BIMformative.DynamoExtension.Models.Scripts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BIMformative.DynamoExtension.Services.Script
{
    public interface IScriptService
    {
        // QUERIES
        Task<PagedResponse<ScriptDto>> GetPublicAsync(
            int page,
            int limit,
            string? search = null,
            string? scriptType = null,
            ScriptSortField sortField = ScriptSortField.updated_at,
            SortOrder sortOrder = SortOrder.desc,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<MyScriptDto>> GetMyScriptsAsync(
            string? search = null,
            string? scriptType = null,
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

        Task<ScriptAnalyzeResponseDto> AnalyzeWorkspaceAsync(
            CancellationToken ct = default);

        Task<bool> PublishAsync(
            ScriptPublishRequestDto request,
            IProgress<double> progress = null,
            CancellationToken ct = default);

        Task<string> PublishVersionAsync(
            string slug, 
            string version, 
            CancellationToken ct = default);

        Task<string> UpdateMetadataAsync(
            string slug, 
            string metadata, 
            CancellationToken ct = default);

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

        Task<RemoteScriptInfo?> GetLatestInfoAsync(
            string slug,
            CancellationToken ct = default);

        Task<string> GetHashAsync(
            string scriptJson, 
            CancellationToken ct = default);

        Task<bool> HasOpenScript();

        Task<bool> HasLiked(string slug, CancellationToken ct = default);

        Task<ScriptLikesDto> LikeAsync(string slug, CancellationToken ct = default);

        Task<ScriptLikesDto> UnlikeAsync(string slug, CancellationToken ct = default);
    }
}
