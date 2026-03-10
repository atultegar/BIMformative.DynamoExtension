using System;

namespace BIMformative.DynamoExtension.Infrastructure.Environment
{
    public sealed class AppEnvironment
    {
        public Uri BaseApiUrl { get; init; } = null;
        public string ApiVersion { get; init; } = "v1";

        public Uri ApiBaseAddress => new Uri(BaseApiUrl, $"api/{ApiVersion}/");

        public Uri ApiPublicBaseAddress => new Uri(BaseApiUrl, $"api/public/{ApiVersion}/");
    }
}
