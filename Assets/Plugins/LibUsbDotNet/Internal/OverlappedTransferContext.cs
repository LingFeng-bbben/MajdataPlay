using System.Runtime.InteropServices;
using System.Threading;
using LibUsbDotNet.Main;

namespace LibUsbDotNet.Internal
{
	internal class OverlappedTransferContext : UsbTransfer
	{
		private readonly SafeOverlapped mOverlapped = new SafeOverlapped();

		public SafeOverlapped Overlapped => mOverlapped;

		public OverlappedTransferContext(UsbEndpointBase endpointBase)
			: base(endpointBase)
		{
		}

		public override ErrorCode Submit()
		{
			ErrorCode result = ErrorCode.None;
			if (mTransferCancelEvent.WaitOne(0))
			{
				return ErrorCode.IoCancelled;
			}
			if (!mTransferCompleteEvent.WaitOne(0))
			{
				return ErrorCode.ResourceBusy;
			}
			mHasWaitBeenCalled = false;
			mTransferCompleteEvent.Reset();
			Overlapped.Init(mTransferCompleteEvent.GetSafeWaitHandle().DangerousGetHandle());
			int lengthTransferred;
			int num = base.EndpointBase.PipeTransferSubmit(base.NextBufPtr, base.RequestCount, out lengthTransferred, mIsoPacketSize, Overlapped.GlobalOverlapped);
			if (num != 0 && num != 997)
			{
				mTransferCompleteEvent.Set();
				result = UsbError.Error(ErrorCode.Win32Error, Marshal.GetLastWin32Error(), "PipeTransferSubmit", base.EndpointBase).ErrorCode;
			}
			return result;
		}

		public override ErrorCode Wait(out int transferredCount, bool cancel)
		{
			if (mHasWaitBeenCalled)
			{
				throw new UsbException(this, "Repeated calls to wait with a submit is not allowed.");
			}
			transferredCount = 0;
			int num = WaitHandle.WaitAny(new WaitHandle[2] { mTransferCompleteEvent, mTransferCancelEvent }, mTimeout);
			if (num == 258 && !cancel)
			{
				return ErrorCode.IoTimedOut;
			}
			mHasWaitBeenCalled = true;
			if (num != 0)
			{
				bool flag = base.EndpointBase.mUsbApi.AbortPipe(base.EndpointBase.Handle, base.EndpointBase.EpNum);
				bool flag2 = mTransferCompleteEvent.WaitOne(100);
				mTransferCompleteEvent.Set();
				if (!flag || !flag2)
				{
					int num2 = (flag ? (-16367) : (-16373));
					UsbError.Error((ErrorCode)num2, Marshal.GetLastWin32Error(), "Wait:AbortPipe Failed", this);
					return (ErrorCode)num2;
				}
				if (num == 258)
				{
					return ErrorCode.IoTimedOut;
				}
				return ErrorCode.IoCancelled;
			}
			if (!base.EndpointBase.mUsbApi.GetOverlappedResult(base.EndpointBase.Handle, Overlapped.GlobalOverlapped, out transferredCount, wait: true))
			{
				return UsbError.Error(ErrorCode.Win32Error, Marshal.GetLastWin32Error(), "GetOverlappedResult", base.EndpointBase).ErrorCode;
			}
			return ErrorCode.None;
		}
	}
}
