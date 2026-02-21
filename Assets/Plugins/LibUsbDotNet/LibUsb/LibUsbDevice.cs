using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using LibUsbDotNet.Info;
using LibUsbDotNet.Internal;
using LibUsbDotNet.Internal.LibUsb;
using LibUsbDotNet.Main;
using Microsoft.Win32.SafeHandles;

namespace LibUsbDotNet.LibUsb
{
	public class LibUsbDevice : UsbDevice, IUsbDevice, IUsbInterface
	{
		private readonly string mDeviceFilename;

		public static List<LibUsbDevice> LegacyLibUsbDeviceList
		{
			get
			{
				List<LibUsbDevice> list = new List<LibUsbDevice>();
				for (int i = 1; i < 256; i++)
				{
					if (Open(LibUsbDriverIO.GetDeviceNameString(i), out var usbDevice))
					{
						usbDevice.mDeviceInfo = new UsbDeviceInfo(usbDevice);
						usbDevice.Close();
						list.Add(usbDevice);
					}
				}
				return list;
			}
		}

		public string DeviceFilename => mDeviceFilename;

		public override DriverModeType DriverMode => DriverModeType.LibUsb;

		public override string DevicePath => mDeviceFilename;

		internal LibUsbDevice(UsbApiBase api, SafeHandle usbHandle, string deviceFilename)
			: base(api, usbHandle)
		{
			mDeviceFilename = deviceFilename;
		}

		public bool GetAltInterface(out int alternateID)
		{
			int interfaceID = ((mClaimedInterfaces.Count != 0) ? mClaimedInterfaces[mClaimedInterfaces.Count - 1] : 0);
			return GetAltInterface(interfaceID, out alternateID);
		}

		public override bool Open()
		{
			if (base.IsOpen)
			{
				return true;
			}
			mUsbHandle = LibUsbDriverIO.OpenDevice(mDeviceFilename);
			if (!base.IsOpen)
			{
				UsbError.Error(ErrorCode.Win32Error, Marshal.GetLastWin32Error(), "LibUsbDevice.Open Failed", this);
				return false;
			}
			return true;
		}

		public bool ClaimInterface(int interfaceID)
		{
			if (mClaimedInterfaces.Contains(interfaceID))
			{
				return true;
			}
			LibUsbRequest libUsbRequest = new LibUsbRequest();
			libUsbRequest.Iface.ID = interfaceID;
			libUsbRequest.Timeout = 1000;
			int ret;
			bool num = UsbIoSync(LibUsbIoCtl.CLAIM_INTERFACE, libUsbRequest, LibUsbRequest.Size, IntPtr.Zero, 0, out ret);
			if (num)
			{
				mClaimedInterfaces.Add(interfaceID);
			}
			return num;
		}

		public override bool Close()
		{
			if (base.IsOpen)
			{
				ReleaseAllInterfaces();
				base.ActiveEndpoints.Clear();
				mUsbHandle.Dispose();
			}
			return true;
		}

		public bool ReleaseInterface(int interfaceID)
		{
			LibUsbRequest libUsbRequest = new LibUsbRequest();
			libUsbRequest.Iface.ID = interfaceID;
			if (!mClaimedInterfaces.Remove(interfaceID))
			{
				return true;
			}
			libUsbRequest.Timeout = 1000;
			int ret;
			return UsbIoSync(LibUsbIoCtl.RELEASE_INTERFACE, libUsbRequest, LibUsbRequest.Size, IntPtr.Zero, 0, out ret);
		}

		public bool SetAltInterface(int alternateID)
		{
			if (mClaimedInterfaces.Count == 0)
			{
				throw new UsbException(this, "You must claim an interface before setting an alternate interface.");
			}
			return SetAltInterface(mClaimedInterfaces[mClaimedInterfaces.Count - 1], alternateID);
		}

		public bool SetConfiguration(byte config)
		{
			UsbSetupPacket setupPacket = new UsbSetupPacket
			{
				RequestType = 0,
				Request = 9,
				Value = config,
				Index = 0,
				Length = 0
			};
			int lengthTransferred;
			bool num = ControlTransfer(ref setupPacket, null, 0, out lengthTransferred);
			if (num)
			{
				mCurrentConfigValue = config;
				return num;
			}
			UsbError.Error(ErrorCode.Win32Error, Marshal.GetLastWin32Error(), "SetConfiguration", this);
			return num;
		}

