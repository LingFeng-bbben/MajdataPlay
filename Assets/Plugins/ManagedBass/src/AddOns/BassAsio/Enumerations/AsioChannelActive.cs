namespace ManagedBass.Asio
{
    /// <summary>
    /// BassAsio active values return by <see cref="BassAsio.ChannelIsActive" />.
    /// </summary>
    public enum AsioChannelActive
    {
        /// <summary>
        /// The channel is not enabled.
        /// </summary>
        Disabled,

        /// <summary>
        /// The channel is enabled.
        /// </summary>
        Enabled,

        /// <summary>
        /// The channel is enabled and paused.
        /// </summary>
        Paused
    }
}