using System.Collections.Generic;
using System.Collections.ObjectModel;
using LibUsbDotNet.Descriptors;
using LibUsbDotNet.Main;

namespace LibUsbDotNet.Info
{
	public class UsbConfigInfo : UsbBaseInfo
	{
		private readonly List<UsbInterfaceInfo> mInterfaceList = new List<UsbInterfaceInfo>();

		internal readonly UsbConfigDescriptor mUsbConfigDescriptor;

		private string mConfigString;

		internal UsbDevice mUsbDevice;

		public UsbConfigDescriptor Descriptor => mUsbConfigDescriptor;

		public string ConfigString
		{
			get
			{
				if (mConfigString == null)
				{
					mConfigString = string.Empty;
					if (Descriptor.StringIndex > 0)
					{
						mUsbDevice.GetString(out mConfigString, mUsbDevice.Info.CurrentCultureLangID, Descriptor.StringIndex);
					}
				}
				return mConfigString;
			}
		}

		public ReadOnlyCollection<UsbInterfaceInfo> InterfaceInfoList => mInterfaceList.AsReadOnly();

		internal UsbConfigInfo(UsbDevice usbDevice, UsbConfigDescriptor descriptor, ref List<byte[]> rawDescriptors)
		{
			mUsbDevice = usbDevice;
			mUsbConfigDescriptor = descriptor;
			mRawDescriptors = rawDescriptors;
			UsbInterfaceInfo usbInterfaceInfo = null;
			for (int i = 0; i < rawDescriptors.Count; i++)
			{
				byte[] array = rawDescriptors[i];
				switch (array[1])
				{
					case 4:
						usbInterfaceInfo = new UsbInterfaceInfo(usbDevice, array);
						mRawDescriptors.RemoveAt(i);
						mInterfaceList.Add(usbInterfaceInfo);
						i--;
						break;
					case 5:
						if (usbInterfaceInfo == null)
						{
							throw new UsbException(this, "Recieved and endpoint descriptor before receiving an interface descriptor.");
						}
						usbInterfaceInfo.mEndpointInfo.Add(new UsbEndpointInfo(array));
						mRawDescriptors.RemoveAt(i);
						i--;
						break;
					default:
						if (usbInterfaceInfo != null)
						{
							usbInterfaceInfo.mRawDescriptors.Add(array);
							mRawDescriptors.RemoveAt(i);
							i--;
						}
						break;
				}
			}
		}

		public override string ToString()
		{
			return ToString("", UsbDescriptor.ToStringParamValueSeperator, UsbDescriptor.ToStringFieldSeperator);
		}

		public string ToString(string prefixSeperator, string entitySperator, string suffixSeperator)
		{
			object[] array = new object[1] { ConfigString };
			string[] array2 = new string[1] { "ConfigString" };
			return Descriptor.ToString(prefixSeperator, entitySperator, suffixSeperator) + Helper.ToString(prefixSeperator, array2, entitySperator, array, suffixSeperator);
		}
	}
}
