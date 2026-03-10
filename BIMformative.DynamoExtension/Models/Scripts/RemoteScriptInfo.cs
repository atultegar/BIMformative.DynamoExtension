using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIMformative.DynamoExtension.Models.Scripts
{
    public class RemoteScriptInfo
    {
        public string Id { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public int Current_Version_Number { get; set; }
        public string Version_Id { get; set; } = string.Empty;
        public string Hash { get; set; } = string.Empty;
    }
}
