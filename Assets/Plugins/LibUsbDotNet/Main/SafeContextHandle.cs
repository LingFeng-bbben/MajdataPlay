using System;
using System.Runtime.InteropServices;

namespace LibUsbDotNet.Main
{
	public abstract class SafeContextHandle : SafeHandle
	{
		public override bool IsInvalid
		{
			get
			{
				if (handle != IntPtr.Zero)
				{
					return handle == new IntPtr(-1);
				}
				return true;
			}
		}

		protected SafeContextHandle(IntPtr pHandle, bool ownsHandle)
			: base(IntPtr.Zero, ownsHandle)
		{
			SetHandle(pHandle);
		}

		protected SafeContextHandle(IntPtr pHandleToOwn)
			: this(pHandleToOwn, ownsHandle: true)
		{
		}
	}
}
