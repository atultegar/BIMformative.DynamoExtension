using Dynamo.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIMformative.DynamoExtension.Models.Scripts
{
    public class DownloadedScript
    {
        public string Id { get; set; } = default!;
        public string Slug { get; set; } = default!;

        public string Title { get; set; } = default!;
        public string ScriptType { get; set; } = default!;

        public string LocalPath { get; set; } = default!;

        // Versioning
        public string DownloadedVersion { get; set; } = default!;
        public string? LatestVersion { get; set; }

        // Hashes
        public string DownloadedHash { get; set; } = default!; // hash at download/update time
        public string CurrentLocalHash { get; set; } = default!; // recalculated        
        
        // State flags
        public ScriptSyncStatus SyncStatus { get; set; }

        public DateTime DownloadedAt { get; set; }
        public DateTime? LastCheckedAt { get; set; }
        public DateTime? LastLocalFileWriteTime { get; set; }
    }
}
