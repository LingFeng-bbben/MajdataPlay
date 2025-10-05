using System;

namespace ManagedBass
{
    /// <summary>
    /// Voice allocation management flags.
    /// These flags enable hardware resource stealing... if the hardware has no	available voices, a currently playing Buffer will be stopped to make room for the new Buffer.
    /// </summary>
    /// <remarks>
    /// NOTE: only samples loaded/created with the <see cref="BassFlags.VAM"/> flag are considered for termination by the DX7 voice management.
    /// </remarks>
    [Flags]
    public enum VAMMode
    {
        /// <summary>
        /// Play the sample in hardware (default).
        /// If no hardware voices are available then the "play" call will fail.
        /// </summary>
        Hardware = 0x1,

        /// <summary>
        /// Play the sample in software (ie. non-accelerated).
        /// No other VAM flags may be used together with this flag.
        /// </summary>
        Software = 0x2,

        /// <summary>
        /// If there are no free hardware voices,
        /// the Buffer to be terminated will be the one with the least time left to play.
        /// </summary>
        TerminateTime = 0x4,

        /// <summary>
        /// If there are no free hardware voices, the Buffer to be terminated will be
        /// one that was loaded/created with the <see cref="BassFlags.MuteMax"/> flag and is beyond
        /// it's max distance. If there are no buffers that match this criteria, then
        /// the "play" call will fail.
        /// </summary>
        TerminateDistance = 0x8,

        /// <summary>
        /// If there are no free hardware voices,
        /// the Buffer to be terminated will be the one with the lowest priority.
        /// </summary>
        TerminatePriority = 0x10
    }
}
