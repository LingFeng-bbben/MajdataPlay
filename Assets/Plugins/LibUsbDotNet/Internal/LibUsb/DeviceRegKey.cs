using System.Runtime.InteropServices;

namespace LibUsbDotNet.Internal.LibUsb
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	internal struct DeviceRegKey
	{
		public int KeyType;

		public int NameOffset;

		public int ValueOffset;

		public int ValueLength;
	}
}
