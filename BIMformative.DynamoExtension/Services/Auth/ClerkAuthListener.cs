using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace BIMformative.DynamoExtension.Services.Auth
{
    public class ClerkAuthListener
    {
        private readonly string _callbackUrl;

        public ClerkAuthListener(string callbackUrl)
        {
            _callbackUrl = callbackUrl;            
        }

        public async Task<string> WaitForTokenAsync()
        {
            var listener = new HttpListener();
            listener.Prefixes.Add(_callbackUrl + "/");
            listener.Start();

            var context = await listener.GetContextAsync();
            var request = context.Request;

            var token = request.QueryString["token"];
            if (string.IsNullOrEmpty(token))
                throw new InvalidOperationException("JWT token not found in callback");

            var responseHtml = "<html<body>You may now return to Dynamo.</body></html>";
            var buffer = Encoding.UTF8.GetBytes(responseHtml);

            context.Response.ContentLength64 = buffer.Length;
            await context.Response.OutputStream.WriteAsync(buffer);
            context.Response.OutputStream.Close();

            listener.Stop();
            return token;            
        }
    }
}
