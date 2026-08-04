using System;
using System.Runtime.InteropServices;

namespace LibUsbDotNet.Descriptors
{
	internal abstract class UsbMemChunk
	{
		private readonly int mMaxSize;

		private IntPtr mMemPointer = IntPtr.Zero;

		public int MaxSize => mMaxSize;

		public IntPtr Ptr => mMemPointer;

		protected UsbMemChunk(int maxSize)
		{
			mMaxSize = maxSize;
			mMemPointer = Marshal.AllocHGlobal(maxSize);
		}

		public void Free()
		{
			if (mMemPointer != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(mMemPointer);
				mMemPointer = IntPtr.Zero;
			}
		}

		~UsbMemChunk()
		{
			Free();
		}
	}
}
