using System;
using System.Runtime.InteropServices;
using LibUsbDotNet.WinUsb.Internal;

namespace LibUsbDotNet.Internal.WinUsb
{
	internal class SafeLibusbKInterfaceHandle : SafeHandle
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

		public SafeLibusbKInterfaceHandle()
			: base(IntPtr.Zero, ownsHandle: true)
		{
		}

		public SafeLibusbKInterfaceHandle(IntPtr handle)
			: base(handle, ownsHandle: true)
		{
		}

		protected override bool ReleaseHandle()
		{
			bool result = true;
			if (!IsInvalid)
			{
				result = LibusbKAPI.UsbK_Free(handle);
				handle = IntPtr.Zero;
			}
			return result;
		}
	}
}
