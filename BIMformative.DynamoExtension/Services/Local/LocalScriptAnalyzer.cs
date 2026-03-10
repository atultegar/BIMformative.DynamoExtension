using BIMformative.Core.Analyzer;
using BIMformative.Core.Hashing;
using BIMformative.DynamoExtension.Models.Scripts;
using BIMformative.DynamoExtension.Services.Script;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIMformative.DynamoExtension.Services.Local
{
    public class LocalScriptAnalyzer
    {
        private readonly IScriptService _scriptService;

        public LocalScriptAnalyzer(IScriptService scriptService)
        {
            _scriptService = scriptService ?? throw new ArgumentNullException(nameof(scriptService));
        }

        public async Task<string> GetServerHashAsync(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("Script not found.", filePath);

            var json = await File.ReadAllTextAsync(filePath);

            return await _scriptService.GetHashAsync(json);
        }
    }
}
