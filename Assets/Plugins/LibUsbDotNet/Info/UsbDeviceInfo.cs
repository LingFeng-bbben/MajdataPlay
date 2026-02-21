using System.Globalization;
using System.Runtime.InteropServices;
using LibUsbDotNet.Descriptors;
using LibUsbDotNet.Main;

namespace LibUsbDotNet.Info
{
	public class UsbDeviceInfo
	{
		private const short NO_LANG = short.MaxValue;

		private readonly UsbDeviceDescriptor mDeviceDescriptor;

		private short mCurrentCultureLangID = short.MaxValue;

		private string mManufacturerString;

		private string mProductString;

		private string mSerialString;

		internal UsbDevice mUsbDevice;

		public UsbDeviceDescriptor Descriptor => mDeviceDescriptor;

		public short CurrentCultureLangID
		{
			get
			{
				if (mCurrentCultureLangID == short.MaxValue)
				{
					if (mUsbDevice.GetLangIDs(out var langIDs))
					{
						short num = (short)CultureInfo.CurrentCulture.LCID;
						short[] array = langIDs;
						foreach (short num2 in array)
						{
							if (num2 == num)
							{
								mCurrentCultureLangID = num2;
								return mCurrentCultureLangID;
							}
						}
					}
					mCurrentCultureLangID = (short)((langIDs.Length != 0) ? langIDs[0] : 0);
				}
				return mCurrentCultureLangID;
			}
		}

		public string ManufacturerString
		{
			get
			{
				if (mManufacturerString == null)
				{
					mManufacturerString = string.Empty;
					if (Descriptor.ManufacturerStringIndex > 0)
					{
						mUsbDevice.GetString(out mManufacturerString, CurrentCultureLangID, Descriptor.ManufacturerStringIndex);
					}
				}
				return mManufacturerString;
			}
		}

		public string ProductString
		{
			get
			{
				if (mProductString == null)
				{
					mProductString = string.Empty;
					if (Descriptor.ProductStringIndex > 0)
					{
						mUsbDevice.GetString(out mProductString, CurrentCultureLangID, Descriptor.ProductStringIndex);
					}
				}
				return mProductString;
			}
		}

		public string SerialString
		{
			get
			{
				if (mSerialString == null)
				{
					mSerialString = string.Empty;
					if (Descriptor.SerialStringIndex > 0)
					{
						mUsbDevice.GetString(out mSerialString, 1033, Descriptor.SerialStringIndex);
					}
				}
				return mSerialString;
			}
		}

		internal UsbDeviceInfo(UsbDevice usbDevice)
		{
			mUsbDevice = usbDevice;
			GetDeviceDescriptor(mUsbDevice, out mDeviceDescriptor);
		}

		public override string ToString()
		{
			return ToString("", UsbDescriptor.ToStringParamValueSeperator, UsbDescriptor.ToStringFieldSeperator);
		}

		public string ToString(string prefixSeperator, string entitySperator, string suffixSeperator)
		{
			string[] array = new string[3] { "ManufacturerString", "ProductString", "SerialString" };
			object[] array2 = new object[3] { ManufacturerString, ProductString, SerialString };
			return Descriptor.ToString(prefixSeperator, entitySperator, suffixSeperator) + Helper.ToString(prefixSeperator, array, entitySperator, array2, suffixSeperator);
		}

		internal static bool GetDeviceDescriptor(UsbDevice usbDevice, out UsbDeviceDescriptor deviceDescriptor)
		{
			if (usbDevice.mCachedDeviceDescriptor != null)
			{
				deviceDescriptor = usbDevice.mCachedDeviceDescriptor;
				return true;
			}
			deviceDescriptor = new UsbDeviceDescriptor();
			GCHandle gCHandle = GCHandle.Alloc(deviceDescriptor, GCHandleType.Pinned);
			int transferLength;
			bool descriptor = usbDevice.GetDescriptor(1, 0, 0, gCHandle.AddrOfPinnedObject(), UsbDeviceDescriptor.Size, out transferLength);
			gCHandle.Free();
			if (descriptor)
			{
				return true;
			}
			return false;
		}
	}
}
