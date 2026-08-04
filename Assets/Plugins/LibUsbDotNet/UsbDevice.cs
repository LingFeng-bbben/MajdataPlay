using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using LibUsbDotNet.Descriptors;
using LibUsbDotNet.Info;
using LibUsbDotNet.Internal;
using LibUsbDotNet.Internal.LibUsb;
using LibUsbDotNet.LibUsb;
using LibUsbDotNet.Main;
using LibUsbDotNet.WinUsb;
using LibUsbDotNet.WinUsb.Internal;

namespace LibUsbDotNet
{
	public abstract class UsbDevice : IDisposable
	{
		public enum DriverModeType
		{
			Unknown,
			LibUsb,
			WinUsb,
			MonoLibUsb,
			LibUsbWinBack,
			LibusbK,
			Windows
		}

		private static LibUsbAPI _libUsbApi;

		private static WinUsbAPI _winUsbApi;

		private static LibusbKAPI _libusbKApi;

		private static object mHasWinUsbDriver;

		private static object mHasLibusbKDriver;

		private static object mHasLibUsbWinBackDriver;

		private static LibUsbKernelType mLibUsbKernelType;

		private static UsbKernelVersion mUsbKernelVersion;

		internal readonly UsbEndpointList mActiveEndpoints;

		internal readonly UsbApiBase mUsbApi;

		internal UsbDeviceDescriptor mCachedDeviceDescriptor;

		internal List<UsbConfigInfo> mConfigs;

		internal int mCurrentConfigValue = -1;

		internal UsbDeviceInfo mDeviceInfo;

		internal SafeHandle mUsbHandle;

		internal UsbRegistry mUsbRegistry;

		protected readonly byte[] UsbAltInterfaceSettings = new byte[256];

		protected readonly List<int> mClaimedInterfaces = new List<int>();

		public const bool ForceLegacyLibUsb = false;

		public static bool ForceLibUsbWinBack = false;

		public static UsbRegDeviceList AllDevices
		{
			get
			{
				UsbRegDeviceList usbRegDeviceList = new UsbRegDeviceList();
				foreach (UsbRegistry allWinUsbDevice in AllWinUsbDevices)
				{
					usbRegDeviceList.Add(allWinUsbDevice);
				}
				foreach (UsbRegistry allLibusbKDevice in AllLibusbKDevices)
				{
					usbRegDeviceList.Add(allLibusbKDevice);
				}
				foreach (UsbRegistry allLibUsbDevice in AllLibUsbDevices)
				{
					usbRegDeviceList.Add(allLibUsbDevice);
				}
				return usbRegDeviceList;
			}
		}

		public static UsbRegDeviceList AllLibUsbDevices
		{
			get
			{
				UsbRegDeviceList usbRegDeviceList = new UsbRegDeviceList();
				if (HasLibUsbWinBackDriver && ForceLibUsbWinBack)
				{
				}
				else if (!ForceLegacyLibUsb && KernelType == LibUsbKernelType.NativeLibUsb)
				{
					foreach (LibUsbRegistry device in LibUsbRegistry.DeviceList)
					{
						usbRegDeviceList.Add(device);
					}
				}
				else
				{
					foreach (LegacyUsbRegistry device2 in LegacyUsbRegistry.DeviceList)
					{
						usbRegDeviceList.Add(device2);
					}
				}
				return usbRegDeviceList;
			}
		}

		public static int LastErrorNumber => UsbError.mLastErrorNumber;

		public static string LastErrorString => UsbError.mLastErrorString;

		internal static LibUsbAPI LibUsbApi
		{
			get
			{
				if (_libUsbApi == null)
				{
					_libUsbApi = new LibUsbAPI();
				}
				return _libUsbApi;
			}
		}

		internal static WinUsbAPI WinUsbApi
		{
			get
			{
				if (_winUsbApi == null)
				{
					_winUsbApi = new WinUsbAPI();
				}
				return _winUsbApi;
			}
		}

