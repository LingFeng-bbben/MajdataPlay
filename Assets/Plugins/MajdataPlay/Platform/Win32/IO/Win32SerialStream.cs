using MajdataPlay.Platform.Win32.PInvoke;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MajdataPlay.Platform.Win32.IO
{
    public class Win32SerialStream : Stream
    {
        private readonly IntPtr _handle;
        private readonly ManualResetEvent _readEvent;
        private readonly ManualResetEvent _writeEvent;

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => true;
        public override long Length => throw new NotSupportedException();
        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        /// <summary>
        /// 初始化串口流
        /// </summary>
        /// <param name="portName">串口名，如 "COM3"</param>
        /// <param name="baudRate">波特率</param>
        public Win32SerialStream(string portName, int baudRate = 9600)
        {
            // 1. 打开串口，必须带有 FILE_FLAG_OVERLAPPED 标志以支持异步事件
            string fullPortName = portName.StartsWith(@"\\.\") ? portName : $@"\\.\{portName}";

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
                int error = Marshal.GetLastWin32Error();
                throw new IOException($"Failed to open port {portName}. Win32 Error: {error}");
            }

            // 2. 初始化串口配置 (DCB)
            Win32API.IO.DCB dcb = new Win32API.IO.DCB();
            dcb.DCBlength = (uint)Marshal.SizeOf(dcb);
            if (Win32API.IO.GetCommState(_handle, ref dcb))
            {
                dcb.BaudRate = (uint)baudRate;
                dcb.ByteSize = 8;
                dcb.Parity = 0; // NOPARITY
                dcb.StopBits = 0; // ONESTOPBIT
                Win32API.IO.SetCommState(_handle, ref dcb);
            }

            // 3. 配置超时时间
            Win32API.IO.COMMTIMEOUTS timeouts = new Win32API.IO.COMMTIMEOUTS
            {
                ReadIntervalTimeout = 0,
                ReadTotalTimeoutMultiplier = 0,
                ReadTotalTimeoutConstant = 0,
                WriteTotalTimeoutMultiplier = 0,
                WriteTotalTimeoutConstant = 0
            };
            Win32API.IO.SetCommTimeouts(_handle, ref timeouts);

            // 4. 创建用于 Overlapped I/O 的事件句柄
            _readEvent = new ManualResetEvent(false);
            _writeEvent = new ManualResetEvent(false);
        }

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (buffer == null) throw new ArgumentNullException(nameof(buffer));
            if (offset < 0 || count < 0 || offset + count > buffer.Length) throw new ArgumentOutOfRangeException();

            // 锁住 buffer，防止在非托管写入时被 GC 移动
            GCHandle gcBuffer = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            IntPtr pBuffer = gcBuffer.AddrOfPinnedObject() + offset;

            // 分配并初始化 OVERLAPPED 结构
            IntPtr pOverlapped = Marshal.AllocHGlobal(Marshal.SizeOf<Win32API.IO.OVERLAPPED>());
            var overlapped = new Win32API.IO.OVERLAPPED { hEvent = _readEvent.SafeWaitHandle.DangerousGetHandle() };
            Marshal.StructureToPtr(overlapped, pOverlapped, false);

            _readEvent.Reset();
            uint bytesRead = 0;

            bool success = Win32API.IO.ReadFile(_handle, pBuffer, (uint)count, out bytesRead, pOverlapped);
            if (success)
            {
                // 立即完成
                Cleanup(gcBuffer, pOverlapped);
                return (int)bytesRead;
            }

            int error = Marshal.GetLastWin32Error();
            if (error != Win32API.IO.ERROR_IO_PENDING)
            {
                Cleanup(gcBuffer, pOverlapped);
                throw new IOException($"ReadFile failed. Error: {error}");
            }

            // 挂起状态：等待 Win32 Event 触发
            return await WaitOverlappedAsync(_handle, pOverlapped, _readEvent, gcBuffer, cancellationToken);
        }

        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (buffer == null) throw new ArgumentNullException(nameof(buffer));
            if (offset < 0 || count < 0 || offset + count > buffer.Length) throw new ArgumentOutOfRangeException();

            GCHandle gcBuffer = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            IntPtr pBuffer = gcBuffer.AddrOfPinnedObject() + offset;

            IntPtr pOverlapped = Marshal.AllocHGlobal(Marshal.SizeOf<Win32API.IO.OVERLAPPED>());
            var overlapped = new Win32API.IO.OVERLAPPED { hEvent = _writeEvent.SafeWaitHandle.DangerousGetHandle() };
            Marshal.StructureToPtr(overlapped, pOverlapped, false);

            _writeEvent.Reset();
            uint bytesWritten = 0;

            bool success = Win32API.IO.WriteFile(_handle, pBuffer, (uint)count, out bytesWritten, pOverlapped);
            if (success)
            {
                Cleanup(gcBuffer, pOverlapped);
                return;
            }

            int error = Marshal.GetLastWin32Error();
            if (error != Win32API.IO.ERROR_IO_PENDING)
            {
                Cleanup(gcBuffer, pOverlapped);
                throw new IOException($"WriteFile failed. Error: {error}");
            }

            await WaitOverlappedAsync(_handle, pOverlapped, _writeEvent, gcBuffer, cancellationToken);
        }

        /// <summary>
        /// 将 Win32 Event 转换为 Task
        /// </summary>
        private Task<int> WaitOverlappedAsync(IntPtr handle, IntPtr pOverlapped, ManualResetEvent waitEvent, GCHandle gcBuffer, CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);

            // 如果传入了取消令牌，注册取消 IO API
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
                                tcs.TrySetCanceled();
                            else
                                tcs.TrySetException(new IOException($"Overlapped I/O failed. Error: {err}"));
                        }
                    }
                    finally
                    {
                        // 释放资源
                        ctr.Dispose();
                        Cleanup(gcBuffer, pOverlapped);
                        registeredWait?.Unregister(null);
                    }
                },
                null,
                -1, // Infinite timeout
                true // 执行一次
            );

            return tcs.Task;
        }

        private void Cleanup(GCHandle gcBuffer, IntPtr pOverlapped)
        {
            if (gcBuffer.IsAllocated) gcBuffer.Free();
            if (pOverlapped != IntPtr.Zero) Marshal.FreeHGlobal(pOverlapped);
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

        // -- 以下为 Stream 基类的同步抽象，强制包装为异步 --
        public override int Read(byte[] buffer, int offset, int count) => ReadAsync(buffer, offset, count).GetAwaiter().GetResult();
        public override void Write(byte[] buffer, int offset, int count) => WriteAsync(buffer, offset, count).GetAwaiter().GetResult();
        public override void Flush() { /* Serial port relies on OS buffers */ }
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
    }
}
