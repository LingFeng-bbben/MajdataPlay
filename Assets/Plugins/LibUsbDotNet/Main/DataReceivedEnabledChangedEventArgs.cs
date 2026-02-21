using System;

namespace LibUsbDotNet.Main
{
	public class DataReceivedEnabledChangedEventArgs : EventArgs
	{
		private readonly bool mEnabled;

		private readonly ErrorCode mErrorCode;

		public ErrorCode ErrorCode => mErrorCode;

		public bool Enabled => mEnabled;

		internal DataReceivedEnabledChangedEventArgs(bool enabled, ErrorCode errorCode)
		{
			mEnabled = enabled;
			mErrorCode = errorCode;
		}

		internal DataReceivedEnabledChangedEventArgs(bool enabled)
			: this(enabled, ErrorCode.None)
		{
		}
	}
}
