using System;

namespace ManagedBass
{
    /// <summary>
    /// <see cref="StreamProcedure"/> flag used with <see cref="Bass.CreateStream(int,int,BassFlags,StreamProcedure,System.IntPtr)" /> resp. used with a User sample stream to be used with <see cref="StreamProcedure" />.
    /// </summary>
    public enum StreamProcedureType
    {
        /// <summary>
        /// Flag to signify that the end of the stream is reached.
        /// </summary>
        End = -2147483648,

        /// <summary>
        /// Create a "push" stream.
        /// Instead of BASS pulling data from a StreamProcedure function, data is pushed to
        /// BASS via <see cref="Bass.StreamPutData(int,IntPtr,int)"/>.
        /// </summary>
        Push = -1,

        /// <summary>
        /// Create a "dummy" stream.
        /// A dummy stream doesn't have any sample data of its own, but a decoding dummy
        /// stream (with <see cref="BassFlags.Decode"/> flag) can be used to apply DSP/FX processing
        /// to any sample data, by setting DSP/FX on the stream and feeding the data through <see cref="Bass.ChannelGetData(int,IntPtr,int)"/>.
        /// The dummy stream should have the same sample format as the data being fed through it.
        /// </summary>
        Dummy = 0,

        /// <summary>
        /// Create a "dummy" stream for the device's final output mix.
        /// This allows DSP/FX to be applied to all channels that are playing on the device, rather than individual channels.
        /// DSP/FX parameter change latency is also reduced because channel playback buffering is avoided.
        /// The stream is created with the device's current output sample format; the freq, chans, and flags parameters are ignored.
        /// It will always be floating-point except on platforms/architectures that do not support floating-point, where it will be 16-bit instead.
        /// </summary>
        Device = -2
    }
}
