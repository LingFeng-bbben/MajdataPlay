using System;

namespace LibUsbDotNet.Descriptors
{
	[Flags]
	public enum DescriptorType : byte
	{
		Device = 1,
		Configuration = 2,
		String = 3,
		Interface = 4,
		Endpoint = 5,
		DeviceQualifier = 6,
		OtherSpeedConfiguration = 7,
		InterfacePower = 8,
		OTG = 9,
		Debug = 0xA,
		InterfaceAssociation = 0xB,
		Hid = 0x21,
		HidReport = 0x22,
		Physical = 0x23,
		Hub = 0x29
	}
}
