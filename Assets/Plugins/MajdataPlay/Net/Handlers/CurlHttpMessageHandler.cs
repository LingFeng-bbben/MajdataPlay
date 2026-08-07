using MajdataPlay.Net.Curl;
using MajdataPlay.Net.Curl.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
#nullable enable
namespace MajdataPlay.Net.Curl
{
    public class CurlHttpMessageHandler : HttpMessageHandler
    {
        public SslProtocols SslProtocols { get; set; } = SslProtocols.Tls12 | SslProtocols.Tls13;
        public IWebProxy? Proxy { get; set; }
        public bool PreAuthenticate { get; set; }
        public int MaxResponseHeadersLength { get; set; } = 64 * 1024; // 64KB
        public long MaxRequestContentBufferSize { get; set; } = int.MaxValue;
        public int MaxConnectionsPerServer { get; set; } = 6;
        public int MaxAutomaticRedirections { get; set; } = 50;
        public ICredentials? DefaultProxyCredentials { get; set; }
        public ICredentials? Credentials { get; set; }
        public CookieContainer? CookieContainer { get; set; }
        public bool CheckCertificateRevocationList { get; set; }
        public DecompressionMethods AutomaticDecompression { get; set; } = DecompressionMethods.None;
        public bool AllowAutoRedirect { get; set; } = true;
        public bool UseDefaultCredentials { get; set; }
        public bool UseProxy { get; set; } = true;

        readonly CurlMulti _curlMulti;

        public CurlHttpMessageHandler()
        {
            _curlMulti = ClientUrl.CreateMulti();
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var config = CopyCurrentConfig();
            var content = request.Content;
            var contentStream = default(Stream?);
            if(content is not null)
            {
                contentStream = await content.ReadAsStreamAsync();
            }
            var curlTask = _curlMulti.AddToQueue(request, contentStream, config, cancellationToken);

            try
            {
                var rsp = await curlTask;

                return rsp.Message;
            }
            catch(Exception e)
            {
                throw new HttpRequestException(e.Message, e);
            }           
        }

        CurlHttpConfig CopyCurrentConfig()
        {
            var config = new CurlHttpConfig()
            {
                SslProtocols = SslProtocols,
                Proxy = Proxy,
                PreAuthenticate = PreAuthenticate,
                MaxResponseHeadersLength = MaxResponseHeadersLength,
                MaxRequestContentBufferSize = MaxRequestContentBufferSize,
                MaxConnectionsPerServer = MaxConnectionsPerServer,
                MaxAutomaticRedirections = MaxAutomaticRedirections,
                DefaultProxyCredentials = DefaultProxyCredentials,
                Credentials = Credentials,
                CookieContainer = CookieContainer,
                CheckCertificateRevocationList = CheckCertificateRevocationList,
                AutomaticDecompression = AutomaticDecompression,
                AllowAutoRedirect = AllowAutoRedirect,
                UseDefaultCredentials = UseDefaultCredentials,
                UseProxy = UseProxy
            };

            return config;
        }
    }
}
