using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace LibUsbDotNet.Main
{
	public static class Helper
	{
		[StructLayout(LayoutKind.Explicit, Pack = 1)]
		internal struct HostEndian16BitValue
		{
			[FieldOffset(0)]
			public readonly ushort U16;

			[FieldOffset(0)]
			public readonly byte B0;

			[FieldOffset(1)]
			public readonly byte B1;

			public HostEndian16BitValue(short x)
			{
				U16 = 0;
				B1 = (byte)(x >> 8);
				B0 = (byte)(x & 0xFF);
			}
		}

		private static OperatingSystem mOs;

		public static OperatingSystem OSVersion
		{
			get
			{
				if (mOs == null)
				{
					mOs = Environment.OSVersion;
				}
				return mOs;
			}
		}

		public static void BytesToObject(byte[] sourceBytes, int iStartIndex, int iLength, object destObject)
		{
			GCHandle gCHandle = GCHandle.Alloc(destObject, GCHandleType.Pinned);
			IntPtr destination = gCHandle.AddrOfPinnedObject();
			Marshal.Copy(sourceBytes, iStartIndex, destination, iLength);
			gCHandle.Free();
		}

		public static Dictionary<string, int> GetEnumData(Type type)
		{
			Dictionary<string, int> dictionary = new Dictionary<string, int>();
			FieldInfo[] fields = type.GetFields();
			for (int i = 1; i < fields.Length; i++)
			{
				object rawConstantValue = fields[i].GetRawConstantValue();
				dictionary.Add(fields[i].Name, Convert.ToInt32(rawConstantValue));
			}
			return dictionary;
		}

		public static short HostEndianToLE16(short swapValue)
		{
			return (short)new HostEndian16BitValue(swapValue).U16;
		}

		public static string ShowAsHex(object standardValue)
		{
			if (standardValue == null)
			{
				return "";
			}
			if (standardValue is long)
			{
				return "0x" + ((long)standardValue).ToString("X16");
			}
			if (standardValue is uint)
			{
				return "0x" + ((uint)standardValue).ToString("X8");
			}
			if (standardValue is int)
			{
				return "0x" + ((int)standardValue).ToString("X8");
			}
			if (standardValue is ushort)
			{
				return "0x" + ((ushort)standardValue).ToString("X4");
			}
			if (standardValue is short)
			{
				return "0x" + ((short)standardValue).ToString("X4");
			}
			if (standardValue is byte)
			{
				return "0x" + ((byte)standardValue).ToString("X2");
			}
			if (standardValue is string)
			{
				return HexString(standardValue as byte[], "", " ");
			}
			return "";
		}

		public static string ToString(string sep0, string[] names, string sep1, object[] values, string sep2)
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < names.Length; i++)
			{
				stringBuilder.Append(sep0 + names[i] + sep1 + values[i]?.ToString() + sep2);
			}
			return stringBuilder.ToString();
		}

		public static string HexString(byte[] data, string prefix, string suffix)
		{
			if (prefix == null)
			{
				prefix = string.Empty;
			}
			if (suffix == null)
			{
				suffix = string.Empty;
			}
			StringBuilder stringBuilder = new StringBuilder(data.Length * 2 + data.Length * prefix.Length + data.Length * suffix.Length);
			foreach (byte b in data)
			{
				stringBuilder.Append(prefix + b.ToString("X2") + suffix);
			}
			return stringBuilder.ToString();
		}
	}
}
