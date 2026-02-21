namespace LibUsbDotNet
{
	public interface IUsbDevice : IUsbInterface
	{
		bool DetachKernelDriver(int interfaceID);

		bool SetAutoDetachKernelDriver(bool autoDetach);

		bool SetConfiguration(byte config);

		bool GetConfiguration(out byte config);

		bool GetAltInterfaceSetting(byte interfaceID, out byte selectedAltInterfaceID);

		bool ClaimInterface(int interfaceID);

		bool ReleaseInterface(int interfaceID);

		bool ResetDevice();
	}
}
