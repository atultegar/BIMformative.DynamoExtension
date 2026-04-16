using System;

namespace BIMformative.Core.Models.Auth
{
    public class AuthCache
    {
        public string AccessToken { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public UserProfileDto User { get; set; }
    }
}
