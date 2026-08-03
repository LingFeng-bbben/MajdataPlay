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
    public enum CurlCode
    {
        /// <summary>All fine. Proceed as usual.</summary>
        Ok = 0,

        /// <summary>The URL uses an unsupported protocol.</summary>
        UnsupportedProtocol = 1,

        /// <summary>Early initialization failed.</summary>
        FailedInit = 2,

        /// <summary>The URL was malformed.</summary>
        UrlMalformed = 3,

        /// <summary>The requested feature was not built into libcurl.</summary>
        NotBuiltIn = 4,

        /// <summary>Could not resolve proxy host.</summary>
        CouldNotResolveProxy = 5,

        /// <summary>Could not resolve remote host.</summary>
        CouldNotResolveHost = 6,

        /// <summary>Failed to connect to host or proxy.</summary>
        CouldNotConnect = 7,

        /// <summary>The server reply could not be parsed.</summary>
        WeirdServerReply = 8,

        /// <summary>Access denied.</summary>
        RemoteAccessDenied = 9,

        /// <summary>FTP accept failed.</summary>
        FtpAcceptFailed = 10,

        /// <summary>FTP password reply was unexpected.</summary>
        FtpWeirdPassReply = 11,

        /// <summary>FTP accept timeout.</summary>
        FtpAcceptTimeout = 12,

        /// <summary>FTP PASV reply was invalid.</summary>
        FtpWeirdPasvReply = 13,

        /// <summary>FTP 227 response format error.</summary>
        FtpWeird227Format = 14,

        /// <summary>Could not get FTP host.</summary>
        FtpCannotGetHost = 15,

        /// <summary>HTTP/2 framing layer error.</summary>
        Http2 = 16,

        /// <summary>Failed setting FTP transfer type.</summary>
        FtpCouldNotSetType = 17,

        /// <summary>Partial file transfer.</summary>
        PartialFile = 18,

        /// <summary>FTP retrieval failed.</summary>
        FtpCouldNotRetrieveFile = 19,

        /// <summary>Obsolete error code.</summary>
        [Obsolete("No longer used by libcurl", true)]
        Obsolete20 = 20,

        /// <summary>QUOTE command failed.</summary>
        QuoteError = 21,

        /// <summary>HTTP server returned an error.</summary>
        HttpReturnedError = 22,

        /// <summary>Write callback failed.</summary>
        WriteError = 23,

        /// <summary>Upload failed.</summary>
        UploadFailed = 25,

        /// <summary>Read callback failed.</summary>
        ReadError = 26,

        /// <summary>Out of memory.</summary>
        OutOfMemory = 27,

        /// <summary>Operation timed out.</summary>
        OperationTimedOut = 28,

        /// <summary>FTP PORT command failed.</summary>
        FtpPortFailed = 30,

        /// <summary>HTTP POST error.</summary>
        HttpPostError = 34,

        /// <summary>SSL connection error.</summary>
        SslConnectError = 35,

        /// <summary>SSL certificate problem.</summary>
        SslCertificateProblem = 58,

        /// <summary>Peer certificate verification failed.</summary>
        PeerFailedVerification = 60,

        /// <summary>Bad content encoding.</summary>
        BadContentEncoding = 61,

        /// <summary>SSL shutdown failed.</summary>
        SslShutdownFailed = 80,

        /// <summary>Socket is not ready.</summary>
        Again = 81,

        /// <summary>HTTP/2 stream error.</summary>
        Http2Stream = 92,

        /// <summary>Recursive API call.</summary>
        RecursiveApiCall = 93,

        /// <summary>HTTP/3 layer error.</summary>
        Http3 = 95,

        /// <summary>QUIC connection error.</summary>
        QuicConnectError = 96,

        /// <summary>Proxy handshake error.</summary>
        Proxy = 97,

        /// <summary>SSL client certificate required.</summary>
        SslClientCertificate = 98,

        /// <summary>Unrecoverable poll error.</summary>
        UnrecoverablePoll = 99
    }
}
