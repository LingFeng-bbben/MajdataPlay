using System;

namespace ManagedBass.DShow
{
	/// <summary>
	/// Flags to be used with <see cref="BassDShow.ChannelSetColors" /> and <see cref="BassDShow.ChannelColorRange(int,BassDShowColorControl,out BassDShowColorRange)" />.
	/// </summary>
	[Flags]
	public enum BassDShowColorControl
	{
		/// <summary>
		/// Gets of sets the Brightness value.
		/// </summary>
		Brightness = 1,

		/// <summary>
		/// Gets of sets the Contrast value.
		/// </summary>
		Contrast = 2,
		
		/// <summary>
		/// Gets of sets the Hue value.
		/// </summary>
		Hue = 4,
		
		/// <summary>
		/// Gets of sets the Saturatio value.
		/// </summary>
		Saturation = 8
	}
}