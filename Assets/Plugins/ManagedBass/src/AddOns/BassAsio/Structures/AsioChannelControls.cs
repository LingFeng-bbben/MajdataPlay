using System.Runtime.InteropServices;

namespace ManagedBass.Asio
{
	/// <summary>
	/// Used with <see cref="BassAsio.Future" /> and the Get/SetInput resp. Get/SetOutput selector.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public class AsioChannelControls
	{
		/// <summary>
		/// the channel index
		/// </summary>
		public int Channel;

		/// <summary>
		/// TRUE = Input, FALSE = Output
		/// </summary>
		public bool IsInput;

		/// <summary>
		/// the gain value, ranging 0 - 0x7fffffffL (-inf to +12 dB)
		/// </summary>
		public int Gain;

		/// <summary>
		/// returned meter value, ranging 0 - 0x7fffffffL (-inf to +12 dB)
		/// </summary>
		public int Meter;

		/// <summary>
		/// up to 32 chars
		/// </summary>
		public string Future = string.Empty;
	}
}