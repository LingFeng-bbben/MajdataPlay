using System.Runtime.InteropServices;
using System.Text;

namespace LibUsbDotNet.Descriptors
{
	internal class LangStringDescriptor : UsbMemChunk
	{
		private static readonly int OfsDescriptorType = Marshal.OffsetOf(typeof(UsbDescriptor), "DescriptorType").ToInt32();

		private static readonly int OfsLength = Marshal.OffsetOf(typeof(UsbDescriptor), "Length").ToInt32();

		public DescriptorType DescriptorType
		{
			get
			{
				return (DescriptorType)Marshal.ReadByte(base.Ptr, OfsDescriptorType);
			}
			set
			{
				Marshal.WriteByte(base.Ptr, OfsDescriptorType, (byte)value);
			}
		}

		public byte Length
		{
			get
			{
				return Marshal.ReadByte(base.Ptr, OfsLength);
			}
			set
			{
				Marshal.WriteByte(base.Ptr, OfsLength, value);
			}
		}

		public LangStringDescriptor(int maxSize)
			: base(maxSize)
		{
		}

		public bool Get(out short[] langIds)
		{
			langIds = new short[0];
			int length = Length;
			if (length <= 2)
			{
				return false;
			}
			int num = (length - 2) / 2;
			langIds = new short[num];
			int size = UsbDescriptor.Size;
			for (int i = 0; i < langIds.Length; i++)
			{
				langIds[i] = Marshal.ReadInt16(base.Ptr, size + 2 * i);
			}
			return true;
		}

		public bool Get(out byte[] bytes)
		{
			bytes = new byte[Length];
			Marshal.Copy(base.Ptr, bytes, 0, bytes.Length);
			return true;
		}

		public bool Get(out string str)
		{
			str = string.Empty;
			if (Get(out byte[] bytes))
			{
				if (bytes.Length <= UsbDescriptor.Size)
				{
					str = string.Empty;
				}
				else
				{
					str = Encoding.Unicode.GetString(bytes, UsbDescriptor.Size, bytes.Length - UsbDescriptor.Size);
				}
				return true;
			}
			return false;
		}
	}
}
