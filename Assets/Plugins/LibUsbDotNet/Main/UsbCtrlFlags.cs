using System;

namespace LibUsbDotNet.Main
{
	[Flags]
	public enum UsbCtrlFlags : byte
	{
		Direction_In = 0x80,
		Direction_Out = 0,
		Recipient_Device = 0,
		Recipient_Endpoint = 2,
		Recipient_Interface = 1,
		Recipient_Other = 3,
		RequestType_Class = 0x20,
		RequestType_Reserved = 0x60,
		RequestType_Standard = 0,
		RequestType_Vendor = 0x40
	}
}
