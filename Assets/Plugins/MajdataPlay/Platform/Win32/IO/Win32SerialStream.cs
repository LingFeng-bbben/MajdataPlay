using MajdataPlay.IO.Ports;
using MajdataPlay.Platform.Win32.PInvoke;
using System;
using System.Buffers;
using System.IO;
using System.IO.Ports;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace MajdataPlay.Platform.Win32.IO
{
    public class Win32SerialStream : SerialStream
    {
        public override string PortName
        {
            get => _portName;
        }

        public override int BaudRate
        {
            get => (int)GetDCB().BaudRate;
            set
            {
                var dcb = GetDCB();
                dcb.BaudRate = (uint)value;
                SetDCB(ref dcb);
            }
        }

        public override int DataBits
        {
            get => GetDCB().ByteSize;
            set
            {
                var dcb = GetDCB();
                dcb.ByteSize = (byte)value;
                SetDCB(ref dcb);
            }
        }

        public override Parity Parity
        {
            get => (Parity)GetDCB().Parity;
            set
            {
                var dcb = GetDCB();
                dcb.Parity = (byte)value;
                SetFlag(ref dcb.Flags, 0x0002, value != Parity.None);
                SetDCB(ref dcb);
            }
        }

        public override StopBits StopBits
        {
            get => GetDCB().StopBits switch
            {
                0 => StopBits.One,
                1 => StopBits.OnePointFive,
                2 => StopBits.Two,
                _ => StopBits.None
            };
            set
            {
                var dcb = GetDCB();
                dcb.StopBits = value switch
                {
                    StopBits.One => 0,
                    StopBits.OnePointFive => 1,
                    StopBits.Two => 2,
                    _ => throw new ArgumentOutOfRangeException(nameof(value))
                };
                SetDCB(ref dcb);
            }
        }

        public override Handshake Handshake
        {
            get
            {
                var dcb = GetDCB();
                var cts = GetFlag(dcb.Flags, 0x0004); // fOutxCtsFlow (Bit 2)
                var xonoff = GetFlag(dcb.Flags, 0x0100) || GetFlag(dcb.Flags, 0x0200); // fOutX (Bit 8) || fInX (Bit 9)

                if (cts && xonoff)
                {
                    return Handshake.RequestToSendXOnXOff;
                }
                if (cts)
                {
                    return Handshake.RequestToSend;
                }
                if (xonoff)
                {
                    return Handshake.XOnXOff;
                }
                return Handshake.None;
            }
            set
            {
                var dcb = GetDCB();
                switch (value)
                {
                    case Handshake.None:
                        SetFlag(ref dcb.Flags, 0x0004, false); // fOutxCtsFlow = false
                        SetBits(ref dcb.Flags, 12, 0x3, 1u);   // fRtsControl = RTS_CONTROL_ENABLE
                        SetFlag(ref dcb.Flags, 0x0100, false); // fOutX = false
                        SetFlag(ref dcb.Flags, 0x0200, false); // fInX = false
                        break;
                    case Handshake.XOnXOff:
                        SetFlag(ref dcb.Flags, 0x0004, false);
                        SetBits(ref dcb.Flags, 12, 0x3, 1u);
                        SetFlag(ref dcb.Flags, 0x0100, true);  // fOutX = true
                        SetFlag(ref dcb.Flags, 0x0200, true);  // fInX = true
                        break;
                    case Handshake.RequestToSend:
                        SetFlag(ref dcb.Flags, 0x0004, true);  // fOutxCtsFlow = true
                        SetBits(ref dcb.Flags, 12, 0x3, 2u);   // fRtsControl = RTS_CONTROL_HANDSHAKE
                        SetFlag(ref dcb.Flags, 0x0100, false);
                        SetFlag(ref dcb.Flags, 0x0200, false);
                        break;
                    case Handshake.RequestToSendXOnXOff:
                        SetFlag(ref dcb.Flags, 0x0004, true);
                        SetBits(ref dcb.Flags, 12, 0x3, 2u);
                        SetFlag(ref dcb.Flags, 0x0100, true);
                        SetFlag(ref dcb.Flags, 0x0200, true);
                        break;
                }
                SetDCB(ref dcb);
            }
        }

        public override byte ParityReplace
        {
            get => (byte)GetDCB().ErrorChar;
            set
            {
                var dcb = GetDCB();
                dcb.ErrorChar = (char)value;
                SetFlag(ref dcb.Flags, 0x0400, value != 0); // fErrorChar (Bit 10)
                SetDCB(ref dcb);
            }
        }

        public override bool DtrEnable
        {
            get
            {
                var dcb = GetDCB();
                return GetBits(dcb.Flags, 4, 0x3) == 1; // fDtrControl == DTR_CONTROL_ENABLE
            }
            set
            {
                var dcb = GetDCB();
                SetBits(ref dcb.Flags, 4, 0x3, value ? 1u : 0u); // 1: Enable, 0: Disable
                SetDCB(ref dcb);
            }
        }

        public override bool CtsHolding
        {
            get => (GetModemStatus() & Win32API.IO.MS_CTS_ON) != 0;
        }

        public override bool DsrHolding
        {
            get => (GetModemStatus() & Win32API.IO.MS_DSR_ON) != 0;
        }

        public override bool CDHolding
        {
            get => (GetModemStatus() & Win32API.IO.MS_RLSD_ON) != 0;
        }

        public override bool BreakState
        {
            get => _breakState;
            set
            {
                if (value)
                {
                    if (!Win32API.IO.SetCommBreak(_handle))
                    {
                        ThrowLastWin32Error();
                    }
                }
                else
                {
                    if (!Win32API.IO.ClearCommBreak(_handle))
                    {
                        ThrowLastWin32Error();
                    }
                }
                _breakState = value;
            }
        }

        public override int ReadTimeout
        {
            get => _readTimeout;
            set
            {
                if (value < 0 && value != Timeout.Infinite)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }
                _readTimeout = value;
                ApplyTimeouts();
            }
        }

        public override int WriteTimeout
        {
            get => _writeTimeout;
            set
            {
                if (value < 0 && value != Timeout.Infinite)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }
                _writeTimeout = value;
                ApplyTimeouts();
            }
        }

        private bool _breakState;
        private int _readTimeout = Timeout.Infinite;
        private int _writeTimeout = Timeout.Infinite;

        private readonly IntPtr _handle;
        private readonly string _portName;
        private readonly ManualResetEvent _readEvent;
        private readonly ManualResetEvent _writeEvent;

        public Win32SerialStream(string portName)
        : this(portName, 9600, Parity.None, 8, StopBits.One) { }

        public Win32SerialStream(string portName, int baudRate)
            : this(portName, baudRate, Parity.None, 8, StopBits.One) { }

        public Win32SerialStream(string portName, int baudRate, Parity parity)
            : this(portName, baudRate, parity, 8, StopBits.One) { }

        public Win32SerialStream(string portName, int baudRate, Parity parity, int dataBits)
            : this(portName, baudRate, parity, dataBits, StopBits.One) { }

        public Win32SerialStream(string portName, int baudRate, Parity parity, int dataBits, StopBits stopBits)
        {
            var fullPortName = portName.StartsWith(@"\\.\") ? portName : $@"\\.\{portName}";

            _handle = Win32API.IO.CreateFile(
                fullPortName,
                Win32API.IO.GENERIC_READ | Win32API.IO.GENERIC_WRITE,
                0,
                IntPtr.Zero,
                Win32API.IO.OPEN_EXISTING,
                Win32API.IO.FILE_FLAG_OVERLAPPED,
                IntPtr.Zero);

            if (_handle == new IntPtr(-1))
            {
                var error = Marshal.GetLastWin32Error();
                throw new IOException($"Failed to open port {portName}. Win32 Error: {error}");
            }
            _portName = portName;

            var dcb = new Win32API.IO.DCB();
            dcb.DCBlength = (uint)Marshal.SizeOf(dcb);
            if (Win32API.IO.GetCommState(_handle, ref dcb))
            {
                dcb.BaudRate = (uint)baudRate;
                dcb.ByteSize = (byte)dataBits;
                dcb.Parity = (byte)parity; // NOPARITY
                dcb.StopBits = (byte)stopBits; // ONESTOPBIT
                Win32API.IO.SetCommState(_handle, ref dcb);
            }

            var timeouts = new Win32API.IO.COMMTIMEOUTS
            {
                ReadIntervalTimeout = 0,
                ReadTotalTimeoutMultiplier = 0,
                ReadTotalTimeoutConstant = 0,
                WriteTotalTimeoutMultiplier = 0,
                WriteTotalTimeoutConstant = 0
            };
            Win32API.IO.SetCommTimeouts(_handle, ref timeouts);

            _readEvent = new ManualResetEvent(false);
            _writeEvent = new ManualResetEvent(false);
        }



        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            if (buffer.IsEmpty)
            {
                return new ValueTask<int>(0);
            }
            var resource = CreateWin32Resource(buffer, _readEvent);

            _readEvent.Reset();

            var bytesRead = 0U;
            var success = Win32API.IO.ReadFile(_handle, resource.BufferPtr, (uint)buffer.Length, out bytesRead, resource.OverlappedPtr);

            if (success)
            {
                CleanUp(resource);
                return new ValueTask<int>((int)bytesRead);
            }

            var error = Marshal.GetLastWin32Error();
            if (error != Win32API.IO.ERROR_IO_PENDING)
            {
                CleanUp(resource);
                throw new IOException($"ReadFile failed. Error: {error}");
            }

            return new ValueTask<int>(WaitOverlappedAsync(_handle, resource, _readEvent, cancellationToken));
        }
        public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken)
        {
            if (buffer.IsEmpty)
            {
                return new ValueTask();
            }
            var resource = CreateWin32Resource(buffer, _writeEvent);

            _writeEvent.Reset();

            var bytesWritten = 0U;
            var success = Win32API.IO.WriteFile(_handle, resource.BufferPtr, (uint)buffer.Length, out bytesWritten, resource.OverlappedPtr);

            if (success)
            {
                CleanUp(resource);
                return new ValueTask();
            }

            var error = Marshal.GetLastWin32Error();
            if (error != Win32API.IO.ERROR_IO_PENDING)
            {
                CleanUp(resource);
                throw new IOException($"WriteFile failed. Error: {error}");
            }

            return new ValueTask(WaitOverlappedAsync(_handle, resource, _writeEvent, cancellationToken));
        }

        
        public override int Read(byte[] buffer, int offset, int count)
        {
            var mBuffer = buffer.AsMemory(offset, count);
            var resource = CreateWin32Resource(mBuffer, _readEvent);
            try
            {
                _readEvent.Reset();

                if (Win32API.IO.ReadFile(_handle, resource.BufferPtr, (uint)count, out uint bytesRead, resource.OverlappedPtr))
                {
                    return (int)bytesRead;
                }

                int error = Marshal.GetLastWin32Error();
                if (error == Win32API.IO.ERROR_IO_PENDING)
                {
                    if (Win32API.IO.GetOverlappedResult(_handle, resource.OverlappedPtr, out bytesRead, true))
                    {
                        return (int)bytesRead;
                    }
                    error = Marshal.GetLastWin32Error();
                }

                throw new IOException($"Synchronous ReadFile failed. Win32 Error: {error}");
            }
            finally
            {
                CleanUp(resource);
            }
        }
        public override void Write(byte[] buffer, int offset, int count)
        {
            var mBuffer = buffer.AsMemory(offset, count);
            var resource = CreateWin32Resource(mBuffer, _writeEvent);

            try
            {
                _writeEvent.Reset();

                if (Win32API.IO.WriteFile(_handle, resource.BufferPtr, (uint)count, out uint bytesWritten, resource.OverlappedPtr))
                {
                    return;
                }

                var error = Marshal.GetLastWin32Error();
                if (error == Win32API.IO.ERROR_IO_PENDING)
                {
                    if (Win32API.IO.GetOverlappedResult(_handle, resource.OverlappedPtr, out bytesWritten, true))
                    {
                        return;
                    }
                    error = Marshal.GetLastWin32Error();
                }

                throw new IOException($"Synchronous WriteFile failed. Win32 Error: {error}");
            }
            finally
            {
                CleanUp(resource);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _readEvent?.Dispose();
                _writeEvent?.Dispose();
            }

            if (_handle != IntPtr.Zero && _handle != new IntPtr(-1))
            {
                Win32API.IO.CloseHandle(_handle);
            }

            base.Dispose(disposing);
        }
        public override void Flush() {  }


        private Task<int> WaitOverlappedAsync(IntPtr handle, Win32Resource resource, ManualResetEvent waitEvent, CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
            var pOverlapped = resource.OverlappedPtr;

            CancellationTokenRegistration ctr = default;
            if (cancellationToken.CanBeCanceled)
            {
                ctr = cancellationToken.Register(() => Win32API.IO.CancelIoEx(handle, pOverlapped));
            }

            RegisteredWaitHandle registeredWait = null;
            registeredWait = ThreadPool.RegisterWaitForSingleObject(
                waitEvent,
                (state, timedOut) =>
                {
                    try
                    {
                        // I/O 完成，获取实际传输的字节数
                        if (Win32API.IO.GetOverlappedResult(handle, pOverlapped, out uint transferred, false))
                        {
                            tcs.TrySetResult((int)transferred);
                        }
                        else
                        {
                            int err = Marshal.GetLastWin32Error();
                            if (err == Win32API.IO.ERROR_OPERATION_ABORTED)
                            {
                                tcs.TrySetCanceled();
                            }
                            else
                            {
                                tcs.TrySetException(new IOException($"Overlapped I/O failed. Error: {err}"));
                            }
                        }
                    }
                    finally
                    {
                        ctr.Dispose();
                        CleanUp(resource);
                        registeredWait?.Unregister(null);
                    }
                },
                null,
                -1, // Infinite timeout
                true // execute only once
            );

            return tcs.Task;
        }
        private void CleanUp(Win32Resource resource)
        {
            resource.BufferHandle.Dispose();
            var pOverlapped = resource.OverlappedPtr;
            if (pOverlapped != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(pOverlapped);
            }
        }
        private Win32Resource CreateWin32Resource(ReadOnlyMemory<byte> buffer, ManualResetEvent waitEvent)
        {
            return CreateWin32Resource(MemoryMarshal.AsMemory(buffer), waitEvent);
        }
        private unsafe Win32Resource CreateWin32Resource(Memory<byte> buffer, ManualResetEvent waitEvent)
        {
            var bufferHandle = buffer.Pin();
            var pBuffer = (IntPtr)bufferHandle.Pointer;

            var pOverlapped = Marshal.AllocHGlobal(sizeof(Win32API.IO.OVERLAPPED));
            var overlapped = new Win32API.IO.OVERLAPPED 
            { 
                hEvent = waitEvent.SafeWaitHandle.DangerousGetHandle() 
            };
            Marshal.StructureToPtr(overlapped, pOverlapped, false);

            return new()
            {
                BufferHandle = bufferHandle,
                BufferPtr = pBuffer,
                OverlappedPtr = pOverlapped
            };
        }

        #region Helper
        private void ApplyTimeouts()
        {
            var timeouts = default(Win32API.IO.COMMTIMEOUTS);

            if (_readTimeout == Timeout.Infinite)
            {
                timeouts.ReadIntervalTimeout = 0;
                timeouts.ReadTotalTimeoutMultiplier = 0;
                timeouts.ReadTotalTimeoutConstant = 0;
            }
            else if (_readTimeout == 0)
            {
                timeouts.ReadIntervalTimeout = 0xFFFFFFFF; // MAXDWORD
                timeouts.ReadTotalTimeoutMultiplier = 0;
                timeouts.ReadTotalTimeoutConstant = 0;
            }
            else
            {
                timeouts.ReadIntervalTimeout = 0xFFFFFFFF;
                timeouts.ReadTotalTimeoutMultiplier = 0xFFFFFFFF;
                timeouts.ReadTotalTimeoutConstant = (uint)_readTimeout;
            }

            if (_writeTimeout == Timeout.Infinite)
            {
                timeouts.WriteTotalTimeoutMultiplier = 0;
                timeouts.WriteTotalTimeoutConstant = 0;
            }
            else
            {
                timeouts.WriteTotalTimeoutMultiplier = 0;
                timeouts.WriteTotalTimeoutConstant = (uint)_writeTimeout;
            }

            if (!Win32API.IO.SetCommTimeouts(_handle, ref timeouts))
            {
                ThrowLastWin32Error();
            }
        }

        private Win32API.IO.DCB GetDCB()
        {
            var dcb = new Win32API.IO.DCB();
            dcb.DCBlength = (uint)Marshal.SizeOf(dcb);
            if (!Win32API.IO.GetCommState(_handle, ref dcb))
            {
                ThrowLastWin32Error();
            }
            return dcb;
        }

        private void SetDCB(ref Win32API.IO.DCB dcb)
        {
            if (!Win32API.IO.SetCommState(_handle, ref dcb))
            {
                ThrowLastWin32Error();
            }
        }

        private uint GetModemStatus()
        {
            if (!Win32API.IO.GetCommModemStatus(_handle, out uint status))
            {
                ThrowLastWin32Error();
            }
            return status;
        }

        private static void ThrowLastWin32Error()
        {
            int error = Marshal.GetLastWin32Error();
            throw new IOException($"Serial operation failed. Win32 Error Code: {error}");
        }

        private static bool GetFlag(uint flags, uint mask)
        {
            return (flags & mask) != 0;
        }

        private static void SetFlag(ref uint flags, uint mask, bool value)
        {
            if (value)
            {
                flags |= mask;
            }
            else
            {
                flags &= ~mask;
            }
        }

        private static uint GetBits(uint flags, int shift, uint mask)
        {
            return (flags >> shift) & mask;
        }

        private static void SetBits(ref uint flags, int shift, uint mask, uint value)
        {
            flags = (flags & ~(mask << shift)) | ((value & mask) << shift);
        }
        #endregion

        struct Win32Resource
        {
            public MemoryHandle BufferHandle { get; init; }
            public IntPtr BufferPtr { get; init; }
            public IntPtr OverlappedPtr { get; init; }
        }
    }
}
