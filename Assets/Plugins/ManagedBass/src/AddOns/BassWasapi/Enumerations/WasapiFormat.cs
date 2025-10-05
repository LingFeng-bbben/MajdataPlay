namespace ManagedBass.Wasapi
{
    /// <summary>
    /// BassWasapi sample formats to be used with <see cref="WasapiInfo" /> and <see cref="BassWasapi.GetInfo" />.
    /// </summary>
    public enum WasapiFormat
    {
        /// <summary>
        /// Unknown
        /// </summary>
        Unknown = -1,

        /// <summary>
        /// 32-bit floating-point.
        /// </summary>
        Float,

        /// <summary>
        /// 8-bit integer.
        /// </summary>
        Bit8,

        /// <summary>
        /// 16-bit integer.
        /// </summary>
        Bit16,

        /// <summary>
        /// 24-bit integer.
        /// </summary>
        Bit24,

        /// <summary>
        /// 32-bit integer.
        /// </summary>
        Bit32
    }
}