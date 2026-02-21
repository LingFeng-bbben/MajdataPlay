using System;
using System.Runtime.InteropServices;
using System.Threading;
using LibUsbDotNet.Main;
using Microsoft.Win32.SafeHandles;

namespace LibUsbDotNet.Internal.LibUsb
{
	internal class LibUsbDriverIO
	{
		public const int ERROR_IO_PENDING = 997;

		public const int FALSE = 0;

		public const int FILE_FLAG_OVERLAPPED = 1073741824;

		internal const string LIBUSB_DEVICE_NAME = "\\\\.\\libusb0-";

		public const int TRUE = 1;

		private static byte[] _tempCfgBuf;

		public const int EINVAL = 22;

		internal static byte[] GlobalTempCfgBuffer
		{
			get
			{
				if (_tempCfgBuf == null)
				{
					_tempCfgBuf = new byte[4096];
				}
				return _tempCfgBuf;
			}
		}

		internal static string GetDeviceNameString(int index)
		{
			return string.Format("{0}{1}", "\\\\.\\libusb0-", index.ToString("0000"));
		}

		internal static SafeFileHandle OpenDevice(string deviceFileName)
		{
			return Kernel32.CreateFile(deviceFileName, NativeFileAccess.FILE_SPECIAL, NativeFileShare.NONE, IntPtr.Zero, NativeFileMode.OPEN_EXISTING, NativeFileFlag.FILE_FLAG_OVERLAPPED, IntPtr.Zero);
		}

		internal static bool UsbIOSync(SafeHandle dev, int code, object inBuffer, int inSize, IntPtr outBuffer, int outSize, out int ret)
		{
			SafeOverlapped safeOverlapped = new SafeOverlapped();
			ManualResetEvent manualResetEvent = new ManualResetEvent(initialState: false);
			safeOverlapped.Init(manualResetEvent.GetSafeWaitHandle().DangerousGetHandle());
			ret = 0;
			if (!Kernel32.DeviceIoControlAsObject(dev, code, inBuffer, inSize, outBuffer, outSize, ref ret, safeOverlapped.GlobalOverlapped))
			{
				int lastWin32Error = Marshal.GetLastWin32Error();
				if (lastWin32Error != 997)
				{
					if (code != LibUsbIoCtl.GET_REG_PROPERTY && code != LibUsbIoCtl.GET_CUSTOM_REG_PROPERTY)
					{
						UsbError.Error(ErrorCode.Win32Error, lastWin32Error, $"DeviceIoControl code {code:X8} failed:{Kernel32.FormatSystemMessage(lastWin32Error)}", typeof(LibUsbDriverIO));
					}
					manualResetEvent.Close();
					return false;
				}
			}
			if (Kernel32.GetOverlappedResult(dev, safeOverlapped.GlobalOverlapped, out ret, bWait: true))
			{
				manualResetEvent.Close();
				return true;
			}
			UsbError.Error(ErrorCode.Win32Error, Marshal.GetLastWin32Error(), "GetOverlappedResult failed.\nIoCtlCode:" + code, typeof(LibUsbDriverIO));
			manualResetEvent.Close();
			return false;
		}

		internal static bool ControlTransferEx(SafeHandle interfaceHandle, UsbSetupPacket setupPacket, IntPtr buffer, int bufferLength, out int lengthTransferred, int timeout)
		{
			lengthTransferred = 0;
			byte[] bytes = new LibUsbRequest
			{
				Timeout = timeout,
				Control = 
				{
					RequestType = setupPacket.RequestType,
					Request = setupPacket.Request,
					Value = (ushort)setupPacket.Value,
					Index = (ushort)setupPacket.Index,
					Length = (ushort)setupPacket.Length
				}
			}.Bytes;
			byte[] array = bytes;
			if ((setupPacket.RequestType & 0x80) == 0)
			{
				array = new byte[LibUsbRequest.Size + bufferLength];
				bytes.CopyTo(array, 0);
				if (buffer != IntPtr.Zero)
				{
					Marshal.Copy(buffer, array, LibUsbRequest.Size, bufferLength);
				}
				buffer = IntPtr.Zero;
				bufferLength = 0;
			}
			if (UsbIOSync(interfaceHandle, LibUsbIoCtl.CONTROL_TRANSFER, array, array.Length, buffer, bufferLength, out lengthTransferred))
			{
				if ((setupPacket.RequestType & 0x80) == 0)
				{
					lengthTransferred = array.Length - bytes.Length;
				}
				return true;
			}
			return false;
		}

