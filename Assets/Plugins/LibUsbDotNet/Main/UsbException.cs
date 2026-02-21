using System;

namespace LibUsbDotNet.Main
{
	public class UsbException : Exception
	{
		private readonly object mSender;

		public object Sender => mSender;

		internal UsbException(object sender, string description)
			: base(description)
		{
			mSender = sender;
		}
	}
}
