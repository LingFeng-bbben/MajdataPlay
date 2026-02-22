using System;
using System.Collections.ObjectModel;
using LibUsbDotNet.Info;
using LibUsbDotNet.Main;

namespace LibUsbDotNet
{
	public interface IUsbInterface
	{
		UsbEndpointList ActiveEndpoints { get; }

		ReadOnlyCollection<UsbConfigInfo> Configs { get; }

		UsbDevice.DriverModeType DriverMode { get; }

		UsbDeviceInfo Info { get; }

		bool IsOpen { get; }

		UsbRegistry UsbRegistryInfo { get; }

		bool Close();

		bool ControlTransfer(ref UsbSetupPacket setupPacket, IntPtr buffer, int bufferLength, out int lengthTransferred);

		bool ControlTransfer(ref UsbSetupPacket setupPacket, object buffer, int bufferLength, out int lengthTransferred);

		bool GetDescriptor(byte descriptorType, byte index, short langId, IntPtr buffer, int bufferLength, out int transferLength);

		bool GetDescriptor(byte descriptorType, byte index, short langId, object buffer, int bufferLength, out int transferLength);

		bool GetLangIDs(out short[] langIDs);

		bool GetString(out string stringData, short langId, byte stringIndex);

		bool SetAltInterface(int alternateID);

		bool GetAltInterface(out int alternateID);

		bool Open();

		UsbEndpointReader OpenEndpointReader(ReadEndpointID readEndpointID, int readBufferSize);

		UsbEndpointReader OpenEndpointReader(ReadEndpointID readEndpointID, int readBufferSize, EndpointType endpointType);

		UsbEndpointReader OpenEndpointReader(ReadEndpointID readEndpointID);

		UsbEndpointWriter OpenEndpointWriter(WriteEndpointID writeEndpointID);

		UsbEndpointWriter OpenEndpointWriter(WriteEndpointID writeEndpointID, EndpointType endpointType);
	}
}
