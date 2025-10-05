namespace ManagedBass.DShow
{
	/// <summary>
	/// <see cref="BassDShow.ChannelGetState" /> return values.
	/// </summary>
	public enum BassDShowState
	{
		/// <summary>
		/// The channel state is unknown.
		/// </summary>
		Unknown = -1,

		/// <summary>
		/// The channel is playing.
		/// </summary>
		Playing = 1,
		
		/// <summary>
		/// The channel is paused.
		/// </summary>
		Paused = 2,
		
		/// <summary>
		/// The channel is stopped.
		/// </summary>
		Stopped = 3
	}
}