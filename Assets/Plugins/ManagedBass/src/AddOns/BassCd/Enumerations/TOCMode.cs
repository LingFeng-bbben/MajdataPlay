using System;

namespace ManagedBass.Cd
{
    /// <summary>
    /// The mode to use with <see cref="BassCd.GetTOC(int, TOCMode, out TOC)"/>.
    /// </summary>
    [Flags]
    public enum TOCMode
    {
        /// <summary>
        /// Get the track start address in LBA form.
        /// </summary>
        LBA,

        /// <summary>
        /// Get the track start address in time form (hour, minute, second, frame).
        /// </summary>
        Time = 0x100,
        
        /// <summary>
        /// + track#, Get the position of indexes (instead of tracks)
        /// <para>When this option is used, the 'first' and 'last' members of the TOC structure are index numbers, 
        /// and the 'track' member of the <see cref="TOCTrack"/> structure is also an index number and the "lba" or "hmsf" member 
        /// (depending on whether "<see cref="Time"/>" is used) is an offset from the start of the track</para>
        /// </summary>
        Index = 0x200
    }
}