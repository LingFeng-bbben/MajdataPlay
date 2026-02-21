using System;

namespace LibUsbDotNet.Main
{
	public class EndpointDataEventArgs : EventArgs
	{
		private readonly byte[] mBytesReceived;

		private readonly int mCount;

		public byte[] Buffer => mBytesReceived;

		public int Count => mCount;

		internal EndpointDataEventArgs(byte[] bytes, int size)
		{
			mBytesReceived = bytes;
			mCount = size;
		}
	}
}
