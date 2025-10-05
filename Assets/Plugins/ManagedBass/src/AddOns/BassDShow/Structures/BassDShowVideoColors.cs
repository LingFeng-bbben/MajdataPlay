using System;
using System.Runtime.InteropServices;

namespace ManagedBass.DShow
{
	/// <summary>
	/// Used with <see cref="BassDShow.ChannelSetColors" /> to set new values for color control.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public struct BassDShowVideoColors
	{
		/// <summary>
		/// The contrast value of the color.
		/// </summary>
		public float Contrast;

		/// <summary>
		/// The brightness value of the color.
		/// </summary>
		public float Brightness;

		/// <summary>
		/// The hue value of the color.
		/// </summary>
		public float Hue;

		/// <summary>
		/// The saturation value of the color.
		/// </summary>
		public float Saturation;
	}
}