using System.Runtime.InteropServices;
using LibUsbDotNet.Main;

namespace LibUsbDotNet.WinUsb
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public class PipeInformation
	{
		public static readonly int Size = Marshal.SizeOf(typeof(PipeInformation));

		public EndpointType PipeType;

		public byte PipeId;

		public short MaximumPacketSize;

		public byte Interval;
	}
}
