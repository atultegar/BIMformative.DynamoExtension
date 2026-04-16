namespace BIMformative.Core.Models.Scripts
{
    public sealed class ScriptAnalyzeResponseDto
    {
        public bool Success { get; set; }
        public string UploadId { get; set; } = string.Empty;
        public string StoragePath { get; set; } = string.Empty;
        public ScriptAnalyzeDataDto ScriptData { get; set; }
    }
}