		public static bool Open(string deviceFilename, out LibUsbDevice usbDevice)
		{
			usbDevice = null;
			SafeFileHandle safeFileHandle = LibUsbDriverIO.OpenDevice(deviceFilename);
			if (!safeFileHandle.IsClosed && !safeFileHandle.IsInvalid)
			{
				usbDevice = new LibUsbDevice(UsbDevice.LibUsbApi, safeFileHandle, deviceFilename);
				return true;
			}
			return false;
		}

		public int ReleaseAllInterfaces()
		{
			int num = 0;
			while (mClaimedInterfaces.Count > 0)
			{
				num++;
				ReleaseInterface(mClaimedInterfaces[mClaimedInterfaces.Count - num]);
			}
			return num;
		}

		public bool ReleaseInterface()
		{
			if (mClaimedInterfaces.Count == 0)
			{
				return true;
			}
			return ReleaseInterface(mClaimedInterfaces[mClaimedInterfaces.Count - 1]);
		}

		public bool SetAltInterface(int interfaceID, int alternateID)
		{
			if (!mClaimedInterfaces.Contains(interfaceID))
			{
				throw new UsbException(this, $"You must claim interface {interfaceID} before setting an alternate interface.");
			}
			LibUsbRequest libUsbRequest = new LibUsbRequest();
			libUsbRequest.Iface.ID = interfaceID;
			libUsbRequest.Iface.AlternateID = alternateID;
			libUsbRequest.Timeout = 1000;
			if (UsbIoSync(LibUsbIoCtl.SET_INTERFACE, libUsbRequest, LibUsbRequest.Size, IntPtr.Zero, 0, out var _))
			{
				UsbAltInterfaceSettings[interfaceID] = (byte)alternateID;
				return true;
			}
			return false;
		}

		public bool GetAltInterface(int interfaceID, out int alternateID)
		{
			LibUsbRequest libUsbRequest = new LibUsbRequest();
			libUsbRequest.Iface.ID = interfaceID;
			libUsbRequest.Timeout = 1000;
			GCHandle gCHandle = GCHandle.Alloc(libUsbRequest, GCHandleType.Pinned);
			alternateID = -1;
			int ret;
			bool num = UsbIoSync(LibUsbIoCtl.GET_INTERFACE, libUsbRequest, LibUsbRequest.Size, gCHandle.AddrOfPinnedObject(), 1, out ret);
			if (num)
			{
				alternateID = Marshal.ReadByte(gCHandle.AddrOfPinnedObject());
				UsbAltInterfaceSettings[interfaceID] = (byte)alternateID;
			}
			gCHandle.Free();
			return num;
		}

		public bool ResetDevice()
		{
			if (!base.IsOpen)
			{
				throw new UsbException(this, "Device is not opened.");
			}
			base.ActiveEndpoints.Clear();
			bool num = UsbDevice.LibUsbApi.ResetDevice(mUsbHandle);
			if (!num)
			{
				UsbError.Error(ErrorCode.Win32Error, Marshal.GetLastWin32Error(), "ResetDevice Failed", this);
				return num;
			}
			Close();
			return num;
		}

		internal bool ControlTransferEx(UsbSetupPacket setupPacket, IntPtr buffer, int bufferLength, out int lengthTransferred, int timeout)
		{
			return LibUsbDriverIO.ControlTransferEx(mUsbHandle, setupPacket, buffer, bufferLength, out lengthTransferred, timeout);
		}

		internal bool UsbIoSync(int controlCode, object inBuffer, int inSize, IntPtr outBuffer, int outSize, out int ret)
		{
			return LibUsbDriverIO.UsbIOSync(mUsbHandle, controlCode, inBuffer, inSize, outBuffer, outSize, out ret);
		}

		public bool DetachKernelDriver(int interfaceID)
		{
			throw new PlatformNotSupportedException();
		}

		public bool SetAutoDetachKernelDriver(bool autoDetach)
		{
			throw new PlatformNotSupportedException();
		}
	}
}
