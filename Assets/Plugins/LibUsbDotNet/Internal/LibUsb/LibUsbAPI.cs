using System;
using System.Runtime.InteropServices;
using LibUsbDotNet.Main;

namespace LibUsbDotNet.Internal.LibUsb
{
	internal class LibUsbAPI : UsbApiBase
	{
		public override bool AbortPipe(SafeHandle interfaceHandle, byte pipeID)
		{
			LibUsbRequest libUsbRequest = new LibUsbRequest();
			libUsbRequest.Endpoint.ID = pipeID;
			libUsbRequest.Timeout = 1000;
			int ret;
			return LibUsbDriverIO.UsbIOSync(interfaceHandle, LibUsbIoCtl.ABORT_ENDPOINT, libUsbRequest, LibUsbRequest.Size, IntPtr.Zero, 0, out ret);
		}

		public bool ResetDevice(SafeHandle interfaceHandle)
		{
			LibUsbRequest libUsbRequest = new LibUsbRequest();
			libUsbRequest.Timeout = 1000;
			int ret;
			return LibUsbDriverIO.UsbIOSync(interfaceHandle, LibUsbIoCtl.RESET_DEVICE, libUsbRequest, LibUsbRequest.Size, IntPtr.Zero, 0, out ret);
		}

		public override bool ControlTransfer(SafeHandle interfaceHandle, UsbSetupPacket setupPacket, IntPtr buffer, int bufferLength, out int lengthTransferred)
		{
			return LibUsbDriverIO.ControlTransfer(interfaceHandle, setupPacket, buffer, bufferLength, out lengthTransferred, 1000);
		}

		public override bool FlushPipe(SafeHandle interfaceHandle, byte pipeID)
		{
			return true;
		}

		public override bool GetDescriptor(SafeHandle interfaceHandle, byte descriptorType, byte index, ushort languageID, IntPtr buffer, int bufferLength, out int lengthTransferred)
		{
			LibUsbRequest libUsbRequest = new LibUsbRequest();
			libUsbRequest.Descriptor.Index = index;
			libUsbRequest.Descriptor.LangID = languageID;
			libUsbRequest.Descriptor.Recipient = 0;
			libUsbRequest.Descriptor.Type = descriptorType;
			return LibUsbDriverIO.UsbIOSync(interfaceHandle, LibUsbIoCtl.GET_DESCRIPTOR, libUsbRequest, LibUsbRequest.Size, buffer, bufferLength, out lengthTransferred);
		}

		public override bool GetOverlappedResult(SafeHandle interfaceHandle, IntPtr pOverlapped, out int numberOfBytesTransferred, bool wait)
		{
			return Kernel32.GetOverlappedResult(interfaceHandle, pOverlapped, out numberOfBytesTransferred, wait);
		}

		public override bool ReadPipe(UsbEndpointBase endPointBase, IntPtr buffer, int bufferLength, out int lengthTransferred, int isoPacketSize, IntPtr pOverlapped)
		{
			LibUsbRequest libUsbRequest = new LibUsbRequest();
			libUsbRequest.Endpoint.ID = endPointBase.EpNum;
			libUsbRequest.Endpoint.PacketSize = isoPacketSize;
			libUsbRequest.Timeout = 1000;
			int ioControlCode = ((endPointBase.Type == EndpointType.Isochronous) ? LibUsbIoCtl.ISOCHRONOUS_READ : LibUsbIoCtl.INTERRUPT_OR_BULK_READ);
			return Kernel32.DeviceIoControl(endPointBase.Device.Handle, ioControlCode, libUsbRequest, LibUsbRequest.Size, buffer, bufferLength, out lengthTransferred, pOverlapped);
		}

		public override bool ResetPipe(SafeHandle interfaceHandle, byte pipeID)
		{
			LibUsbRequest libUsbRequest = new LibUsbRequest();
			libUsbRequest.Endpoint.ID = pipeID;
			libUsbRequest.Timeout = 1000;
			int ret;
			return LibUsbDriverIO.UsbIOSync(interfaceHandle, LibUsbIoCtl.RESET_ENDPOINT, libUsbRequest, LibUsbRequest.Size, IntPtr.Zero, 0, out ret);
		}

		public override bool WritePipe(UsbEndpointBase endPointBase, IntPtr buffer, int bufferLength, out int lengthTransferred, int isoPacketSize, IntPtr pOverlapped)
		{
			LibUsbRequest libUsbRequest = new LibUsbRequest();
			libUsbRequest.Endpoint.ID = endPointBase.EpNum;
			libUsbRequest.Endpoint.PacketSize = isoPacketSize;
			libUsbRequest.Timeout = 1000;
			int ioControlCode = ((endPointBase.Type == EndpointType.Isochronous) ? LibUsbIoCtl.ISOCHRONOUS_WRITE : LibUsbIoCtl.INTERRUPT_OR_BULK_WRITE);
			return Kernel32.DeviceIoControl(endPointBase.Handle, ioControlCode, libUsbRequest, LibUsbRequest.Size, buffer, bufferLength, out lengthTransferred, pOverlapped);
		}
	}
}
