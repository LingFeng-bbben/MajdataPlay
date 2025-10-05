namespace ManagedBass.DShow
{
	/// <summary>
	/// BASS_DSHOW error codes as returned by <see cref="BassDShow.LastError" />.
	/// </summary>
	public enum BassDShowError
	{
		/// <summary>
		/// All is OK.
		/// </summary>
		OK,
		
		/// <summary>
		/// Invalid channel.
		/// </summary>
		InvalidChannel,
		
		/// <summary>
		/// Unknown error.
		/// </summary>
		Unknown,
		
		/// <summary>
		/// Not initialized.
		/// </summary>
		NotInitialized,
		
		/// <summary>
		/// Position not available.
		/// </summary>
		PositionNotAvailable,
		
		/// <summary>
		/// No DSHOW/xVideo.
		/// </summary>
		NoDShow,
		
		/// <summary>
		/// Invalid window.
		/// </summary>
		InvalidWindow,
		
		/// <summary>
		/// No Audio.
		/// </summary>
		NoAudio,
		
		/// <summary>
		/// No Video.
		/// </summary>
		NoVideo,
		
		/// <summary>
		/// Memory error.
		/// </summary>
		Memory,
		
		/// <summary>
		/// Invalid callback.
		/// </summary>
		Callback,
		
		/// <summary>
		/// Invalid flag(s).
		/// </summary>
		Flag,
		
		/// <summary>
		/// Not available.
		/// </summary>
		NotAvailable,
		
		/// <summary>
		/// Not initialized.
		/// </summary>
		Init,
		
		/// <summary>
		/// Already registered.
		/// </summary>
		AlreadyRegistered,

		/// <summary>
		/// Invalid Registration.
		/// </summary>
		InvalidRegistration
	}
}