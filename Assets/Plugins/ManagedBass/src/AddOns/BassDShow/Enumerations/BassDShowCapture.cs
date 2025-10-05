namespace ManagedBass.DShow
{
	/// <summary>
	/// Capture options to be used with <see cref="BassDShow.CaptureGetDevices" /> or <see cref="BassDShow.CaptureDeviceProfiles" />.
	/// </summary>
	public enum BassDShowCapture
	{
		/// <summary>
		/// Capture Audio.
		/// </summary>
		CaptureAudio = 65638,

		/// <summary>
		/// Capture Video.
		/// </summary>
		CaptureVideo = 65664
	}
}