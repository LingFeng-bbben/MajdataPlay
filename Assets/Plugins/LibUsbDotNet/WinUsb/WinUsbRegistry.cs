using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using LibUsbDotNet.Internal;
using LibUsbDotNet.Internal.UsbRegex;
using LibUsbDotNet.Main;
using Microsoft.Win32;

namespace LibUsbDotNet.WinUsb
{
	public class WinUsbRegistry : UsbRegistry
	{
		internal const string DEVICE_INTERFACE_GUID = "DeviceInterfaceGuid";

		private bool mIsDeviceIDParsed;

		private string mDeviceID;

		private byte mInterfaceID;

		private ushort mVid;

		private ushort mPid;

		public static List<WinUsbRegistry> DeviceList
		{
			get
			{
				List<WinUsbRegistry> list = new List<WinUsbRegistry>();
				SetupApi.EnumClassDevs(null, SetupApi.DICFG.PRESENT | SetupApi.DICFG.ALLCLASSES, WinUsbRegistryCallBack, list);
				return list.Where((WinUsbRegistry device) => !((string)device.DeviceProperties["ClassGuid"]).Equals("{ecfb0cfd-74c4-4f52-bbf7-343461cd72ac}")).ToList();
			}
		}

		public override Guid[] DeviceInterfaceGuids => mDeviceInterfaceGuids;

