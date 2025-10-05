using System;

namespace ManagedBass
{
    /// <summary>
    /// User file stream read callback function (to be used with <see cref="FileProcedures" />).
    /// </summary>
    /// <param name="Buffer">Pointer to the Buffer to put the data in.</param>
    /// <param name="Length">Maximum number of bytes to read.</param>
    /// <param name="User">The User instance data given when <see cref="Bass.CreateStream(StreamSystem,BassFlags,FileProcedures,IntPtr)" /> was called.</param>
    /// <returns>The number of bytes read... -1 = end of file, 0 = end of file (buffered file stream only).</returns>
    /// <remarks>During creation of the stream, this function should try to return the amount of data requested.
    /// After that, it can just return whatever is available up to the requested amount.
    /// <para>For an unbuffered file stream during playback, this function should be as quick as possible -
    /// any delays will not only affect the decoding of the current stream, but also all other streams and MOD musics that are playing.
    /// It is better to return less data (even none) rather than wait for more data.
    /// A buffered file stream isn't affected by delays like this, as this function runs in its own thread then.</para>
    /// </remarks>
    public delegate int FileReadProcedure(IntPtr Buffer, int Length, IntPtr User);
}
