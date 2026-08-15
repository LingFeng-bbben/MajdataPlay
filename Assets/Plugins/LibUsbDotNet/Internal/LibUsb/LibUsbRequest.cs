using System.Runtime.InteropServices;
using LibUsbDotNet.Main;
#pragma warning disable CS0618 // Legacy marshaling helper is retained for driver request compatibility.

namespace LibUsbDotNet.Internal.LibUsb
{
	[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 24)]
	internal class LibUsbRequest
	{
		public static int Size = Marshal.SizeOf(typeof(LibUsbRequest));

		[FieldOffset(0)]
		public int Timeout = 1000;

		[FieldOffset(4)]
		public Control Control;

		[FieldOffset(4)]
		public Config Config;

		[FieldOffset(4)]
		public Debug Debug;

		[FieldOffset(4)]
		public Descriptor Descriptor;

		[FieldOffset(4)]
		public Endpoint Endpoint;

		[FieldOffset(4)]
		public Feature Feature;

		[FieldOffset(4)]
		public Iface Iface;

		[FieldOffset(4)]
		public Status Status;

		[FieldOffset(4)]
		public Vendor Vendor;

		[FieldOffset(4)]
		public UsbKernelVersion Version;

		[FieldOffset(4)]
		public DeviceProperty DeviceProperty;

		[FieldOffset(4)]
		public DeviceRegKey DeviceRegKey;

		[FieldOffset(4)]
		public BusQueryID BusQueryID;

		public byte[] Bytes
		{
			get
			{
				byte[] array = new byte[Size];
				for (int i = 0; i < Size; i++)
				{
					array[i] = Marshal.ReadByte(this, i);
				}
				return array;
			}
		}

		public void RequestConfigDescriptor(int index)
		{
			Timeout = 1000;
			int num = 512 + index;
			Descriptor.Recipient = 0;
			Descriptor.Type = (num >> 8) & 0xFF;
			Descriptor.Index = num & 0xFF;
			Descriptor.LangID = 0;
		}

		public void RequestStringDescriptor(int index, short langid)
		{
			Timeout = 1000;
			int num = 768 + index;
			Descriptor.Recipient = 0;
			Descriptor.Type = (num >> 8) & 0xFF;
			Descriptor.Index = num & 0xFF;
			Descriptor.LangID = langid;
		}
	}
}
