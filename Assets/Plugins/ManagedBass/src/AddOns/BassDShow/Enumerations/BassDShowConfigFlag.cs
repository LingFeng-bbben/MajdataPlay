namespace ManagedBass.DShow
{
    /// <summary>
    /// Configuration option flags to be used with <see cref="BassDShow.SetConfig(BassDShowConfig,int)" />.
    /// </summary>
    public enum BassDShowConfigFlag
	{
		/// <summary>
		/// Used with <see cref="BassDShowConfig.VideoRenderer" /> to select the VMR7 Windowed renderer.
		/// </summary>
		VMR7 = 1,
		
		/// <summary>
		/// Used with <see cref="BassDShowConfig.VideoRenderer" /> to select the VMR9 Windowed renderer.
		/// </summary>
		VMR9,
		
		/// <summary>
		/// Used with <see cref="BassDShowConfig.VideoRenderer" /> to select the VMR7 window less renderer.
		/// </summary>
		VMR7WindowsLess,
		
		/// <summary>
		/// Used with <see cref="BassDShowConfig.VideoRenderer" /> to select the VMR9 window less renderer.
		/// </summary>
		VMR9WindowsLess,
		
		/// <summary>
		/// Used with <see cref="BassDShowConfig.VideoRenderer" /> to select the Enhanced video renderer.
		/// </summary>
		EVR,
		
		/// <summary>
		/// Used with <see cref="BassDShowConfig.VideoRenderer" /> to select the NULL video renderer.
		/// </summary>
		NullVideo,

		/// <summary>
		/// Used with <see cref="BassDShowConfig.AudioRenderer" /> to select the NULL audio renderer.
		/// </summary>
		NullAudio = 5170,
		
		/// <summary>
		/// Used with <see cref="BassDShowConfig.AudioRenderer" /> to select the Windows default audio device.
		/// </summary>
		BASS_DSHOW_DefaultAudio = 5171
	}
}