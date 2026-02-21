using System;
using System.Runtime.InteropServices;
using System.Threading;
using LibUsbDotNet.Main;

namespace LibUsbDotNet.Internal
{
	internal class SafeOverlapped : SafeContextHandle
	{
		private static readonly int FieldOffsetEventHandle = Marshal.OffsetOf(typeof(NativeOverlapped), "EventHandle").ToInt32();

		private static readonly int FieldOffsetInternalHigh = Marshal.OffsetOf(typeof(NativeOverlapped), "InternalHigh").ToInt32();

		private static readonly int FieldOffsetInternalLow = Marshal.OffsetOf(typeof(NativeOverlapped), "InternalLow").ToInt32();

		private static readonly int FieldOffsetOffsetHigh = Marshal.OffsetOf(typeof(NativeOverlapped), "OffsetHigh").ToInt32();

		private static readonly int FieldOffsetOffsetLow = Marshal.OffsetOf(typeof(NativeOverlapped), "OffsetLow").ToInt32();

		public IntPtr InternalLow
		{
			get
			{
				return Marshal.ReadIntPtr(DangerousGetHandle(), FieldOffsetInternalLow);
			}
			set
			{
				Marshal.WriteIntPtr(DangerousGetHandle(), FieldOffsetInternalLow, value);
			}
		}

		public IntPtr InternalHigh
		{
			get
			{
				return Marshal.ReadIntPtr(DangerousGetHandle(), FieldOffsetInternalHigh);
			}
			set
			{
				Marshal.WriteIntPtr(DangerousGetHandle(), FieldOffsetInternalHigh, value);
			}
		}

		public int OffsetLow
		{
			get
			{
				return Marshal.ReadInt32(DangerousGetHandle(), FieldOffsetOffsetLow);
			}
			set
			{
				Marshal.WriteInt32(DangerousGetHandle(), FieldOffsetOffsetLow, value);
			}
		}

		public int OffsetHigh
		{
			get
			{
				return Marshal.ReadInt32(DangerousGetHandle(), FieldOffsetOffsetHigh);
			}
			set
			{
				Marshal.WriteInt32(DangerousGetHandle(), FieldOffsetOffsetHigh, value);
			}
		}

		public IntPtr EventHandle
		{
			get
			{
				return Marshal.ReadIntPtr(DangerousGetHandle(), FieldOffsetEventHandle);
			}
			set
			{
				Marshal.WriteIntPtr(DangerousGetHandle(), FieldOffsetEventHandle, value);
			}
		}

		public IntPtr GlobalOverlapped => DangerousGetHandle();

		public SafeOverlapped()
			: base(Marshal.AllocHGlobal(Marshal.SizeOf(typeof(NativeOverlapped))))
		{
		}

		protected override bool ReleaseHandle()
		{
			if (!IsInvalid)
			{
				Marshal.FreeHGlobal(handle);
				SetHandleAsInvalid();
			}
			return true;
		}

		public void Init(IntPtr hEventOverlapped)
		{
			EventHandle = hEventOverlapped;
			InternalLow = IntPtr.Zero;
			InternalHigh = IntPtr.Zero;
			OffsetLow = 0;
			OffsetHigh = 0;
		}
	}
}
