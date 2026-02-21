using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using LibUsbDotNet.Internal.LibUsb;
using LibUsbDotNet.Main;
using Microsoft.Win32.SafeHandles;

namespace LibUsbDotNet.LibUsb
{
	public class LibUsbRegistry : UsbRegistry
	{
		private readonly string mDeviceFilename;

		private readonly int mDeviceIndex;

		public int DeviceIndex => mDeviceIndex;

		public static List<LibUsbRegistry> DeviceList
		{
			get
			{
				List<LibUsbRegistry> list = new List<LibUsbRegistry>();
				for (int i = 1; i < 256; i++)
				{
					string deviceNameString = LibUsbDriverIO.GetDeviceNameString(i);
					SafeFileHandle safeFileHandle = LibUsbDriverIO.OpenDevice(deviceNameString);
					if (safeFileHandle != null && !safeFileHandle.IsInvalid && !safeFileHandle.IsClosed)
					{
						try
						{
							LibUsbRegistry item = new LibUsbRegistry(safeFileHandle, deviceNameString, i);
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

		public override bool IsAlive
		{
			get
			{
				if (string.IsNullOrEmpty(base.SymbolicName))
				{
					throw new UsbException(this, "A symbolic name is required for this property.");
				}
				foreach (LibUsbRegistry device in DeviceList)
				{
					if (!string.IsNullOrEmpty(device.SymbolicName) && device.SymbolicName == base.SymbolicName)
					{
						return true;
					}
				}
				return false;
			}
		}

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

		public override string DevicePath => mDeviceFilename;

		private static string FixSymbolicName(string symbolicName)
		{
			if (symbolicName.Length >= 3 && symbolicName[0] == '\\' && symbolicName[1] == '?' && symbolicName[2] == '?')
			{
				symbolicName = new StringBuilder(symbolicName) { [1] = '\\' }.ToString();
			}
			return symbolicName;
		}

		private LibUsbRegistry(SafeFileHandle usbHandle, string deviceFileName, int deviceIndex)
		{
			mDeviceFilename = deviceFileName;
			mDeviceIndex = deviceIndex;
			GetPropertiesSPDRP(usbHandle);
			if (GetCustomDeviceKeyValue(usbHandle, "SymbolicName", out string propData, 512) == ErrorCode.None)
			{
				propData = FixSymbolicName(propData);
				mDeviceProperties.Add("SymbolicName", propData);
			}
			GetObjectName(usbHandle, 0, out var objectName);
			mDeviceProperties.Add("DevicePlugPlayRegistryKey", objectName);
			if (!mDeviceProperties.ContainsKey("SymbolicName") || string.IsNullOrEmpty(propData))
			{
				if (!(mDeviceProperties[SPDRP.HardwareId.ToString()] is string[] array) || array.Length == 0)
				{
					LegacyUsbRegistry.GetPropertiesSPDRP(new LibUsbDevice(UsbDevice.LibUsbApi, usbHandle, deviceFileName), mDeviceProperties);
					return;
				}
				if (array.Length != 0)
				{
					mDeviceProperties.Add("SymbolicName", array[0]);
				}
			}
			if (GetCustomDeviceKeyValue(usbHandle, "LibUsbInterfaceGUIDs", out string propData2, 512) == ErrorCode.None)
			{
				string[] value = propData2.Split(new char[1], StringSplitOptions.RemoveEmptyEntries);
				mDeviceProperties.Add("LibUsbInterfaceGUIDs", value);
			}
		}

		private bool GetObjectName(SafeFileHandle usbHandle, int objectNameIndex, out string objectName)
		{
			byte[] array = new byte[512];
			GCHandle gCHandle = GCHandle.Alloc(array, GCHandleType.Pinned);
			int ret;
			bool num = LibUsbDriverIO.UsbIOSync(usbHandle, LibUsbIoCtl.GET_OBJECT_NAME, array, array.Length, gCHandle.AddrOfPinnedObject(), array.Length, out ret);
			gCHandle.Free();
			if (num && ret > 1)
			{
				objectName = Encoding.ASCII.GetString(array, 0, ret - 1);
				return num;
			}
			objectName = string.Empty;
			return num;
		}

		public bool Open(out LibUsbDevice usbDevice)
		{
			bool num = LibUsbDevice.Open(mDeviceFilename, out usbDevice);
			if (num)
			{
				usbDevice.mUsbRegistry = this;
			}
			return num;
		}

		public override bool Open(out UsbDevice usbDevice)
		{
			usbDevice = null;
			LibUsbDevice usbDevice2;
			bool num = Open(out usbDevice2);
			if (num)
			{
				usbDevice = usbDevice2;
			}
			return num;
		}

		internal ErrorCode GetCustomDeviceKeyValue(SafeFileHandle usbHandle, string key, out string propData, int maxDataLength)
		{
			byte[] propData2;
			ErrorCode customDeviceKeyValue = GetCustomDeviceKeyValue(usbHandle, key, out propData2, maxDataLength);
			if (customDeviceKeyValue == ErrorCode.None)
			{
				propData = Encoding.Unicode.GetString(propData2);
				int num = propData.IndexOf('\0');
				if (num >= 0)
				{
					propData = propData.Substring(0, num);
					return customDeviceKeyValue;
				}
			}
			else
			{
				propData = null;
			}
			return customDeviceKeyValue;
		}

		internal ErrorCode GetCustomDeviceKeyValue(SafeFileHandle usbHandle, string key, out byte[] propData, int maxDataLength)
		{
			ErrorCode result = ErrorCode.None;
			propData = null;
			byte[] array = LibUsbDeviceRegistryKeyRequest.RegGetRequest(key, maxDataLength);
			GCHandle gCHandle = GCHandle.Alloc(array, GCHandleType.Pinned);
			int ret;
			bool num = LibUsbDriverIO.UsbIOSync(usbHandle, LibUsbIoCtl.GET_CUSTOM_REG_PROPERTY, array, array.Length, gCHandle.AddrOfPinnedObject(), array.Length, out ret);
			gCHandle.Free();
			if (num)
			{
				propData = new byte[ret];
				Array.Copy(array, propData, ret);
			}
			else
			{
				result = ErrorCode.GetDeviceKeyValueFailed;
			}
			return result;
		}

		private void GetPropertiesSPDRP(SafeHandle usbHandle)
		{
			byte[] array = new byte[1024];
			GCHandle gCHandle = GCHandle.Alloc(array, GCHandleType.Pinned);
			LibUsbRequest libUsbRequest = new LibUsbRequest();
			foreach (KeyValuePair<string, int> enumDatum in Helper.GetEnumData(typeof(DevicePropertyType)))
			{
				object value = string.Empty;
				libUsbRequest.DeviceProperty.ID = enumDatum.Value;
				if (LibUsbDriverIO.UsbIOSync(usbHandle, LibUsbIoCtl.GET_REG_PROPERTY, libUsbRequest, LibUsbRequest.Size, gCHandle.AddrOfPinnedObject(), array.Length, out var ret))
				{
					switch ((DevicePropertyType)enumDatum.Value)
					{
						case DevicePropertyType.DeviceDesc:
						case DevicePropertyType.Class:
						case DevicePropertyType.ClassGuid:
						case DevicePropertyType.Driver:
						case DevicePropertyType.Mfg:
						case DevicePropertyType.FriendlyName:
						case DevicePropertyType.LocationInformation:
						case DevicePropertyType.PhysicalDeviceObjectName:
						case DevicePropertyType.EnumeratorName:
							value = UsbRegistry.GetAsString(array, ret);
							break;
						case DevicePropertyType.HardwareId:
						case DevicePropertyType.CompatibleIds:
							value = UsbRegistry.GetAsStringArray(array, ret);
							break;
						case DevicePropertyType.LegacyBusType:
						case DevicePropertyType.BusNumber:
						case DevicePropertyType.Address:
						case DevicePropertyType.UiNumber:
						case DevicePropertyType.InstallState:
						case DevicePropertyType.RemovalPolicy:
							value = UsbRegistry.GetAsStringInt32(array, ret);
							break;
						case DevicePropertyType.BusTypeGuid:
							value = UsbRegistry.GetAsGuid(array, ret);
							break;
					}
				}
				else
				{
					value = string.Empty;
				}
				mDeviceProperties.Add(enumDatum.Key, value);
			}
			gCHandle.Free();
		}
	}
}
