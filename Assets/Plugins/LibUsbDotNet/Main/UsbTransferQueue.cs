using System.Threading;

namespace LibUsbDotNet.Main
{
	public class UsbTransferQueue
	{
		public class Handle
		{
			public readonly UsbTransfer Context;

			public readonly byte[] Data;

			public int Transferred;

			internal bool InUse;

			internal Handle(UsbTransfer context, byte[] data)
			{
				Context = context;
				Data = data;
			}
		}

		public readonly UsbEndpointBase EndpointBase;

		public readonly int MaxOutstandingIO;

		public readonly int BufferSize;

		public readonly int Timeout;

		public readonly int IsoPacketSize;

		private int mOutstandingTransferCount;

		private readonly Handle[] mTransferHandles;

		private readonly byte[][] mBuffer;

		private int mTransferHandleNextIndex;

		private int mTransferHandleWaitIndex;

		public byte[] this[int index] => mBuffer[index];

		public byte[][] Buffer => mBuffer;

		public UsbTransferQueue(UsbEndpointBase endpointBase, int maxOutstandingIO, int bufferSize, int timeout, int isoPacketSize)
		{
			EndpointBase = endpointBase;
			IsoPacketSize = isoPacketSize;
			Timeout = timeout;
			BufferSize = bufferSize;
			MaxOutstandingIO = maxOutstandingIO;
			mTransferHandles = new Handle[maxOutstandingIO];
			mBuffer = new byte[maxOutstandingIO][];
			for (int i = 0; i < maxOutstandingIO; i++)
			{
				mBuffer[i] = new byte[bufferSize];
			}
			IsoPacketSize = ((isoPacketSize > 0) ? isoPacketSize : endpointBase.EndpointInfo.Descriptor.MaxPacketSize);
		}

		private static void IncWithRoll(ref int incField, int rollOverValue)
		{
			if (++incField >= rollOverValue)
			{
				incField = 0;
			}
		}

		public ErrorCode Transfer(out Handle handle)
		{
			return transfer(this, out handle);
		}

		private static ErrorCode transfer(UsbTransferQueue transferParam, out Handle handle)
		{
			handle = null;
			ErrorCode errorCode = ErrorCode.None;
			while (true)
			{
				if (transferParam.mOutstandingTransferCount < transferParam.MaxOutstandingIO)
				{
					if (transferParam.mTransferHandles[transferParam.mTransferHandleNextIndex] == null)
					{
						handle = (transferParam.mTransferHandles[transferParam.mTransferHandleNextIndex] = new Handle(transferParam.EndpointBase.NewAsyncTransfer(), transferParam.mBuffer[transferParam.mTransferHandleNextIndex]));
						handle.Context.Fill(handle.Data, 0, handle.Data.Length, transferParam.Timeout, transferParam.IsoPacketSize);
					}
					else
					{
						handle = transferParam.mTransferHandles[transferParam.mTransferHandleNextIndex];
					}
					handle.Transferred = 0;
					handle.Context.Reset();
					errorCode = handle.Context.Submit();
					if (errorCode != ErrorCode.None)
					{
						break;
					}
					handle.InUse = true;
					transferParam.mOutstandingTransferCount++;
					IncWithRoll(ref transferParam.mTransferHandleNextIndex, transferParam.MaxOutstandingIO);
					continue;
				}
				if (transferParam.mOutstandingTransferCount != transferParam.MaxOutstandingIO)
				{
					break;
				}
				handle = transferParam.mTransferHandles[transferParam.mTransferHandleWaitIndex];
				errorCode = handle.Context.Wait(out handle.Transferred, cancel: false);
				if (errorCode != ErrorCode.None)
				{
					break;
				}
				handle.InUse = false;
				transferParam.mOutstandingTransferCount--;
				IncWithRoll(ref transferParam.mTransferHandleWaitIndex, transferParam.MaxOutstandingIO);
				return ErrorCode.None;
			}
			return errorCode;
		}

		public void Free()
		{
			free(this);
		}

		private static void free(UsbTransferQueue transferParam)
		{
			for (int i = 0; i < transferParam.MaxOutstandingIO; i++)
			{
				if (transferParam.mTransferHandles[i] == null)
				{
					continue;
				}
				if (transferParam.mTransferHandles[i].InUse)
				{
					if (!transferParam.mTransferHandles[i].Context.IsCompleted)
					{
						transferParam.EndpointBase.Abort();
						Thread.Sleep(1);
					}
					transferParam.mTransferHandles[i].InUse = false;
					transferParam.mTransferHandles[i].Context.Dispose();
				}
				transferParam.mTransferHandles[i] = null;
			}
			transferParam.mOutstandingTransferCount = 0;
			transferParam.mTransferHandleNextIndex = 0;
			transferParam.mTransferHandleWaitIndex = 0;
		}
	}
}
