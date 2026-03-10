using BIMformative.Core.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIMformative.DynamoExtension.Models.Scripts
{
    public class LocalScriptState
    {
        public string Name { get; set; } = string.Empty;
        public string DynamoVersion { get; set; } = string.Empty;
        public string Hash { get; set; } = string.Empty;

        public SemanticScript Semantic { get; set; } = default!;
    }
}
