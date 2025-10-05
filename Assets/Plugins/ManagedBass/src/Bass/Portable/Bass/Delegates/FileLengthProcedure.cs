using System;

namespace ManagedBass
{
    /// <summary>
    /// User file stream Length callback function (to be used with <see cref="FileProcedures" />).
    /// </summary>
    /// <param name="User">The User instance data given when <see cref="Bass.CreateStream(StreamSystem,BassFlags,FileProcedures,IntPtr)" /> was called.</param>
    /// <returns>The Length of the file in bytes.
    /// Returning 0 for a buffered file stream, makes BASS stream the file in blocks, and is equivalent to using the <see cref="BassFlags.StreamDownloadBlocks"/> flag
    /// in the <see cref="Bass.CreateStream(StreamSystem,BassFlags,FileProcedures,IntPtr)" /> call.</returns>
    /// <remarks>This function is called first thing, and is only used the once with buffered streams.
    /// With unbuffered streams, it may be used again when testing for EOF (end of file),
    /// allowing the file to grow in size.
    /// </remarks>
    public delegate long FileLengthProcedure(IntPtr User);
}
