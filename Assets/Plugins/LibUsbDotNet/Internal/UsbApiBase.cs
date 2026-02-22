using System;
using System.Runtime.InteropServices;
using LibUsbDotNet.Main;

namespace LibUsbDotNet.Internal
{
	internal abstract class UsbApiBase
	{
		public abstract bool AbortPipe(SafeHandle interfaceHandle, byte pipeID);

		public abstract bool ControlTransfer(SafeHandle interfaceHandle, UsbSetupPacket setupPacket, IntPtr buffer, int bufferLength, out int lengthTransferred);

		public abstract bool FlushPipe(SafeHandle interfaceHandle, byte pipeID);

		public abstract bool GetDescriptor(SafeHandle interfaceHandle, byte descriptorType, byte index, ushort languageID, IntPtr buffer, int bufferLength, out int lengthTransferred);

		public abstract bool GetOverlappedResult(SafeHandle interfaceHandle, IntPtr pOverlapped, out int numberOfBytesTransferred, bool wait);

		public abstract bool ReadPipe(UsbEndpointBase endPointBase, IntPtr pBuffer, int bufferLength, out int lengthTransferred, int isoPacketSize, IntPtr pOverlapped);

		public abstract bool ResetPipe(SafeHandle interfaceHandle, byte pipeID);

		public abstract bool WritePipe(UsbEndpointBase endPointBase, IntPtr pBuffer, int bufferLength, out int lengthTransferred, int isoPacketSize, IntPtr pOverlapped);
	}
}
