using System.Runtime.InteropServices;

namespace LibUsbDotNet.Internal.LibUsb
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	internal struct Vendor
	{
		public int Type;

		public int Recipient;

		public int Request;

		public int ID;

		public int Index;
	}
}
