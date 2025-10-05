using System.Runtime.InteropServices;

namespace ManagedBass.DShow
{
	/// <summary>
	/// Used with <see cref="BassDShow.ChannelColorRange(int,BassDShowColorControl,out BassDShowColorRange)" /> to retrieve the color controls range.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public struct BassDShowColorRange
	{
		/// <summary>
		/// The minimum value that a color control can have.
		/// </summary>
		public float MinValue;

		/// <summary>
		/// The maximum value that a color control can have.
		/// </summary>
		public float MaxValue;

		/// <summary>
		/// The default value of the color controls (most of the time this is the value that don't affect video).
		/// </summary>
		public float DefaultValue;

		/// <summary>
		/// The step size between minimum and maximum values.
		/// </summary>
		public float StepSize;

		/// <summary>
		/// One of the <see cref="BassDShowColorControl" /> flags.
		/// </summary>
		public BassDShowColorControl Type;
	}
}