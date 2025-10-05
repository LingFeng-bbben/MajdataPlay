namespace ManagedBass.Asio
{
    /// <summary>
    /// Asio IO Format type to be used with <see cref="AsioIOFormat.FormatType"/>.
    /// </summary>
    public enum AsioIOFormatType
    {
        /// <summary>
        /// PCM.
        /// </summary>
        PCM,

        /// <summary>
        /// DSD.
        /// </summary>
        DSD,

        /// <summary>
        /// Invalid.
        /// </summary>
        Invalid = -1
    }
}