using System;
using System.Runtime.InteropServices;

namespace ManagedBass.Cd
{
    /// <summary>
	/// Used with <see cref="BassCd.GetInfo(int, out CDInfo)" /> to retrieve information on a drive.
	/// </summary>
	/// <remarks>The <see cref="ReadWriteFlags"/>, <see cref="MaxSpeed"/> and <see cref="Cache"/> members are unavailable when the WIO interface is used.</remarks>
	[StructLayout(LayoutKind.Sequential)]
    public struct CDInfo
    {
        IntPtr vendor;
        IntPtr product;
        IntPtr rev;
        int letter;
        CDReadWriteFlags rwflags;
        bool canopen;
        bool canlock;
        int maxspeed;
        int cache;
        bool cdtext;

        /// <summary>
        /// The drive product/model name.
        /// </summary>
        public string Name => product == IntPtr.Zero ? null : Marshal.PtrToStringAnsi(product);

        /// <summary>
        /// The drive manufacturer name.
        /// </summary>
        public string Manufacturer => vendor == IntPtr.Zero ? null : Marshal.PtrToStringAnsi(vendor);

        /// <summary>
        /// The revision number as a string.
        /// </summary>
        public string Revision => rev == IntPtr.Zero ? null : Marshal.PtrToStringAnsi(rev);

        /// <summary>
        /// Gets the real-time Speed Multiplier = <see cref="MaxSpeed"/> / 176.4.
        /// </summary>
        public int SpeedMultiplier => (int)(maxspeed / 176.4);

        /// <summary>
        /// The character letter of the drive.
        /// </summary>
        public char DriveLetter => letter != -1 ? char.ToUpper((char)(letter + 65)) : '_';

        /// <summary>
        /// If <see langword="true" />, <see cref="BassCd.Door" /> can be used and <see cref="CDDoorAction.Open"/>/<see cref="CDDoorAction.Close"/> is supported?
        /// </summary>
        public bool CanOpen => canopen;

        /// <summary>
        /// If <see langword="true" />, <see cref="BassCd.Door" /> can be used and <see cref="CDDoorAction.Lock"/>/<see cref="CDDoorAction.Unlock"/> is supported?
        /// </summary>
        public bool CanLock => canlock;

        /// <summary>
        /// The maximum read speed, in kilobytes per second (KB/s). Divide by 176.4 to get the real-time speed multiplier, eg. 5645 / 176.4 = "32x speed".
        /// </summary>
        public int MaxSpeed => maxspeed;

        /// <summary>
        /// The drive's cache size, in kilobytes (KB).
        /// </summary>
        public int Cache => cache;

        /// <summary>
        /// The drive can read CD-TEXT?
        /// </summary>
        public bool CDText => cdtext;

        /// <summary>
        /// Read/Write capability flags (any combination of <see cref="CDReadWriteFlags" />).
        /// </summary>
        public CDReadWriteFlags ReadWriteFlags => rwflags;
    }
}