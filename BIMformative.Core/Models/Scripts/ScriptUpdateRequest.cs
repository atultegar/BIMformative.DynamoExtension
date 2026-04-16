using Newtonsoft.Json;
using System.Collections.Generic;

namespace BIMformative.Core.Models.Scripts
{
    public class ScriptUpdateRequest
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("script_type")]
        public string Script_Type { get; set; }

        [JsonProperty("tags")]
        public IEnumerable<string> Tags { get; set; }

        [JsonProperty("current_version")]
        public string Current_Version { get; set; }

        [JsonProperty("is_public")]
        public bool Is_Public { get; set; }
    }
}
