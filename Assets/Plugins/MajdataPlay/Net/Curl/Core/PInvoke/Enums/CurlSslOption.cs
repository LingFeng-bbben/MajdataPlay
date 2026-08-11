using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MajdataPlay.Net.Curl.Core.PInvoke
{
    /// <summary>
    /// Specifies SSL/TLS behavior options for libcurl.
    /// 
    /// This enumeration maps to libcurl's CURLSSLOPT_* constants
    /// and is used with CURLOPT_SSL_OPTIONS.
    /// </summary>
    [Flags]
    public enum CurlSslOption : long
    {
        /// <summary>
        /// No special SSL options.
        /// 
        /// The default behavior of libcurl is used.
        /// </summary>
        None = 0,

        /// <summary>
        /// Disable the TLS/SSL workaround for the BEAST attack.
        /// 
        /// This option allows libcurl to use SSL 3.0 and TLS 1.0
        /// connections without the BEAST mitigation workaround.
        /// 
        /// Warning:
        /// Disabling this workaround reduces security and should only
        /// be used for compatibility with legacy servers.
        /// 
        /// Supported by Secure Transport and OpenSSL backends.
        /// </summary>
        AllowBeast = 1L << 0,

        /// <summary>
        /// Disable certificate revocation checks.
        /// 
        /// This option is only supported by the Schannel backend
        /// on Windows.
        /// </summary>
        NoRevoke = 1L << 1,

        /// <summary>
        /// Reject partial certificate chains.
        /// 
        /// By default, libcurl may accept a certificate chain that ends
        /// with an intermediate certificate. When enabled, verification
        /// fails unless the chain ends with a trusted root certificate.
        /// 
        /// Supported by OpenSSL and compatible TLS backends.
        /// </summary>
        NoPartialChain = 1L << 2,

        /// <summary>
        /// Perform certificate revocation checking on a best-effort basis.
        /// 
        /// If revocation information cannot be downloaded or is unavailable,
        /// the certificate verification continues.
        /// 
        /// This option is only supported by the Schannel backend.
        /// 
        /// This option has no effect when NoRevoke is enabled.
        /// </summary>
        RevokeBestEffort = 1L << 3,

        /// <summary>
        /// Use the operating system's native certificate authority store.
        /// 
        /// This allows libcurl to verify server certificates using the
        /// native CA store provided by the operating system.
        /// 
        /// Support depends on the TLS backend and libcurl version.
        /// </summary>
        NativeCa = 1L << 4,

        /// <summary>
        /// Automatically locate and use a client certificate when requested
        /// by the server.
        /// 
        /// This option is only supported by the Schannel backend.
        /// </summary>
        AutoClientCert = 1L << 5,

        /// <summary>
        /// If possible, send data using TLS 1.3 early data
        /// </summary>
        EarlyData = 1L << 6,
    }
}
