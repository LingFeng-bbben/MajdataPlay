namespace LibUsbDotNet.Main
{
	public static class UsbConstants
	{
		public const int DEFAULT_TIMEOUT = 1000;

		internal const bool EXIT_CONTEXT = false;

		public const int MAX_CONFIG_SIZE = 4096;

		public const int MAX_DEVICES = 256;

		public const int MAX_ENDPOINTS = 32;

		public const byte ENDPOINT_DIR_MASK = 128;

		public const byte ENDPOINT_NUMBER_MASK = 15;
	}
}
