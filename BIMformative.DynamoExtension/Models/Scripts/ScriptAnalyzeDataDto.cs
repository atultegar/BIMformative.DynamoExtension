using BIMformative.DynamoExtension.UI.Views.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIMformative.DynamoExtension.Models.Scripts
{
    public sealed class ScriptAnalyzeDataDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }

        public string? DynamoVersion { get; set; }
        public bool DynamoPlayerReady { get; set; }
        public bool PythonScripts { get; set; }
        public IReadOnlyList<string>? ExternalPackages { get; set; }

        public IReadOnlyList<object>? Nodes { get; set; }
        public IReadOnlyList<object>? Connectors { get; set; }
    }
}
