using MajdataPlay.Net.Curl.Core.PInvoke;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;
using MajdataPlay.Net.Curl.Core;
using MajdataPlay.Net.Curl.Utils;

#nullable enable
namespace MajdataPlay.Net.Curl
{
    public readonly struct CurlHttpConfig
    {
        public SslProtocols SslProtocols { get; init; }
        public IWebProxy? Proxy { get; init; }
        public bool PreAuthenticate { get; init; }
        public int MaxResponseHeadersLength { get; init; }
        public long MaxRequestContentBufferSize { get; init; }
        public int MaxConnectionsPerServer { get; init; }
        public int MaxAutomaticRedirections { get; init; }
        public ICredentials? DefaultProxyCredentials { get; init; }
        public ICredentials? Credentials { get; init; }
        public CookieContainer? CookieContainer { get; init; }
        public bool CheckCertificateRevocationList { get; init; }
        public DecompressionMethods AutomaticDecompression { get; init; }
        public bool AllowAutoRedirect { get; init; }
        public bool UseDefaultCredentials { get; init; }
        public bool UseProxy { get; init; }

        internal readonly void ApplyToMulti(CurlMulti multi)
        {
            var multiHandle = multi.Handle;

            LibCurl.curl_multi_setopt(multiHandle, CURLMoption.CURLMOPT_MAX_HOST_CONNECTIONS, (IntPtr)MaxConnectionsPerServer);
        }

        internal readonly void ApplyToRequest(CurlRequest request)
        {
            var easyHandle = request.Handle;
            var requestUri = request.RequestUri;
            var returnCode = default(CurlCode);

            returnCode = LibCurl.curl_easy_setopt(easyHandle, CurlOption.FollowLocation, AllowAutoRedirect ? 1 : 0);
            CurlUtility.EnsureSuccess(easyHandle, returnCode);
            if (AllowAutoRedirect)
            {
                returnCode = LibCurl.curl_easy_setopt(easyHandle, CurlOption.MaxRedirs, MaxAutomaticRedirections);
                CurlUtility.EnsureSuccess(easyHandle, returnCode);
            }

            if (AutomaticDecompression != DecompressionMethods.None)
            {
                var encodings = GetEncodingString(AutomaticDecompression);
                returnCode = LibCurl.curl_easy_setopt(easyHandle, CurlOption.AcceptEncoding, encodings);
                CurlUtility.EnsureSuccess(easyHandle, returnCode);
            }

            if (UseProxy && Proxy != null && !Proxy.IsBypassed(requestUri))
            {
                var proxyUri = Proxy.GetProxy(requestUri);
                returnCode = LibCurl.curl_easy_setopt(easyHandle, CurlOption.Proxy, proxyUri.ToString());
                CurlUtility.EnsureSuccess(easyHandle, returnCode);

                var proxyCred = (DefaultProxyCredentials ?? Proxy.Credentials)?.GetCredential(proxyUri, "Basic");
                if (proxyCred != null)
                {
                    var userPwd = $"{proxyCred.UserName}:{proxyCred.Password}";
                    returnCode = LibCurl.curl_easy_setopt(easyHandle, CurlOption.ProxyUserPwd, userPwd);
                    CurlUtility.EnsureSuccess(easyHandle, returnCode);
                }
            }
            else if (!UseProxy)
            {
                returnCode = LibCurl.curl_easy_setopt(easyHandle, CurlOption.Proxy, "");
                CurlUtility.EnsureSuccess(easyHandle, returnCode);
            }

            if (UseDefaultCredentials)
            {
                returnCode = LibCurl.curl_easy_setopt(easyHandle, CurlOption.UserPassword, ":");
                CurlUtility.EnsureSuccess(easyHandle, returnCode);
                returnCode = LibCurl.curl_easy_setopt(easyHandle, CurlOption.HttpAuth, (long)(CurlAuth.NTLM | CurlAuth.GSSNEGOTIATE));
                CurlUtility.EnsureSuccess(easyHandle, returnCode);
            }
            else if (Credentials != null)
            {
                var cred = Credentials.GetCredential(requestUri, "Basic");
                if (cred != null)
                {
                    returnCode = LibCurl.curl_easy_setopt(easyHandle, CurlOption.UserPassword, $"{cred.UserName}:{cred.Password}");
                    CurlUtility.EnsureSuccess(easyHandle, returnCode);

                    var authType = PreAuthenticate ? (long)CurlAuth.Basic : (long)CurlAuth.Any;
                    returnCode = LibCurl.curl_easy_setopt(easyHandle, CurlOption.HttpAuth, authType);
                    CurlUtility.EnsureSuccess(easyHandle, returnCode);
                }
            }

            // SSL/TLS version
            var sslVersion = MapSslProtocols(SslProtocols);
            returnCode = LibCurl.curl_easy_setopt(easyHandle, CurlOption.SslVersion, sslVersion);
            CurlUtility.EnsureSuccess(easyHandle, returnCode);

            if (!CheckCertificateRevocationList)
            {
                // CURLSSLOPT_NO_REVOKE = 2
                returnCode = LibCurl.curl_easy_setopt(easyHandle, CurlOption.SslOptions, 2L);
                CurlUtility.EnsureSuccess(easyHandle, returnCode);
            }
            else
            {
                returnCode = LibCurl.curl_easy_setopt(easyHandle, CurlOption.SslOptions, 0L);
                CurlUtility.EnsureSuccess(easyHandle, returnCode);
            }

            if(CookieContainer is not null)
            {
                var cookieHeader = CookieContainer.GetCookieHeader(requestUri);

                if (!string.IsNullOrEmpty(cookieHeader))
                {
                    returnCode = LibCurl.curl_easy_setopt(easyHandle, CurlOption.Cookie, cookieHeader);
                    CurlUtility.EnsureSuccess(easyHandle, returnCode);
                }
            }

        }
        readonly string GetEncodingString(DecompressionMethods methods)
        {
            var sb = new StringBuilder();
            if (methods.HasFlag(DecompressionMethods.GZip))
            {
                sb.Append("gzip");
            }
            if (methods.HasFlag(DecompressionMethods.Deflate))
            {
                sb.Append(',');
                sb.Append("deflate");
            }
            //if (methods.HasFlag(DecompressionMethods.Brotli))
            //{
            //    list.Add("br");
            //}
            return sb.ToString();
        }
        readonly long MapSslProtocols(SslProtocols protocols)
        {
            // CURL_SSLVERSION_TLSv1_2 = 6, CURL_SSLVERSION_TLSv1_3 = 7
            if (protocols.HasFlag(SslProtocols.Tls13))
            {
                return 7;
            }
            if (protocols.HasFlag(SslProtocols.Tls12))
            {
                return 6;
            }
            return 0; // CURL_SSLVERSION_DEFAULT
        }
    }
}
