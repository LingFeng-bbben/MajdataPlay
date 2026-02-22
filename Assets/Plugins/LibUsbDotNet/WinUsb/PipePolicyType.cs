namespace LibUsbDotNet.WinUsb
{
	internal enum PipePolicyType : byte
	{
		ShortPacketTerminate = 1,
		AutoClearStall,
		PipeTransferTimeout,
		IgnoreShortPackets,
		AllowPartialReads,
		AutoFlush,
		RawIo,
		MaximumTransferSize
	}
}
