namespace ManagedBass.DShow
{
	/// <summary>
	/// Channel attribute options used by <see cref="BassDShow.ChannelSetAttribute" /> and <see cref="BassDShow.ChannelGetAttribute" />.
	/// </summary>
	public enum BassDShowAttribute
	{
		/// <summary>
		/// The audio volume level.
		/// <para>volume: The volume level... 0 (silent) to 1 (full).</para>
		/// </summary>
		Volume = 1,

		/// <summary>
		/// The audio panning/balance position.
		/// <para>pan: The pan position... -1 (full left) to +1 (full right), 0 = centre.</para>
		/// </summary>
		Pan = 2,
		
		/// <summary>
		/// The video graph rate.
		/// </summary>
		Rate = 3,
		
		/// <summary>
		/// The main video alpha bland value.
		/// </summary>
		Alpha = 4
	}
}