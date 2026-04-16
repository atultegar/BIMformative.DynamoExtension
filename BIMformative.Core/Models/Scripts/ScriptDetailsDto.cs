using System;
using System.Collections.Generic;

namespace BIMformative.Core.Models.Scripts
{
    public sealed class ScriptDetailsDto
    {
        public Guid Id { get; set; }
        public string Owner_Id { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public string Script_Type { get; set; } = string.Empty;
        public int Current_Version_Number { get; set; }

        public string Owner_First_Name { get; set; } = string.Empty;
        public string Owner_Last_Name { get; set; } = string.Empty;
        public string Owner_Avatar_Url { get; set; }

        public string Demo_Link { get; set; }

        public int Downloads_Count { get; set; }
        public int Likes_Count { get; set; }

        public DateTime Updated_At { get; set; }

        // Convenience (VM-friendly)
        public string OwnerFullName => $"{Owner_First_Name} {Owner_Last_Name}";

        public string Dynamo_Version { get; set; } = string.Empty;
        public bool Is_Player_Ready { get; set; }

        public IReadOnlyList<string> Tags { get; set; }
        public IReadOnlyList<string> External_Packages { get; set; }

        public bool Is_Public { get; set; } = false;
    }
}