		internal static LibusbKAPI LibusbKApi
		{
			get
			{
				if (_libusbKApi == null)
				{
					_libusbKApi = new LibusbKAPI();
				}
				return _libusbKApi;
			}
		}

		public virtual ReadOnlyCollection<UsbConfigInfo> Configs
		{
			get
			{
				if (mConfigs == null)
				{
					mConfigs = GetDeviceConfigs(this);
				}
				return mConfigs.AsReadOnly();
			}
		}

		public virtual UsbDeviceInfo Info
		{
			get
			{
				if (mDeviceInfo == null)
				{
					mDeviceInfo = new UsbDeviceInfo(this);
				}
				return mDeviceInfo;
			}
		}

		public virtual UsbRegistry UsbRegistryInfo => mUsbRegistry;

		public bool IsOpen
		{
			get
			{
				if (mUsbHandle != null && !mUsbHandle.IsClosed)
				{
					return !mUsbHandle.IsInvalid;
				}
				return false;
			}
		}

		public UsbEndpointList ActiveEndpoints => mActiveEndpoints;

		internal SafeHandle Handle => mUsbHandle;

		public abstract DriverModeType DriverMode { get; }

		public abstract string DevicePath { get; }

		public static UsbRegDeviceList AllWinUsbDevices
		{
			get
			{
				UsbRegDeviceList usbRegDeviceList = new UsbRegDeviceList();
				if (ForceLibUsbWinBack)
				{
					return usbRegDeviceList;
				}
				if (HasWinUsbDriver)
				{
					foreach (WinUsbRegistry device in WinUsbRegistry.DeviceList)
					{
						usbRegDeviceList.Add(device);
					}
				}
				return usbRegDeviceList;
			}
		}

		public static UsbRegDeviceList AllLibusbKDevices
		{
			get
			{
				UsbRegDeviceList usbRegDeviceList = new UsbRegDeviceList();
				if (ForceLibUsbWinBack)
				{
					return usbRegDeviceList;
				}
				if (HasLibusbKDriver)
				{
					foreach (LibusbKRegistry device in LibusbKRegistry.DeviceList)
					{
						usbRegDeviceList.Add(device);
					}
				}
				return usbRegDeviceList;
			}
		}

		[Obsolete("Always returns true")]
		public static bool HasLibUsbDriver => true;

		public static bool HasWinUsbDriver
		{
			get
			{
				if (mHasWinUsbDriver == null)
				{
					try
					{
						WinUsbAPI.WinUsb_Free(IntPtr.Zero);
						mHasWinUsbDriver = true;
					}
					catch (Exception)
					{
						mHasWinUsbDriver = false;
					}
				}
				return (bool)mHasWinUsbDriver;
			}
		}

		public static bool HasLibusbKDriver
		{
			get
			{
				if (mHasLibusbKDriver == null)
				{
					try
					{
						LibusbKAPI.UsbK_Free(IntPtr.Zero);
						mHasLibusbKDriver = true;
					}
					catch (Exception)
					{
						mHasLibusbKDriver = false;
					}
				}
				return (bool)mHasLibusbKDriver;
			}
		}

		public static bool HasLibUsbWinBackDriver
		{
			get
			{
				if (mHasLibUsbWinBackDriver == null)
				{
					try
					{
						mHasLibUsbWinBackDriver = true;
					}
					catch (Exception)
					{
						mHasLibUsbWinBackDriver = false;
					}
				}
				return (bool)mHasLibUsbWinBackDriver;
			}
		}

		public static LibUsbKernelType KernelType
		{
			get
			{
				if (mLibUsbKernelType == LibUsbKernelType.Unknown)
				{
					UsbKernelVersion kernelVersion = KernelVersion;
					if (!kernelVersion.IsEmpty)
					{
						mLibUsbKernelType = ((kernelVersion.BcdLibUsbDotNetKernelMod != 0) ? LibUsbKernelType.NativeLibUsb : LibUsbKernelType.LegacyLibUsb);
					}
				}
				return mLibUsbKernelType;
			}
		}