		public override bool IsAlive
		{
			get
			{
				if (string.IsNullOrEmpty(base.SymbolicName))
				{
					throw new UsbException(this, "A symbolic name is required for this property.");
				}
				foreach (WinUsbRegistry device in DeviceList)
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

		public string DeviceID
		{
			get
			{
				if (mDeviceID == null)
				{
					if (mDeviceProperties.TryGetValue("DeviceID", out var value))
					{
						mDeviceID = value.ToString();
					}
					else
					{
						mDeviceID = string.Empty;
					}
				}
				return mDeviceID;
			}
		}

		public override int Vid
		{
			get
			{
				parseDeviceID();
				return mVid;
			}
		}

		public override int Pid
		{
			get
			{
				parseDeviceID();
				return mPid;
			}
		}

		public byte InterfaceID
		{
			get
			{
				parseDeviceID();
				return mInterfaceID;
			}
		}

		public override string DevicePath => base.SymbolicName;

		public static bool GetDevicePathList(Guid deviceInterfaceGuid, out List<string> devicePathList)
		{
			devicePathList = new List<string>();
			int i = 0;
			SetupApi.SP_DEVICE_INTERFACE_DATA deviceInterfaceData = SetupApi.SP_DEVICE_INTERFACE_DATA.Empty;
			IntPtr intPtr = SetupApi.SetupDiGetClassDevs(ref deviceInterfaceGuid, null, IntPtr.Zero, SetupApi.DICFG.PRESENT | SetupApi.DICFG.DEVICEINTERFACE);
			if (intPtr != IntPtr.Zero)
			{
				for (; SetupApi.SetupDiEnumDeviceInterfaces(intPtr, null, ref deviceInterfaceGuid, i, ref deviceInterfaceData); i++)
				{
					int requiredSize = 1024;
					SetupApi.DeviceInterfaceDetailHelper deviceInterfaceDetailHelper = new SetupApi.DeviceInterfaceDetailHelper(requiredSize);
					if (SetupApi.SetupDiGetDeviceInterfaceDetail(intPtr, ref deviceInterfaceData, deviceInterfaceDetailHelper.Handle, requiredSize, out requiredSize, null))
					{
						devicePathList.Add(deviceInterfaceDetailHelper.DevicePath);
					}
				}
			}
			if (i == 0)
			{
				UsbError.Error(ErrorCode.Win32Error, Marshal.GetLastWin32Error(), "GetDevicePathList", typeof(SetupApi));
			}
			if (intPtr != IntPtr.Zero)
			{
				SetupApi.SetupDiDestroyDeviceInfoList(intPtr);
			}
			return i > 0;
		}

		public static bool GetWinUsbRegistryList(Guid deviceInterfaceGuid, out List<WinUsbRegistry> deviceRegistryList)
		{
			deviceRegistryList = new List<WinUsbRegistry>();
			int i = 0;
			SetupApi.SP_DEVICE_INTERFACE_DATA deviceInterfaceData = SetupApi.SP_DEVICE_INTERFACE_DATA.Empty;
			SetupApi.SP_DEVINFO_DATA deviceInfoData = SetupApi.SP_DEVINFO_DATA.Empty;
			IntPtr intPtr = SetupApi.SetupDiGetClassDevs(ref deviceInterfaceGuid, null, IntPtr.Zero, SetupApi.DICFG.PRESENT | SetupApi.DICFG.DEVICEINTERFACE);
			if (intPtr != IntPtr.Zero)
			{
				for (; SetupApi.SetupDiEnumDeviceInterfaces(intPtr, null, ref deviceInterfaceGuid, i, ref deviceInterfaceData); i++)
				{
					int requiredSize = 1024;
					SetupApi.DeviceInterfaceDetailHelper deviceInterfaceDetailHelper = new SetupApi.DeviceInterfaceDetailHelper(requiredSize);
					if (SetupApi.SetupDiGetDeviceInterfaceDetail(intPtr, ref deviceInterfaceData, deviceInterfaceDetailHelper.Handle, requiredSize, out requiredSize, ref deviceInfoData))
					{
						WinUsbRegistry winUsbRegistry = new WinUsbRegistry();
						SetupApi.getSPDRPProperties(intPtr, ref deviceInfoData, winUsbRegistry.mDeviceProperties);
						winUsbRegistry.mDeviceProperties.Add("SymbolicName", deviceInterfaceDetailHelper.DevicePath);
						winUsbRegistry.mDeviceInterfaceGuids = new Guid[1] { deviceInterfaceGuid };
						StringBuilder stringBuilder = new StringBuilder(1024);
						if (SetupApi.CM_Get_Device_ID(deviceInfoData.DevInst, stringBuilder, stringBuilder.Capacity, 0) == SetupApi.CR.SUCCESS)
						{
							winUsbRegistry.mDeviceProperties["DeviceID"] = stringBuilder.ToString();
						}
						deviceRegistryList.Add(winUsbRegistry);
					}
				}
			}
			if (i == 0)
			{
				UsbError.Error(ErrorCode.Win32Error, Marshal.GetLastWin32Error(), "GetDevicePathList", typeof(SetupApi));
			}
			if (intPtr != IntPtr.Zero)
			{
				SetupApi.SetupDiDestroyDeviceInfoList(intPtr);
			}
			return i > 0;
		}

		internal WinUsbRegistry()
		{
		}

		private void parseDeviceID()
		{
			if (mIsDeviceIDParsed)
			{
				return;
			}
			mIsDeviceIDParsed = true;
			foreach (Match item in RegHardwareID.GlobalInstance.Matches(DeviceID))
			{
				NamedGroup[] nAMED_GROUPS = RegHardwareID.NAMED_GROUPS;
				for (int i = 0; i < nAMED_GROUPS.Length; i++)
				{
					NamedGroup namedGroup = nAMED_GROUPS[i];
					Group obj = item.Groups[namedGroup.GroupNumber];
					if (!obj.Success)
					{
						continue;
					}
					ushort result2;
					switch ((RegHardwareID.ENamedGroups)namedGroup.GroupNumber)
					{
						case RegHardwareID.ENamedGroups.Vid:
							if (ushort.TryParse(obj.Value, NumberStyles.HexNumber, null, out result2))
							{
								mVid = result2;
							}
							break;
						case RegHardwareID.ENamedGroups.Pid:
							if (ushort.TryParse(obj.Value, NumberStyles.HexNumber, null, out result2))
							{
								mPid = result2;
							}
							break;
						case RegHardwareID.ENamedGroups.MI:
						{
							if (byte.TryParse(obj.Value, NumberStyles.HexNumber, null, out var result))
							{
								mInterfaceID = result;
							}
							break;
						}
						default:
							throw new ArgumentOutOfRangeException();
						case RegHardwareID.ENamedGroups.Rev:
							break;
					}
				}
			}
		}

		public override bool Open(out UsbDevice usbDevice)
		{
			usbDevice = null;
			WinUsbDevice usbDevice2;
			bool num = Open(out usbDevice2);
			if (num)
			{
				usbDevice = usbDevice2;
			}
			return num;
		}

		public bool Open(out WinUsbDevice usbDevice)
		{
			usbDevice = null;
			if (string.IsNullOrEmpty(base.SymbolicName))
			{
				return false;
			}
			if (WinUsbDevice.Open(base.SymbolicName, out usbDevice))
			{
				usbDevice.mUsbRegistry = this;
				return true;
			}
			return false;
		}

		private static bool WinUsbRegistryCallBack(IntPtr deviceInfoSet, int deviceIndex, ref SetupApi.SP_DEVINFO_DATA deviceInfoData, object classEnumeratorCallbackParam1)
		{
			List<WinUsbRegistry> list = (List<WinUsbRegistry>)classEnumeratorCallbackParam1;
			byte[] array = new byte[256];
			RegistryValueKind PropertyRegDataType;
			int RequiredSize;
			bool flag = SetupApi.SetupDiGetCustomDeviceProperty(deviceInfoSet, ref deviceInfoData, "DeviceInterfaceGuids", SetupApi.DICUSTOMDEVPROP.NONE, out PropertyRegDataType, array, array.Length, out RequiredSize);
			if (!flag)
			{
				flag = SetupApi.SetupDiGetCustomDeviceProperty(deviceInfoSet, ref deviceInfoData, "DeviceInterfaceGuid", SetupApi.DICUSTOMDEVPROP.NONE, out PropertyRegDataType, array, array.Length, out RequiredSize);
			}
			if (flag)
			{
				string[] asStringArray = UsbRegistry.GetAsStringArray(array, RequiredSize);
				foreach (string g in asStringArray)
				{
					Guid guid = new Guid(g);
					if (!GetWinUsbRegistryList(guid, out var deviceRegistryList))
					{
						continue;
					}
					foreach (WinUsbRegistry item in deviceRegistryList)
					{
						WinUsbRegistry winUsbRegistry = null;
						foreach (WinUsbRegistry item2 in list)
						{
							if (item2.SymbolicName == item.SymbolicName)
							{
								winUsbRegistry = item2;
								break;
							}
						}
						if (winUsbRegistry == null)
						{
							list.Add(item);
							continue;
						}
						List<Guid> list2 = new List<Guid>(winUsbRegistry.mDeviceInterfaceGuids);
						if (!list2.Contains(guid))
						{
							list2.Add(guid);
							winUsbRegistry.mDeviceInterfaceGuids = list2.ToArray();
						}
					}
				}
			}
			return false;
		}
	}
}
