using System;
using System.Runtime.InteropServices;

namespace ManagedBass
{
    /// <summary>
    /// BWF BEXT block tag structure.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class BextTag
    {
        /// <summary>
        /// The description or title (Max 256 characters).
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string Description;

        /// <summary>
        /// The name of the originator or artist (Max 32 characters).
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string Originator;

        /// <summary>
        /// The reference of the originator or encoded by (Max 32 characters).
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string OriginatorReference;

        /// <summary>
        /// The date of creation (Max 10 characters).
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 10)]
        public string OriginationDate;

        /// <summary>
        /// The time of creation (max. 10 characters).
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 8)]
        public string OriginationTime;

        /// <summary>
        /// The Date and Time of creation.
        /// </summary>
        public DateTime OriginationDateTime
        {
            get
            {
                var year = int.Parse(OriginationDate.Substring(0, 4));
                var month = int.Parse(OriginationDate.Substring(5, 2));
                var day = int.Parse(OriginationDate.Substring(8, 2));

                var hour = int.Parse(OriginationTime.Substring(0, 2));
                var minute = int.Parse(OriginationTime.Substring(3, 2));
                var second = int.Parse(OriginationTime.Substring(6, 2));

                return new DateTime(year, month, day, hour, minute, second);
            }
        }

        /// <summary>
        /// First sample count since midnight (little-endian).
        /// </summary>
        public long TimeReference;

        /// <summary>
        /// The BWF version (little-endian)
        /// </summary>
        public short Version;

        /// <summary>
        /// The SMPTE UMID.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        public byte[] UMID;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 190)]
        byte[] reserved;
        
        IntPtr codinghistory;

        /// <summary>
        /// Coding history.
        /// </summary>
        public string CodingHistory => codinghistory == IntPtr.Zero ? null : Marshal.PtrToStringAnsi(codinghistory);

        /// <summary>
        /// Reads the tag from a channel.
        /// </summary>
        /// <param name="Channel">The Channel to read the tag from.</param>
        public static BextTag Read(int Channel)
        {
            return BassMarshal.PtrToStructure<BextTag>(Bass.ChannelGetTags(Channel, TagType.RiffBext));
        }
    }
}