		public static UsbKernelVersion KernelVersion
		{
			get
			{
				if (mUsbKernelVersion.IsEmpty)
				{
					for (int i = 1; i < 256; i++)
					{
						if (LibUsbDevice.Open(LibUsbDriverIO.GetDeviceNameString(i), out var usbDevice))
						{
							LibUsbRequest libUsbRequest = new LibUsbRequest();
							GCHandle gCHandle = GCHandle.Alloc(libUsbRequest, GCHandleType.Pinned);
							int ret;
							bool num = usbDevice.UsbIoSync(LibUsbIoCtl.GET_VERSION, libUsbRequest, LibUsbRequest.Size, gCHandle.AddrOfPinnedObject(), LibUsbRequest.Size, out ret);
							gCHandle.Free();
							usbDevice.Close();
							if (num && ret == LibUsbRequest.Size)
							{
								mUsbKernelVersion = libUsbRequest.Version;
								break;
							}
						}
					}
				}
				return mUsbKernelVersion;
			}
		}

		public static OperatingSystem OSVersion => Helper.OSVersion;

		public static event EventHandler<UsbError> UsbErrorEvent;

		public static UsbDevice OpenUsbDevice(UsbDeviceFinder usbDeviceFinder)
		{
			return OpenUsbDevice(usbDeviceFinder.Check);
		}

		public static UsbDevice OpenUsbDevice(Predicate<UsbRegistry> findDevicePredicate)
		{
			return AllDevices.Find(findDevicePredicate)?.Device;
		}

		public static bool OpenUsbDevice(ref Guid devInterfaceGuid, out UsbDevice usbDevice)
		{
			usbDevice = null;
			foreach (UsbRegistry allDevice in AllDevices)
			{
				Guid[] deviceInterfaceGuids = allDevice.DeviceInterfaceGuids;
				for (int i = 0; i < deviceInterfaceGuids.Length; i++)
				{
					if (deviceInterfaceGuids[i] == devInterfaceGuid)
					{
						usbDevice = allDevice.Device;
						if (usbDevice != null)
						{
							return true;
						}
					}
				}
			}
			return false;
		}

		internal UsbDevice(UsbApiBase usbApi, SafeHandle usbHandle)
		{
			mUsbApi = usbApi;
			mUsbHandle = usbHandle;
			mActiveEndpoints = new UsbEndpointList();
		}

		public abstract bool Close();

		public abstract bool Open();

		public virtual bool ControlTransfer(ref UsbSetupPacket setupPacket, IntPtr buffer, int bufferLength, out int lengthTransferred)
		{
			bool num = mUsbApi.ControlTransfer(mUsbHandle, setupPacket, buffer, bufferLength, out lengthTransferred);
			if (!num)
			{
				UsbError.Error(ErrorCode.Win32Error, Marshal.GetLastWin32Error(), "ControlTransfer", this);
			}
			return num;
		}

		public virtual bool ControlTransfer(ref UsbSetupPacket setupPacket, object buffer, int bufferLength, out int lengthTransferred)
		{
			PinnedHandle pinnedHandle = new PinnedHandle(buffer);
			bool result = ControlTransfer(ref setupPacket, pinnedHandle.Handle, bufferLength, out lengthTransferred);
			pinnedHandle.Dispose();
			return result;
		}

		public virtual bool GetConfiguration(out byte config)
		{
			config = 0;
			byte[] array = new byte[1];
			UsbSetupPacket setupPacket = new UsbSetupPacket
			{
				RequestType = 128,
				Request = 8,
				Value = 0,
				Index = 0,
				Length = 1
			};
			if (ControlTransfer(ref setupPacket, array, array.Length, out var lengthTransferred) && lengthTransferred == 1)
			{
				config = array[0];
				mCurrentConfigValue = config;
				return true;
			}
			UsbError.Error(ErrorCode.Win32Error, Marshal.GetLastWin32Error(), "GetConfiguration", this);
			return false;
		}

