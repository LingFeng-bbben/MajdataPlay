using System;
using LibUsbDotNet.Internal;
using LibUsbDotNet.Main;

namespace LibUsbDotNet
{
	public class UsbError : EventArgs
	{
		internal static int mLastErrorNumber;

		internal static string mLastErrorString = string.Empty;

		internal string mDescription;

		internal ErrorCode mErrorCode;

		private object mSender;

		internal int mWin32ErrorNumber;

		internal string mWin32ErrorString = "None";

		public object Sender => mSender;

		public ErrorCode ErrorCode => mErrorCode;

		public int Win32ErrorNumber => mWin32ErrorNumber;

		public string Description => mDescription;

		public string Win32ErrorString => mWin32ErrorString;

		internal UsbError(ErrorCode errorCode, int win32ErrorNumber, string win32ErrorString, string description, object sender)
		{
			mSender = sender;
			string text = string.Empty;
			if (mSender is UsbEndpointBase || mSender is UsbTransfer)
			{
				UsbEndpointBase usbEndpointBase = ((!(mSender is UsbTransfer)) ? (mSender as UsbEndpointBase) : ((UsbTransfer)mSender).EndpointBase);
				if (usbEndpointBase.mEpNum != 0)
				{
					text = (text += $" Ep 0x{usbEndpointBase.mEpNum:X2} ");
				}
			}
			else if (mSender is Type)
			{
				Type type = mSender as Type;
				text = (text += $" {type.Name} ");
			}
			mErrorCode = errorCode;
			mWin32ErrorNumber = win32ErrorNumber;
			mWin32ErrorString = win32ErrorString;
			mDescription = description + text;
		}

		public override string ToString()
		{
			if (Win32ErrorNumber != 0)
			{
				return $"{ErrorCode}:{Description}\r\n{Win32ErrorNumber}:{mWin32ErrorString}";
			}
			return $"{ErrorCode}:{Description}";
		}

		internal static UsbError Error(ErrorCode errorCode, int ret, string description, object sender)
		{
			string win32ErrorString = string.Empty;
			if (errorCode == ErrorCode.Win32Error && ret != 0)
			{
				win32ErrorString = Kernel32.FormatSystemMessage(ret);
			}
			UsbError usbError = new UsbError(errorCode, ret, win32ErrorString, description, sender);
			lock (mLastErrorString)
			{
				mLastErrorNumber = (int)usbError.ErrorCode;
				mLastErrorString = usbError.ToString();
			}
			UsbDevice.FireUsbError(sender, usbError);
			return usbError;
		}
	}
}
