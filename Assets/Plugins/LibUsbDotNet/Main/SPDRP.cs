using System;

namespace LibUsbDotNet.Main
{
	[Flags]
	public enum SPDRP
	{
		DeviceDesc = 0,
		HardwareId = 1,
		CompatibleIds = 2,
		Class = 7,
		ClassGuid = 8,
		Driver = 9,
		Mfg = 0xB,
		FriendlyName = 0xC,
		LocationInformation = 0xD,
		PhysicalDeviceObjectName = 0xE,
		UiNumber = 0x10,
		BusTypeGuid = 0x13,
		LegacyBusType = 0x14,
		BusNumber = 0x15,
		EnumeratorName = 0x16,
		Address = 0x1C,
		RemovalPolicy = 0x1F,
		InstallState = 0x22,
		LocationPaths = 0x23
	}
}
