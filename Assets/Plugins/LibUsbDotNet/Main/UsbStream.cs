using System;
using System.IO;
using System.Threading;

namespace LibUsbDotNet.Main
{
	public class UsbStream : Stream
	{
		private readonly UsbEndpointBase mUsbEndpoint;

		private int mTimeout = 1000;

		private Thread mWaitThread;

		public override long Length
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		public override long Position
		{
			get
			{
				throw new NotSupportedException();
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		private Thread WaitThread
		{
			get
			{
				if (mWaitThread == null)
				{
					mWaitThread = new Thread(AsyncTransferFn);
				}
				while (mWaitThread.IsAlive)
				{
				}
				return mWaitThread;
			}
		}

		public override bool CanRead => (mUsbEndpoint.EpNum & 0x80) == 128;

		public override bool CanSeek => false;

		public override bool CanTimeout => true;

		public override bool CanWrite => (mUsbEndpoint.EpNum & 0x80) == 0;

		public override int ReadTimeout
		{
			get
			{
				return mTimeout;
			}
			set
			{
				mTimeout = value;
			}
		}

		public override int WriteTimeout
		{
			get
			{
				return mTimeout;
			}
			set
			{
				mTimeout = value;
			}
		}

		public UsbStream(UsbEndpointBase usbEndpoint)
		{
			mUsbEndpoint = usbEndpoint;
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotSupportedException();
		}

		public override void SetLength(long value)
		{
			throw new NotSupportedException();
		}

		public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
		{
			UsbStreamAsyncTransfer usbStreamAsyncTransfer = new UsbStreamAsyncTransfer(mUsbEndpoint, buffer, offset, count, callback, state, ReadTimeout);
			WaitThread.Start(usbStreamAsyncTransfer);
			return usbStreamAsyncTransfer;
		}

		public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
		{
			UsbStreamAsyncTransfer usbStreamAsyncTransfer = new UsbStreamAsyncTransfer(mUsbEndpoint, buffer, offset, count, callback, state, WriteTimeout);
			WaitThread.Start(usbStreamAsyncTransfer);
			return usbStreamAsyncTransfer;
		}

		public override int EndRead(IAsyncResult asyncResult)
		{
			UsbStreamAsyncTransfer usbStreamAsyncTransfer = (UsbStreamAsyncTransfer)asyncResult;
			usbStreamAsyncTransfer.mCompleteEvent.WaitOne();
			if (usbStreamAsyncTransfer.Result == ErrorCode.None)
			{
				return usbStreamAsyncTransfer.TransferredLength;
			}
			if (usbStreamAsyncTransfer.Result == ErrorCode.IoTimedOut)
			{
				throw new TimeoutException($"{usbStreamAsyncTransfer.Result}:Endpoint 0x{mUsbEndpoint.EpNum:X2} IO timed out.");
			}
			if (usbStreamAsyncTransfer.Result == ErrorCode.IoCancelled)
			{
				throw new IOCancelledException($"{usbStreamAsyncTransfer.Result}:Endpoint 0x{mUsbEndpoint.EpNum:X2} IO was cancelled.");
			}
			throw new IOException($"{usbStreamAsyncTransfer.Result}:Failed reading from endpoint:{mUsbEndpoint.EpNum}");
		}

		public override void EndWrite(IAsyncResult asyncResult)
		{
			UsbStreamAsyncTransfer usbStreamAsyncTransfer = (UsbStreamAsyncTransfer)asyncResult;
			usbStreamAsyncTransfer.mCompleteEvent.WaitOne();
			if (usbStreamAsyncTransfer.Result == ErrorCode.None && usbStreamAsyncTransfer.mCount == usbStreamAsyncTransfer.TransferredLength)
			{
				return;
			}
			if (usbStreamAsyncTransfer.Result == ErrorCode.IoTimedOut)
			{
				throw new TimeoutException($"{usbStreamAsyncTransfer.Result}:Endpoint 0x{mUsbEndpoint.EpNum:X2} IO timed out.");
			}
			if (usbStreamAsyncTransfer.Result == ErrorCode.IoCancelled)
			{
				throw new IOCancelledException($"{usbStreamAsyncTransfer.Result}:Endpoint 0x{mUsbEndpoint.EpNum:X2} IO was cancelled.");
			}
			if (usbStreamAsyncTransfer.mCount != usbStreamAsyncTransfer.TransferredLength)
			{
				throw new IOException($"{usbStreamAsyncTransfer.Result}:Failed writing {usbStreamAsyncTransfer.mCount - usbStreamAsyncTransfer.TransferredLength} byte(s) to endpoint 0x{mUsbEndpoint.EpNum:X2}.");
			}
			throw new IOException($"{usbStreamAsyncTransfer.Result}:Failed writing to endpoint 0x{mUsbEndpoint.EpNum:X2}");
		}

		public override void Flush()
		{
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			if (!CanRead)
			{
				throw new InvalidOperationException($"Cannot read from WriteEndpoint {(WriteEndpointID)mUsbEndpoint.EpNum}.");
			}
			int transferLength;
			ErrorCode errorCode = mUsbEndpoint.Transfer(buffer, offset, count, ReadTimeout, out transferLength);
			return errorCode switch
			{
				ErrorCode.None => transferLength, 
				ErrorCode.IoTimedOut => throw new TimeoutException($"{errorCode}:Endpoint 0x{mUsbEndpoint.EpNum:X2} IO timed out."), 
				ErrorCode.IoCancelled => throw new IOCancelledException($"{errorCode}:Endpoint 0x{mUsbEndpoint.EpNum:X2} IO was cancelled."), 
				_ => throw new IOException($"{errorCode}:Failed reading from endpoint:{mUsbEndpoint.EpNum}"), 
			};
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			if (!CanWrite)
			{
				throw new InvalidOperationException($"Cannot write to ReadEndpoint {(ReadEndpointID)mUsbEndpoint.EpNum}.");
			}
			int transferLength;
			ErrorCode errorCode = mUsbEndpoint.Transfer(buffer, offset, count, WriteTimeout, out transferLength);
			if (errorCode == ErrorCode.None && count == transferLength)
			{
				return;
			}
			switch (errorCode)
			{
				case ErrorCode.IoTimedOut:
					throw new TimeoutException($"{errorCode}:Endpoint 0x{mUsbEndpoint.EpNum:X2} IO timed out.");
				case ErrorCode.IoCancelled:
					throw new IOCancelledException($"{errorCode}:Endpoint 0x{mUsbEndpoint.EpNum:X2} IO was cancelled.");
				default:
					if (count != transferLength)
					{
						throw new IOException($"{errorCode}:Failed writing {count - transferLength} byte(s) to endpoint 0x{mUsbEndpoint.EpNum:X2}.");
					}
					throw new IOException($"{errorCode}:Failed writing to endpoint 0x{mUsbEndpoint.EpNum:X2}");
			}
		}

		private static void AsyncTransferFn(object oContext)
		{
			(oContext as UsbStreamAsyncTransfer).SyncTransfer();
		}
	}
}
