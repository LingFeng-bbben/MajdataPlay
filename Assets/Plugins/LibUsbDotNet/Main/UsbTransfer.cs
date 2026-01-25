using System;
using System.Threading;
using LibUsbDotNet.Info;

namespace LibUsbDotNet.Main
{
	public abstract class UsbTransfer : IDisposable, IAsyncResult
	{
		private readonly UsbEndpointBase mEndpointBase;

		private IntPtr mBuffer;

		private int mCurrentOffset;

		private int mCurrentRemaining;

		private int mCurrentTransmitted;

		protected int mIsoPacketSize;

		protected int mOriginalCount;

		protected int mOriginalOffset;

		private PinnedHandle mPinnedHandle;

		protected int mTimeout;

		protected bool mHasWaitBeenCalled = true;

		protected readonly object mTransferLOCK = new object();

		protected ManualResetEvent mTransferCancelEvent = new ManualResetEvent(initialState: false);

		protected internal ManualResetEvent mTransferCompleteEvent = new ManualResetEvent(initialState: true);

		protected bool mbDisposed;

		public UsbEndpointBase EndpointBase => mEndpointBase;

		protected int RequestCount
		{
			get
			{
				if (mCurrentRemaining <= UsbEndpointBase.MaxReadWrite)
				{
					return mCurrentRemaining;
				}
				return UsbEndpointBase.MaxReadWrite;
			}
		}

		protected IntPtr NextBufPtr => new IntPtr(mBuffer.ToInt64() + mCurrentOffset);

		public bool IsCancelled => mTransferCancelEvent.WaitOne(0);

		public WaitHandle CancelWaitHandle => mTransferCancelEvent;

		public int IsoPacketSize => mIsoPacketSize;

		public int Transmitted => mCurrentTransmitted;

		public int Remaining => mCurrentRemaining;

		public bool IsCompleted => mTransferCompleteEvent.WaitOne(0);

		public WaitHandle AsyncWaitHandle => mTransferCompleteEvent;

		public object AsyncState
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public bool CompletedSynchronously => false;

		protected UsbTransfer(UsbEndpointBase endpointBase)
		{
			mEndpointBase = endpointBase;
		}

		public virtual void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (mbDisposed)
			{
				return;
			}
			mbDisposed = true;
			if (!disposing)
			{
				return;
			}
			try
			{
				if (!IsCancelled)
				{
					Cancel();
				}
				if (!mHasWaitBeenCalled)
				{
					Wait(out var _);
				}
				if (disposing && mPinnedHandle != null)
				{
					mPinnedHandle.Dispose();
				}
				mPinnedHandle = null;
			}
			catch (Exception)
			{
			}
		}

		~UsbTransfer()
		{
			Dispose(disposing: false);
		}

		public virtual ErrorCode Cancel()
		{
			mTransferCancelEvent.Set();
			return ErrorCode.None;
		}

		public abstract ErrorCode Submit();

		public abstract ErrorCode Wait(out int transferredCount, bool cancel);

		public ErrorCode Wait(out int transferredCount)
		{
			return Wait(out transferredCount, cancel: true);
		}

		public virtual void Fill(object buffer, int offset, int count, int timeout)
		{
			if (mPinnedHandle != null)
			{
				mPinnedHandle.Dispose();
			}
			mPinnedHandle = new PinnedHandle(buffer);
			Fill(mPinnedHandle.Handle, offset, count, timeout);
		}

		public virtual void Fill(object buffer, int offset, int count, int timeout, int isoPacketSize)
		{
			if (mPinnedHandle != null)
			{
				mPinnedHandle.Dispose();
			}
			mPinnedHandle = new PinnedHandle(buffer);
			Fill(mPinnedHandle.Handle, offset, count, timeout, isoPacketSize);
		}

		public virtual void Fill(IntPtr buffer, int offset, int count, int timeout)
		{
			mBuffer = buffer;
			mOriginalOffset = offset;
			mOriginalCount = count;
			mTimeout = timeout;
			Reset();
		}

		public virtual void Fill(IntPtr buffer, int offset, int count, int timeout, int isoPacketSize)
		{
			mBuffer = buffer;
			mOriginalOffset = offset;
			mOriginalCount = count;
			mTimeout = timeout;
			mIsoPacketSize = isoPacketSize;
			Reset();
		}

		internal static ErrorCode SyncTransfer(UsbTransfer transferContext, IntPtr buffer, int offset, int length, int timeout, out int transferLength)
		{
			return SyncTransfer(transferContext, buffer, offset, length, timeout, 0, out transferLength);
		}

		internal static ErrorCode SyncTransfer(UsbTransfer transferContext, IntPtr buffer, int offset, int length, int timeout, int isoPacketSize, out int transferLength)
		{
			if (transferContext == null)
			{
				throw new NullReferenceException("Invalid transfer context.");
			}
			if (offset < 0)
			{
				throw new ArgumentException("must be >=0", "offset");
			}
			if (isoPacketSize == 0 && transferContext.EndpointBase.Type == EndpointType.Isochronous)
			{
				UsbEndpointInfo endpointInfo = transferContext.EndpointBase.EndpointInfo;
				if (endpointInfo != null)
				{
					isoPacketSize = endpointInfo.Descriptor.MaxPacketSize;
				}
			}
			lock (transferContext.mTransferLOCK)
			{
				transferLength = 0;
				transferContext.Fill(buffer, offset, length, timeout, isoPacketSize);
				ErrorCode errorCode;
				int transferredCount;
				do
				{
					errorCode = transferContext.Submit();
					if (errorCode != ErrorCode.None)
					{
						return errorCode;
					}
					errorCode = transferContext.Wait(out transferredCount);
					if (errorCode != ErrorCode.None)
					{
						return errorCode;
					}
					transferLength += transferredCount;
				}
				while (errorCode == ErrorCode.None && transferredCount == UsbEndpointBase.MaxReadWrite && transferContext.IncrementTransfer(transferredCount));
				return errorCode;
			}
		}

		public bool IncrementTransfer(int amount)
		{
			mCurrentTransmitted += amount;
			mCurrentRemaining -= amount;
			mCurrentOffset += amount;
			if (mCurrentRemaining <= 0)
			{
				return false;
			}
			return true;
		}

		public void Reset()
		{
			mCurrentOffset = mOriginalOffset;
			mCurrentRemaining = mOriginalCount;
			mCurrentTransmitted = 0;
			mTransferCancelEvent.Reset();
		}
	}
}
