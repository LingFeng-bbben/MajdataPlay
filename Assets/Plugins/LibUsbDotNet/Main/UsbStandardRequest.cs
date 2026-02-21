using System;

namespace LibUsbDotNet.Main
{
	[Flags]
	public enum UsbStandardRequest : byte
	{
		ClearFeature = 1,
		GetConfiguration = 8,
		GetDescriptor = 6,
		GetInterface = 0xA,
		GetStatus = 0,
		SetAddress = 5,
		SetConfiguration = 9,
		SetDescriptor = 7,
		SetFeature = 3,
		SetInterface = 0xB,
		SynchFrame = 0xC
	}
}
