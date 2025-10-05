using System;

namespace ManagedBass
{
    /// <summary>
    /// Internet stream download callback function (to be used with <see cref="Bass.CreateStream(string,int,BassFlags,DownloadProcedure,IntPtr)" />).
    /// </summary>
    /// <param name="Buffer">The pointer to the Buffer containing the downloaded data... <see cref="IntPtr.Zero" /> = finished downloading.</param>
    /// <param name="Length">The number of bytes in the Buffer... 0 = HTTP or ICY tags.</param>
    /// <param name="User">The User instance data given when <see cref="Bass.CreateStream(string,int,BassFlags,DownloadProcedure,IntPtr)" /> was called.</param>
    /// <remarks>
    /// <para>
    /// The callback will be called before the <see cref="Bass.CreateStream(string,int,BassFlags,DownloadProcedure,IntPtr)" /> call returns (if it's successful), with the initial downloaded data.
    /// So any initialization (eg. creating the file if writing to disk) needs to be done either before the call, or in the callback function.
    /// </para>
    /// <para>
    /// When the <see cref="BassFlags.StreamStatus"/> flag is specified in the <see cref="Bass.CreateStream(string,int,BassFlags,DownloadProcedure,IntPtr)" /> call,
    /// HTTP and ICY tags may be passed to the callback during connection, before any stream data is received.
    /// The tags are given exactly as would be returned by <see cref="Bass.ChannelGetTags" />.
    /// You can destinguish between HTTP and ICY tags by checking what the first string starts with ("HTTP" or "ICY").
    /// </para>
    /// <para>
    /// A download callback function could be used in conjunction with a <see cref="SyncFlags.MetadataReceived"/> sync set via <see cref="Bass.ChannelSetSync" />,
    /// to save individual tracks to disk from a Shoutcast stream.
    /// </para>
    /// </remarks>
    public delegate void DownloadProcedure(IntPtr Buffer, int Length, IntPtr User);
}
