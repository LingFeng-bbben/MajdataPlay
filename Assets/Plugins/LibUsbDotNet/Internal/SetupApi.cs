using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using LibUsbDotNet.Main;
#pragma warning disable CS0618 // Legacy Win32 interop signatures require UnmanagedType.AsAny.

namespace LibUsbDotNet.Internal
{
	internal class SetupApi
	{
		public delegate bool ClassEnumeratorDelegate(IntPtr DeviceInfoSet, int deviceIndex, ref SP_DEVINFO_DATA DeviceInfoData, object classEnumeratorCallbackParam1);

		public enum CR
		{
			SUCCESS = 0,
			DEFAULT = 1,
			OUT_OF_MEMORY = 2,
			INVALID_POINTER = 3,
			INVALID_FLAG = 4,
			INVALID_DEVNODE = 5,
			INVALID_DEVINST = 5,
			INVALID_RES_DES = 6,
			INVALID_LOG_CONF = 7,
			INVALID_ARBITRATOR = 8,
			INVALID_NODELIST = 9,
			DEVNODE_HAS_REQS = 10,
			DEVINST_HAS_REQS = 10,
			INVALID_RESOURCEID = 11,
			DLVXD_NOT_FOUND = 12,
			NO_SUCH_DEVNODE = 13,
			NO_SUCH_DEVINST = 13,
			NO_MORE_LOG_CONF = 14,
			NO_MORE_RES_DES = 15,
			ALREADY_SUCH_DEVNODE = 16,
			ALREADY_SUCH_DEVINST = 16,
			INVALID_RANGE_LIST = 17,
			INVALID_RANGE = 18,
			FAILURE = 19,
			NO_SUCH_LOGICAL_DEV = 20,
			CREATE_BLOCKED = 21,
			NOT_SYSTEM_VM = 22,
			REMOVE_VETOED = 23,
			APM_VETOED = 24,
			INVALID_LOAD_TYPE = 25,
			BUFFER_SMALL = 26,
			NO_ARBITRATOR = 27,
			NO_REGISTRY_HANDLE = 28,
			REGISTRY_ERROR = 29,
			INVALID_DEVICE_ID = 30,
			INVALID_DATA = 31,
			INVALID_API = 32,
			DEVLOADER_NOT_READY = 33,
			NEED_RESTART = 34,
			NO_MORE_HW_PROFILES = 35,
			DEVICE_NOT_THERE = 36,
			NO_SUCH_VALUE = 37,
			WRONG_TYPE = 38,
			INVALID_PRIORITY = 39,
			NOT_DISABLEABLE = 40,
			FREE_RESOURCES = 41,
			QUERY_VETOED = 42,
			CANT_SHARE_IRQ = 43,
			NO_DEPENDENT = 44,
			SAME_RESOURCES = 45,
			NO_SUCH_REGISTRY_KEY = 46,
			INVALID_MACHINENAME = 47,
			REMOTE_COMM_FAILURE = 48,
			MACHINE_UNAVAILABLE = 49,
			NO_CM_SERVICES = 50,
			ACCESS_DENIED = 51,
			CALL_NOT_IMPLEMENTED = 52,
			INVALID_PROPERTY = 53,
			DEVICE_INTERFACE_ACTIVE = 54,
			NO_SUCH_DEVICE_INTERFACE = 55,
			INVALID_REFERENCE_STRING = 56,
			INVALID_CONFLICT_LIST = 57,
			INVALID_INDEX = 58,
			INVALID_STRUCTURE_SIZE = 59,
			NUM_CR_RESULTS = 60
		}

		public enum DeviceInterfaceDataFlags : uint
		{
			Active = 1u,
			Default = 2u,
			Removed = 4u
		}

		[Flags]
		public enum DICFG
		{
			DEFAULT = 1,
			PRESENT = 2,
			ALLCLASSES = 4,
			PROFILE = 8,
			DEVICEINTERFACE = 0x10
		}

		public enum DICUSTOMDEVPROP
		{
			NONE,
			MERGE_MULTISZ
		}

		[Flags]
		public enum DevKeyType
		{
			DEV = 1,
			DRV = 2,
			BOTH = 4
		}

		public struct DEVICE_INTERFACE_DETAIL_HANDLE
		{
			private IntPtr mPtr;

			internal DEVICE_INTERFACE_DETAIL_HANDLE(IntPtr ptrInit)
			{
				mPtr = ptrInit;
			}
		}

		public class DeviceInterfaceDetailHelper
		{
			public static readonly int SIZE = (Is64Bit ? 8 : 6);

			private IntPtr mpDevicePath;

			private IntPtr mpStructure;

