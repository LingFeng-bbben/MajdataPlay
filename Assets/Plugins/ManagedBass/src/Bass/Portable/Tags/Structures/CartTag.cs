using System;
using System.Runtime.InteropServices;

namespace ManagedBass
{
    /// <summary>
    /// BWF CART Timer structure.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class CartTimer
    {
        /// <summary>
        /// Usage
        /// </summary>
        public int Usage;

        /// <summary>
        /// Value
        /// </summary>
        public int Value;
    }

    /// <summary>
    /// BWF CART block tag structure.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class CartTag
    {
        /// <summary>
        /// Version of the data structure.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 4)]
        public string Version;

        /// <summary>
        /// Title of cart audio sequence (Max 64 characters).
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string Title;

        /// <summary>
        /// Artist or Creator name (Max 64 characters).
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string Artist;

        /// <summary>
        /// Cut Number Identification (Max 64 characters).
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string CutID;

        /// <summary>
        /// Client Identification (Max 64 characters).
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string ClientID;

        /// <summary>
        /// Category ID (e.g. PSA, NEWS, etc | Max 64 characters).
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string Category;

        /// <summary>
        /// Classification or Auxiliary Key (Max 64 characters).
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string Classification;

        /// <summary>
        /// Out Cue Text (Max 64 characters).
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string OutCue;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 10)]
        string startDate;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 8)]
        string startTime;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 10)]
        string endDate;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 8)]
        string endTime;

        /// <summary>
        /// Name of vendor or application (Max 64 characters).
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string ProducerAppID;

        /// <summary>
        /// Version of producer application (Max 64 characters).
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string ProducerAppVersion;

        /// <summary>
        /// User defined text (Max 64 characters).
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string UserDef;

        /// <summary>
        /// Sample value for 0 dB reference.
        /// </summary>
        public int dwLevelReference;

        /// <summary>
        /// 8 time markers after head.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public CartTimer[] PostTimer;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 276)]
        char[] Reserved;

        /// <summary>
        /// Uniform resource locator.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 1024)]
        public string URL;

        IntPtr tagText;

        /// <summary>
        /// Free form text for scripts or tags.
        /// </summary>
        public string TagText => tagText == IntPtr.Zero ? null : Marshal.PtrToStringAnsi(tagText);

        /// <summary>
        /// Start time.
        /// </summary>
        public DateTime StartTime
        {
            get
            {
                string[] date = startDate.Split('-'),
                         time = startTime.Split(':');

                return new DateTime(int.Parse(date[0]), int.Parse(date[1]), int.Parse(date[2]),
                                    int.Parse(time[0]), int.Parse(time[1]), int.Parse(time[2]));
            }
        }

        /// <summary>
        /// End time.
        /// </summary>
        public DateTime EndTime
        {
            get
            {
                string[] date = endDate.Split('-'),
                         time = endTime.Split(':');

                return new DateTime(int.Parse(date[0]), int.Parse(date[1]), int.Parse(date[2]),
                                    int.Parse(time[0]), int.Parse(time[1]), int.Parse(time[2]));
            }
        }

        /// <summary>
        /// Read the tag from a Channel.
        /// </summary>
        public static CartTag Read(int Channel)
        {
            return BassMarshal.PtrToStructure<CartTag>(Bass.ChannelGetTags(Channel, TagType.RiffCart));
        }
    }
}