using System;
using System.Runtime.InteropServices;
using LibUsbDotNet.WinUsb.Internal;

namespace LibUsbDotNet.Internal.WinUsb
{
	internal class SafeWinUsbInterfaceHandle : SafeHandle
	{
		public override bool IsInvalid
		{
			get
			{
				if (!(handle == IntPtr.Zero))
				{
					return handle.ToInt64() == -1;
				}
				return true;
			}
		}

		public SafeWinUsbInterfaceHandle()
			: base(IntPtr.Zero, ownsHandle: true)
		{
		}

		public SafeWinUsbInterfaceHandle(IntPtr handle)
			: base(handle, ownsHandle: true)
		{
		}

		protected override bool ReleaseHandle()
		{
			bool result = true;
			if (!IsInvalid)
			{
				result = WinUsbAPI.WinUsb_Free(handle);
				handle = IntPtr.Zero;
			}
			return result;
		}
	}
}
