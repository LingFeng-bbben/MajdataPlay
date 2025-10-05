namespace ManagedBass.DShow
{
	/// <summary>
	/// Configuration options to be used with <see cref="BassDShow.SetConfig(BassDShowConfig,int)" />.
	/// </summary>
	public enum BassDShowConfig
	{
		/// <summary>
		/// Selects the video renderer.
		/// </summary>
		VideoRenderer = 4096,

		/// <summary>
		/// VMR7/VMR9 WindowLess Mode need an initial window so set a HWND to use properly VMR.
		/// </summary>
		WindowLessHandle = 4097,
		
		/// <summary>
		/// Sets the number of streams in a VMR7/9 windows less mode.
		/// </summary>
		WindowLessStreams = 4098,
		
		/// <summary>
		/// Selects audio renderer.
		/// </summary>
		AudioRenderer = 4100,
		
		/// <summary>
		/// Enables/disables floating point processing DSP.
		/// </summary>
		FloatDsp = 4101
	}
}