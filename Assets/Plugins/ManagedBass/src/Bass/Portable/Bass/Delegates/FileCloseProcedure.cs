using System;

namespace ManagedBass
{
    /// <summary>
    /// User file stream close callback function (to be used with <see cref="FileProcedures" />).
    /// </summary>
    /// <param name="User">The User instance data given when <see cref="Bass.CreateStream(StreamSystem,BassFlags,FileProcedures,IntPtr)" /> was called.</param>
    /// <remarks>With a buffered file stream, this function is called as soon as reading reaches the end of the file.
    /// If the stream is freed before then, this function could be called while its <see cref="FileReadProcedure" /> function is in progress.
    /// If that happens, the <see cref="FileReadProcedure" /> function call should be immediately cancelled.
    /// </remarks>
    public delegate void FileCloseProcedure(IntPtr User);
}
