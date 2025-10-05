using System;

namespace ManagedBass
{
    /// <summary>
    /// User stream writing callback delegate (to be used with <see cref="Bass.CreateStream(int,int,BassFlags,StreamProcedure,IntPtr)" />).
    /// </summary>
    /// <param name="Handle">The stream that needs writing.</param>
    /// <param name="Buffer">The pointer to the Buffer to write the sample data in. The sample data must be written in standard Windows PCM format - 8-bit samples are unsigned, 16-bit samples are signed, 32-bit floating-point samples range from -1 to 1.</param>
    /// <param name="Length">The number of bytes to write.</param>
    /// <param name="User">The User instance data given when <see cref="Bass.CreateStream(int,int,BassFlags,StreamProcedure,IntPtr)" /> was called.</param>
    /// <returns>The number of bytes written by the function, optionally using the <see cref="StreamProcedureType.End"/> (<see cref="StreamProcedure" />) flag to signify that the end of the stream is reached.</returns>
    /// <remarks>
    /// <para>A stream writing function should obviously be as quick as possible, because other streams (and MOD musics) can't be updated until it's finished.</para>
    /// <para>It is better to return less data quickly, rather than spending a long time delivering the amount BASS requested.</para>
    /// <para>Although a STREAMPROC may return less data than BASS requests, be careful not to do so by too much, too often. If the Buffer level gets too low, BASS will automatically stall playback of the stream, until the whole Buffer has refilled.</para>
    /// <para><see cref="Bass.ChannelGetData(int,IntPtr,int)" /> <see cref="DataFlags"/> can be used to check the Buffer level, and <see cref="Bass.ChannelIsActive" /> can be used to check if playback has stalled.</para>
    /// <para>A <see cref="SyncFlags.Stalled"/> sync can also be set via <see cref="Bass.ChannelSetSync" />, to be triggered upon playback stalling or resuming.</para>
    /// <para>If you do return less than the requested amount of data, the number of bytes should still equate to a whole number of samples.</para>
    /// <para>Some functions can cause problems if called from within a stream (or DSP) function. Do not call these functions from within a stream callback:</para>
    /// <para><see cref="Bass.ChannelStop" />, <see cref="Bass.Free()" />, <see cref="Bass.MusicLoad(string,long,int,BassFlags,int)" />, <see cref="Bass.CreateStream(int,int,BassFlags,StreamProcedure,IntPtr)" /> or any other stream creation functions.</para>
    /// <para>Also, do not call <see cref="Bass.StreamFree" /> or <see cref="Bass.ChannelStop" /> with the same Handle as received by the callback.</para>
    /// <para>When streaming multi-channel sample data, the channel order of each sample is as follows:</para>
    /// <para>3 channels: left-front, right-front, center.</para>
    /// <para>4 channels: left-front, right-front, left-rear/side, right-rear/side.</para>
    /// <para>6 channels(5.1): left-front, right-front, center, LFE, left-rear/side, right-rear/side.</para>
    /// <para>8 channels(7.1): left-front, right-front, center, LFE, left-rear/side, right-rear/side, left-rear center, right-rear center.</para>
    /// </remarks>
    public delegate int StreamProcedure(int Handle, IntPtr Buffer, int Length, IntPtr User);
}
