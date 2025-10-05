using System;
using System.Runtime.InteropServices;

namespace ManagedBass.Cd
{
    /// <summary>
	/// Represents one track of a CD's TOC (see <see cref="TOC" />).
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
    public struct TOCTrack
    {
        byte res1;
        byte adrcon;
        byte track;
        byte res2;
        int lba;

        /// <summary>
        /// ADR
        /// </summary>
        public byte ADR => (byte)(adrcon >> 4 & 15);

        /// <summary>
        /// Control
        /// </summary>
        public TOCControlFlags Control => (TOCControlFlags)(byte)(adrcon & 15);

		/// <summary>
		/// The track number... 170 = lead-out area (or index number if <see cref="TOCMode.Index" /> is used).
		/// </summary>
        public byte Track => track;

        /// <summary>
        /// Logical Block Address.
        /// </summary>
        public int LBA => lba;

        /// <summary>
        /// The address in time form (frame part).
        /// </summary>
        public byte Frame => (byte)(lba & 15);

        /// <summary>
        /// The Address in Time format.
        /// </summary>
        public TimeSpan Address => new TimeSpan(lba >> 24 & 15, lba >> 16 & 15, lba >> 8 & 15);
    }
}