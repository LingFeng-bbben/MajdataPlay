using System;

namespace ManagedBass.Asio
{
    /// <summary>
    /// User defined ASIO channel callback function (to be used with <see cref="BassAsio.ChannelEnable" />).
    /// </summary>
    /// <param name="Input">Dealing with an Input channel? <see langword="false" /> = an output channel.</param>
    /// <param name="Channel">The Input/output channel number... 0 = first.</param>
    /// <param name="Buffer">The pointer to the Buffer containing the recorded data (Input channel), or in which to put the data to output (output channel).</param>
    /// <param name="Length">The number of bytes to process.</param>
    /// <param name="User">The User instance data given when <see cref="BassAsio.ChannelEnable" /> was called.</param>
    /// <returns>The number of bytes written (ignored with Input channels).</returns>
    /// <remarks>
    /// <para>ASIO is a low latency system, so a channel callback function should obviously be as quick as possible. <see cref="BassAsio.CPUUsage" /> can be used to monitor that.</para>
    /// <para>
    /// When multiple channels are joined together, the sample data of the channels is interleaved;
    /// the channel that was enabled via <see cref="BassAsio.ChannelEnable" /> comes first, followed by the channels that have been joined to it.
    /// The order of the joined channels defaults to numerically ascending order unless the <see cref="AsioInitFlags.JoinOrder"/> flag was used in the <see cref="BassAsio.Init" /> call,
    /// in which case they will be in the order in which <see cref="BassAsio.ChannelJoin" /> was called to join then.
    /// </para>
    /// <para>
    /// When an output channel's function returns less data than requested, the remainder of the Buffer is filled with silence, and some processing is saved by that. 
    /// When 0 is returned, the level of processing is the same as if the channel had been paused with <see cref="BassAsio.ChannelPause" />, ie. the ASIO Buffer is simply filled with silence and all additional processing (resampling/etc) is bypassed.
    /// </para>
    /// <para>
    /// ASIO is a low latency system, so a channel callback function should obviously be as quick as possible. 
    /// <see cref="BassAsio.CPUUsage" /> can be used to monitor that.
    /// Do not call the <see cref="BassAsio.Stop" /> or <see cref="BassAsio.Free" /> functions from within an ASIO callback. 
    /// Also, if it is an output channel, <see cref="BassAsio.ChannelSetFormat" /> and <see cref="BassAsio.ChannelSetRate" /> should not be used on the channel being processed by the callback.
    /// </para>
    /// <para>
    /// Prior to calling this function, BassAsio will set the thread's device context to the device that the channel belongs to.
    /// So when using multiple devices, <see cref="BassAsio.CurrentDevice" /> can be used to determine which device the channel is on.
    /// </para>
    /// </remarks>
    public delegate int AsioProcedure(bool Input, int Channel, IntPtr Buffer, int Length, IntPtr User);
}