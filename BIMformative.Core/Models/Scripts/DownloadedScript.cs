using System;

namespace BIMformative.Core.Models.Scripts
{
    public class DownloadedScript
    {
        public string Id { get; set; } = "";
        public string Slug { get; set; } = "";

        public string Title { get; set; } = "";
        public string ScriptType { get; set; } = "";

        public string LocalPath { get; set; } = "";

        // Versioning
        public string DownloadedVersion { get; set; } = "";
        public string LatestVersion { get; set; } = "";

        // Hashes
        public string DownloadedHash { get; set; } = ""; // hash at download/update time
        public string CurrentLocalHash { get; set; } = ""; // recalculated        

        // State flags
        public ScriptSyncStatus SyncStatus { get; set; }

        public DateTime DownloadedAt { get; set; }
        public DateTime? LastCheckedAt { get; set; }
        public DateTime? LastLocalFileWriteTime { get; set; }
    }
}
