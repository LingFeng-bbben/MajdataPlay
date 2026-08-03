using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MajdataPlay.Net.Curl
{
    public class CurlHttpMessageHandler : HttpMessageHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return await Task.FromCanceled<HttpResponseMessage>(cancellationToken); // Ensure asynchronous behavior
        }
    }
}