		public virtual bool GetDescriptor(byte descriptorType, byte index, short langId, IntPtr buffer, int bufferLength, out int transferLength)
		{
			transferLength = 0;
			bool isOpen = IsOpen;
			if (!isOpen)
			{
				Open();
			}
			if (!IsOpen)
			{
				return false;
			}
			bool descriptor = mUsbApi.GetDescriptor(mUsbHandle, descriptorType, index, (ushort)langId, buffer, bufferLength, out transferLength);
			if (!descriptor)
			{
				UsbError.Error(ErrorCode.Win32Error, Marshal.GetLastWin32Error(), "GetDescriptor", this);
			}
			if (!isOpen && IsOpen)
			{
				Close();
			}
			return descriptor;
		}

		public virtual UsbEndpointWriter OpenEndpointWriter(WriteEndpointID writeEndpointID)
		{
			return OpenEndpointWriter(writeEndpointID, EndpointType.Bulk);
		}

		public virtual UsbEndpointWriter OpenEndpointWriter(WriteEndpointID writeEndpointID, EndpointType endpointType)
		{
			foreach (UsbEndpointBase activeEndpoint in ActiveEndpoints)
			{
				if (activeEndpoint.EpNum == (uint)writeEndpointID)
				{
					return (UsbEndpointWriter)activeEndpoint;
				}
			}
			byte alternateInterfaceID = ((mClaimedInterfaces.Count == 0) ? UsbAltInterfaceSettings[0] : UsbAltInterfaceSettings[mClaimedInterfaces[mClaimedInterfaces.Count - 1]]);
			UsbEndpointWriter item = new UsbEndpointWriter(this, alternateInterfaceID, writeEndpointID, endpointType);
			return (UsbEndpointWriter)mActiveEndpoints.Add(item);
		}

		internal static List<UsbConfigInfo> GetDeviceConfigs(UsbDevice usbDevice)
		{
			List<UsbConfigInfo> list = new List<UsbConfigInfo>();
			byte[] array = new byte[4096];
			int configurationCount = usbDevice.Info.Descriptor.ConfigurationCount;
			for (int i = 0; i < configurationCount; i++)
			{
				if (usbDevice.GetDescriptor(2, 0, 0, array, array.Length, out var transferLength))
				{
					if (transferLength >= UsbConfigDescriptor.Size && array[1] == 2)
					{
						UsbConfigDescriptor usbConfigDescriptor = new UsbConfigDescriptor();
						Helper.BytesToObject(array, 0, Math.Min(UsbConfigDescriptor.Size, array[0]), usbConfigDescriptor);
						if (usbConfigDescriptor.TotalLength == transferLength)
						{
							List<byte[]> rawDescriptors = new List<byte[]>();
							byte[] array2;
							for (int j = usbConfigDescriptor.Length; j < usbConfigDescriptor.TotalLength; j += array2.Length)
							{
								array2 = new byte[array[j]];
								if (j + array2.Length > transferLength)
								{
									throw new UsbException(usbDevice, "Descriptor length is out of range.");
								}
								Array.Copy(array, j, array2, 0, array2.Length);
								rawDescriptors.Add(array2);
							}
							list.Add(new UsbConfigInfo(usbDevice, usbConfigDescriptor, ref rawDescriptors));
						}
						else
						{
							UsbError.Error(ErrorCode.InvalidConfig, 0, "GetDeviceConfigs: USB config descriptor length doesn't match the length received.", usbDevice);
						}
					}
					else
					{
						UsbError.Error(ErrorCode.InvalidConfig, 0, "GetDeviceConfigs: USB config descriptor is invalid.", usbDevice);
					}
				}
				else
				{
					UsbError.Error(ErrorCode.InvalidConfig, 0, "GetDeviceConfigs", usbDevice);
				}
			}
			return list;
		}

