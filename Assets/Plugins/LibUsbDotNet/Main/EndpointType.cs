using System;

namespace LibUsbDotNet.Main
{
	[Flags]
	public enum EndpointType : byte
	{
		Control = 0,
		Isochronous = 1,
		Bulk = 2,
		Interrupt = 3
	}
}
