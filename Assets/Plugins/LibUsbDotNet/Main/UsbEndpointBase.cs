using System;
using System.Runtime.InteropServices;
using LibUsbDotNet.Info;
using LibUsbDotNet.Internal;

namespace LibUsbDotNet.Main
{
	public abstract class UsbEndpointBase : IDisposable
	{
		internal delegate int TransferDelegate(IntPtr pBuffer, int bufferLength, out int lengthTransferred, int isoPacketSize, IntPtr pOverlapped);

		public static int MaxReadWrite = int.MaxValue;

		internal readonly byte mEpNum;

		internal readonly UsbApiBase mUsbApi;

		private readonly UsbDevice mUsbDevice;

		private readonly byte alternateInterfaceID;

		private readonly SafeHandle mUsbHandle;

		private bool mIsDisposed;

		internal TransferDelegate mPipeTransferSubmit;

		private UsbTransfer mTransferContext;

		private UsbEndpointInfo mUsbEndpointInfo;

		private EndpointType mEndpointType;

		private UsbInterfaceInfo mUsbInterfacetInfo;

		internal virtual TransferDelegate PipeTransferSubmit => mPipeTransferSubmit;

		internal UsbTransfer TransferContext
		{
			get
			{
				if (mTransferContext == null)
				{
					mTransferContext = CreateTransferContext();
				}
				return mTransferContext;
			}
		}

		public bool IsDisposed => mIsDisposed;

		public UsbDevice Device => mUsbDevice;

		internal SafeHandle Handle => mUsbHandle;

		public byte EpNum => mEpNum;

		public EndpointType Type => mEndpointType;

		public UsbEndpointInfo EndpointInfo
		{
			get
			{
				if (mUsbEndpointInfo == null && !LookupEndpointInfo(Device.Configs[0], alternateInterfaceID, mEpNum, out mUsbInterfacetInfo, out mUsbEndpointInfo))
				{
					return null;
				}
				return mUsbEndpointInfo;
			}
		}

		internal UsbEndpointBase(UsbDevice usbDevice, byte alternateInterfaceID, byte epNum, EndpointType endpointType)
		{
			mUsbDevice = usbDevice;
			this.alternateInterfaceID = alternateInterfaceID;
			mUsbApi = mUsbDevice.mUsbApi;
			mUsbHandle = mUsbDevice.Handle;
			mEpNum = epNum;
			mEndpointType = endpointType;
			if ((mEpNum & 0x80) > 0)
			{
				mPipeTransferSubmit = ReadPipe;
			}
			else
			{
				mPipeTransferSubmit = WritePipe;
			}
		}

		public virtual void Dispose()
		{
			DisposeAndRemoveFromList();
		}

		internal abstract UsbTransfer CreateTransferContext();

