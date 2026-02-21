using System.Runtime.InteropServices;

namespace LibUsbDotNet.Internal.LibUsb
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	internal struct Control
	{
		public byte RequestType;

		public byte Request;

		public ushort Value;

		public ushort Index;

		public ushort Length;
	}
}
