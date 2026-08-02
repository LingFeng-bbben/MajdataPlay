using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MajdataPlay.Net.Curl.PInvoke
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
        // ==========================================
        // 1. Callback Functions (FUNCTIONPOINT: 20000+)
        // ==========================================

        /// <summary>Callback function for receiving downloaded data.</summary>
        CURLOPT_WRITEFUNCTION = 20011,

        /// <summary>Callback function for reading data to upload.</summary>
        CURLOPT_READFUNCTION = 20012,

        /// <summary>Callback function for progress meter updates.</summary>
        CURLOPT_PROGRESSFUNCTION = 20056,

        /// <summary>Callback function for receiving HTTP header data.</summary>
        CURLOPT_HEADERFUNCTION = 20079,

        /// <summary>Callback function for debug information and tracing.</summary>
        CURLOPT_DEBUGFUNCTION = 20094,

        /// <summary>Modern progress meter callback (replaces <see cref="CURLOPT_PROGRESSFUNCTION"/>).</summary>
        CURLOPT_XFERINFOFUNCTION = 20219,

        // ==========================================
        // 2. Pointers & Strings (OBJECTPOINT: 10000+)
        // ==========================================

        /// <summary>Custom pointer/stream passed to <see cref="CURLOPT_WRITEFUNCTION"/>.</summary>
        CURLOPT_WRITEDATA = 10001,

        /// <summary>Target URL to fetch or communicate with (String).</summary>
        CURLOPT_URL = 10002,

        /// <summary>Proxy server host address (String).</summary>
        CURLOPT_PROXY = 10004,

        /// <summary>Authentication credentials in "user:password" format (String).</summary>
        CURLOPT_USERPWD = 10005,

        /// <summary>Proxy authentication credentials (String).</summary>
        CURLOPT_PROXYUSERPWD = 10006,

        /// <summary>HTTP byte-range to download (String).</summary>
        CURLOPT_RANGE = 10007,

        /// <summary>Custom pointer/stream passed to <see cref="CURLOPT_READFUNCTION"/>.</summary>
        CURLOPT_READDATA = 10009,

        /// <summary>Pointer to a buffer for human-readable error messages.</summary>
        CURLOPT_ERRORBUFFER = 10010,

        /// <summary>HTTP POST payload data (String or Pointer).</summary>
        CURLOPT_POSTFIELDS = 10015,

        /// <summary>HTTP Referer header value (String).</summary>
        CURLOPT_REFERER = 10016,

        /// <summary>HTTP User-Agent header value (String).</summary>
        CURLOPT_USERAGENT = 10018,

        /// <summary>HTTP Cookie header content to send in the request (String).</summary>
        CURLOPT_COOKIE = 10022,

        /// <summary>Custom HTTP headers (curl_slist pointer).</summary>
        CURLOPT_HTTPHEADER = 10023,

        /// <summary>Path to the client SSL certificate file (String).</summary>
        CURLOPT_SSLCERT = 10025,

        /// <summary>Password required for the SSL private key (String).</summary>
        CURLOPT_KEYPASSWD = 10026,

        /// <summary>List of FTP/SFTP commands to execute before the transfer (curl_slist).</summary>
        CURLOPT_QUOTE = 10028,

        /// <summary>Custom pointer/stream passed to <see cref="CURLOPT_HEADERFUNCTION"/>.</summary>
        CURLOPT_HEADERDATA = 10029,

        /// <summary>File path to read initial cookies from (String).</summary>
        CURLOPT_COOKIEFILE = 10031,

        /// <summary>Custom HTTP request method to use, e.g., "DELETE" or "PATCH" (String).</summary>
        CURLOPT_CUSTOMREQUEST = 10036,

        /// <summary>List of FTP/SFTP commands to execute after the transfer (curl_slist).</summary>
        CURLOPT_POSTQUOTE = 10039,

        /// <summary>File path to save updated cookies to upon closing (String).</summary>
        CURLOPT_COOKIEJAR = 10082,

        /// <summary>Path to the Certificate Authority (CA) bundle file (String).</summary>
        CURLOPT_CAINFO = 10065,

        /// <summary>Directory containing SSL CA certificates (String).</summary>
        CURLOPT_CAPATH = 10097,

        /// <summary>Supported encodings for automatic decompression, e.g., "gzip, deflate" (String).</summary>
        CURLOPT_ACCEPT_ENCODING = 10102,

        // ==========================================
        // 3. Integers & Booleans (LONG: 0+)
        // ==========================================

        /// <summary>Override the remote port number (long).</summary>
        CURLOPT_PORT = 3,

        /// <summary>Maximum timeout for the entire request in seconds (long).</summary>
        CURLOPT_TIMEOUT = 13,

        /// <summary>Size of the file being uploaded (long).</summary>
        CURLOPT_INFILESIZE = 14,

        /// <summary>Low transfer speed threshold in bytes per second (long).</summary>
        CURLOPT_LOW_SPEED_LIMIT = 19,

        /// <summary>Time limit in seconds to stay below <see cref="CURLOPT_LOW_SPEED_LIMIT"/> before aborting (long).</summary>
        CURLOPT_LOW_SPEED_TIME = 20,

        /// <summary>Offset in bytes to resume a transfer from (long).</summary>
        CURLOPT_RESUME_FROM = 21,

        /// <summary>Convert Unix newlines to CRLF (1 = Yes, 0 = No).</summary>
        CURLOPT_CRLF = 27,

        /// <summary>Preferred SSL/TLS version to use (long).</summary>
        CURLOPT_SSLVERSION = 32,

        /// <summary>Enable verbose debugging output (1 = Enable, 0 = Disable).</summary>
        CURLOPT_VERBOSE = 41,

        /// <summary>Include HTTP headers in the body data output (1 = Enable, 0 = Disable).</summary>
        CURLOPT_HEADER = 42,

        /// <summary>Disable the internal progress meter (1 = Disable [Default], 0 = Enable).</summary>
        CURLOPT_NOPROGRESS = 43,

        /// <summary>Issue an HTTP HEAD request without downloading the body (1 = Yes, 0 = No).</summary>
        CURLOPT_NOBODY = 44,

        /// <summary>Fail the request silently if HTTP status code is &gt;= 400 (1 = Yes, 0 = No).</summary>
        CURLOPT_FAILONERROR = 45,

        /// <summary>Enable data upload mode (1 = Enable, 0 = Disable).</summary>
        CURLOPT_UPLOAD = 46,

        /// <summary>Enable standard HTTP POST mode (1 = Enable, 0 = Disable).</summary>
        CURLOPT_POST = 47,

        /// <summary>List FTP directory names only (1 = Yes, 0 = No).</summary>
        CURLOPT_DIRLISTONLY = 48,

        /// <summary>Append to a remote FTP file instead of overwriting (1 = Yes, 0 = No).</summary>
        CURLOPT_APPEND = 50,

        /// <summary>Automatically follow HTTP 3xx redirections (1 = Enable, 0 = Disable).</summary>
        CURLOPT_FOLLOWLOCATION = 52,

        /// <summary>Use ASCII transfer mode for FTP/LDAP (1 = Enable, 0 = Disable).</summary>
        CURLOPT_TRANSFERTEXT = 53,

        /// <summary>Enable HTTP PUT mode (1 = Enable, 0 = Disable).</summary>
        CURLOPT_PUT = 54,

        /// <summary>Automatically update the Referer header on redirections (1 = Enable, 0 = Disable).</summary>
        CURLOPT_AUTOREFERER = 58,

        /// <summary>Port number of the proxy server (long).</summary>
        CURLOPT_PROXYPORT = 59,

        /// <summary>Size of the POST payload data (long).</summary>
        CURLOPT_POSTFIELDSIZE = 60,

        /// <summary>Tunnel all HTTP operations through the specified proxy (1 = Enable, 0 = Disable).</summary>
        CURLOPT_HTTPPROXYTUNNEL = 61,

        /// <summary>Verify the authenticity of the SSL peer certificate (1 = Verify, 0 = Ignore).</summary>
        CURLOPT_SSL_VERIFYPEER = 64,

        /// <summary>Maximum allowed number of HTTP redirections to follow (long).</summary>
        CURLOPT_MAXREDIRS = 68,

        /// <summary>Request the last modification timestamp of the remote file (1 = Yes, 0 = No).</summary>
        CURLOPT_FILETIME = 69,

        /// <summary>Maximum number of connections to retain in the connection pool (long).</summary>
        CURLOPT_MAXCONNECTS = 71,

        /// <summary>Force a new connection instead of reusing a cached one (1 = Enable, 0 = Disable).</summary>
        CURLOPT_FRESH_CONNECT = 74,

        /// <summary>Close the connection immediately after the transfer completes (1 = Enable, 0 = Disable).</summary>
        CURLOPT_FORBID_REUSE = 75,

        /// <summary>Maximum timeout for the connection establishment phase in seconds (long).</summary>
        CURLOPT_CONNECTTIMEOUT = 78,

        /// <summary>Reset HTTP method to standard GET (1 = Enable, 0 = Disable).</summary>
        CURLOPT_HTTPGET = 80,

        /// <summary>Verify the SSL certificate host name (2 = Verify, 0 = Ignore).</summary>
        CURLOPT_SSL_VERIFYHOST = 81,

        /// <summary>Force a specific HTTP protocol version (long).</summary>
        CURLOPT_HTTP_VERSION = 84,

        /// <summary>Use EPSV commands for FTP transfers (1 = Yes, 0 = No).</summary>
        CURLOPT_FTP_USE_EPSV = 85,

        /// <summary>Use EPRT commands for FTP transfers (1 = Yes, 0 = No).</summary>
        CURLOPT_FTP_USE_EPRT = 106,

        /// <summary>HTTP authentication schemes, such as Basic or Digest (long/bitmask).</summary>
        CURLOPT_HTTPAUTH = 107,

        /// <summary>Ignore the Content-Length header returned by the server (1 = Yes, 0 = No).</summary>
        CURLOPT_IGNORE_CONTENT_LENGTH = 136,

        /// <summary>Enable TCP Keep-Alive probes (1 = Enable, 0 = Disable).</summary>
        CURLOPT_TCP_KEEPALIVE = 213,

        // ==========================================
        // 4. Large Integers (OFF_T: 30000+) - For files > 2GB
        // ==========================================

        /// <summary>Size of the uploaded file, supporting sizes &gt; 2GB (long/64-bit).</summary>
        CURLOPT_INFILESIZE_LARGE = 30115,

        /// <summary>Offset in bytes to resume a transfer from, supporting sizes &gt; 2GB (long/64-bit).</summary>
        CURLOPT_RESUME_FROM_LARGE = 30116,

        /// <summary>Maximum upload rate limit in bytes per second (long/64-bit).</summary>
        CURLOPT_MAX_SEND_SPEED_LARGE = 30145,

        /// <summary>Maximum download rate limit in bytes per second (long/64-bit).</summary>
        CURLOPT_MAX_RECV_SPEED_LARGE = 30146
    }
}
