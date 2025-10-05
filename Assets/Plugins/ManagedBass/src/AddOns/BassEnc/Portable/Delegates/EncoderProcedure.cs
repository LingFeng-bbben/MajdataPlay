using System;

namespace ManagedBass.Enc
{
    /// <summary>
    /// User defined callback function to encode sample data. 
    /// </summary>
    /// <param name="Handle">The encoder handle.</param>
    /// <param name="Channel">The channel that the encoder is set on.</param>
    /// <param name="Buffer">Buffer containing the sample data on input, and containing the encoded data on output.</param>
    /// <param name="Length">The number of bytes in the buffer... -1 = the encoder is being freed (no data in the buffer).</param>
    /// <param name="MaxOut">The maximum amount of encoded data that can be placed in the buffer.</param>
    /// <param name="User">The user instance data given when <see cref="BassEnc.EncodeStart(int, string, EncodeFlags, EncoderProcedure, IntPtr)" /> was called.</param>
    /// <returns>The number of bytes of encoded data placed in buffer... -1 = stop encoding.</returns>
    /// <remarks>
    /// If the encoder output exceeds the outmax value, then only the first outmax bytes should be delivered and the remainder retained.
    /// The function will be called again immediately to get the remainder.
    /// </remarks>
    public delegate int EncoderProcedure(int Handle, int Channel, IntPtr Buffer, int Length, int MaxOut, IntPtr User);
}