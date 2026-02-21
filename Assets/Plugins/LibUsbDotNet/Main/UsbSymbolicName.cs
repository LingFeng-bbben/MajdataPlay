using System;
using System.Globalization;
using System.Text.RegularExpressions;
using LibUsbDotNet.Internal.UsbRegex;

namespace LibUsbDotNet.Main
{
	public class UsbSymbolicName
	{
		private static RegHardwareID _regHardwareId;

		private static RegSymbolicName _regSymbolicName;

		private readonly string mSymbolicName;

		private Guid mClassGuid = Guid.Empty;

		private bool mIsParsed;

		private int mProductID;

		private int mRevisionCode;

		private string mSerialNumber = string.Empty;

		private int mVendorID;

		private static RegSymbolicName RegSymbolicName
		{
			get
			{
				if (_regSymbolicName == null)
				{
					_regSymbolicName = new RegSymbolicName();
				}
				return _regSymbolicName;
			}
		}

		private static RegHardwareID RegHardwareId
		{
			get
			{
				if (_regHardwareId == null)
				{
					_regHardwareId = new RegHardwareID();
				}
				return _regHardwareId;
			}
		}

		public string FullName
		{
			get
			{
				if (mSymbolicName != null)
				{
					return mSymbolicName.TrimStart('\\', '?');
				}
				return string.Empty;
			}
		}

		public int Vid
		{
			get
			{
				Parse();
				return mVendorID;
			}
		}

		public int Pid
		{
			get
			{
				Parse();
				return mProductID;
			}
		}

		public string SerialNumber
		{
			get
			{
				Parse();
				return mSerialNumber;
			}
		}

		public Guid ClassGuid
		{
			get
			{
				Parse();
				return mClassGuid;
			}
		}

		public int Rev
		{
			get
			{
				Parse();
				return mRevisionCode;
			}
		}

		internal UsbSymbolicName(string symbolicName)
		{
			mSymbolicName = symbolicName;
		}

		public static UsbSymbolicName Parse(string identifiers)
		{
			return new UsbSymbolicName(identifiers);
		}

		public override string ToString()
		{
			object[] args = new object[5]
			{
				FullName,
				Vid.ToString("X4"),
				Pid.ToString("X4"),
				SerialNumber,
				ClassGuid
			};
			return string.Format("FullName:{0}\r\nVid:0x{1}\r\nPid:0x{2}\r\nSerialNumber:{3}\r\nClassGuid:{4}\r\n", args);
		}

		private void Parse()
		{
			if (mIsParsed)
			{
				return;
			}
			mIsParsed = true;
			if (mSymbolicName == null)
			{
				return;
			}
			foreach (Match item in RegSymbolicName.Matches(mSymbolicName))
			{
				Group obj2 = item.Groups[1];
				Group obj3 = item.Groups[2];
				Group obj4 = item.Groups[3];
				Group obj5 = item.Groups[5];
				Group obj6 = item.Groups[4];
				if (obj2.Success && mVendorID == 0)
				{
					int.TryParse(obj2.Captures[0].Value, NumberStyles.HexNumber, null, out mVendorID);
				}
				if (obj3.Success && mProductID == 0)
				{
					int.TryParse(obj3.Captures[0].Value, NumberStyles.HexNumber, null, out mProductID);
				}
				if (obj4.Success && mRevisionCode == 0)
				{
					int.TryParse(obj4.Captures[0].Value, out mRevisionCode);
				}
				if (obj5.Success && mSerialNumber == string.Empty)
				{
					mSerialNumber = obj5.Captures[0].Value;
				}
				if (obj6.Success && mClassGuid == Guid.Empty)
				{
					try
					{
						mClassGuid = new Guid(obj6.Captures[0].Value);
					}
					catch (Exception)
					{
						mClassGuid = Guid.Empty;
					}
				}
			}
		}
	}
}
