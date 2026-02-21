using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace LibUsbDotNet.Main
{
	public class UsbStreamAsyncTransfer : IAsyncResult
	{
		internal readonly int mCount;

		internal readonly int mOffset;

		internal readonly object mState;

		private readonly int mTimeout;

		internal AsyncCallback mCallback;

		internal ManualResetEvent mCompleteEvent = new ManualResetEvent(initialState: false);

		internal GCHandle mGCBuffer;

		internal bool mIsComplete;

		private ErrorCode mResult;

		private int mTrasferredLength;

		internal UsbEndpointBase mUsbEndpoint;

		public ErrorCode Result => mResult;

		public int TransferredLength => mTrasferredLength;

		public bool IsCompleted => mIsComplete;

		public WaitHandle AsyncWaitHandle => mCompleteEvent;

		public object AsyncState => mState;

		public bool CompletedSynchronously => false;

		public UsbStreamAsyncTransfer(UsbEndpointBase usbEndpoint, byte[] buffer, int offset, int count, AsyncCallback callback, object state, int timeout)
		{
			mUsbEndpoint = usbEndpoint;
			mOffset = offset;
			mCount = count;
			mState = state;
			mTimeout = timeout;
			mCallback = callback;
			mGCBuffer = GCHandle.Alloc(buffer, GCHandleType.Pinned);
		}

		public ErrorCode SyncTransfer()
		{
			mResult = mUsbEndpoint.Transfer(mGCBuffer.AddrOfPinnedObject(), mOffset, mCount, mTimeout, out mTrasferredLength);
			mGCBuffer.Free();
			mIsComplete = true;
			if (mCallback != null)
			{
				mCallback(this);
			}
			mCompleteEvent.Set();
			return mResult;
		}
	}
}
