using System.Runtime.InteropServices;

namespace LibUsbDotNet.Internal.LibUsb
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	internal struct Descriptor
	{
		public int Type;

		public int Index;

		public int LangID;

		public int Recipient;
	}
}
