namespace ManagedBass.DShow
{
	/// <summary>
	/// DVD property options used by <see cref="BassDShow.DVDGetProperty" />.
	/// </summary>
	public enum BassDShowDVDGetProperty
	{
		/// <summary>
		/// Flag to be used as the 'value' parameter with the <see cref="CurrentDVDTitle" /> option to get the current title duration.
		/// </summary>
		CurrentTitleDuration = 145,
		
		/// <summary>
		/// Flag to be used as the 'value' parameter with the <see cref="CurrentDVDTitle" /> option to get the current title position.
		/// </summary>
		CurrentTitlePosition = 146,
		
		/// <summary>
		/// Gets either the current title duration or position (see <see cref="CurrentTitleDuration" /> and <see cref="CurrentTitlePosition" />).
		/// </summary>
		CurrentDVDTitle = 65552,
		
		/// <summary>
		/// Gets the number of titles of a DVD stream.
		/// </summary>
		DVDTitles = 65568,
		
		/// <summary>
		/// Gets the number of chapters of a title.
		/// </summary>
		DVDTitleChapters = 65584
	}
}