using System.Runtime.InteropServices;

namespace ManagedBass.Asio
{
	/// <summary>
	/// Used with <see cref="BassAsio.Future" /> and the SetInputMonitor selector.
	/// </summary>
    [StructLayout(LayoutKind.Sequential)]
	public class AsioInputMonitor
	{
		/// <summary>
		/// this Input was set to monitor (or off), -1: all
		/// </summary>
		public int Input;

		/// <summary>
		/// suggested output for monitoring the Input (if so)
		/// </summary>
		public int Output;

		/// <summary>
		/// suggested gain, ranging 0 - 0x7fffffffL (-inf to +12 dB)
		/// </summary>
		public int Gain;

		/// <summary>
		/// TRUE = on, FALSE = off
		/// </summary>
		public bool State;

		/// <summary>
		/// suggested pan, 0 = all left, 0x7fffffff = right
		/// </summary>
		public int Pan;
	}
}