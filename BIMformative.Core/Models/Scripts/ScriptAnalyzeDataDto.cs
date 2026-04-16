using System.Collections.Generic;

namespace BIMformative.Core.Models.Scripts
{
    public sealed class ScriptAnalyzeDataDto
    {
        public string Name { get; set; }
        public string Description { get; set; }

        public string DynamoVersion { get; set; }
        public bool DynamoPlayerReady { get; set; }
        public bool PythonScripts { get; set; }
        public IReadOnlyList<string> ExternalPackages { get; set; }

        public IReadOnlyList<object> Nodes { get; set; }
        public IReadOnlyList<object> Connectors { get; set; }
    }
}
