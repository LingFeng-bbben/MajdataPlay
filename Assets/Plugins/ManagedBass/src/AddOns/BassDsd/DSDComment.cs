using System;
using System.Runtime.InteropServices;

namespace ManagedBass.Dsd
{
    /// <summary>
    /// DSD Comment tag structure.
    /// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public class DSDComment
	{
		short year;

		byte month;

		byte day;

		byte hour;

        byte minutes;

        /// <summary>
        /// Time Stamp.
        /// </summary>
        public DateTime TimeStamp => new DateTime(year, month, day, hour, minutes, 0);

		/// <summary>
		/// The comment type
		/// </summary>
		public DSDCommentType CommentType;

		/// <summary>
		/// The comment reference. Together with CommentType this indicates to what the comment refers. 
		/// If CommentType=General then this should be 0. 
		/// If CommentType=Channel then 0 = all channels, 1 = 1st channel, 2 = 2nd channel, etc. 
		/// If CommentType=SoundSource then 0 = DSD recording, 1 = analogue recording, 2 = PCM recording. 
		/// If CommentType=FileHistory then 0 = general remark, 1 = name of the operator, 2 = name or type of the creating machine, 3 = time zone information, 4 = revision of the file.
		/// </summary>
		public short Reference;

		int count;

		IntPtr _commentText;

		/// <summary>
		/// The description of the comment.
		/// </summary>
		public string Description => Marshal.PtrToStringAnsi(_commentText, count);
        
        /// <summary>
        /// Reads a tag at an <paramref name="Index"/> from a <paramref name="Channel"/>.
        /// </summary>
		public static DSDComment Read(int Channel, int Index)
		{
            return BassMarshal.PtrToStructure<DSDComment>(Bass.ChannelGetTags(Channel, TagType.DSDComment + Index));
		}
	}
}