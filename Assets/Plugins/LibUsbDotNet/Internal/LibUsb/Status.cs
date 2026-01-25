using System.Runtime.InteropServices;

namespace LibUsbDotNet.Internal.LibUsb
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	internal struct Status
	{
		public int Recipient;

		public int Index;

		public int ID;
	}
}