			public DEVICE_INTERFACE_DETAIL_HANDLE Handle
			{
				get
				{
					Marshal.WriteInt32(mpStructure, SIZE);
					return new DEVICE_INTERFACE_DETAIL_HANDLE(mpStructure);
				}
			}

			public string DevicePath => Marshal.PtrToStringUni(mpDevicePath);

			public DeviceInterfaceDetailHelper(int maximumLength)
			{
				mpStructure = Marshal.AllocHGlobal(maximumLength);
				mpDevicePath = new IntPtr(mpStructure.ToInt64() + Marshal.SizeOf(typeof(int)));
			}

			public void Free()
			{
				if (mpStructure != IntPtr.Zero)
				{
					Marshal.FreeHGlobal(mpStructure);
				}
				mpDevicePath = IntPtr.Zero;
				mpStructure = IntPtr.Zero;
			}

			~DeviceInterfaceDetailHelper()
			{
				Free();
			}
		}

		private class MaxStructSizes
		{
			public const int SP_DEVINFO_DATA = 40;
		}

		public struct SP_DEVICE_INTERFACE_DATA
		{
			public static readonly SP_DEVICE_INTERFACE_DATA Empty = new SP_DEVICE_INTERFACE_DATA(Marshal.SizeOf(typeof(SP_DEVICE_INTERFACE_DATA)));

			public uint cbSize;

			public Guid interfaceClassGuid;

			public uint flags;

			private UIntPtr reserved;

			private SP_DEVICE_INTERFACE_DATA(int size)
			{
				cbSize = (uint)size;
				reserved = UIntPtr.Zero;
				flags = 0u;
				interfaceClassGuid = Guid.Empty;
			}
		}

		public struct SP_DEVINFO_DATA
		{
			public static readonly SP_DEVINFO_DATA Empty = new SP_DEVINFO_DATA(Marshal.SizeOf(typeof(SP_DEVINFO_DATA)));

			public uint cbSize;

			public Guid ClassGuid;

			public uint DevInst;

			public IntPtr Reserved;

			private SP_DEVINFO_DATA(int size)
			{
				cbSize = (uint)size;
				ClassGuid = Guid.Empty;
				DevInst = 0u;
				Reserved = IntPtr.Zero;
			}
		}

		private const string STRUCT_END_MARK = "STRUCT_END_MARK";

		public static readonly Guid GUID_DEVINTERFACE_USB_DEVICE = new Guid("f18a0e88-c30c-11d0-8815-00a0c906bed8");

		public static bool Is64Bit => IntPtr.Size == 8;

		[DllImport("setupapi.dll", CharSet = CharSet.Unicode)]
		public static extern CR CM_Get_Device_ID(IntPtr dnDevInst, IntPtr Buffer, int BufferLen, int ulFlags);

		[DllImport("setupapi.dll")]
		public static extern CR CM_Get_Parent(out IntPtr pdnDevInst, IntPtr dnDevInst, int ulFlags);

		[DllImport("setupapi.dll", CharSet = CharSet.Unicode)]
		public static extern bool SetupDiDestroyDeviceInfoList(IntPtr hDevInfo);

		[DllImport("setupapi.dll", SetLastError = true)]
		public static extern bool SetupDiEnumDeviceInfo(IntPtr DeviceInfoSet, int MemberIndex, ref SP_DEVINFO_DATA DeviceInfoData);

