using System.Runtime.InteropServices;
using LibUsbDotNet.Main;

namespace LibUsbDotNet.Descriptors
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public class UsbInterfaceDescriptor : UsbDescriptor
	{
		public new static readonly int Size = Marshal.SizeOf(typeof(UsbInterfaceDescriptor));

		public readonly byte InterfaceID;

		public readonly byte AlternateID;

		public readonly byte EndpointCount;

		public readonly ClassCodeType Class;

		public readonly byte SubClass;

		public readonly byte Protocol;

		public readonly byte StringIndex;

		public override string ToString()
		{
			return ToString("", UsbDescriptor.ToStringParamValueSeperator, UsbDescriptor.ToStringFieldSeperator);
		}

		public string ToString(string prefixSeperator, string entitySperator, string suffixSeperator)
		{
			object[] array = new object[9]
			{
				Length,
				DescriptorType,
				InterfaceID,
				AlternateID,
				EndpointCount,
				Class,
				"0x" + SubClass.ToString("X2"),
				"0x" + Protocol.ToString("X2"),
				StringIndex
			};
			string[] array2 = new string[9] { "Length", "DescriptorType", "InterfaceID", "AlternateID", "EndpointCount", "Class", "SubClass", "Protocol", "StringIndex" };
			return Helper.ToString(prefixSeperator, array2, entitySperator, array, suffixSeperator);
		}

		internal UsbInterfaceDescriptor()
		{
		}
	}
}
