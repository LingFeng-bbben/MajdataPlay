using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MajdataPlay.Net.Curl.Core.PInvoke
{
    /// <summary>
    /// Specifies the initialization options for the libcurl global environment.
    /// </summary>
    /// <remarks>
    /// This enumeration is used with <c>curl_global_init</c> to specify which
    /// global components should be initialized by libcurl.
    /// 
    /// The values can be combined using bitwise operations.
    /// In normal usage, <see cref="Default"/> should be used.
    /// </remarks>
    [Flags]
    internal enum CurlInitOption : long
    {
        /// <summary>
        /// Initialize nothing.
        /// </summary>
        /// <remarks>
        /// This option disables all optional global initialization performed by
        /// libcurl. Use this only when the required initialization is handled
        /// externally by the application.
        /// </remarks>
        None = 0,

        /// <summary>
        /// Initialize the SSL/TLS layer.
        /// </summary>
        /// <remarks>
        /// This flag initializes the SSL backend used by libcurl.
        /// For recent libcurl versions this flag has no practical effect because
        /// SSL initialization is handled automatically.
        /// </remarks>
        Ssl = 1 << 0,

        /// <summary>
        /// Initialize the Win32 socket library.
        /// </summary>
        /// <remarks>
        /// This flag initializes Winsock on Windows platforms.
        /// If this flag is not specified, the application must initialize
        /// Winsock before using networking features that require it.
        /// </remarks>
        Win32 = 1 << 1,

        /// <summary>
        /// Initialize all supported global components.
        /// </summary>
        All = Ssl | Win32,

        /// <summary>
        /// Initialize libcurl using the recommended default settings.
        /// </summary>
        /// <remarks>
        /// This value currently has the same effect as <see cref="All"/>.
        /// </remarks>
        Default = All,
    }
}
