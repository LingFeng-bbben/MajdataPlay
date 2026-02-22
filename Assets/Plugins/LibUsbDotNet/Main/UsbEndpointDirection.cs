using System;

namespace LibUsbDotNet.Main
{
	[Flags]
	public enum UsbEndpointDirection : byte
	{
		EndpointIn = 0x80,
		EndpointOut = 0
	}
}
