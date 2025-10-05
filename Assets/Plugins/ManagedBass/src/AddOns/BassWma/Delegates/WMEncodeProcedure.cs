using System;

namespace ManagedBass.Wma
{
    /// <summary>
    /// Encoded data processing callback function.
    /// </summary>
    /// <param name="Handle">The encoder handle (as returned by <see cref="BassWma.EncodeOpen(int, int, WMAEncodeFlags, int, WMEncodeProcedure, IntPtr)" />).</param>
    /// <param name="Type">The type of data to process</param>
    /// <param name="Buffer">The pointer to the data to process. The buffer can contain the sample data or the header. The sample data is in standard Windows PCM format - 8-bit samples are unsigned, 16-bit samples are signed.</param>
    /// <param name="Length">The number of bytes in the buffer.</param>
    /// <param name="User">The user instance data given when <see cref="BassWma.EncodeOpen(int, int, WMAEncodeFlags, int, WMEncodeProcedure, IntPtr)" /> was called.</param>
    /// <remarks>
    /// <para>
    /// When encoding begins, an initial header is given. When encoding is completed, an updated header is given (with the duration info, etc.).
    /// When encoding to a file (whether that's on disk or not), the initial header should be replaced by the updated one.
    /// </para>
    /// </remarks>
    public delegate void WMEncodeProcedure(int Handle, WMEncodeType Type, IntPtr Buffer, int Length, IntPtr User);
}