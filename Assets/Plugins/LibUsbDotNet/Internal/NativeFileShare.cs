using System;

namespace LibUsbDotNet.Internal
{
	[Flags]
	internal enum NativeFileShare : uint
	{
		NONE = 0u,
		FILE_SHARE_READ = 1u,
		FILE_SHARE_WRITE = 2u,
		FILE_SHARE_DEELETE = 4u
	}
}
