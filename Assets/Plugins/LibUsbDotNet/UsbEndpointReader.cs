using System;
using System.Runtime.InteropServices;
using System.Threading;
using LibUsbDotNet.Internal;
using LibUsbDotNet.Main;

namespace LibUsbDotNet
{
	public class UsbEndpointReader : UsbEndpointBase
	{
		private static int mDefReadBufferSize = 4096;

		private bool mDataReceivedEnabled;

		private int mReadBufferSize;

		private Thread mReadThread;

		private ThreadPriority mReadThreadPriority = ThreadPriority.Normal;

		public static int DefReadBufferSize
		{
			get
			{
				return mDefReadBufferSize;
			}
			set
			{
				mDefReadBufferSize = value;
			}
		}

		public virtual bool DataReceivedEnabled
		{
			get
			{
				return mDataReceivedEnabled;
			}
			set
			{
				if (value != mDataReceivedEnabled)
				{
					StartStopReadThread();
				}
			}
		}

		public int ReadBufferSize
		{
			get
			{
				return mReadBufferSize;
			}
			set
			{
				mReadBufferSize = value;
			}
		}

		public ThreadPriority ReadThreadPriority
		{
			get
			{
				return mReadThreadPriority;
			}
			set
			{
				mReadThreadPriority = value;
			}
		}

		public virtual event EventHandler<EndpointDataEventArgs> DataReceived;

		public virtual event EventHandler<DataReceivedEnabledChangedEventArgs> DataReceivedEnabledChanged;

		internal UsbEndpointReader(UsbDevice usbDevice, int readBufferSize, byte alternateInterfaceID, ReadEndpointID readEndpointID, EndpointType endpointType)
			: base(usbDevice, alternateInterfaceID, (byte)readEndpointID, endpointType)
		{
			mReadBufferSize = readBufferSize;
		}

		public virtual ErrorCode Read(byte[] buffer, int timeout, out int transferLength)
		{
			return Read(buffer, 0, buffer.Length, timeout, out transferLength);
		}

		public virtual ErrorCode Read(IntPtr buffer, int offset, int count, int timeout, out int transferLength)
		{
			return Transfer(buffer, offset, count, timeout, out transferLength);
		}

		public virtual ErrorCode Read(byte[] buffer, int offset, int count, int timeout, out int transferLength)
		{
			return Transfer(buffer, offset, count, timeout, out transferLength);
		}

		public virtual ErrorCode Read(object buffer, int offset, int count, int timeout, out int transferLength)
		{
			return Transfer(buffer, offset, count, timeout, out transferLength);
		}

		public virtual ErrorCode Read(object buffer, int timeout, out int transferLength)
		{
			return Transfer(buffer, 0, Marshal.SizeOf(buffer), timeout, out transferLength);
		}

		public virtual ErrorCode ReadFlush()
		{
			byte[] buffer = new byte[64];
			int num = 0;
			int transferLength;
			while (Read(buffer, 10, out transferLength) == ErrorCode.None && num < 128)
			{
				num++;
			}
			return ErrorCode.None;
		}

		private static void ReadData(object context)
		{
			UsbTransfer usbTransfer = (UsbTransfer)context;
			UsbEndpointReader usbEndpointReader = (UsbEndpointReader)usbTransfer.EndpointBase;
			usbEndpointReader.mDataReceivedEnabled = true;
			usbEndpointReader.DataReceivedEnabledChanged?.Invoke(usbEndpointReader, new DataReceivedEnabledChangedEventArgs(usbEndpointReader.mDataReceivedEnabled));
			usbTransfer.Reset();
			byte[] array = new byte[usbEndpointReader.mReadBufferSize];
			try
			{
				while (!usbTransfer.IsCancelled)
				{
					int transferLength;
					switch (usbEndpointReader.Transfer(array, 0, array.Length, -1, out transferLength))
					{
						case ErrorCode.None:
						{
							EventHandler<EndpointDataEventArgs> eventHandler = usbEndpointReader.DataReceived;
							if (eventHandler != null && !usbTransfer.IsCancelled)
							{
								eventHandler(usbEndpointReader, new EndpointDataEventArgs(array, transferLength));
							}
							break;
						}
						case ErrorCode.IoTimedOut:
							break;
						default:
							return;
					}
				}
			}
			catch (ThreadAbortException)
			{
				UsbError.Error(ErrorCode.ReceiveThreadTerminated, 0, "ReadData:Read thread aborted.", usbEndpointReader);
			}
			finally
			{
				usbEndpointReader.Abort();
				usbEndpointReader.mDataReceivedEnabled = false;
				usbEndpointReader.DataReceivedEnabledChanged?.Invoke(usbEndpointReader, new DataReceivedEnabledChangedEventArgs(usbEndpointReader.mDataReceivedEnabled));
			}
		}

		private void StartReadThread()
		{
			mReadThread = new Thread(ReadData);
			mReadThread.Priority = ReadThreadPriority;
			mReadThread.Start(base.TransferContext);
			Thread.Sleep(1);
		}

		private bool StopReadThread()
		{
			Abort();
			Thread.Sleep(1);
			DateTime now = DateTime.Now;
			while (mReadThread.IsAlive && (DateTime.Now - now).TotalSeconds < 5.0)
			{
				Thread.Sleep(100);
			}
			if (mReadThread.IsAlive)
			{
				UsbError.Error(ErrorCode.ReceiveThreadTerminated, 0, "Failed stopping read thread.", this);
				mReadThread.Abort();
				return false;
			}
			return true;
		}

		private void StartStopReadThread()
		{
			if (base.IsDisposed)
			{
				throw new ObjectDisposedException(GetType().FullName);
			}
			if (mDataReceivedEnabled)
			{
				StopReadThread();
			}
			else
			{
				StartReadThread();
			}
		}

		internal override UsbTransfer CreateTransferContext()
		{
			return new OverlappedTransferContext(this);
		}
	}
}
