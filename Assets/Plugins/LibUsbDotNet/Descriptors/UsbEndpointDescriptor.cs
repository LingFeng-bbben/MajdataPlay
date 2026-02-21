using System.Runtime.InteropServices;
using LibUsbDotNet.Main;

namespace LibUsbDotNet.Descriptors
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public class UsbEndpointDescriptor : UsbDescriptor
	{
		public new static readonly int Size = Marshal.SizeOf(typeof(UsbEndpointDescriptor));

		public readonly byte EndpointID;

		public readonly byte Attributes;

		public readonly short MaxPacketSize;

		public readonly byte Interval;

		public readonly byte Refresh;

		public readonly byte SynchAddress;

		internal UsbEndpointDescriptor()
		{
		}

		public override string ToString()
		{
			return ToString("", UsbDescriptor.ToStringParamValueSeperator, UsbDescriptor.ToStringFieldSeperator);
		}

		public string ToString(string prefixSeperator, string entitySperator, string suffixSeperator)
		{
			object[] array = new object[8]
			{
				Length,
				DescriptorType,
				"0x" + EndpointID.ToString("X2"),
				"0x" + Attributes.ToString("X2"),
				MaxPacketSize,
				Interval,
				Refresh,
				"0x" + SynchAddress.ToString("X2")
			};
			string[] array2 = new string[8] { "Length", "DescriptorType", "EndpointID", "Attributes", "MaxPacketSize", "Interval", "Refresh", "SynchAddress" };
			return Helper.ToString(prefixSeperator, array2, entitySperator, array, suffixSeperator);
		}
	}
}
