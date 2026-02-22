using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using LibUsbDotNet.Descriptors;
using LibUsbDotNet.Internal;
using LibUsbDotNet.Internal.WinUsb;
using LibUsbDotNet.Main;
using LibUsbDotNet.WinUsb.Internal;
using Microsoft.Win32.SafeHandles;

namespace LibUsbDotNet.WinUsb
{
	public class WinUsbDevice : WindowsDevice
	{
		public override DriverModeType DriverMode => DriverModeType.WinUsb;

		internal WinUsbDevice(UsbApiBase usbApi, SafeFileHandle usbHandle, SafeHandle handle, string devicePath)
			: base(usbApi, usbHandle, handle, devicePath)
		{
		}

		public override bool SetAltInterface(int alternateID)
		{
			bool num = WinUsbAPI.WinUsb_SetCurrentAlternateSetting(mUsbHandle, (byte)alternateID);
			if (!num)
			{
				UsbError.Error(ErrorCode.Win32Error, Marshal.GetLastWin32Error(), "SetCurrentAlternateSetting", this);
				return num;
			}
			UsbAltInterfaceSettings[0] = (byte)alternateID;
			return num;
		}

		public override bool GetAltInterface(out int alternateID)
		{
			alternateID = -1;
			byte SettingNumber;
			bool num = WinUsbAPI.WinUsb_GetCurrentAlternateSetting(mUsbHandle, out SettingNumber);
			if (!num)
			{
				UsbError.Error(ErrorCode.Win32Error, Marshal.GetLastWin32Error(), "GetCurrentAlternateSetting", this);
				return num;
			}
			alternateID = SettingNumber;
			UsbAltInterfaceSettings[0] = SettingNumber;
			return num;
		}

		public override bool Open()
		{
			if (base.IsOpen)
			{
				return true;
			}
			SafeFileHandle sfhDevice;
			bool flag = WinUsbAPI.OpenDevice(out sfhDevice, mDevicePath);
			if (flag)
			{
				SafeWinUsbInterfaceHandle InterfaceHandle = new SafeWinUsbInterfaceHandle();
				if (flag = WinUsbAPI.WinUsb_Initialize(sfhDevice, ref InterfaceHandle))
				{
					mSafeDevHandle = sfhDevice;
					mUsbHandle = InterfaceHandle;
					mPowerPolicies = new PowerPolicies(this);
				}
				else
				{
					UsbError.Error(ErrorCode.Win32Error, Marshal.GetLastWin32Error(), "Open:Initialize", typeof(UsbDevice));
				}
			}
			else
			{
				UsbError.Error(ErrorCode.Win32Error, Marshal.GetLastWin32Error(), "Open", typeof(UsbDevice));
			}
			return flag;
		}

		public static bool Open(string devicePath, out WinUsbDevice usbDevice)
		{
			usbDevice = null;
			SafeFileHandle sfhDevice;
			bool flag = WinUsbAPI.OpenDevice(out sfhDevice, devicePath);
			if (flag)
			{
				SafeWinUsbInterfaceHandle InterfaceHandle = new SafeWinUsbInterfaceHandle();
				flag = WinUsbAPI.WinUsb_Initialize(sfhDevice, ref InterfaceHandle);
				if (flag)
				{
					usbDevice = new WinUsbDevice(UsbDevice.WinUsbApi, sfhDevice, InterfaceHandle, devicePath);
				}
				else
				{
					UsbError.Error(ErrorCode.Win32Error, Marshal.GetLastWin32Error(), "Open:Initialize", typeof(UsbDevice));
				}
			}
			else
			{
				UsbError.Error(ErrorCode.Win32Error, Marshal.GetLastWin32Error(), "Open", typeof(UsbDevice));
			}
			return flag;
		}

		public override bool GetAssociatedInterface(byte associatedInterfaceIndex, out WindowsDevice usbDevice)
		{
			usbDevice = null;
			IntPtr AssociatedInterfaceHandle = IntPtr.Zero;
			bool num = WinUsbAPI.WinUsb_GetAssociatedInterface(mUsbHandle, associatedInterfaceIndex, ref AssociatedInterfaceHandle);
			if (num)
			{
				usbDevice = new WinUsbDevice(handle: new SafeWinUsbInterfaceHandle(AssociatedInterfaceHandle), usbApi: mUsbApi, usbHandle: null, devicePath: mDevicePath);
			}
			if (!num)
			{
				UsbError.Error(ErrorCode.Win32Error, Marshal.GetLastWin32Error(), "GetAssociatedInterface", this);
			}
			return num;
		}

		public override bool QueryDeviceSpeed(out DeviceSpeedTypes deviceSpeed)
		{
			deviceSpeed = DeviceSpeedTypes.Undefined;
			byte[] array = new byte[1];
			int BufferLength = 1;
			bool num = WinUsbAPI.WinUsb_QueryDeviceInformation(mUsbHandle, DeviceInformationTypes.DeviceSpeed, ref BufferLength, array);
			if (num)
			{
				deviceSpeed = (DeviceSpeedTypes)array[0];
				return num;
			}
			UsbError.Error(ErrorCode.Win32Error, Marshal.GetLastWin32Error(), "QueryDeviceInformation:QueryDeviceSpeed", this);
			return num;
		}

		public override bool QueryInterfaceSettings(byte alternateInterfaceNumber, ref UsbInterfaceDescriptor usbAltInterfaceDescriptor)
		{
			bool num = WinUsbAPI.WinUsb_QueryInterfaceSettings(base.Handle, alternateInterfaceNumber, usbAltInterfaceDescriptor);
			if (!num)
			{
				UsbError.Error(ErrorCode.Win32Error, Marshal.GetLastWin32Error(), "QueryInterfaceSettings", this);
			}
			return num;
		}

		internal override bool GetPowerPolicy(PowerPolicyType policyType, ref int valueLength, IntPtr pBuffer)
		{
			bool num = WinUsbAPI.WinUsb_GetPowerPolicy(mUsbHandle, policyType, ref valueLength, pBuffer);
			if (!num)
			{
				UsbError.Error(ErrorCode.Win32Error, Marshal.GetLastWin32Error(), "GetPowerPolicy", this);
			}
			return num;
		}

		public static bool GetDevicePathList(Guid interfaceGuid, out List<string> devicePathList)
		{
			return WinUsbRegistry.GetDevicePathList(interfaceGuid, out devicePathList);
		}

		internal override bool SetPowerPolicy(PowerPolicyType policyType, int valueLength, IntPtr pBuffer)
		{
			bool num = WinUsbAPI.WinUsb_SetPowerPolicy(mUsbHandle, policyType, valueLength, pBuffer);
			if (!num)
			{
				UsbError.Error(ErrorCode.Win32Error, Marshal.GetLastWin32Error(), "SetPowerPolicy", this);
			}
			return num;
		}

		~WinUsbDevice()
		{
			Close();
		}
	}
}
