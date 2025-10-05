namespace ManagedBass
{
    /// <summary>
    /// 3D Channel Mode flags used with <see cref="SampleInfo" />.
    /// </summary>
    public enum Mode3D
    {
        /// <summary>
        /// To be used with <see cref="Bass.ChannelSet3DAttributes"/>
        /// in order to leave the current 3D processing mode unchanged.
        /// </summary>
        LeaveCurrent = -1,

        /// <summary>
        /// Normal 3D processing
        /// </summary>
        Normal,

        /// <summary>
        /// The channel's 3D position (position/velocity/orientation) are relative to the listener.
        /// When the listener's position/velocity/orientation is changed with <see cref="Bass.Set3DPosition"/>,
        /// the channel's position relative to the listener does not change.
        /// </summary>
        Relative,

        /// <summary>
        /// Turn off 3D processing on the channel, the sound will be played in the center.
        /// </summary>
        Off
    }
}
