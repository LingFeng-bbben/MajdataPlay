using System;
using System.Runtime.InteropServices;
using LibUsbDotNet.Descriptors;
using LibUsbDotNet.Internal;
using LibUsbDotNet.Main;
using Microsoft.Win32.SafeHandles;

namespace LibUsbDotNet.WinUsb
{
	public abstract class WindowsDevice : UsbDevice, IUsbInterface
	{
		protected readonly string mDevicePath;

		protected PowerPolicies mPowerPolicies;

		protected SafeFileHandle mSafeDevHandle;

		public PowerPolicies PowerPolicy => mPowerPolicies;

		public override string DevicePath => mDevicePath;

		public override DriverModeType DriverMode => DriverModeType.Windows;

		internal WindowsDevice(UsbApiBase usbApi, SafeFileHandle usbHandle, SafeHandle handle, string devicePath)
			: base(usbApi, handle)
		{
			mDevicePath = devicePath;
			mSafeDevHandle = usbHandle;
			mPowerPolicies = new PowerPolicies(this);
		}

		public override bool Close()
		{
			if (base.IsOpen)
			{
				base.ActiveEndpoints.Clear();
				mUsbHandle.Dispose();
				if (mSafeDevHandle != null && !mSafeDevHandle.IsClosed)
				{
					mSafeDevHandle.Dispose();
				}
			}
			return true;
		}

		public abstract bool SetAltInterface(int alternateID);

		public abstract bool GetAltInterface(out int alternateID);

		public PipePolicies EndpointPolicies(ReadEndpointID epNum)
		{
			return new PipePolicies(mUsbHandle, (byte)epNum);
		}

		public PipePolicies EndpointPolicies(WriteEndpointID epNum)
		{
			return new PipePolicies(mUsbHandle, (byte)epNum);
		}

		public abstract bool GetAssociatedInterface(byte associatedInterfaceIndex, out WindowsDevice usbDevice);

		public abstract bool QueryDeviceSpeed(out DeviceSpeedTypes deviceSpeed);

		public abstract bool QueryInterfaceSettings(byte alternateInterfaceNumber, ref UsbInterfaceDescriptor usbAltInterfaceDescriptor);

		internal abstract bool GetPowerPolicy(PowerPolicyType policyType, ref int valueLength, IntPtr pBuffer);

		internal abstract bool SetPowerPolicy(PowerPolicyType policyType, int valueLength, IntPtr pBuffer);

		~WindowsDevice()
		{
			Close();
		}
	}
}
