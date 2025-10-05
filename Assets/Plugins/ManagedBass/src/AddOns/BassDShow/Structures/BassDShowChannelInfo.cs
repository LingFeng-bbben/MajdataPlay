using System.Runtime.InteropServices;

namespace ManagedBass.DShow
{
	/// <summary>
	/// Used with <see cref="BassDShow.ChannelGetInfo(int,out BassDShowChannelInfo)" /> to retrieve information about a video.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public struct BassDShowChannelInfo
	{
		/// <summary>
		/// The video frames per seconds (0 if no video).
		/// </summary>
		public float AvgTimePerFrame;

		/// <summary>
		/// The height of the video in pixel (0 if no video).
		/// </summary>
		public int Height;

		/// <summary>
		/// The width of the video in pixel (0 if no video).
		/// </summary>
		public int Width;

		/// <summary>
		/// Number of audio channels (0 if no audio).
		/// </summary>
		public int ChannelCount;

		/// <summary>
		/// Audio sample rate (0 if no audio).
		/// </summary>
		public int Frequency;

		/// <summary>
		/// Audio resolution (8, 16, 32 bits) (0 if no audio).
		/// </summary>
		public int BitsPerSample;

		/// <summary>
		/// Indicates if the audio is 32 bit floating point or not (<see langword="false" /> if no audio).
		/// </summary>
		public bool FloatingPoint;
	}
}