using BIMformative.DynamoExtension.Models.Scripts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BIMformative.DynamoExtension.Services.Interfaces
{
    public interface IScriptDownloadService
    {
        Task<string> DownloadAsync(ScriptDto script, string accessToken, CancellationToken ct);

        Task<string> GetScriptCurrentHash(ScriptDto script);
    }
}
