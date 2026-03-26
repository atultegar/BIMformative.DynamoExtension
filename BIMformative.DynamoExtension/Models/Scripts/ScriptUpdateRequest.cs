using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BIMformative.DynamoExtension.Models.Scripts
{
    public class ScriptUpdateRequest
    {
        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("script_type")]
        public string Script_Type { get; set; }

        [JsonPropertyName("tags")]
        public IEnumerable<string> Tags { get; set; }

        [JsonPropertyName("current_version")]
        public string Current_Version { get; set; }

        [JsonPropertyName("is_public")]
        public bool Is_Public { get; set; }
    }
}
