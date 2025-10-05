namespace ManagedBass.Asio
{
    /// <summary>
    /// BassAsio sample formats to be used with <see cref="AsioChannelInfo" /> and <see cref="BassAsio.ChannelGetInfo(bool,int,out AsioChannelInfo)" />.
    /// </summary>
    public enum AsioSampleFormat
    {
        /// <summary>
        /// Unknown format. Error.
        /// </summary>
        Unknown = -1,

        /// <summary>
        /// 16-bit integer.
        /// </summary>
        Bit16 = 0x10,

        /// <summary>
        /// 24-bit integer.
        /// </summary>
        Bit24,

        /// <summary>
        /// 32-bit integer.
        /// </summary>
        Bit32,

        /// <summary>
        /// 32-bit floating-point.
        /// </summary>
        Float,

        /// <summary>
        /// DSD (LSB 1st)
        /// </summary>
        DSD_LSB = 0x20,

        /// <summary>
        /// DSD (MSB 1st)
        /// </summary>
        DSD_MSB
    }
}