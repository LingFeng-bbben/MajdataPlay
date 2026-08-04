using System.Runtime.InteropServices;

namespace LibUsbDotNet.Main
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct UsbSetupPacket
	{
		public byte RequestType;

		public byte Request;

		public short Value;

		public short Index;

		public short Length;

		public UsbSetupPacket(byte bRequestType, byte bRequest, int wValue, int wIndex, int wlength)
		{
			RequestType = (byte)(bRequestType & 0xFF);
			Request = (byte)(bRequest & 0xFF);
			Value = (short)(wValue & 0xFFFF);
			Index = (short)(wIndex & 0xFFFF);
			Length = (short)(wlength & 0xFFFF);
		}
	}
}
