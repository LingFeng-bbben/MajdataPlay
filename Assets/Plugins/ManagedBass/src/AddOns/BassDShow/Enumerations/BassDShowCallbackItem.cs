namespace ManagedBass.DShow
{
	/// <summary>
	/// Option to be used with <see cref="BassDShow.CallbackItemByIndex" />.
	/// </summary>
	public enum BassDShowCallbackItem
	{
		/// <summary>
		/// Used to get the audio capture device.
		/// </summary>
		AudioCapture = 1,

		/// <summary>
		/// Used to get the video capture device.
		/// </summary>
		VideoCapture = 2,
		
		/// <summary>
		/// Used to get the audio renderer device.
		/// </summary>
		AudioRenderer = 3
	}
}