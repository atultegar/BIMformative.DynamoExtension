using BIMformative.Core.Interfaces;
using System;
using System.Net.Http;

namespace BIMformative.Infrastructure.Api
{
    public class AuthApiClient : BaseApiClient
    {
        private readonly IAuthService _auth;
        public AuthApiClient(HttpClient http, IAuthService auth)
            :base(http)
        {
            _auth = auth ?? throw new ArgumentNullException(nameof(auth));
        }

        protected override void AddHeaders(HttpRequestMessage request)
        {
            if (_auth.IsAuthenticated && !string.IsNullOrEmpty(_auth.AccessToken))
            {
                request.Headers.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _auth.AccessToken);
            }
        }

    }
}
