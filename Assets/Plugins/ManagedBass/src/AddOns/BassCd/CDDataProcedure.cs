using System;

namespace ManagedBass.Cd
{
    /// <summary>
    /// User defined CD data callback delegate (to be used with <see cref="BassCd.CreateStream(int, int, BassFlags, CDDataProcedure, IntPtr)" /> or <see cref="BassCd.CreateStream(string, BassFlags, CDDataProcedure, IntPtr)" />).
    /// </summary>
    /// <param name="Handle">The CD stream that provided the data.</param>
    /// <param name="Position">The stream position (in bytes) that the data is from.</param>
    /// <param name="Type">The Type of data.</param>
    /// <param name="Buffer">The pointer to the Buffer data.</param>
    /// <param name="Length">The amount of data in bytes.</param>
    /// <param name="User">The User instance data given on stream creation.</param>
    /// <remarks>
    /// <para>
    /// Sub-channel data or C2 error info is delivered to this function as soon as it is read from the CD, before the associated audio data is played, or delivered by <see cref="Bass.ChannelGetData(int,IntPtr,int)" /> in the case of a decoding channel.
    /// The Position value can be used to synchronize the delivered data with the stream's audio data.
    /// </para>
    /// <para>
    /// CDs are read from in units of a frame, and so this function will always receive a whole number of frames' worth of data;
    /// there are 96 bytes of sub-channel data per-frame, and 296 bytes of C2 error info.
    /// When <see cref="BassCd.SetOffset" /> has been used to set a read offset, playback may begin mid-frame, and as a result of that,
    /// <paramref name="Position" /> may start out negative because the delivered data is from the start of the 1st frame, before where playback will begin from.
    /// </para>
    /// <para>Neither sub-channel data or C2 error info will be delivered in the case of a silenced frame resulting from the <see cref="BassCd.SkipError" /> config option being enabled.</para>
    /// </remarks>
    public delegate void CDDataProcedure(int Handle, int Position, CDDataType Type, IntPtr Buffer, int Length, IntPtr User);
}