using BIMformative.DynamoExtension.Models.Scripts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BIMformative.DynamoExtension.Services.Interfaces
{
    public interface IScriptAnalyzeService
    {
        Task<ScriptAnalyzeResponseDto> AnalyzeAsync(
            string filePath,
            CancellationToken ct = default);
    }
}
