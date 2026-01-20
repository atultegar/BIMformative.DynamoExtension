using BIMformative.DynamoExtension.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BIMformative.DynamoExtension.Services.Interfaces
{
    public interface IScriptLoadService
    {
        Task<bool> LoadScriptAsync(
            ScriptDto script,
            CancellationToken ct = default);
    }
}
