using System;

namespace ManagedBass.Vst
{
	/// <summary>
	/// VST DSP flags to be used within the <see cref="BassVst.ChannelSetDSP" /> method.
	/// </summary>
	[Flags]
	public enum BassVstDsp
	{
		/// <summary>
		/// File is a Unicode (16-bit characters) filename.
		/// </summary>
		Unicode = -2147483648,

		/// <summary>
		/// Default VST DSP processing.
		/// </summary>
		Default = 0,

		/// <summary>
		/// By default, mono effects assigned to stereo channels are mixed down before processing and converted back to stereo afterwards.
		/// Set this flag to avoid this behaviour in which case only the first channel is affected by processing.
		/// </summary>
		KeepChannels = 1
	}
}