using System;
using System.Collections.Generic;

namespace BIMformative.Core.Models.Scripts
{
    public class ScriptPublishRequestDto
    {
        public string StoragePath { get; set; } = string.Empty;
        public string ParsedJson { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ScriptType { get; set; } = string.Empty;
        public IReadOnlyCollection<string> Tags { get; set; } = Array.Empty<string>();
        public string DemoLink { get; set; } = string.Empty;
        public bool IsPublic { get; set; } = false;
    }
}
