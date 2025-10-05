using System;

namespace ManagedBass.DShow
{
	/// <summary>
	/// Initialization flags to be used with <see cref="BassDShow.Init" />
	/// </summary>
	[Flags]
	public enum BassDShowInit
	{
		/// <summary>
		/// Normal mode.
		/// </summary>
		Default,

		/// <summary>
		/// Enable multithread support.
		/// </summary>
		MultiThread = 16
	}
}