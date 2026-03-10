using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIMformative.DynamoExtension.Models
{
    public sealed class AppSettings
    {
        public string? DefaultScriptSavePath { get; set; }

        public bool AskBeforeOverwrite { get; set; }
    }
}