		[DllImport("setupapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		public static extern bool SetupDiEnumDeviceInterfaces(IntPtr hDevInfo, ref SP_DEVINFO_DATA devInfo, ref Guid interfaceClassGuid, int memberIndex, ref SP_DEVICE_INTERFACE_DATA deviceInterfaceData);

		[DllImport("setupapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		public static extern bool SetupDiEnumDeviceInterfaces(IntPtr hDevInfo, [MarshalAs(UnmanagedType.AsAny)] object devInfo, ref Guid interfaceClassGuid, int memberIndex, ref SP_DEVICE_INTERFACE_DATA deviceInterfaceData);

		[DllImport("setupapi.dll", CharSet = CharSet.Ansi, EntryPoint = "SetupDiGetClassDevsA")]
		public static extern IntPtr SetupDiGetClassDevs(ref Guid ClassGuid, [MarshalAs(UnmanagedType.LPTStr)] string Enumerator, IntPtr hwndParent, DICFG Flags);

		[DllImport("setupapi.dll", CharSet = CharSet.Ansi, EntryPoint = "SetupDiGetClassDevsA")]
		public static extern IntPtr SetupDiGetClassDevs(ref Guid ClassGuid, int Enumerator, IntPtr hwndParent, DICFG Flags);

		[DllImport("setupapi.dll", CharSet = CharSet.Ansi, EntryPoint = "SetupDiGetClassDevsA")]
		public static extern IntPtr SetupDiGetClassDevs(int ClassGuid, string Enumerator, IntPtr hwndParent, DICFG Flags);

		[DllImport("setupapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		public static extern bool SetupDiGetCustomDeviceProperty(IntPtr DeviceInfoSet, ref SP_DEVINFO_DATA DeviceInfoData, string CustomPropertyName, DICUSTOMDEVPROP Flags, out int PropertyRegDataType, byte[] PropertyBuffer, int PropertyBufferSize, out int RequiredSize);

		[DllImport("setupapi.dll", CharSet = CharSet.Ansi, EntryPoint = "SetupDiGetDeviceInstanceIdA", SetLastError = true)]
		public static extern bool SetupDiGetDeviceInstanceId(IntPtr DeviceInfoSet, ref SP_DEVINFO_DATA DeviceInfoData, StringBuilder DeviceInstanceId, int DeviceInstanceIdSize, out int RequiredSize);

		[DllImport("setupapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		public static extern bool SetupDiGetDeviceInterfaceDetail(IntPtr hDevInfo, ref SP_DEVICE_INTERFACE_DATA deviceInterfaceData, DEVICE_INTERFACE_DETAIL_HANDLE deviceInterfaceDetailData, int deviceInterfaceDetailDataSize, out int requiredSize, [MarshalAs(UnmanagedType.AsAny)] object deviceInfoData);

		[DllImport("setupapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		public static extern bool SetupDiGetDeviceInterfaceDetail(IntPtr hDevInfo, ref SP_DEVICE_INTERFACE_DATA deviceInterfaceData, DEVICE_INTERFACE_DETAIL_HANDLE deviceInterfaceDetailData, int deviceInterfaceDetailDataSize, out int requiredSize, ref SP_DEVINFO_DATA deviceInfoData);

		[DllImport("setupapi.dll", CharSet = CharSet.Unicode)]
		public static extern bool SetupDiGetDeviceInterfacePropertyKeys(IntPtr DeviceInfoSet, ref SP_DEVICE_INTERFACE_DATA DeviceInterfaceData, byte[] propKeyBuffer, int propKeyBufferElements, out int RequiredPropertyKeyCount, int Flags);

		[DllImport("setupapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		public static extern bool SetupDiGetDeviceRegistryProperty(IntPtr DeviceInfoSet, ref SP_DEVINFO_DATA DeviceInfoData, SPDRP Property, out int PropertyRegDataType, byte[] PropertyBuffer, int PropertyBufferSize, out int RequiredSize);

		[DllImport("setupapi.dll", CharSet = CharSet.Unicode)]
		public static extern CR CM_Get_Device_ID(uint dnDevInst, StringBuilder Buffer, int BufferLen, int ulFlags);

		[DllImport("Setupapi", CharSet = CharSet.Unicode, SetLastError = true)]
		public static extern IntPtr SetupDiOpenDevRegKey(IntPtr hDeviceInfoSet, ref SP_DEVINFO_DATA deviceInfoData, int scope, int hwProfile, DevKeyType keyType, int samDesired);

		[DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		public static extern int RegEnumValue(IntPtr hKey, int index, StringBuilder lpValueName, ref int lpcValueName, IntPtr lpReserved, out int lpType, byte[] data, ref int dataLength);

		[DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		public static extern int RegEnumValue(IntPtr hKey, int index, StringBuilder lpValueName, ref int lpcValueName, IntPtr lpReserved, out int lpType, StringBuilder data, ref int dataLength);

		[DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		public static extern int RegCloseKey(IntPtr hKey);

		public static bool EnumClassDevs(string enumerator, DICFG flags, ClassEnumeratorDelegate classEnumeratorCallback, object classEnumeratorCallbackParam1)
		{
			SP_DEVINFO_DATA DeviceInfoData = SP_DEVINFO_DATA.Empty;
			int i = 0;
			IntPtr intPtr = SetupDiGetClassDevs(0, enumerator, IntPtr.Zero, flags);
			if (intPtr == IntPtr.Zero || intPtr.ToInt64() == -1)
			{
				return false;
			}
			bool result = false;
			for (; SetupDiEnumDeviceInfo(intPtr, i, ref DeviceInfoData); i++)
			{
				if (classEnumeratorCallback(intPtr, i, ref DeviceInfoData, classEnumeratorCallbackParam1))
				{
					result = true;
					break;
				}
			}
			SetupDiDestroyDeviceInfoList(intPtr);
			return result;
		}

		public static void getSPDRPProperties(IntPtr deviceInfoSet, ref SP_DEVINFO_DATA deviceInfoData, Dictionary<string, object> deviceProperties)
		{
			byte[] array = new byte[1024];
			foreach (KeyValuePair<string, int> enumDatum in Helper.GetEnumData(typeof(SPDRP)))
			{
				object value = string.Empty;
				if (SetupDiGetDeviceRegistryProperty(deviceInfoSet, ref deviceInfoData, (SPDRP)enumDatum.Value, out var _, array, array.Length, out var RequiredSize))
				{
					switch ((SPDRP)enumDatum.Value)
					{
						case SPDRP.DeviceDesc:
						case SPDRP.Class:
						case SPDRP.ClassGuid:
						case SPDRP.Driver:
						case SPDRP.Mfg:
						case SPDRP.FriendlyName:
						case SPDRP.LocationInformation:
						case SPDRP.PhysicalDeviceObjectName:
						case SPDRP.EnumeratorName:
							value = UsbRegistry.GetAsString(array, RequiredSize);
							break;
						case SPDRP.HardwareId:
						case SPDRP.CompatibleIds:
						case SPDRP.LocationPaths:
							value = UsbRegistry.GetAsStringArray(array, RequiredSize);
							break;
						case SPDRP.UiNumber:
						case SPDRP.LegacyBusType:
						case SPDRP.BusNumber:
						case SPDRP.Address:
						case SPDRP.RemovalPolicy:
						case SPDRP.InstallState:
							value = UsbRegistry.GetAsStringInt32(array, RequiredSize);
							break;
						case SPDRP.BusTypeGuid:
							value = UsbRegistry.GetAsGuid(array, RequiredSize);
							break;
					}
				}
				else
				{
					value = string.Empty;
				}
				deviceProperties.Add(enumDatum.Key, value);
			}
		}

		public static bool SetupDiGetDeviceInterfaceDetailLength(IntPtr hDevInfo, ref SP_DEVICE_INTERFACE_DATA deviceInterfaceData, out int requiredLength)
		{
			return SetupDiGetDeviceInterfaceDetail(hDevInfo, ref deviceInterfaceData, default(DEVICE_INTERFACE_DETAIL_HANDLE), 0, out requiredLength, null);
		}

		public static bool SetupDiGetDeviceRegistryProperty(out byte[] regBytes, IntPtr DeviceInfoSet, ref SP_DEVINFO_DATA DeviceInfoData, SPDRP Property)
		{
			regBytes = null;
			byte[] array = new byte[1024];
			if (!SetupDiGetDeviceRegistryProperty(DeviceInfoSet, ref DeviceInfoData, Property, out var _, array, array.Length, out var RequiredSize))
			{
				UsbError.Error(ErrorCode.Win32Error, Marshal.GetLastWin32Error(), "SetupDiGetDeviceRegistryProperty", typeof(SetupApi));
				return false;
			}
			regBytes = new byte[RequiredSize];
			Array.Copy(array, regBytes, regBytes.Length);
			return true;
		}

		public static bool SetupDiGetDeviceRegistryProperty(out string regSZ, IntPtr DeviceInfoSet, ref SP_DEVINFO_DATA DeviceInfoData, SPDRP Property)
		{
			regSZ = null;
			if (SetupDiGetDeviceRegistryProperty(out byte[] regBytes, DeviceInfoSet, ref DeviceInfoData, Property))
			{
				regSZ = Encoding.Unicode.GetString(regBytes).TrimEnd(default(char));
				return true;
			}
			return false;
		}

		public static bool SetupDiGetDeviceRegistryProperty(out string[] regMultiSZ, IntPtr DeviceInfoSet, ref SP_DEVINFO_DATA DeviceInfoData, SPDRP Property)
		{
			regMultiSZ = null;
			if (SetupDiGetDeviceRegistryProperty(out string regSZ, DeviceInfoSet, ref DeviceInfoData, Property))
			{
				regMultiSZ = regSZ.Split(new char[1], StringSplitOptions.RemoveEmptyEntries);
				return true;
			}
			return false;
		}

		private static bool cbHasDeviceInterfaceGUID(IntPtr DeviceInfoSet, int deviceIndex, ref SP_DEVINFO_DATA DeviceInfoData, object devInterfaceGuid)
		{
			byte[] array = new byte[256];
			if (SetupDiGetCustomDeviceProperty(DeviceInfoSet, ref DeviceInfoData, "DeviceInterfaceGuids", DICUSTOMDEVPROP.NONE, out var _, array, array.Length, out var RequiredSize))
			{
				Guid guid = (Guid)devInterfaceGuid;
				string[] array2 = Encoding.Unicode.GetString(array, 0, RequiredSize).Split(new char[1], StringSplitOptions.RemoveEmptyEntries);
				Guid guid2 = new Guid(array2[0]);
				return guid == guid2;
			}
			return false;
		}
	}
}
