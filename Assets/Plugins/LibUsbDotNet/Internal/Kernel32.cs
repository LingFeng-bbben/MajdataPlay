using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using Microsoft.Win32.SafeHandles;
#pragma warning disable CS0618 // Legacy Win32 interop signatures require UnmanagedType.AsAny.

namespace LibUsbDotNet.Internal
{
	[SuppressUnmanagedCodeSecurity]
	internal static class Kernel32
	{
		private const int FORMAT_MESSAGE_FROM_SYSTEM = 4096;

		private static readonly StringBuilder m_sbSysMsg = new StringBuilder(1024);

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		public static extern SafeFileHandle CreateFile(string fileName, [MarshalAs(UnmanagedType.U4)] NativeFileAccess fileAccess, [MarshalAs(UnmanagedType.U4)] NativeFileShare fileShare, IntPtr securityAttributes, [MarshalAs(UnmanagedType.U4)] NativeFileMode creationDisposition, NativeFileFlag flags, IntPtr template);

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		private static extern int FormatMessage(int dwFlags, IntPtr lpSource, int dwMessageId, int dwLanguageId, [Out] StringBuilder lpBuffer, int nSize, IntPtr lpArguments);

		[DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
		public static extern bool GetOverlappedResult(SafeHandle hDevice, IntPtr lpOverlapped, out int lpNumberOfBytesTransferred, bool bWait);

		public static string FormatSystemMessage(int dwMessageId)
		{
			lock (m_sbSysMsg)
			{
				int num = FormatMessage(4096, IntPtr.Zero, dwMessageId, CultureInfo.CurrentCulture.LCID, m_sbSysMsg, m_sbSysMsg.Capacity - 1, IntPtr.Zero);
				if (num > 0)
				{
					return m_sbSysMsg.ToString(0, num);
				}
				return null;
			}
		}

		[DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
		public static extern bool DeviceIoControl(SafeHandle hDevice, int IoControlCode, [In][MarshalAs(UnmanagedType.AsAny)] object InBuffer, int nInBufferSize, IntPtr OutBuffer, int nOutBufferSize, out int pBytesReturned, IntPtr pOverlapped);

		[DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
		public static extern bool DeviceIoControl(SafeHandle hDevice, int IoControlCode, [In][MarshalAs(UnmanagedType.AsAny)] object InBuffer, int nInBufferSize, [Out][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 6)] byte[] OutBuffer, int nOutBufferSize, out int pBytesReturned, IntPtr Overlapped);

		[DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
		public static extern bool DeviceIoControl(SafeHandle hDevice, int IoControlCode, IntPtr InBuffer, int nInBufferSize, IntPtr OutBuffer, int nOutBufferSize, out int pBytesReturned, IntPtr Overlapped);

		[DllImport("kernel32.dll", CharSet = CharSet.Ansi, EntryPoint = "DeviceIoControl", SetLastError = true)]
		public static extern bool DeviceIoControlAsObject(SafeHandle hDevice, int IoControlCode, [In][MarshalAs(UnmanagedType.AsAny)] object InBuffer, int nInBufferSize, IntPtr OutBuffer, int nOutBufferSize, ref int pBytesReturned, IntPtr Overlapped);
	}
}
