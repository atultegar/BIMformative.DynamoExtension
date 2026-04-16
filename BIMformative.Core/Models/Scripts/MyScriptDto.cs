using System;
using System.Collections.Generic;

namespace BIMformative.Core.Models.Scripts
{
    public class MyScriptDto
    {
        public Guid Id { get; set; }
        public string Owner_Id { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public string Script_Type { get; set; } = string.Empty;
        public int Current_Version_Number { get; set; }
        
        public string Demo_Link { get; set; }

        public int Downloads_Count { get; set; }
        public int Likes_Count { get; set; }

        public bool Is_Public { get; set; }

        public IReadOnlyList<string> Tags { get; set; }

        public DateTime Updated_At { get; set; }

    }
}