		public virtual bool Abort()
		{
			if (mIsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			return TransferContext.Cancel() == ErrorCode.None;
		}

		public virtual bool Flush()
		{
			if (mIsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			bool num = mUsbApi.FlushPipe(mUsbHandle, EpNum);
			if (!num)
			{
				UsbError.Error(ErrorCode.Win32Error, Marshal.GetLastWin32Error(), "FlushPipe", this);
			}
			return num;
		}

		public virtual bool Reset()
		{
			if (mIsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
			bool num = mUsbApi.ResetPipe(mUsbHandle, EpNum);
			if (!num)
			{
				UsbError.Error(ErrorCode.Win32Error, Marshal.GetLastWin32Error(), "ResetPipe", this);
			}
			return num;
		}

		public virtual ErrorCode Transfer(IntPtr buffer, int offset, int length, int timeout, out int transferLength)
		{
			return UsbTransfer.SyncTransfer(TransferContext, buffer, offset, length, timeout, out transferLength);
		}

		public virtual ErrorCode SubmitAsyncTransfer(object buffer, int offset, int length, int timeout, out UsbTransfer transferContext)
		{
			transferContext = CreateTransferContext();
			transferContext.Fill(buffer, offset, length, timeout);
			ErrorCode errorCode = transferContext.Submit();
			if (errorCode != ErrorCode.None)
			{
				transferContext.Dispose();
				transferContext = null;
				UsbError.Error(errorCode, 0, "SubmitAsyncTransfer Failed", this);
			}
			return errorCode;
		}

		public virtual ErrorCode SubmitAsyncTransfer(IntPtr buffer, int offset, int length, int timeout, out UsbTransfer transferContext)
		{
			transferContext = CreateTransferContext();
			transferContext.Fill(buffer, offset, length, timeout);
			ErrorCode errorCode = transferContext.Submit();
			if (errorCode != ErrorCode.None)
			{
				transferContext.Dispose();
				transferContext = null;
				UsbError.Error(errorCode, 0, "SubmitAsyncTransfer Failed", this);
			}
			return errorCode;
		}

		public UsbTransfer NewAsyncTransfer()
		{
			return CreateTransferContext();
		}

		public static bool LookupEndpointInfo(UsbConfigInfo currentConfigInfo, int altInterfaceID, byte endpointAddress, out UsbInterfaceInfo usbInterfaceInfo, out UsbEndpointInfo usbEndpointInfo)
		{
			bool flag = false;
			usbInterfaceInfo = null;
			usbEndpointInfo = null;
			foreach (UsbInterfaceInfo interfaceInfo in currentConfigInfo.InterfaceInfoList)
			{
				if (altInterfaceID != -1 && altInterfaceID != interfaceInfo.Descriptor.AlternateID)
				{
					continue;
				}
				foreach (UsbEndpointInfo endpointInfo in interfaceInfo.EndpointInfoList)
				{
					if ((endpointAddress & 0xF) == 0)
					{
						if ((endpointAddress & 0x80) == 0 && (endpointInfo.Descriptor.EndpointID & 0x80) == 0)
						{
							flag = true;
						}
						if ((endpointAddress & 0x80) != 0 && (endpointInfo.Descriptor.EndpointID & 0x80) != 0)
						{
							flag = true;
						}
					}
					else if (endpointInfo.Descriptor.EndpointID == endpointAddress)
					{
						flag = true;
					}
					if (flag)
					{
						usbInterfaceInfo = interfaceInfo;
						usbEndpointInfo = endpointInfo;
						return true;
					}
				}
			}
			return false;
		}

		public static bool LookupEndpointInfo(UsbConfigInfo currentConfigInfo, byte endpointAddress, out UsbInterfaceInfo usbInterfaceInfo, out UsbEndpointInfo usbEndpointInfo)
		{
			return LookupEndpointInfo(currentConfigInfo, -1, endpointAddress, out usbInterfaceInfo, out usbEndpointInfo);
		}

		public ErrorCode Transfer(object buffer, int offset, int length, int timeout, out int transferLength)
		{
			PinnedHandle pinnedHandle = new PinnedHandle(buffer);
			ErrorCode result = Transfer(pinnedHandle.Handle, offset, length, timeout, out transferLength);
			pinnedHandle.Dispose();
			return result;
		}

		private void DisposeAndRemoveFromList()
		{
			if (!mIsDisposed)
			{
				if (this is UsbEndpointReader { DataReceivedEnabled: not false } usbEndpointReader)
				{
					usbEndpointReader.DataReceivedEnabled = false;
				}
				Abort();
				mUsbDevice.ActiveEndpoints.RemoveFromList(this);
			}
			mIsDisposed = true;
		}

		private int ReadPipe(IntPtr pBuffer, int bufferLength, out int lengthTransferred, int isoPacketSize, IntPtr pOverlapped)
		{
			if (!mUsbApi.ReadPipe(this, pBuffer, bufferLength, out lengthTransferred, isoPacketSize, pOverlapped))
			{
				return Marshal.GetLastWin32Error();
			}
			return 0;
		}

		private int WritePipe(IntPtr pBuffer, int bufferLength, out int lengthTransferred, int isoPacketSize, IntPtr pOverlapped)
		{
			if (!mUsbApi.WritePipe(this, pBuffer, bufferLength, out lengthTransferred, isoPacketSize, pOverlapped))
			{
				return Marshal.GetLastWin32Error();
			}
			return 0;
		}
	}
}
