using System.Runtime.InteropServices;

namespace LibUsbDotNet.Internal.LibUsb
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	internal struct Config
	{
		public int ID;
	}
}
