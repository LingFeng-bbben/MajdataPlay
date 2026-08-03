using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;
#nullable enable
namespace MajdataPlay.Net.Curl.Core
{
    internal class CurlResponseStream : Stream
    {
        MemoryChunk _currentChunk;
        Exception? _abortException;

        readonly BlockingCollection<MemoryChunk> _bufferQueue = new();        

        public void WriteChunk(ReadOnlySpan<byte> chunk)
        {
            if (chunk.IsEmpty)
            {
                return;
            }
            var buffer = ArrayPool<byte>.Shared.Rent(chunk.Length);
            var chunkInfo = new MemoryChunk()
            {
                Buffer = buffer,
                Length = chunk.Length,
                Offset = 0
            };
            chunk.CopyTo(buffer);
            _bufferQueue.Add(chunkInfo);
        }

        public void CompleteWriting()
        {
            _bufferQueue.CompleteAdding();
        }
        public void Abort(Exception ex)
        {
            _abortException = ex;
            _bufferQueue.CompleteAdding();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_abortException != null)
            {
                ExceptionDispatchInfo.Capture(_abortException).Throw();
            }
            var totalBytesRead = 0;

            while (count > 0)
            {
                if (_currentChunk.IsCompleted)
                {
                    ArrayPool<byte>.Shared.Return(_currentChunk.Buffer ?? Array.Empty<byte>());
                    _currentChunk = default;
                    if (_bufferQueue.IsCompleted)
                    {
                        break;
                    }

                    try
                    {
                        _currentChunk = _bufferQueue.Take();
                    }
                    catch (InvalidOperationException)
                    {
                        if (_abortException != null)
                        {
                            ExceptionDispatchInfo.Capture(_abortException).Throw();
                        }
                        break;
                    }
                }
                ref var chunk = ref _currentChunk;
                var bytesToCopy = Math.Min(count, chunk.Length - chunk.Offset);
                Buffer.BlockCopy(chunk.Buffer, chunk.Offset, buffer, offset, bytesToCopy);

                offset += bytesToCopy;
                count -= bytesToCopy;
                totalBytesRead += bytesToCopy;
                chunk.Offset += bytesToCopy;

                if (totalBytesRead > 0)
                {
                    break;
                }
            }

            return totalBytesRead;
        }

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public override void Flush() { }
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (!_bufferQueue.IsAddingCompleted)
                {
                    _bufferQueue.CompleteAdding();
                }
                while (_bufferQueue.TryTake(out var chunk))
                {
                    if (chunk.Buffer != null)
                    {
                        ArrayPool<byte>.Shared.Return(chunk.Buffer);
                    }
                }
                ArrayPool<byte>.Shared.Return(_currentChunk.Buffer ?? Array.Empty<byte>());
                _currentChunk = default;
                _bufferQueue.Dispose();
            }
            base.Dispose(disposing);
        }

        struct MemoryChunk
        {
            public byte[] Buffer { get; init; }
            public int Length { get; init; }
            public int Offset { get; set; }
            public bool IsCompleted
            {
                get => Offset >= Length;
            }
        }
    }
}
