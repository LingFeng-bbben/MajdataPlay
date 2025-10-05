namespace ManagedBass
{
    /// <summary>
    /// DX8 effect phase.
    /// </summary>
    public enum DXPhase
    {
        /// <summary>
        /// Phase differential between left and right LFOs (-180)
        /// </summary>
        Negative180,

        /// <summary>
        /// Phase differential between left and right LFOs (-90)
        /// </summary>
        Negative90,

        /// <summary>
        /// Phase differential between left and right LFOs (+/-0)
        /// </summary>
        Zero,

        /// <summary>
        /// Phase differential between left and right LFOs (+90)
        /// </summary>
        Positive90,

        /// <summary>
        /// Phase differential between left and right LFOs (+180)
        /// </summary>
        Positive180
    }
}
