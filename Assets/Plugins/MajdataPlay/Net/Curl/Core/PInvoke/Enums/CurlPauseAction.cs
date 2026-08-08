using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MajdataPlay.Net.Curl.Core.PInvoke
{
    /// <summary>
    /// Specifies the pause state changes for a libcurl transfer.
    /// </summary>
    /// <remarks>
    /// This enumeration is used with <c>curl_easy_pause</c> to pause or resume
    /// data transmission in one or both directions. The values can be combined
    /// using bitwise operations.
    /// 
    /// When <see cref="Recv"/> is specified, receiving data is paused and the
    /// write callback will not be invoked until the receive pause flag is cleared.
    /// When <see cref="Send"/> is specified, sending data is paused and the
    /// read callback will not be invoked until the send pause flag is cleared.
    /// 
    /// Passing <see cref="None"/> resumes all paused directions.
    /// </remarks>
    [Flags]
    public enum CurlPauseAction
    {
        /// <summary>
        /// No pause flags are set. Resumes both receiving and sending.
        /// </summary>
        None = 0,

        /// <summary>
        /// Pause receiving data from the remote server.
        /// </summary>
        /// <remarks>
        /// When this flag is set, libcurl stops delivering received data to the
        /// write callback until the flag is removed.
        /// </remarks>
        Recv = 1 << 0,

        /// <summary>
        /// Pause sending data to the remote server.
        /// </summary>
        /// <remarks>
        /// When this flag is set, libcurl stops requesting upload data from the
        /// read callback until the flag is removed.
        /// </remarks>
        Send = 1 << 2,

        /// <summary>
        /// Pause both receiving and sending data.
        /// </summary>
        All = Recv | Send,
    }
}
