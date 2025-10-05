namespace ManagedBass.Asio
{
	/// <summary>
	/// BassAsio Future values that might be used with the <see cref="BassAsio.Future" /> method.
	/// <para>Note: Other/Additional values might be possible - see your drivers manual for details.</para>
	/// </summary>
	public enum AsioFuture
	{
		/// <summary>
		/// No arguments
		/// </summary>
		EnableTimeCodeRead = 1,

		/// <summary>
		/// No arguments
		/// </summary>
		DisableTimeCodeRead,

		/// <summary>
		/// ASIOInputMonitor* in params
		/// </summary>
		SetInputMonitor,

		/// <summary>
		/// ASIOTransportParameters* in params
		/// </summary>
		Transport,

		/// <summary>
		/// ASIOChannelControls* in params, apply gain
		/// </summary>
		SetInputGain,

		/// <summary>
		/// ASIOChannelControls* in params, fill meter
		/// </summary>
		GetInputMeter,

		/// <summary>
		/// ASIOChannelControls* in params, apply gain
		/// </summary>
		SetOutputGain,

		/// <summary>
		/// ASIOChannelControls* in params, fill meter
		/// </summary>
		GetOutputMeter,

		/// <summary>
		/// No arguments
		/// </summary>
		CanInputMonitor,

		/// <summary>
		/// No arguments
		/// </summary>
		CanTimeInfo,

		/// <summary>
		/// No arguments
		/// </summary>
		CanTimeCode,

		/// <summary>
		/// No arguments
		/// </summary>
		CanTransport,

		/// <summary>
		/// No arguments
		/// </summary>
		CanInputGain,

		/// <summary>
		/// No arguments
		/// </summary>
		CanInputMeter,

		/// <summary>
		/// No arguments
		/// </summary>
		CanOutputGain,

		/// <summary>
		/// No arguments
		/// </summary>
		CanOutputMeter,

		/// <summary>
		/// DSD support: ASIOIoFormat * in params
		/// </summary>
		SetIoFormat = 588323169,

		/// <summary>
		/// DSD support: ASIOIoFormat * in params
		/// </summary>
		GetIoFormat = 588323203,

		/// <summary>
		/// DSD support: ASIOIoFormat * in params
		/// </summary>
		CanDoIoFormat = 588324868
	}
}