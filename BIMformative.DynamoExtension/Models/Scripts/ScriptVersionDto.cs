using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIMformative.DynamoExtension.Models.Scripts
{
    public sealed class ScriptVersionDto
    {
        public Guid Id { get; set; }
        public Guid Script_Id { get; set; }
        public int Version_Number { get; set; }
        public string Changelog { get; set; } = string.Empty;
        public string Dyn_File_Url { get; set; } = string.Empty;
        public DateTime Updated_At { get; set; }
        public bool Is_Current { get; set; }
    }
}
