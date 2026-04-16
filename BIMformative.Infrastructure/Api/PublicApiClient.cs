using System.Net.Http;

namespace BIMformative.Infrastructure.Api
{
    public class PublicApiClient : BaseApiClient
    {
        public PublicApiClient(HttpClient http) : base(http)
        {
            
        }
    }
}
