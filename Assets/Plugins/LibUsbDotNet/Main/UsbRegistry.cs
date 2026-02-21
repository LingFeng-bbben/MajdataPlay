using System;
using System.Collections.Generic;
using System.Text;

namespace LibUsbDotNet.Main
{
	public abstract class UsbRegistry
	{
		internal const string DEVICE_INTERFACE_GUIDS = "DeviceInterfaceGuids";

		internal const string LIBUSB_INTERFACE_GUIDS = "LibUsbInterfaceGUIDs";

		internal const string LIBUSBK_DEVICE_CLASS_GUID = "{ecfb0cfd-74c4-4f52-bbf7-343461cd72ac}";

		internal const string SYMBOLIC_NAME_KEY = "SymbolicName";

		internal const string DEVICE_ID_KEY = "DeviceID";

		private static readonly char[] ChNull = new char[1];

		public static bool ForceSetupApi = true;

		internal Guid[] mDeviceInterfaceGuids = new Guid[0];

		internal Dictionary<string, object> mDeviceProperties = new Dictionary<string, object>();

		private UsbSymbolicName mSymHardwareId;

		public Dictionary<string, object> DeviceProperties => mDeviceProperties;

		public abstract bool IsAlive { get; }

		public string SymbolicName
		{
			get
			{
				if (mDeviceProperties.ContainsKey("SymbolicName"))
				{
					return (string)mDeviceProperties["SymbolicName"];
				}
				return null;
			}
		}

		public abstract string DevicePath { get; }

		public abstract Guid[] DeviceInterfaceGuids { get; }

		public virtual int Vid
		{
			get
			{
				if (mSymHardwareId == null && mDeviceProperties[SPDRP.HardwareId.ToString()] is string[] array && array.Length != 0)
				{
					mSymHardwareId = UsbSymbolicName.Parse(array[0]);
				}
				if (mSymHardwareId != null)
				{
					return mSymHardwareId.Vid;
				}
				return 0;
			}
		}

		public virtual int Pid
		{
			get
			{
				if (mSymHardwareId == null && mDeviceProperties[SPDRP.HardwareId.ToString()] is string[] array && array.Length != 0)
				{
					mSymHardwareId = UsbSymbolicName.Parse(array[0]);
				}
				if (mSymHardwareId != null)
				{
					return mSymHardwareId.Pid;
				}
				return 0;
			}
		}

		public object this[string name]
		{
			get
			{
				mDeviceProperties.TryGetValue(name, out var value);
				return value;
			}
		}

		public object this[SPDRP spdrp]
		{
			get
			{
				mDeviceProperties.TryGetValue(spdrp.ToString(), out var value);
				return value;
			}
		}

		public object this[DevicePropertyType devicePropertyType]
		{
			get
			{
				mDeviceProperties.TryGetValue(devicePropertyType.ToString(), out var value);
				return value;
			}
		}

		public string Name
		{
			get
			{
				string text = this[SPDRP.DeviceDesc] as string;
				if (string.IsNullOrEmpty(text))
				{
					return string.Empty;
				}
				return text;
			}
		}

		public string FullName
		{
			get
			{
				string name = Name;
				string text = this[SPDRP.Mfg] as string;
				if (text == null)
				{
					text = string.Empty;
				}
				name = name.Trim();
				text = text.Trim();
				int num = text.IndexOf(' ');
				int num2 = name.IndexOf(' ');
				while (num == num2 && num2 != -1 && text.Substring(0, num).Equals(name.Substring(0, num2)))
				{
					name = name.Remove(0, num2 + 1);
					num = text.IndexOf(' ');
					num2 = name.IndexOf(' ');
				}
				if (name.ToLower().Contains(text.ToLower()))
				{
					return name;
				}
				if (text == string.Empty)
				{
					text = "[Not Provided]";
				}
				if (name == string.Empty)
				{
					name = "[Not Provided]";
				}
				return text + " - " + name;
			}
		}

		public int Count => mDeviceProperties.Count;

		public virtual int Rev
		{
			get
			{
				if (mSymHardwareId == null && mDeviceProperties[SPDRP.HardwareId.ToString()] is string[] array && array.Length != 0)
				{
					mSymHardwareId = UsbSymbolicName.Parse(array[0]);
				}
				if (mSymHardwareId != null)
				{
					return mSymHardwareId.Rev;
				}
				return 0;
			}
		}

		public abstract UsbDevice Device { get; }

		public abstract bool Open(out UsbDevice usbDevice);

		internal static Guid GetAsGuid(byte[] buffer, int len)
		{
			Guid result = Guid.Empty;
			if (len == 16)
			{
				byte[] array = new byte[len];
				Array.Copy(buffer, array, array.Length);
				result = new Guid(array);
			}
			return result;
		}

		internal static string GetAsString(byte[] buffer, int len)
		{
			if (len > 2)
			{
				return Encoding.Unicode.GetString(buffer, 0, len).TrimEnd(ChNull);
			}
			return "";
		}

		internal static string[] GetAsStringArray(byte[] buffer, int len)
		{
			return GetAsString(buffer, len).Split(new char[1], StringSplitOptions.RemoveEmptyEntries);
		}

		internal static int GetAsStringInt32(byte[] buffer, int len)
		{
			int result = 0;
			if (len == 4)
			{
				result = buffer[0] | (buffer[1] << 8) | (buffer[2] << 16) | (buffer[3] << 24);
			}
			return result;
		}
	}
}
