using System.Runtime.InteropServices;

namespace LibUsbDotNet.Main
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct UsbKernelVersion
	{
		public readonly int Major;

		public readonly int Minor;

		public readonly int Micro;

		public readonly int Nano;

		public readonly int BcdLibUsbDotNetKernelMod;

		public bool IsEmpty
		{
			get
			{
				if (Major == 0 && Minor == 0 && Micro == 0 && Nano == 0)
				{
					return true;
				}
				return false;
			}
		}

		internal UsbKernelVersion(int major, int minor, int micro, int nano, int bcdLibUsbDotNetKernelMod)
		{
			Major = major;
			Minor = minor;
			Micro = micro;
			Nano = nano;
			BcdLibUsbDotNetKernelMod = bcdLibUsbDotNetKernelMod;
		}

		public override string ToString()
		{
			return $"{Major}.{Minor}.{Micro}.{Nano}";
		}
	}
}