		public bool GetDescriptor(byte descriptorType, byte index, short langId, object buffer, int bufferLength, out int transferLength)
		{
			PinnedHandle pinnedHandle = new PinnedHandle(buffer);
			bool descriptor = GetDescriptor(descriptorType, index, langId, pinnedHandle.Handle, bufferLength, out transferLength);
			pinnedHandle.Dispose();
			return descriptor;
		}

		public bool GetLangIDs(out short[] langIDs)
		{
			LangStringDescriptor langStringDescriptor = new LangStringDescriptor(UsbDescriptor.Size + 32);
			int transferLength;
			bool flag = GetDescriptor(3, 0, 0, langStringDescriptor.Ptr, langStringDescriptor.MaxSize, out transferLength);
			if (flag && transferLength == langStringDescriptor.Length)
			{
				flag = langStringDescriptor.Get(out langIDs);
			}
			else
			{
				langIDs = new short[0];
				UsbError.Error(ErrorCode.Win32Error, Marshal.GetLastWin32Error(), "GetLangIDs", this);
			}
			langStringDescriptor.Free();
			return flag;
		}

		public bool GetString(out string stringData, short langId, byte stringIndex)
		{
			stringData = null;
			LangStringDescriptor langStringDescriptor = new LangStringDescriptor(255);
			int transferLength;
			bool flag = GetDescriptor(3, stringIndex, langId, langStringDescriptor.Ptr, langStringDescriptor.MaxSize, out transferLength);
			if (flag && transferLength > UsbDescriptor.Size && langStringDescriptor.Length == transferLength)
			{
				flag = langStringDescriptor.Get(out stringData);
			}
			else if (!flag)
			{
				UsbError.Error(ErrorCode.Win32Error, Marshal.GetLastWin32Error(), "GetString:GetDescriptor", this);
			}
			else
			{
				stringData = string.Empty;
			}
			return flag;
		}

		public UsbEndpointReader OpenEndpointReader(ReadEndpointID readEndpointID)
		{
			return OpenEndpointReader(readEndpointID, UsbEndpointReader.DefReadBufferSize);
		}

		public UsbEndpointReader OpenEndpointReader(ReadEndpointID readEndpointID, int readBufferSize)
		{
			return OpenEndpointReader(readEndpointID, readBufferSize, EndpointType.Bulk);
		}

		public virtual UsbEndpointReader OpenEndpointReader(ReadEndpointID readEndpointID, int readBufferSize, EndpointType endpointType)
		{
			foreach (UsbEndpointBase mActiveEndpoint in mActiveEndpoints)
			{
				if (mActiveEndpoint.EpNum == (uint)readEndpointID)
				{
					return (UsbEndpointReader)mActiveEndpoint;
				}
			}
			byte alternateInterfaceID = ((mClaimedInterfaces.Count == 0) ? UsbAltInterfaceSettings[0] : UsbAltInterfaceSettings[mClaimedInterfaces[mClaimedInterfaces.Count - 1]]);
			UsbEndpointReader item = new UsbEndpointReader(this, readBufferSize, alternateInterfaceID, readEndpointID, endpointType);
			return (UsbEndpointReader)mActiveEndpoints.Add(item);
		}

		public bool GetAltInterfaceSetting(byte interfaceID, out byte selectedAltInterfaceID)
		{
			byte[] array = new byte[1];
			UsbSetupPacket setupPacket = new UsbSetupPacket
			{
				RequestType = 129,
				Request = 10,
				Value = 0,
				Index = interfaceID,
				Length = 1
			};
			int lengthTransferred;
			bool num = ControlTransfer(ref setupPacket, array, array.Length, out lengthTransferred);
			if (num && lengthTransferred == 1)
			{
				selectedAltInterfaceID = array[0];
				return num;
			}
			selectedAltInterfaceID = 0;
			return num;
		}

		public static void Exit()
		{
		}

		void IDisposable.Dispose()
		{
			Close();
		}

		internal static void FireUsbError(object sender, UsbError usbError)
		{
			UsbErrorEvent?.Invoke(sender, usbError);
		}
	}
}
