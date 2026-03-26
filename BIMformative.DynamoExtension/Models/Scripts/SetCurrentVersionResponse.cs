using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIMformative.DynamoExtension.Models.Scripts
{
    public class SetCurrentVersionResponse
    {
        public string Message { get; set; } = "";
        public string VersionId { get; set; } = "";
        public int VersionNumber { get; set; }
    }
}