		internal static bool ControlTransfer(SafeHandle interfaceHandle, UsbSetupPacket setupPacket, IntPtr buffer, int bufferLength, out int lengthTransferred, int timeout)
		{
			lengthTransferred = 0;
			LibUsbRequest libUsbRequest = new LibUsbRequest();
			libUsbRequest.Timeout = timeout;
			int code;
			switch ((UsbRequestType)(byte)(setupPacket.RequestType & 0x60))
			{
				case UsbRequestType.TypeStandard:
					switch ((UsbStandardRequest)setupPacket.Request)
					{
						case UsbStandardRequest.GetStatus:
							libUsbRequest.Status.Recipient = setupPacket.RequestType & 0x1F;
							libUsbRequest.Status.Index = setupPacket.Index;
							code = LibUsbIoCtl.GET_STATUS;
							break;
						case UsbStandardRequest.ClearFeature:
							libUsbRequest.Feature.Recipient = setupPacket.RequestType & 0x1F;
							libUsbRequest.Feature.ID = setupPacket.Value;
							libUsbRequest.Feature.Index = setupPacket.Index;
							code = LibUsbIoCtl.CLEAR_FEATURE;
							break;
						case UsbStandardRequest.SetFeature:
							libUsbRequest.Feature.Recipient = setupPacket.RequestType & 0x1F;
							libUsbRequest.Feature.ID = setupPacket.Value;
							libUsbRequest.Feature.Index = setupPacket.Index;
							code = LibUsbIoCtl.SET_FEATURE;
							break;
						case UsbStandardRequest.GetDescriptor:
							libUsbRequest.Descriptor.Recipient = setupPacket.RequestType & 0x1F;
							libUsbRequest.Descriptor.Type = (setupPacket.Value >> 8) & 0xFF;
							libUsbRequest.Descriptor.Index = setupPacket.Value & 0xFF;
							libUsbRequest.Descriptor.LangID = setupPacket.Index;
							code = LibUsbIoCtl.GET_DESCRIPTOR;
							break;
						case UsbStandardRequest.SetDescriptor:
							libUsbRequest.Descriptor.Recipient = setupPacket.RequestType & 0x1F;
							libUsbRequest.Descriptor.Type = (setupPacket.Value >> 8) & 0xFF;
							libUsbRequest.Descriptor.Index = setupPacket.Value & 0xFF;
							libUsbRequest.Descriptor.LangID = setupPacket.Index;
							code = LibUsbIoCtl.SET_DESCRIPTOR;
							break;
						case UsbStandardRequest.GetConfiguration:
							code = LibUsbIoCtl.GET_CONFIGURATION;
							break;
						case UsbStandardRequest.SetConfiguration:
							libUsbRequest.Config.ID = setupPacket.Value;
							code = LibUsbIoCtl.SET_CONFIGURATION;
							break;
						case UsbStandardRequest.GetInterface:
							libUsbRequest.Iface.ID = setupPacket.Index;
							code = LibUsbIoCtl.GET_INTERFACE;
							break;
						case UsbStandardRequest.SetInterface:
							libUsbRequest.Iface.ID = setupPacket.Index;
							libUsbRequest.Iface.AlternateID = setupPacket.Value;
							code = LibUsbIoCtl.SET_INTERFACE;
							break;
						default:
							UsbError.Error(ErrorCode.IoControlMessage, 0, $"Invalid request: 0x{setupPacket.Request:X8}", typeof(LibUsbDriverIO));
							return false;
					}
					break;
				case UsbRequestType.TypeClass:
				case UsbRequestType.TypeVendor:
					libUsbRequest.Vendor.Type = (setupPacket.RequestType >> 5) & 3;
					libUsbRequest.Vendor.Recipient = setupPacket.RequestType & 0x1F;
					libUsbRequest.Vendor.Request = setupPacket.Request;
					libUsbRequest.Vendor.ID = setupPacket.Value;
					libUsbRequest.Vendor.Index = setupPacket.Index;
					code = (((setupPacket.RequestType & 0x80) > 0) ? LibUsbIoCtl.VENDOR_READ : LibUsbIoCtl.VENDOR_WRITE);
					break;
				default:
					UsbError.Error(ErrorCode.IoControlMessage, 0, $"invalid or unsupported request type: 0x{setupPacket.RequestType:X8}", typeof(LibUsbDriverIO));
					return false;
			}
			byte[] bytes = libUsbRequest.Bytes;
			byte[] array = bytes;
			if ((setupPacket.RequestType & 0x80) == 0)
			{
				array = new byte[LibUsbRequest.Size + bufferLength];
				bytes.CopyTo(array, 0);
				if (buffer != IntPtr.Zero)
				{
					Marshal.Copy(buffer, array, LibUsbRequest.Size, bufferLength);
				}
				buffer = IntPtr.Zero;
				bufferLength = 0;
			}
			if (UsbIOSync(interfaceHandle, code, array, array.Length, buffer, bufferLength, out lengthTransferred))
			{
				if ((setupPacket.RequestType & 0x80) == 0)
				{
					lengthTransferred = array.Length - bytes.Length;
				}
				return true;
			}
			return false;
		}
	}
}
