using System;

namespace BIMformative.DynamoExtension.Infrastructure.Environment
{
    public sealed class AppEnvironment
    {
        public Uri BaseApiUrl { get; set; } = null;
        public string ApiVersion { get; set; } = "v1";

        public Uri ApiBaseAddress => new Uri(BaseApiUrl, $"api/{ApiVersion}/");

        public Uri ApiPublicBaseAddress => new Uri(BaseApiUrl, $"api/public/{ApiVersion}/");
    }
}
