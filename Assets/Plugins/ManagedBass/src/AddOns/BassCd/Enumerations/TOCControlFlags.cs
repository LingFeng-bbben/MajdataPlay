using System;

namespace ManagedBass.Cd
{
    /// <summary>
    /// The <see cref="TOCTrack.Control" /> flags.
    /// </summary>
    [Flags]
    public enum TOCControlFlags : byte
    {
        /// <summary>
        /// No flag defined.
        /// </summary>
        None,

        /// <summary>
        /// Pre-emphasis of 50/15 µs.
        /// </summary>
        PreEmphasis = 0x1,

        /// <summary>
        /// Digital copy permitted.
        /// </summary>
        DigitalCopyPermitted = 0x2,

        /// <summary>
        /// Data track.
        /// </summary>
        DataTrack = 0x4
    }
}