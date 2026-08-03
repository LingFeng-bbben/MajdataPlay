using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MajdataPlay.Net.Curl.PInvoke
{
    /// <summary>
    /// Defines the libcurl return codes (corresponding to CURLcode in curl/curl.h).
    /// </summary>
    /// <remarks>
    /// Almost all libcurl functions return a code from this enum to indicate success or the reason for failure.
    /// <c>CURLE_OK</c> (0) indicates successful completion.
    /// </remarks>
    internal enum CurlCode
    {
        /// <summary>All fine. Proceed as usual.</summary>
        CURLE_OK = 0,

        /// <summary>The URL you passed to libcurl used a protocol that this libcurl does not support.</summary>
        CURLE_UNSUPPORTED_PROTOCOL = 1,

        /// <summary>Early initialization code failed. This is likely an internal error or problem with resources.</summary>
        CURLE_FAILED_INIT = 2,

        /// <summary>The URL was not properly formatted.</summary>
        CURLE_URL_MALFORMAT = 3,

        /// <summary>A requested feature, protocol, or option was not found built-in in this libcurl due to a build-time decision.</summary>
        CURLE_NOT_BUILT_IN = 4,

        /// <summary>Couldn't resolve the proxy. The given proxy host could not be resolved.</summary>
        CURLE_COULDNT_RESOLVE_PROXY = 5,

        /// <summary>Couldn't resolve host. The given remote host was not resolved.</summary>
        CURLE_COULDNT_RESOLVE_HOST = 6,

        /// <summary>Failed to connect to host or proxy.</summary>
        CURLE_COULDNT_CONNECT = 7,

        /// <summary>The server sent data libcurl could not parse or understand.</summary>
        CURLE_WEIRD_SERVER_REPLY = 8,

        /// <summary>Access denied to the resource given in the URL.</summary>
        CURLE_REMOTE_ACCESS_DENIED = 9,

        /// <summary>While waiting for the server to connect back when an active FTP session is used, an error code was sent over the control connection or similar.</summary>
        CURLE_FTP_ACCEPT_FAILED = 10,

        /// <summary>After having sent the FTP password to the server, libcurl expected an OK reply which it did not get.</summary>
        CURLE_FTP_WEIRD_PASS_REPLY = 11,

        /// <summary>During an active FTP session while waiting for the server to connect, the TIMEOUT expired.</summary>
        CURLE_FTP_ACCEPT_TIMEOUT = 12,

        /// <summary>libcurl failed to get a sensible result back from the server as a response to either a PASV or a EPSV command.</summary>
        CURLE_FTP_WEIRD_PASV_REPLY = 13,

        /// <summary>FTP servers return a 227-line as a response to a PASV command. If libcurl fails to parse that line, this return code is passed back.</summary>
        CURLE_FTP_WEIRD_227_FORMAT = 14,

        /// <summary>An internal failure to lookup the host used for the new connection.</summary>
        CURLE_FTP_CANT_GET_HOST = 15,

        /// <summary>A problem was detected in the HTTP2 framing layer. This is somewhat generic and can be one out of several problems.</summary>
        CURLE_HTTP2 = 16,

        /// <summary>Received an error when trying to set the transfer mode to binary or ASCII.</summary>
        CURLE_FTP_COULDNT_SET_TYPE = 17,

        /// <summary>A file transfer was shorter or larger than expected. This happens when the server first reports an expected transfer size, and then delivers data that doesn't match the previously given size.</summary>
        CURLE_PARTIAL_FILE = 18,

        /// <summary>This was either a weird reply to a 'RETR' command or a zero byte transfer complete.</summary>
        CURLE_FTP_COULDNT_RETR_FILE = 19,

        /// <summary>Obsolete error code 20.</summary>
        /// <remarks>No longer used by libcurl.</remarks>
        [Obsolete("No longer used by libcurl", true)]
        CURLE_OBSOLETE20 = 20,

        /// <summary>When sending custom "QUOTE" commands to the remote server, one of the commands returned an error code that was 400 or higher.</summary>
        CURLE_QUOTE_ERROR = 21,

        /// <summary>This is returned if CURLOPT_FAILONERROR is set TRUE and the HTTP server returns an error code that is >= 400.</summary>
        CURLE_HTTP_RETURNED_ERROR = 22,

        /// <summary>An error occurred when writing received data to a local file, or an error was returned to libcurl from a write callback.</summary>
        CURLE_WRITE_ERROR = 23,

        /// <summary>Obsolete error code 24.</summary>
        /// <remarks>No longer used by libcurl.</remarks>
        [Obsolete("No longer used by libcurl", true)]
        CURLE_OBSOLETE24 = 24,

        /// <summary>Failed starting the upload. For FTP, the server typically denied the STOR command.</summary>
        CURLE_UPLOAD_FAILED = 25,

        /// <summary>There was a problem reading a local file or an error returned by the read callback.</summary>
        CURLE_READ_ERROR = 26,

        /// <summary>A memory allocation request failed. This is a severe error and things are likely to be somewhat messed up.</summary>
        CURLE_OUT_OF_MEMORY = 27,

        /// <summary>Operation timeout. The specified time-out period was reached according to the conditions.</summary>
        CURLE_OPERATION_TIMEDOUT = 28,

        /// <summary>Obsolete error code 29.</summary>
        /// <remarks>No longer used by libcurl.</remarks>
        [Obsolete("No longer used by libcurl", true)]
        CURLE_OBSOLETE29 = 29,

        /// <summary>The FTP PORT command returned error. This mostly happens when you haven't specified a good enough address for libcurl to use.</summary>
        CURLE_FTP_PORT_FAILED = 30,

        /// <summary>The FTP REST command returned error. This should never happen if the server is sane.</summary>
        CURLE_FTP_COULDNT_USE_REST = 31,

        /// <summary>Obsolete error code 32.</summary>
        /// <remarks>No longer used by libcurl.</remarks>
        [Obsolete("No longer used by libcurl", true)]
        CURLE_OBSOLETE32 = 32,

        /// <summary>The server does not support or accept range requests.</summary>
        CURLE_RANGE_ERROR = 33,

        /// <summary>An error occurred when sending the HTTP POST data.</summary>
        CURLE_HTTP_POST_ERROR = 34,

        /// <summary>A problem occurred somewhere in the SSL/TLS handshake.</summary>
        CURLE_SSL_CONNECT_ERROR = 35,

        /// <summary>The download could not be resumed because the specified offset was out of the file boundary.</summary>
        CURLE_BAD_DOWNLOAD_RESUME = 36,

        /// <summary>A file given with FILE:// couldn't be opened. Most likely because the file path doesn't identify an existing file.</summary>
        CURLE_FILE_COULDNT_READ_FILE = 37,

        /// <summary>LDAP cannot bind. LDAP bind operation failed.</summary>
        CURLE_LDAP_CANNOT_BIND = 38,

        /// <summary>LDAP search failed.</summary>
        CURLE_LDAP_SEARCH_FAILED = 39,

        /// <summary>Obsolete error code 40.</summary>
        /// <remarks>No longer used by libcurl.</remarks>
        [Obsolete("No longer used by libcurl", true)]
        CURLE_OBSOLETE40 = 40,

        /// <summary>Function not found. A required zlib function was not found.</summary>
        CURLE_FUNCTION_NOT_FOUND = 41,

        /// <summary>Aborted by callback. A callback returned "abort" to libcurl.</summary>
        CURLE_ABORTED_BY_CALLBACK = 42,

        /// <summary>A function was called with a bad parameter.</summary>
        CURLE_BAD_FUNCTION_ARGUMENT = 43,

        /// <summary>Obsolete error code 44.</summary>
        /// <remarks>No longer used by libcurl.</remarks>
        [Obsolete("No longer used by libcurl", true)]
        CURLE_OBSOLETE44 = 44,

        /// <summary>Interface error. A specified outgoing interface could not be used.</summary>
        CURLE_INTERFACE_FAILED = 45,

        /// <summary>Obsolete error code 46.</summary>
        /// <remarks>No longer used by libcurl.</remarks>
        [Obsolete("No longer used by libcurl", true)]
        CURLE_OBSOLETE46 = 46,

        /// <summary>Too many redirects. When following redirects, libcurl hit the maximum amount.</summary>
        CURLE_TOO_MANY_REDIRECTS = 47,

        /// <summary>An option passed to libcurl is not recognized/known.</summary>
        CURLE_UNKNOWN_OPTION = 48,

        /// <summary>An option passed in to a setopt was incorrectly formatted.</summary>
        CURLE_SETOPT_OPTION_SYNTAX = 49,

        /// <summary>Obsolete error code 50.</summary>
        /// <remarks>No longer used by libcurl.</remarks>
        [Obsolete("No longer used by libcurl", true)]
        CURLE_OBSOLETE50 = 50,

        /// <summary>Obsolete error code 51.</summary>
        /// <remarks>No longer used by libcurl.</remarks>
        [Obsolete("No longer used by libcurl", true)]
        CURLE_OBSOLETE51 = 51,

        /// <summary>Nothing was returned from the server, and under the circumstances, getting nothing is considered an error.</summary>
        CURLE_GOT_NOTHING = 52,

        /// <summary>The specified crypto engine wasn't found.</summary>
        CURLE_SSL_ENGINE_NOTFOUND = 53,

        /// <summary>Failed setting the selected SSL crypto engine as default.</summary>
        CURLE_SSL_ENGINE_SETFAILED = 54,

        /// <summary>Failed sending network data.</summary>
        CURLE_SEND_ERROR = 55,

        /// <summary>Failure with receiving network data.</summary>
        CURLE_RECV_ERROR = 56,

        /// <summary>Obsolete error code 57.</summary>
        /// <remarks>No longer used by libcurl.</remarks>
        [Obsolete("No longer used by libcurl", true)]
        CURLE_OBSOLETE57 = 57,

        /// <summary>Problem with the local client certificate.</summary>
        CURLE_SSL_CERTPROBLEM = 58,

        /// <summary>Couldn't use specified cipher.</summary>
        CURLE_SSL_CIPHER = 59,

        /// <summary>The remote server's SSL certificate or SSH md5 fingerprint was deemed not OK.</summary>
        CURLE_PEER_FAILED_VERIFICATION = 60,

        /// <summary>Unrecognized transfer encoding.</summary>
        CURLE_BAD_CONTENT_ENCODING = 61,

        /// <summary>Obsolete error code 62.</summary>
        /// <remarks>No longer used by libcurl.</remarks>
        [Obsolete("No longer used by libcurl", true)]
        CURLE_OBSOLETE62 = 62,

        /// <summary>Maximum file size exceeded.</summary>
        CURLE_FILESIZE_EXCEEDED = 63,

        /// <summary>Requested FTP SSL level failed.</summary>
        CURLE_USE_SSL_FAILED = 64,

        /// <summary>When doing a send operation curl had to rewind the data to retransmit, but the rewinding operation failed.</summary>
        CURLE_SEND_FAIL_REWIND = 65,

        /// <summary>Initiating the SSL Engine failed.</summary>
        CURLE_SSL_ENGINE_INITFAILED = 66,

        /// <summary>The remote server denied curl to login.</summary>
        CURLE_LOGIN_DENIED = 67,

        /// <summary>File not found on TFTP server.</summary>
        CURLE_TFTP_NOTFOUND = 68,

        /// <summary>Permission problem on TFTP server.</summary>
        CURLE_TFTP_PERM = 69,

        /// <summary>Out of disk space on the server.</summary>
        CURLE_REMOTE_DISK_FULL = 70,

        /// <summary>Illegal TFTP operation.</summary>
        CURLE_TFTP_ILLEGAL = 71,

        /// <summary>Unknown TFTP transfer ID.</summary>
        CURLE_TFTP_UNKNOWNID = 72,

        /// <summary>File already exists and will not be overwritten.</summary>
        CURLE_REMOTE_FILE_EXISTS = 73,

        /// <summary>This error should never be returned by a properly functioning TFTP server.</summary>
        CURLE_TFTP_NOSUCHUSER = 74,

        /// <summary>Obsolete error code 75.</summary>
        /// <remarks>No longer used by libcurl.</remarks>
        [Obsolete("No longer used by libcurl", true)]
        CURLE_OBSOLETE75 = 75,

        /// <summary>Obsolete error code 76.</summary>
        /// <remarks>No longer used by libcurl.</remarks>
        [Obsolete("No longer used by libcurl", true)]
        CURLE_OBSOLETE76 = 76,

        /// <summary>Problem with reading the SSL CA cert (path? access rights?).</summary>
        CURLE_SSL_CACERT_BADFILE = 77,

        /// <summary>The resource referenced in the URL does not exist.</summary>
        CURLE_REMOTE_FILE_NOT_FOUND = 78,

        /// <summary>An unspecified error occurred during the SSH session.</summary>
        CURLE_SSH = 79,

        /// <summary>Failed to shut down the SSL connection.</summary>
        CURLE_SSL_SHUTDOWN_FAILED = 80,

        /// <summary>Socket is not ready for send/recv. Wait until it's ready and try again.</summary>
        CURLE_AGAIN = 81,

        /// <summary>Failed to load CRL file (Certificate Revocation List).</summary>
        CURLE_SSL_CRL_BADFILE = 82,

        /// <summary>Issuer check failed.</summary>
        CURLE_SSL_ISSUER_ERROR = 83,

        /// <summary>The FTP server does not understand the PRET command at all or does not support the given argument.</summary>
        CURLE_FTP_PRET_FAILED = 84,

        /// <summary>Mismatch of RTSP CSeq numbers.</summary>
        CURLE_RTSP_CSEQ_ERROR = 85,

        /// <summary>Mismatch of RTSP Session Identifiers.</summary>
        CURLE_RTSP_SESSION_ERROR = 86,

        /// <summary>Unable to parse FTP file list (during FTP wildcard downloading).</summary>
        CURLE_FTP_BAD_FILE_LIST = 87,

        /// <summary>Chunk callback reported error.</summary>
        CURLE_CHUNK_FAILED = 88,

        /// <summary>No connection available, the session will be queued.</summary>
        CURLE_NO_CONNECTION_AVAILABLE = 89,

        /// <summary>Failed to match the pinned public key specified with CURLOPT_PINNEDPUBLICKEY.</summary>
        CURLE_SSL_PINNEDPUBKEYNOTMATCH = 90,

        /// <summary>Status returned failure when asked with CURLOPT_SSL_VERIFYSTATUS.</summary>
        CURLE_SSL_INVALIDCERTSTATUS = 91,

        /// <summary>Stream error in the HTTP/2 framing layer.</summary>
        CURLE_HTTP2_STREAM = 92,

        /// <summary>An API function was called from inside a callback.</summary>
        CURLE_RECURSIVE_API_CALL = 93,

        /// <summary>An authentication function returned an error.</summary>
        CURLE_AUTH_ERROR = 94,

        /// <summary>A problem was detected in the HTTP/3 layer.</summary>
        CURLE_HTTP3 = 95,

        /// <summary>QUIC connection error. This error may be caused by an SSL library error.</summary>
        CURLE_QUIC_CONNECT_ERROR = 96,

        /// <summary>Proxy handshake error.</summary>
        CURLE_PROXY = 97,

        /// <summary>SSL Client Certificate required.</summary>
        CURLE_SSL_CLIENTCERT = 98,

        /// <summary>An internal call to poll() or select() returned error that is not recoverable.</summary>
        CURLE_UNRECOVERABLE_POLL = 99
    }
}
