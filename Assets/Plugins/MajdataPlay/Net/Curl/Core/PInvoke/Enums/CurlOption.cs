using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MajdataPlay.Net.Curl.Core.PInvoke
{
    /// <summary>
    /// Defines the complete and commonly used libcurl options (corresponding to CURLoption in curl/curl.h).
    /// </summary>
    /// <remarks>
    /// Value calculation rules based on libcurl data types:
    /// <list type="bullet">
    /// <item><description><c>LONG</c> = 0 + offset</description></item>
    /// <item><description><c>OBJECTPOINT</c> (Pointers / Strings) = 10000 + offset</description></item>
    /// <item><description><c>FUNCTIONPOINT</c> (Callback Delegates) = 20000 + offset</description></item>
    /// <item><description><c>OFF_T</c> (Large Integers / 64-bit) = 30000 + offset</description></item>
    /// </list>
    /// </remarks>
    internal enum CurlOption
    {
        // LONG options
        File = 10000 - 1, // placeholder, never used

        /// <summary>Enable verbose output.</summary>
        Verbose = 41,

        /// <summary>Enable progress meter.</summary>
        NoProgress = 43,

        /// <summary>Specify timeout in seconds.</summary>
        Timeout = 13,

        /// <summary>Connection timeout in seconds.</summary>
        ConnectTimeout = 78,

        /// <summary>Maximum number of redirects.</summary>
        MaxRedirs = 68,

        /// <summary>Follow HTTP redirects.</summary>
        FollowLocation = 52,

        /// <summary>HTTP version.</summary>
        HttpVersion = 84,

        /// <summary>Fail on HTTP errors.</summary>
        FailOnError = 45,

        /// <summary>Use TCP keepalive.</summary>
        TcpKeepAlive = 213,

        /// <summary>TCP keepalive idle.</summary>
        TcpKeepIdle = 214,

        /// <summary>TCP keepalive interval.</summary>
        TcpKeepIntvl = 215,

        /// <summary>IPv4 only.</summary>
        IpResolve = 113,

        /// <summary>Transfer direction.</summary>
        Upload = 46,

        /// <summary>HTTP POST.</summary>
        Post = 47,


        // OBJECTPOINT options (10000+)

        /// <summary>Request URL.</summary>
        Url = 10002,

        /// <summary>HTTP user agent.</summary>
        UserAgent = 10018,

        /// <summary>HTTP headers list.</summary>
        HttpHeader = 10023,

        /// <summary>Cookie string.</summary>
        Cookie = 10022,

        /// <summary>Cookie file.</summary>
        CookieFile = 10031,

        /// <summary>Cookie save file.</summary>
        CookieJar = 10082,

        /// <summary>Custom HTTP method.</summary>
        CustomRequest = 10036,

        /// <summary>POST body.</summary>
        PostFields = 10015,

        /// <summary>Proxy URL.</summary>
        Proxy = 10004,

        /// <summary>
        /// Basic Authentication
        /// </summary>
        UserPassword = 10005,

        /// <summary>Proxy username/password.</summary>
        ProxyUserPwd = 10006,

        /// <summary>SSL CA certificate file.</summary>
        CaInfo = 10065,

        /// <summary>SSL CA directory.</summary>
        CaPath = 10097,

        /// <summary>Client certificate.</summary>
        SslCert = 10025,

        /// <summary>Client private key.</summary>
        Key = 10087,

        /// <summary>
        /// Represents a private handle within libcurl.
        /// </summary>
        Private = 10103,

        /// <summary>
        /// Pass a pointer to the write callback userdata.
        /// </summary>
        WriteData = 10001,

        /// <summary>
        /// Pass a pointer to the read callback userdata.
        /// </summary>
        ReadData = 10009,

        /// <summary>
        /// Pass a pointer to the header callback userdata.
        /// </summary>
        HeaderData = 10029,


        // FUNCTIONPOINT options (20000+)

        /// <summary>Write callback.</summary>
        WriteFunction = 20011,

        /// <summary>Header callback.</summary>
        HeaderFunction = 20079,

        /// <summary>Read callback.</summary>
        ReadFunction = 20012,

        /// <summary>Progress callback.</summary>
        ProgressFunction = 20056,

        /// <summary>Debug callback.</summary>
        DebugFunction = 20094,


        // OFF_T options (30000+)

        /// <summary>Upload size.</summary>
        InFileSizeLarge = 30115,

        /// <summary>POST size.</summary>
        PostFieldSizeLarge = 30120,

        /// <summary>Maximum download speed.</summary>
        MaxRecvSpeedLarge = 30146,

        /// <summary>Maximum upload speed.</summary>
        MaxSendSpeedLarge = 30145,


        // More common options

        /// <summary>Buffer size.</summary>
        BufferSize = 98,

        /// <summary>DNS cache timeout.</summary>
        DnsCacheTimeout = 92,

        /// <summary>DNS servers.</summary>
        DnsServers = 102,

        /// <summary>Low speed limit.</summary>
        LowSpeedLimit = 19,

        /// <summary>Low speed time.</summary>
        LowSpeedTime = 20,

        /// <summary>Transfer encoding.</summary>
        TransferEncoding = 207,

        /// <summary>Accept gzip/br compression.</summary>
        AcceptEncoding = 10102,

        /// <summary>HTTP authentication.</summary>
        HttpAuth = 107,

        /// <summary>Proxy authentication.</summary>
        ProxyAuth = 111,

        /// <summary>Referer header.</summary>
        Referer = 10016,

        /// <summary>Range request.</summary>
        Range = 10007,

        /// <summary>Resume offset.</summary>
        ResumeFrom = 21,

        /// <summary>SSL verify peer.</summary>
        SslVerifyPeer = 64,

        /// <summary>
        /// SSL version
        /// </summary>
        SslVersion = 32,

        /// <summary>
        /// SSL options
        /// </summary>
        SslOptions = 216,

        /// <summary>SSL verify host.</summary>
        SslVerifyHost = 81,

        /// <summary>SSL cipher list.</summary>
        SslCipherList = 10083,

        /// <summary>HTTP proxy tunnel.</summary>
        HttpProxyTunnel = 61,

        /// <summary>Disable signals.</summary>
        Nosignal = 99,

        /// <summary>Unix socket path.</summary>
        UnixSocketPath = 10231,

        /// <summary>HTTP/3 support.</summary>
        Http3 = 264,

        /// <summary>Enable HTTP/2 prior knowledge.</summary>
        Http2PriorKnowledge = 150,


        /// <summary>
        /// Last option marker.
        /// </summary>
        Last = 314
    }
}
