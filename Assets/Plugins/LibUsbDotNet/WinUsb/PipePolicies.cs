using System;
using System.Runtime.InteropServices;
using LibUsbDotNet.Main;
using LibUsbDotNet.WinUsb.Internal;

namespace LibUsbDotNet.WinUsb
{
	public class PipePolicies
	{
		private const int MAX_SIZE = 4;

		private readonly byte mEpNum;

		private readonly SafeHandle mUsbHandle;

		private IntPtr mBufferPtr = IntPtr.Zero;

		public bool AllowPartialReads
		{
			get
			{
				int valueLength = 1;
				Marshal.WriteByte(mBufferPtr, 0);
				if (GetPipePolicy(PipePolicyType.AllowPartialReads, ref valueLength, mBufferPtr))
				{
					return Marshal.ReadByte(mBufferPtr) != 0;
				}
				return false;
			}
			set
			{
				int valueLength = 1;
				byte val = (value ? ((byte)1) : ((byte)0));
				Marshal.WriteByte(mBufferPtr, val);
				SetPipePolicy(PipePolicyType.AllowPartialReads, valueLength, mBufferPtr);
			}
		}

		public bool ShortPacketTerminate
		{
			get
			{
				int valueLength = 1;
				Marshal.WriteByte(mBufferPtr, 0);
				if (GetPipePolicy(PipePolicyType.ShortPacketTerminate, ref valueLength, mBufferPtr))
				{
					return Marshal.ReadByte(mBufferPtr) != 0;
				}
				return false;
			}
			set
			{
				int valueLength = 1;
				byte val = (value ? ((byte)1) : ((byte)0));
				Marshal.WriteByte(mBufferPtr, val);
				SetPipePolicy(PipePolicyType.ShortPacketTerminate, valueLength, mBufferPtr);
			}
		}

		public bool AutoClearStall
		{
			get
			{
				int valueLength = 1;
				Marshal.WriteByte(mBufferPtr, 0);
				if (GetPipePolicy(PipePolicyType.AutoClearStall, ref valueLength, mBufferPtr))
				{
					return Marshal.ReadByte(mBufferPtr) != 0;
				}
				return false;
			}
			set
			{
				int valueLength = 1;
				byte val = (value ? ((byte)1) : ((byte)0));
				Marshal.WriteByte(mBufferPtr, val);
				SetPipePolicy(PipePolicyType.AutoClearStall, valueLength, mBufferPtr);
			}
		}

		public bool AutoFlush
		{
			get
			{
				int valueLength = 1;
				Marshal.WriteByte(mBufferPtr, 0);
				if (GetPipePolicy(PipePolicyType.AutoFlush, ref valueLength, mBufferPtr))
				{
					return Marshal.ReadByte(mBufferPtr) != 0;
				}
				return false;
			}
			set
			{
				int valueLength = 1;
				byte val = (value ? ((byte)1) : ((byte)0));
				Marshal.WriteByte(mBufferPtr, val);
				SetPipePolicy(PipePolicyType.AutoFlush, valueLength, mBufferPtr);
			}
		}

		public bool IgnoreShortPackets
		{
			get
			{
				int valueLength = 1;
				Marshal.WriteByte(mBufferPtr, 0);
				if (GetPipePolicy(PipePolicyType.IgnoreShortPackets, ref valueLength, mBufferPtr))
				{
					return Marshal.ReadByte(mBufferPtr) != 0;
				}
				return false;
			}
			set
			{
				int valueLength = 1;
				byte val = (value ? ((byte)1) : ((byte)0));
				Marshal.WriteByte(mBufferPtr, val);
				SetPipePolicy(PipePolicyType.IgnoreShortPackets, valueLength, mBufferPtr);
			}
		}

		public bool RawIo
		{
			get
			{
				int valueLength = 1;
				Marshal.WriteByte(mBufferPtr, 0);
				if (GetPipePolicy(PipePolicyType.RawIo, ref valueLength, mBufferPtr))
				{
					return Marshal.ReadByte(mBufferPtr) != 0;
				}
				return false;
			}
			set
			{
				int valueLength = 1;
				byte val = (value ? ((byte)1) : ((byte)0));
				Marshal.WriteByte(mBufferPtr, val);
				SetPipePolicy(PipePolicyType.RawIo, valueLength, mBufferPtr);
			}
		}

		public int PipeTransferTimeout
		{
			get
			{
				int valueLength = 4;
				Marshal.WriteInt32(mBufferPtr, 0);
				if (GetPipePolicy(PipePolicyType.PipeTransferTimeout, ref valueLength, mBufferPtr))
				{
					return Marshal.ReadInt32(mBufferPtr);
				}
				return -1;
			}
			set
			{
				int valueLength = 4;
				Marshal.WriteInt32(mBufferPtr, value);
				SetPipePolicy(PipePolicyType.PipeTransferTimeout, valueLength, mBufferPtr);
			}
		}

		public int MaxTransferSize
		{
			get
			{
				int valueLength = 4;
				Marshal.WriteInt32(mBufferPtr, 0);
				if (GetPipePolicy(PipePolicyType.MaximumTransferSize, ref valueLength, mBufferPtr))
				{
					return Marshal.ReadInt32(mBufferPtr);
				}
				return -1;
			}
		}

		internal PipePolicies(SafeHandle usbHandle, byte epNum)
		{
			mBufferPtr = Marshal.AllocCoTaskMem(4);
			mEpNum = epNum;
			mUsbHandle = usbHandle;
		}

		public override string ToString()
		{
			object[] args = new object[8] { AllowPartialReads, ShortPacketTerminate, AutoClearStall, AutoFlush, IgnoreShortPackets, RawIo, PipeTransferTimeout, MaxTransferSize };
			return string.Format("AllowPartialReads:{0}\r\nShortPacketTerminate:{1}\r\nAutoClearStall:{2}\r\nAutoFlush:{3}\r\nIgnoreShortPackets:{4}\r\nRawIO:{5}\r\nPipeTransferTimeout:{6}\r\nMaxTransferSize:{7}\r\n", args);
		}

		internal bool GetPipePolicy(PipePolicyType policyType, ref int valueLength, IntPtr pBuffer)
		{
			bool num = WinUsbAPI.WinUsb_GetPipePolicy(mUsbHandle, mEpNum, policyType, ref valueLength, pBuffer);
			if (!num)
			{
				UsbError.Error(ErrorCode.Win32Error, Marshal.GetLastWin32Error(), "GetPipePolicy", this);
			}
			return num;
		}

		internal bool SetPipePolicy(PipePolicyType policyType, int valueLength, IntPtr pBuffer)
		{
			bool num = WinUsbAPI.WinUsb_SetPipePolicy(mUsbHandle, mEpNum, policyType, valueLength, pBuffer);
			if (!num)
			{
				UsbError.Error(ErrorCode.Win32Error, Marshal.GetLastWin32Error(), "SetPipePolicy", this);
			}
			return num;
		}

		~PipePolicies()
		{
			if (mBufferPtr != IntPtr.Zero)
			{
				Marshal.FreeCoTaskMem(mBufferPtr);
			}
			mBufferPtr = IntPtr.Zero;
		}
	}
}
