using System.Runtime.InteropServices;
using LibUsbDotNet.Main;

namespace LibUsbDotNet.Descriptors
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public abstract class UsbDescriptor
	{
		public static string ToStringParamValueSeperator = ":";

		public static string ToStringFieldSeperator = "\r\n";

		public static readonly int Size = Marshal.SizeOf(typeof(UsbDescriptor));

		public byte Length;

		public DescriptorType DescriptorType;

		public override string ToString()
		{
			object[] array = new object[2] { Length, DescriptorType };
			string[] array2 = new string[2] { "Length", "DescriptorType" };
			return Helper.ToString("", array2, ToStringParamValueSeperator, array, ToStringFieldSeperator);
		}
	}
}
