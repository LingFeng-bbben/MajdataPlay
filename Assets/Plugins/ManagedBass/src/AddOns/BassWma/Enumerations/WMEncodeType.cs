namespace ManagedBass.Wma
{
    /// <summary>
    /// WMA encoding callback flags for use with <see cref="WMEncodeProcedure" />.
    /// </summary>
    public enum WMEncodeType
    {
        /// <summary>
        /// The data in the buffer is the header.
        /// </summary>
        Header,

        /// <summary>
        /// The data in the buffer is encoded sample data.
        /// </summary>
        Data,

        /// <summary>
        /// The encoding has finished... buffer and length will be <see cref="System.IntPtr.Zero" /> and 0 respectively.
        /// </summary>
        Done
    }
}