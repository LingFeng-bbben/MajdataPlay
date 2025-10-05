using System;

namespace ManagedBass.Wasapi
{
    /// <summary>
    /// User defined WASAPI output/input processing callback function (to be used with <see cref="BassWasapi.Init" />).
    /// </summary>
    /// <param name="Buffer">Pointer to the buffer to put the sample data for an output device, or to get the data from an input device. The sample data is always 32-bit floating-point.</param>
    /// <param name="Length">The number of bytes to process.</param>
    /// <param name="User">The user instance data given when <see cref="BassWasapi.Init" /> was called.</param>
    /// <returns>In the case of an output device, the number of bytes written to the buffer. In the case of an input device, 0 = stop the device, else continue.</returns>
    /// <remarks>
    /// <para>
    /// An output/input processing function should obviously be as quick as possible, to avoid buffer underruns (output) or overruns (input).
    /// Using a larger buffer makes that less crucial.
    /// <see cref="BassWasapi.GetData(IntPtr, int)" /> (<see cref="DataFlags.Available"/>) can be used to check how much data is buffered.
    /// </para>
    /// <para>
    /// An output device's <see cref="WasapiProcedure"/> may return less data than requested, but be careful not to do so by too much, too often.
    /// If the buffer gets exhausted, output will stall until more data is provided.
    /// If you do return less than the requested amount of data, the number of bytes should still equate to a whole number of samples.
    /// </para>
    /// <para>
    /// When multiple channels are used, the sample data of the channels is interleaved.
    /// For example, with 2 channels (ie. stereo), the sample data would be arranged as channel 1, channel 2, channel 1, channel 2, channel 1, etc.
    /// </para>
    /// <para>When an output channel needs to be empty/silent but still enabled, the channel's function could fill the buffer with 0s to achieve that.</para>
    /// <para>Do not call <see cref="BassWasapi.Free" /> from within a callback function.</para>
    /// <para>
    /// Prior to calling this function, BassWasapi will set the thread's device context to the device that the channel belongs to.
    /// So when using multiple devices, <see cref="BassWasapi.CurrentDevice" /> can be used to determine which device the channel is on.
    /// </para>
    /// <para>
    /// It is not supported to change the <see cref="WasapiProcedure" /> once a device was initialized via <see cref="BassWasapi.Init" />.
    /// If you need to change some internal processing logic during processing, you might use some kind of "if" statements within this callback procedure.
    /// </para>
    /// </remarks>
    public delegate int WasapiProcedure(IntPtr Buffer, int Length, IntPtr User);
}