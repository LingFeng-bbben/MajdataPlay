using System;
using System.Runtime.InteropServices;

namespace ManagedBass.DShow
{
	/// <summary>
	/// Used with <see cref="BassDShow.ChannelGetStream(int,int)" /> to retrieve information of a stream.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public struct BassDShowStreams
	{
		/// <summary>
		/// 0: unknown, 1: video , 2: audio , 3: subtitle
		/// </summary>
		int format;

		IntPtr name;

		/// <summary>
		/// The format name.
		/// </summary>
		public string Name => name == IntPtr.Zero ? null : Marshal.PtrToStringAnsi(name);

        /// <summary>
        /// The stream index. Use this index to enable or disable it.
        /// </summary>
        public int Index;

		/// <summary>
		/// Is this stream enabled?
		/// </summary>
		public bool Enabled;

		/// <summary>
		/// Returns if the stream is a subtitle format.
		/// </summary>
		public bool IsSubtitle => format == 3;

		/// <summary>
		/// Returns if the stream is an audio format.
		/// </summary>
		public bool IsAudio => format == 2;

		/// <summary>
		/// Returns if the stream is a video format.
		/// </summary>
		public bool IsVideo => format == 1;
	}
}