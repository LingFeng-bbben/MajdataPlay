namespace ManagedBass
{
    /// <summary>
    /// <see cref="Bass.ChannelIsActive" /> return values.
    /// </summary>
    public enum PlaybackState
    {
        /// <summary>
        /// The channel is not active, or Handle is not a valid channel.
        /// </summary>
        Stopped,

        /// <summary>
        /// The channel is playing (or recording).
        /// </summary>
        Playing,

        /// <summary>
        /// Playback of the stream has been stalled due to there not being enough sample
        /// data to continue playing. The playback will automatically resume once there's
        /// sufficient data to do so.
        /// </summary>
        Stalled,

        /// <summary>
        /// The channel is paused.
        /// </summary>
        Paused
    }
}
