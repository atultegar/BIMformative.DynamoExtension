using Dynamo.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace BIMformative.DynamoExtension.Utils
{
    public class DynamoNormalizeOptions
    {
        public bool IgnoreNodePositions { get; set; } = true;
        public bool IgnoreGuids { get; set; } = true;
        public bool IgnoreViewData { get; set; } = true;
        public bool IgnoreAnnotations { get; set; } = true;
        public bool SortCollections { get; set; } = true;
    }
}
