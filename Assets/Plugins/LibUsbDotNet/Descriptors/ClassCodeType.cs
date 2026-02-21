using System;

namespace LibUsbDotNet.Descriptors
{
	[Flags]
	public enum ClassCodeType : byte
	{
		PerInterface = 0,
		Audio = 1,
		Comm = 2,
		Hid = 3,
		Printer = 7,
		Ptp = 6,
		MassStorage = 8,
		Hub = 9,
		Data = 0xA,
		VendorSpec = byte.MaxValue
	}
}
