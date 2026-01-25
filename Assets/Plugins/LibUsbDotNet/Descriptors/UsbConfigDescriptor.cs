using System.Runtime.InteropServices;
using LibUsbDotNet.Main;

namespace LibUsbDotNet.Descriptors
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public class UsbConfigDescriptor : UsbDescriptor
	{
		public new static readonly int Size = Marshal.SizeOf(typeof(UsbConfigDescriptor));

		public readonly short TotalLength;

		public readonly byte InterfaceCount;

		public readonly byte ConfigID;

		public readonly byte StringIndex;

		public readonly byte Attributes;

		public readonly byte MaxPower;

		internal UsbConfigDescriptor()
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
				TotalLength,
				InterfaceCount,
				ConfigID,
				StringIndex,
				"0x" + Attributes.ToString("X2"),
				MaxPower
			};
			string[] array2 = new string[8] { "Length", "DescriptorType", "TotalLength", "InterfaceCount", "ConfigID", "StringIndex", "Attributes", "MaxPower" };
			return Helper.ToString(prefixSeperator, array2, entitySperator, array, suffixSeperator);
		}
	}
}
