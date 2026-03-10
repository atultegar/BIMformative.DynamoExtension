using System;

namespace BIMformative.DynamoExtension.Infrastructure.Environment
{
    public static class Environments
    {
        public static AppEnvironment Local =>
            new()
            {
                BaseApiUrl = new Uri("http://localhost:3000/"),
                ApiVersion = "v1"
            };

        public static AppEnvironment Production =>
            new()
            {
                BaseApiUrl = new Uri("https://www.bimformative.com/"),
                ApiVersion = "v1"
            };
    }
}
