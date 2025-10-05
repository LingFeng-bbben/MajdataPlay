using System;

namespace ManagedBass
{
    /// <summary>
    /// User defined callback function to process recorded sample data.
    /// </summary>
    /// <param name="Handle">The recording Handle that the data is from.</param>
    /// <param name="Buffer">
    /// The pointer to the Buffer containing the recorded sample data.
    /// The sample data is in standard Windows PCM format, that is 8-bit samples are unsigned, 16-bit samples are signed, 32-bit floating-point samples range from -1 to +1.
    /// </param>
    /// <param name="Length">The number of bytes in the Buffer.</param>
    /// <param name="User">The User instance data given when <see cref="Bass.RecordStart(int, int, BassFlags, RecordProcedure, IntPtr)" /> was called.</param>
    /// <returns>Return <see langword="true" /> to stop recording, and anything else to continue recording.</returns>
    /// <remarks>
    /// <see cref="Bass.RecordFree" /> should not be used to free the recording device within a recording callback function.
    /// Nor should <see cref="Bass.ChannelStop" /> be used to stop the recording; return <see langword="false" /> to do that instead.
    /// </remarks>
    public delegate bool RecordProcedure(int Handle, IntPtr Buffer, int Length, IntPtr User);
}
