using System;
using System.Collections.Generic;
using System.Text;

namespace BIMformative.Core.Models.Scripts
{
    public class ScriptPublishResponse
    {
        public string ScriptId { get; set; }
        public string Slug { get; set; }
        public int Version { get; set; }
        public string DownloadUrl { get; set; } = string.Empty;
        public object VersionRow { get; set; }
    }
}
