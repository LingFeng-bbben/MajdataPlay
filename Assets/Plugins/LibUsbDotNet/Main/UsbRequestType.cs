using System;

namespace LibUsbDotNet.Main
{
	[Flags]
	public enum UsbRequestType : byte
	{
		TypeClass = 0x20,
		TypeReserved = 0x60,
		TypeStandard = 0,
		TypeVendor = 0x40
	}
}
