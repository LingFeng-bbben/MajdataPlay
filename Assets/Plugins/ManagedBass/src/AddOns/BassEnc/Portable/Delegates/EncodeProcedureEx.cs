using System;

namespace ManagedBass.Enc
{
    /// <summary>
    /// User defined callback function to process (receive) encoded sample data (iOS and Mac).
    /// </summary>
    /// <param name="Handle">The encoder that the data is from (as returned by <see cref="BassEnc.EncodeStart(int,string,EncodeFlags,EncodeProcedure,IntPtr)" />).</param>
    /// <param name="Channel">The channel that the data is from.</param>
    /// <param name="Buffer">The pointer to the buffer containing the encoded data.</param>
    /// <param name="Length">The number of bytes in the buffer.</param>
    /// <param name="Offset">File Offset of the data.</param>
    /// <param name="User">The user instance data given when <see cref="BassEnc.EncodeStartCA(int,int,int,EncodeFlags,int,EncodeProcedureEx,IntPtr)" /> was called.</param>
    /// <remarks>To have the encoded data received by this callback function, the encoder needs to be told to output to STDOUT (instead of a file).</remarks>
    public delegate void EncodeProcedureEx(int Handle, int Channel, IntPtr Buffer, int Length, int Offset, IntPtr User);
}