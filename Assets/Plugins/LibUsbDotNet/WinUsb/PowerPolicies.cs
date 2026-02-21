using System;
using System.Runtime.InteropServices;

namespace LibUsbDotNet.WinUsb
{
	public class PowerPolicies
	{
		private const int MAX_SIZE = 4;

		private readonly WindowsDevice mUsbDevice;

		private IntPtr mBufferPtr = IntPtr.Zero;

		public bool AutoSuspend
		{
			get
			{
				int valueLength = 1;
				Marshal.WriteByte(mBufferPtr, 0);
				if (mUsbDevice.GetPowerPolicy(PowerPolicyType.AutoSuspend, ref valueLength, mBufferPtr))
				{
					return Marshal.ReadByte(mBufferPtr) != 0;
				}
				return false;
			}
			set
			{
				int valueLength = 1;
				byte val = (value ? ((byte)1) : ((byte)0));
				Marshal.WriteByte(mBufferPtr, val);
				mUsbDevice.SetPowerPolicy(PowerPolicyType.AutoSuspend, valueLength, mBufferPtr);
			}
		}

		public int SuspendDelay
		{
			get
			{
				int valueLength = Marshal.SizeOf(typeof(int));
				Marshal.WriteInt32(mBufferPtr, 0);
				if (mUsbDevice.GetPowerPolicy(PowerPolicyType.SuspendDelay, ref valueLength, mBufferPtr))
				{
					return Marshal.ReadInt32(mBufferPtr);
				}
				return -1;
			}
			set
			{
				int valueLength = Marshal.SizeOf(typeof(int));
				Marshal.WriteInt32(mBufferPtr, value);
				mUsbDevice.SetPowerPolicy(PowerPolicyType.SuspendDelay, valueLength, mBufferPtr);
			}
		}

		internal PowerPolicies(WindowsDevice usbDevice)
		{
			mBufferPtr = Marshal.AllocCoTaskMem(4);
			mUsbDevice = usbDevice;
		}

		~PowerPolicies()
		{
			if (mBufferPtr != IntPtr.Zero)
			{
				Marshal.FreeCoTaskMem(mBufferPtr);
			}
			mBufferPtr = IntPtr.Zero;
		}
	}
}
