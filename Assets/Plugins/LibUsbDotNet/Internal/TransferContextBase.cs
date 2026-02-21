using System;
using System.Threading;
using LibUsbDotNet.Main;
using Microsoft.Win32.SafeHandles;

namespace LibUsbDotNet.Internal
{
	public abstract class TransferContextBase : IDisposable
	{
		private readonly UsbEndpointBase mEndpointBase;

		private IntPtr mBuffer;

		private int mCurrentOffset;

		private int mCurrentRemaining;

		private int mCurrentTransmitted;

		private int mFailRetries;

		protected int mOriginalCount;

		protected int mOriginalOffset;

		private PinnedHandle mPinnedHandle;

		protected int mTimeout;

		protected bool mHasWaitBeenCalled = true;

		protected ManualResetEvent mTransferCancelEvent = new ManualResetEvent(initialState: false);

		protected internal ManualResetEvent mTransferCompleteEvent = new ManualResetEvent(initialState: true);

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

		protected int FailRetries => mFailRetries;

		protected IntPtr NextBufPtr => new IntPtr(mBuffer.ToInt64() + mCurrentOffset);

		public bool IsCancelled => mTransferCancelEvent.WaitOne(0);

		public bool IsComplete => mTransferCompleteEvent.WaitOne(0);

		public SafeWaitHandle CancelWaitHandle => mTransferCancelEvent.GetSafeWaitHandle();

		public SafeWaitHandle CompleteWaitHandle => mTransferCompleteEvent.GetSafeWaitHandle();

		protected TransferContextBase(UsbEndpointBase endpointBase)
		{
			mEndpointBase = endpointBase;
		}

		public virtual void Dispose()
		{
			if (!IsCancelled)
			{
				Cancel();
			}
			if (!mHasWaitBeenCalled)
			{
				Wait(out var _);
			}
		}

		public virtual ErrorCode Cancel()
		{
			mTransferCancelEvent.Set();
			return ErrorCode.None;
		}

		public abstract ErrorCode Submit();

		public abstract ErrorCode Wait(out int transferredCount);

		public virtual void Fill(object buffer, int offset, int count, int timeout)
		{
			if (mPinnedHandle != null)
			{
				mPinnedHandle.Dispose();
			}
			mPinnedHandle = new PinnedHandle(buffer);
			Fill(mPinnedHandle.Handle, offset, count, timeout);
		}

		public virtual void Fill(IntPtr buffer, int offset, int count, int timeout)
		{
			mBuffer = buffer;
			mOriginalOffset = offset;
			mOriginalCount = count;
			mTimeout = timeout;
			Reset();
		}

		internal static ErrorCode SyncTransfer(TransferContextBase transferContext, IntPtr buffer, int offset, int length, int timeout, out int transferLength)
		{
			if (transferContext == null)
			{
				throw new NullReferenceException("Invalid transfer context.");
			}
			if (offset < 0)
			{
				throw new ArgumentException("must be >=0", "offset");
			}
			lock (transferContext)
			{
				transferLength = 0;
				transferContext.Fill(buffer, offset, length, timeout);
				while (true)
				{
					ErrorCode errorCode = transferContext.Submit();
					switch (errorCode)
					{
						case ErrorCode.IoEndpointGlobalCancelRedo:
							break;
						default:
							return errorCode;
						case ErrorCode.None:
						{
							errorCode = transferContext.Wait(out var transferredCount);
							switch (errorCode)
							{
								case ErrorCode.IoEndpointGlobalCancelRedo:
									break;
								default:
									return errorCode;
								case ErrorCode.None:
									transferLength += transferredCount;
									if (errorCode != ErrorCode.None || transferredCount != UsbEndpointBase.MaxReadWrite || !transferContext.IncrementTransfer(transferredCount))
									{
										return errorCode;
									}
									break;
							}
							break;
						}
					}
				}
			}
		}

		public bool IncrementTransfer(int amount)
		{
			mCurrentTransmitted += amount;
			mCurrentOffset += amount;
			mCurrentRemaining -= amount;
			if (mCurrentRemaining <= 0)
			{
				return false;
			}
			return true;
		}

		protected void IncFailRetries()
		{
			mFailRetries++;
		}

		public void Reset()
		{
			mCurrentOffset = mOriginalOffset;
			mCurrentRemaining = mOriginalCount;
			mCurrentTransmitted = 0;
			mFailRetries = 0;
			mTransferCancelEvent.Reset();
		}
	}
}
