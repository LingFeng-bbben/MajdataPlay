using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
#nullable enable
namespace MajdataPlay.Net.Curl
{
    public class CurlHttpMessageHandler : HttpMessageHandler
    {
        public IWebProxy? Proxy { get; set; }
        public bool UseProxy { get; set; }
        public bool UseCookies { get; set; }
        public CookieContainer? CookieContainer { get; set; }
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return await Task.FromCanceled<HttpResponseMessage>(cancellationToken); // Ensure asynchronous behavior
        }
    }
}
