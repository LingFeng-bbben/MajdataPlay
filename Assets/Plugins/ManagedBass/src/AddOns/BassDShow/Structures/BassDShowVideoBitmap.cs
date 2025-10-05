using System;
using System.Runtime.InteropServices;

namespace ManagedBass.DShow
{
	/// <summary>
	/// Used with <see cref="BassDShow.ChannelOverlayBMP" /> to overlay a HDC to the video window.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public struct BassDShowVideoBitmap
	{
		/// <summary>
		/// The overlay bitmap is visible or not?
		/// </summary>
		public bool Visible;

		/// <summary>
		/// The left position of the HDC.
		/// </summary>
		public int InLeft;

		/// <summary>
		/// The top position of the HDC.
		/// </summary>
		public int InTop;

		/// <summary>
		/// The right position of the HDC.
		/// </summary>
		public int InRight;

		/// <summary>
		/// The bottom position of the HDC.
		/// </summary>
		public int InBottom;

		/// <summary>
		/// The output left destination(0...1.0).
		/// </summary>
		public float OutLeft;

		/// <summary>
		/// The output top destination(0...1.0).
		/// </summary>
		public float OutTop;

		/// <summary>
		/// The output right destination(0...1.0).
		/// </summary>
		public float OutRight;

		/// <summary>
		/// The output bottom destination(0...1.0).
		/// </summary>
		public float OutBottom;

		/// <summary>
		/// The blend value of the overlay(0...1.0).
		/// </summary>
		public float AlphaValue;

		/// <summary>
		/// A RGB value that indicates wich color will not be blend over video.
		/// </summary>
		public int TransparentColor;

		/// <summary>
		/// A valid HDC.
		/// </summary>
		/// <remarks>Make sure to release the HDC once not needed anymore to prevent memory leaks.</remarks>
		public IntPtr HDC;
	}
}