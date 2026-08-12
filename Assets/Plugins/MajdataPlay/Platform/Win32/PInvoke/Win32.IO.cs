using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace MajdataPlay.Platform.Win32.PInvoke
{
    public static partial class Win32API
    {
        public static class IO
        {
            public const uint GENERIC_READ = 0x80000000;
            public const uint GENERIC_WRITE = 0x40000000;
            public const uint OPEN_EXISTING = 3;
            public const uint FILE_FLAG_OVERLAPPED = 0x40000000;
            public const int ERROR_IO_PENDING = 997;
            public const int ERROR_OPERATION_ABORTED = 995;

            public const uint MS_CTS_ON = 0x0010;
            public const uint MS_DSR_ON = 0x0020;
            public const uint MS_RING_ON = 0x0040;
            public const uint MS_RLSD_ON = 0x0080; // CD (Carrier Detect)

            [StructLayout(LayoutKind.Sequential)]
            public struct OVERLAPPED
            {
                public UIntPtr Internal;
                public UIntPtr InternalHigh;
                public uint Offset;
                public uint OffsetHigh;
                public IntPtr hEvent;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct COMMTIMEOUTS
            {
                public uint ReadIntervalTimeout;
                public uint ReadTotalTimeoutMultiplier;
                public uint ReadTotalTimeoutConstant;
                public uint WriteTotalTimeoutMultiplier;
                public uint WriteTotalTimeoutConstant;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct DCB
            {
                public uint DCBlength;
                public uint BaudRate;
                public uint Flags; // Bitfield not fully implemented here for brevity, assuming defaults work
                public ushort wReserved;
                public ushort XonLim;
                public ushort XoffLim;
                public byte ByteSize;
                public byte Parity;
                public byte StopBits;
                public char XonChar;
                public char XoffChar;
                public char ErrorChar;
                public char EofChar;
                public char EvtChar;
                public ushort wReserved1;
            }

            [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
            public static extern IntPtr CreateFile(
                string lpFileName, uint dwDesiredAccess, uint dwShareMode,
                IntPtr SecurityAttributes, uint dwCreationDisposition, uint dwFlagsAndAttributes, IntPtr hTemplateFile);

            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern bool CloseHandle(IntPtr hObject);

            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern bool ReadFile(
                IntPtr hFile, IntPtr lpBuffer, uint nNumberOfBytesToRead,
                out uint lpNumberOfBytesRead, IntPtr lpOverlapped);

            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern bool WriteFile(
                IntPtr hFile, IntPtr lpBuffer, uint nNumberOfBytesToWrite,
                out uint lpNumberOfBytesWritten, IntPtr lpOverlapped);

            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern bool GetOverlappedResult(
                IntPtr hFile, IntPtr lpOverlapped, out uint lpNumberOfBytesTransferred, bool bWait);

            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern bool CancelIoEx(IntPtr hFile, IntPtr lpOverlapped);

            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern bool GetCommState(IntPtr hFile, ref DCB lpDCB);

            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern bool SetCommState(IntPtr hFile, ref DCB lpDCB);

            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern bool SetCommTimeouts(IntPtr hFile, ref COMMTIMEOUTS lpCommTimeouts);

            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern bool GetCommModemStatus(IntPtr hFile, out uint lpModemStat);

            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern bool SetCommBreak(IntPtr hFile);

            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern bool ClearCommBreak(IntPtr hFile);

            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern bool GetCommTimeouts(IntPtr hFile, out COMMTIMEOUTS lpCommTimeouts);
        }
    }
}
