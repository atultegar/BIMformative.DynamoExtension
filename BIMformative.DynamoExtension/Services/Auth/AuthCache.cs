using BIMformative.DynamoExtension.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace BIMformative.DynamoExtension.Services.Auth
{
    public class AuthCache
    {
        public string AccessToken { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public UserProfileDto? User { get; set; }
    }
}
