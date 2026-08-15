using System.Runtime.InteropServices;
using LibUsbDotNet.Main;

namespace LibUsbDotNet.Descriptors
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public class UsbDeviceDescriptor : UsbDescriptor
	{
		public new static readonly int Size = Marshal.SizeOf(typeof(UsbDeviceDescriptor));

		public short BcdUsb;

		public ClassCodeType Class;

		public byte SubClass;

		public byte Protocol;

		public byte MaxPacketSize0;

		public short VendorID;

		public short ProductID;

		public short BcdDevice;

		public byte ManufacturerStringIndex;

		public byte ProductStringIndex;

		public byte SerialStringIndex;

		public byte ConfigurationCount;

		internal UsbDeviceDescriptor()
		{
		}

		public override string ToString()
		{
			return ToString("", UsbDescriptor.ToStringParamValueSeperator, UsbDescriptor.ToStringFieldSeperator);
		}

		public string ToString(string prefixSeperator, string entitySperator, string suffixSeperator)
		{
			object[] array = new object[14]
			{
				Length,
				DescriptorType,
				"0x" + BcdUsb.ToString("X4"),
				Class,
				"0x" + SubClass.ToString("X2"),
				"0x" + Protocol.ToString("X2"),
				MaxPacketSize0,
				"0x" + VendorID.ToString("X4"),
				"0x" + ProductID.ToString("X4"),
				"0x" + BcdDevice.ToString("X4"),
				ManufacturerStringIndex,
				ProductStringIndex,
				SerialStringIndex,
				ConfigurationCount
			};
			string[] array2 = new string[14]
			{
				"Length", "DescriptorType", "BcdUsb", "Class", "SubClass", "Protocol", "MaxPacketSize0", "VendorID", "ProductID", "BcdDevice",
				"ManufacturerStringIndex", "ProductStringIndex", "SerialStringIndex", "ConfigurationCount"
			};
			return Helper.ToString(prefixSeperator, array2, entitySperator, array, suffixSeperator);
		}

		public bool Equals(UsbDeviceDescriptor other)
		{
			if ((object)other == null)
			{
				return false;
			}
			if (ReferenceEquals(this, other))
			{
				return true;
			}
			if (other.BcdUsb == BcdUsb && other.Class == Class && other.SubClass == SubClass && other.Protocol == Protocol && other.MaxPacketSize0 == MaxPacketSize0 && other.VendorID == VendorID && other.ProductID == ProductID && other.BcdDevice == BcdDevice && other.ManufacturerStringIndex == ManufacturerStringIndex && other.ProductStringIndex == ProductStringIndex && other.SerialStringIndex == SerialStringIndex)
			{
				return other.ConfigurationCount == ConfigurationCount;
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}
			if (ReferenceEquals(this, obj))
			{
				return true;
			}
			if (obj.GetType() != typeof(UsbDeviceDescriptor))
			{
				return false;
			}
			return Equals((UsbDeviceDescriptor)obj);
		}

		public override int GetHashCode()
		{
			return (((((((((((((((((((((BcdUsb.GetHashCode() * 397) ^ Class.GetHashCode()) * 397) ^ SubClass.GetHashCode()) * 397) ^ Protocol.GetHashCode()) * 397) ^ MaxPacketSize0.GetHashCode()) * 397) ^ VendorID.GetHashCode()) * 397) ^ ProductID.GetHashCode()) * 397) ^ BcdDevice.GetHashCode()) * 397) ^ ManufacturerStringIndex.GetHashCode()) * 397) ^ ProductStringIndex.GetHashCode()) * 397) ^ SerialStringIndex.GetHashCode()) * 397) ^ ConfigurationCount.GetHashCode();
		}

		public static bool operator ==(UsbDeviceDescriptor left, UsbDeviceDescriptor right)
		{
			return object.Equals(left, right);
		}

		public static bool operator !=(UsbDeviceDescriptor left, UsbDeviceDescriptor right)
		{
			return !object.Equals(left, right);
		}
	}
}
