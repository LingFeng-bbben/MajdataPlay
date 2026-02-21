using System;
using System.Text;
using LibUsbDotNet.Internal.LibUsb;
using LibUsbDotNet.Main;

namespace LibUsbDotNet.LibUsb
{
	internal static class LibUsbDeviceRegistryKeyRequest
	{
		private enum KeyType
		{
			RegSz = 1,
			RegBinary = 3
		}

		public static byte[] RegGetRequest(string name, int valueBufferSize)
		{
			if (valueBufferSize < 1 || name.Trim().Length == 0)
			{
				throw new UsbException("Global", "Invalid DeviceRegistry het parameter.");
			}
			LibUsbRequest libUsbRequest = new LibUsbRequest();
			int size = LibUsbRequest.Size;
			libUsbRequest.DeviceRegKey.KeyType = 3;
			byte[] bytes = Encoding.Unicode.GetBytes(name + "\0");
			libUsbRequest.DeviceRegKey.NameOffset = size;
			size += bytes.Length;
			libUsbRequest.DeviceRegKey.ValueOffset = size;
			libUsbRequest.DeviceRegKey.ValueLength = valueBufferSize;
			size += Math.Max(size + 1, valueBufferSize - (LibUsbRequest.Size + bytes.Length));
			byte[] array = new byte[size];
			byte[] bytes2 = libUsbRequest.Bytes;
			Array.Copy(bytes2, array, bytes2.Length);
			Array.Copy(bytes, 0, array, libUsbRequest.DeviceRegKey.NameOffset, bytes.Length);
			return array;
		}

		public static byte[] RegSetBinaryRequest(string name, byte[] value)
		{
			LibUsbRequest libUsbRequest = new LibUsbRequest();
			int size = LibUsbRequest.Size;
			libUsbRequest.DeviceRegKey.KeyType = 3;
			byte[] bytes = Encoding.Unicode.GetBytes(name + "\0");
			libUsbRequest.DeviceRegKey.NameOffset = size;
			size += bytes.Length;
			libUsbRequest.DeviceRegKey.ValueOffset = size;
			libUsbRequest.DeviceRegKey.ValueLength = value.Length;
			size += value.Length;
			byte[] array = new byte[size];
			byte[] bytes2 = libUsbRequest.Bytes;
			Array.Copy(bytes2, array, bytes2.Length);
			Array.Copy(bytes, 0, array, libUsbRequest.DeviceRegKey.NameOffset, bytes.Length);
			Array.Copy(value, 0, array, libUsbRequest.DeviceRegKey.ValueOffset, value.Length);
			return array;
		}

		public static byte[] RegSetStringRequest(string name, string value)
		{
			LibUsbRequest libUsbRequest = new LibUsbRequest();
			int size = LibUsbRequest.Size;
			libUsbRequest.DeviceRegKey.KeyType = 1;
			byte[] bytes = Encoding.Unicode.GetBytes(name + "\0");
			byte[] bytes2 = Encoding.Unicode.GetBytes(value + "\0");
			libUsbRequest.DeviceRegKey.NameOffset = size;
			size += bytes.Length;
			libUsbRequest.DeviceRegKey.ValueOffset = size;
			libUsbRequest.DeviceRegKey.ValueLength = bytes2.Length;
			size += bytes2.Length;
			byte[] array = new byte[size];
			byte[] bytes3 = libUsbRequest.Bytes;
			Array.Copy(bytes3, array, bytes3.Length);
			Array.Copy(bytes, 0, array, libUsbRequest.DeviceRegKey.NameOffset, bytes.Length);
			Array.Copy(bytes2, 0, array, libUsbRequest.DeviceRegKey.ValueOffset, bytes2.Length);
			return array;
		}
	}
}
