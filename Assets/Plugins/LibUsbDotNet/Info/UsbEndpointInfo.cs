using System;
using LibUsbDotNet.Descriptors;
using LibUsbDotNet.Main;

namespace LibUsbDotNet.Info
{
	public class UsbEndpointInfo : UsbBaseInfo
	{
		internal UsbEndpointDescriptor mUsbEndpointDescriptor;

		public UsbEndpointDescriptor Descriptor => mUsbEndpointDescriptor;

		internal UsbEndpointInfo(byte[] descriptor)
		{
			mUsbEndpointDescriptor = new UsbEndpointDescriptor();
			Helper.BytesToObject(descriptor, 0, Math.Min(UsbEndpointDescriptor.Size, descriptor[0]), mUsbEndpointDescriptor);
		}

		public override string ToString()
		{
			return Descriptor.ToString();
		}

		public string ToString(string prefixSeperator, string entitySperator, string suffixSeperator)
		{
			return Descriptor.ToString(prefixSeperator, entitySperator, suffixSeperator);
		}
	}
}
