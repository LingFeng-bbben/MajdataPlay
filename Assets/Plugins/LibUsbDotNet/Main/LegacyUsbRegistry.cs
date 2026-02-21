using System;
using System.Collections.Generic;
using LibUsbDotNet.Internal.LibUsb;
using LibUsbDotNet.LibUsb;
using Microsoft.Win32.SafeHandles;

namespace LibUsbDotNet.Main
{
	public class LegacyUsbRegistry : UsbRegistry
	{
		private readonly UsbDevice mUSBDevice;

		public override UsbDevice Device
		{
			get
			{
				Open(out var usbDevice);
				return usbDevice;
			}
		}

		public override Guid[] DeviceInterfaceGuids
		{
			get
			{
				if (mDeviceInterfaceGuids == null)
				{
					if (!mDeviceProperties.ContainsKey("LibUsbInterfaceGUIDs"))
					{
						return new Guid[0];
					}
					if (!(mDeviceProperties["LibUsbInterfaceGUIDs"] is string[] array))
					{
						return new Guid[0];
					}
					mDeviceInterfaceGuids = new Guid[array.Length];
					for (int i = 0; i < array.Length; i++)
					{
						string g = array[i].Trim(' ', '{', '}', '[', ']', '\0');
						mDeviceInterfaceGuids[i] = new Guid(g);
					}
				}
				return mDeviceInterfaceGuids;
			}
		}

		public override bool IsAlive
		{
			get
			{
				if (string.IsNullOrEmpty(base.SymbolicName))
				{
					throw new UsbException(this, "A symbolic name is required for this property.");
				}
				foreach (LegacyUsbRegistry device in DeviceList)
				{
					if (!string.IsNullOrEmpty(device.SymbolicName) && device.SymbolicName == base.SymbolicName)
					{
						return true;
					}
				}
				return false;
			}
		}

		public static List<LegacyUsbRegistry> DeviceList
		{
			get
			{
				List<LegacyUsbRegistry> list = new List<LegacyUsbRegistry>();
				for (int i = 1; i < 256; i++)
				{
					string deviceNameString = LibUsbDriverIO.GetDeviceNameString(i);
					SafeFileHandle safeFileHandle = LibUsbDriverIO.OpenDevice(deviceNameString);
					if (safeFileHandle != null && !safeFileHandle.IsInvalid && !safeFileHandle.IsClosed)
					{
						try
						{
							LegacyUsbRegistry item = new LegacyUsbRegistry(new LibUsbDevice(UsbDevice.LibUsbApi, safeFileHandle, deviceNameString));
							list.Add(item);
						}
						catch (Exception)
						{
						}
					}
					if (safeFileHandle != null && !safeFileHandle.IsClosed)
					{
						safeFileHandle.Dispose();
					}
				}
				return list;
			}
		}

		public override int Rev
		{
			get
			{
				if (int.TryParse(mUSBDevice.Info.Descriptor.BcdDevice.ToString("X4"), out var result))
				{
					return result;
				}
				return (ushort)mUSBDevice.Info.Descriptor.BcdDevice;
			}
		}

		public override string DevicePath => mUSBDevice.DevicePath;

		internal LegacyUsbRegistry(UsbDevice usbDevice)
		{
			mUSBDevice = usbDevice;
			GetPropertiesSPDRP(mUSBDevice, mDeviceProperties);
		}

		internal static string GetRegistryHardwareID(ushort vid, ushort pid, ushort rev)
		{
			return string.Format("Vid_{0:X4}&Pid_{1:X4}&Rev_{2}", vid, pid, rev.ToString("0000"));
		}

		public override bool Open(out UsbDevice usbDevice)
		{
			usbDevice = null;
			bool num = mUSBDevice.Open();
			if (num)
			{
				usbDevice = mUSBDevice;
				usbDevice.mUsbRegistry = this;
			}
			return num;
		}

		internal static void GetPropertiesSPDRP(UsbDevice usbDevice, Dictionary<string, object> deviceProperties)
		{
			deviceProperties.Add(DevicePropertyType.Mfg.ToString(), (usbDevice.Info.Descriptor.ManufacturerStringIndex > 0) ? usbDevice.Info.ManufacturerString : string.Empty);
			deviceProperties.Add(DevicePropertyType.DeviceDesc.ToString(), (usbDevice.Info.Descriptor.ProductStringIndex > 0) ? usbDevice.Info.ProductString : string.Empty);
			deviceProperties.Add("SerialNumber", (usbDevice.Info.Descriptor.SerialStringIndex > 0) ? usbDevice.Info.SerialString : string.Empty);
			string registryHardwareID = GetRegistryHardwareID((ushort)usbDevice.Info.Descriptor.VendorID, (ushort)usbDevice.Info.Descriptor.ProductID, (ushort)usbDevice.Info.Descriptor.BcdDevice);
			deviceProperties.Add(DevicePropertyType.HardwareId.ToString(), new string[1] { registryHardwareID });
			string text = registryHardwareID + "{" + Guid.Empty.ToString() + " }";
			if (usbDevice.Info.Descriptor.SerialStringIndex > 0)
			{
				text = text + "#" + deviceProperties["SerialNumber"]?.ToString() + "#";
			}
			deviceProperties.Add("SymbolicName", text);
		}
	}
}
