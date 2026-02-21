namespace LibUsbDotNet.Internal
{
	internal enum NativeFileMode : uint
	{
		CREATE_NEW = 1u,
		CREATE_ALWAYS,
		OPEN_EXISTING,
		OPEN_ALWAYS,
		TRUNCATE_EXISTING
	}
}
