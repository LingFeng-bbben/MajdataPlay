using System;

namespace LibUsbDotNet.Main
{
	[Flags]
	public enum UsbRequestRecipient : byte
	{
		RecipDevice = 0,
		RecipEndpoint = 2,
		RecipInterface = 1,
		RecipOther = 3
	}
}
