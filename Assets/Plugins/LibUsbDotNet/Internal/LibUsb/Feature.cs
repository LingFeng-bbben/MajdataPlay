using System.Runtime.InteropServices;

namespace LibUsbDotNet.Internal.LibUsb
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	internal struct Feature
	{
		public int Recipient;

		public int ID;

		public int Index;
	}
}
