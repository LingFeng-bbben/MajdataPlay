namespace ManagedBass.DShow
{
	/// <summary>
	/// DVD property options used by <see cref="BassDShow.DVDSetProperty" />.
	/// </summary>
	public enum BassDShowDVDSetProperty
	{
		/// <summary>
		/// Go to DVD title menu.
		/// </summary>
		TitleMenu = 100,
		
		/// <summary>
		/// Go to DVD root.
		/// </summary>
		Root = 101,
		
		/// <summary>
		/// Go to dvd next chapter.
		/// </summary>
		NextChapter = 102,
		
		/// <summary>
		/// Go to dvd previous chapter.
		/// </summary>
		PreviousChapter = 103,
		
		/// <summary>
		/// Go to dvd title.
		/// </summary>
		Title = 104,
		
		/// <summary>
		/// Play chapter in current title.
		/// </summary>
		TitleChapter = 105,
		
		/// <summary>
		/// Sets the current title playing position.
		/// </summary>
		CurrentTitlePosition = 146
	}
}