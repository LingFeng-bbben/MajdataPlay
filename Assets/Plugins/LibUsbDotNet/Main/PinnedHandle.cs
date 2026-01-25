using System;
using System.Runtime.InteropServices;

namespace LibUsbDotNet.Main
{
	public class PinnedHandle : IDisposable
	{
		private IntPtr handle = IntPtr.Zero;

		private bool isGCHandleOwner;

		private GCHandle mGCHandle;

		private bool disposed;

		public IntPtr Handle => handle;

		public PinnedHandle(object objectToPin)
		{
			if (objectToPin != null)
			{
				if (objectToPin is GCHandle)
				{
					mGCHandle = (GCHandle)objectToPin;
					handle = mGCHandle.AddrOfPinnedObject();
				}
				else if (objectToPin is IntPtr)
				{
					handle = (IntPtr)objectToPin;
				}
				else
				{
					mGCHandle = GCHandle.Alloc(objectToPin, GCHandleType.Pinned);
					handle = mGCHandle.AddrOfPinnedObject();
					isGCHandleOwner = true;
				}
			}
		}

		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (isGCHandleOwner && handle != IntPtr.Zero)
				{
					isGCHandleOwner = false;
					handle = IntPtr.Zero;
					mGCHandle.Free();
				}
				disposed = true;
			}
		}

		~PinnedHandle()
		{
			Dispose(disposing: false);
		}
	}
}
