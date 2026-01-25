using System;
using System.Runtime.InteropServices;
using LibUsbDotNet.Internal;
using LibUsbDotNet.Main;

namespace LibUsbDotNet
{
	public class UsbEndpointWriter : UsbEndpointBase
	{
		internal UsbEndpointWriter(UsbDevice usbDevice, byte alternateInterfaceID, WriteEndpointID writeEndpointID, EndpointType endpointType)
			: base(usbDevice, alternateInterfaceID, (byte)writeEndpointID, endpointType)
		{
		}

		public virtual ErrorCode Write(byte[] buffer, int timeout, out int transferLength)
		{
			return Write(buffer, 0, buffer.Length, timeout, out transferLength);
		}

		public virtual ErrorCode Write(IntPtr pBuffer, int offset, int count, int timeout, out int transferLength)
		{
			return Transfer(pBuffer, offset, count, timeout, out transferLength);
		}

		public virtual ErrorCode Write(byte[] buffer, int offset, int count, int timeout, out int transferLength)
		{
			return Transfer(buffer, offset, count, timeout, out transferLength);
		}

		public virtual ErrorCode Write(object buffer, int offset, int count, int timeout, out int transferLength)
		{
			return Transfer(buffer, offset, count, timeout, out transferLength);
		}

		public virtual ErrorCode Write(object buffer, int timeout, out int transferLength)
		{
			return Write(buffer, 0, Marshal.SizeOf(buffer), timeout, out transferLength);
		}

		internal override UsbTransfer CreateTransferContext()
		{
			return new OverlappedTransferContext(this);
		}
	}
}
