using System;

namespace ManagedBass.DShow
{
	/// <summary>
	/// Channel Position Mode flags to be used with e.g. <see cref="BassDShow.ChannelGetLength" />, <see cref="BassDShow.ChannelGetPosition" /> or <see cref="BassDShow.ChannelSetPosition" />.
	/// </summary>
	[Flags]
	public enum BassDShowMode
	{
		/// <summary>
		/// Position in seconds.
		/// </summary>
		Seconds,

		/// <summary>
		/// Position in frames.
		/// </summary>
		Frames,
		
		/// <summary>
		/// Position in milliseconds.
		/// </summary>
		Milliseconds,
		
		/// <summary>
		/// MOD Music Flag: Stop all notes when moving position.
		/// </summary>
		RefTime
	}
}