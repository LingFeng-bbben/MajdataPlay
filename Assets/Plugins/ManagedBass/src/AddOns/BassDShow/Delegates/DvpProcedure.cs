using System;

namespace ManagedBass.DShow
{
	/// <summary>
	/// User defined DSP callback function (to be used with <see cref="BassDShow.ChannelSetDVP" />).
	/// </summary>
	/// <param name="Handle">The DVP handle.</param>
	/// <param name="Channel">The video channel that the DVP applies to.</param>
	/// <param name="Buffer">The pointer to the sample data to apply the DVP to.</param>
	/// <param name="Length">The number of bytes to process.</param>
	/// <param name="DataType">One of the <see cref="BassDShowDvpType" /> values.</param>
	/// <param name="Width">The video width.</param>
	/// <param name="Height">The video height.</param>
	/// <param name="User">The user instance data given when <see cref="BassDShow.ChannelSetDVP" /> was called.</param>
	public delegate void DvpProcedure(int Handle, int Channel, IntPtr Buffer, int Length, BassDShowDvpType DataType, int Width, int Height, IntPtr User);
}