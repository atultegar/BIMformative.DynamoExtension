using BIMformative.DynamoExtension.Models;
using BIMformative.DynamoExtension.Services.Script;

namespace BIMformative.DynamoExtension.Services.Local
{
    public class LocalScriptStateService
    {
        private readonly LocalScriptAnalyzer _analyzer;
        private readonly IScriptService _scriptService;

        public LocalScriptStateService(LocalScriptAnalyzer analyzer, IScriptService scriptService)
        {
            _analyzer = analyzer;
            _scriptService = scriptService;
        }
    }
}
