using System;

namespace ManagedBass.Asio
{
    /// <summary>
    /// BassAsio attributes to be used when to reset a channel with <see cref="BassAsio.ChannelReset" />.
    /// </summary>
    [Flags]
    public enum AsioChannelResetFlags
    {
        /// <summary>
        /// Disable Channel
        /// </summary>
        Enable = 0x1,

        /// <summary>
        /// Unjoin Channel
        /// </summary>
        Join = 0x2,

        /// <summary>
        /// Unpause Channel
        /// </summary>
        Pause = 0x4,

        /// <summary>
        /// Reset sample format to native format
        /// </summary>
        Format = 0x8,

        /// <summary>
        /// Reset sample rate to device rate
        /// </summary>
        Rate = 0x10,

        /// <summary>
        /// Reset Volume to 1.0
        /// </summary>
        Volume = 0x20,

        /// <summary>
        /// Apply to joined channels too
        /// </summary>
        Joined = 0x10000
    }
}