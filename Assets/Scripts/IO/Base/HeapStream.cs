using MajdataPlay.Collections;
using MajdataPlay.Extensions;
using System;
using System.IO;
#nullable enable
namespace MajdataPlay.IO
{
    //public unsafe class HeapStream : Stream
    //{
    //    public override bool CanRead => true;
    //    public override bool CanSeek => true;
    //    public override bool CanWrite => _canWrite;
    //    public override long Length => _length;
    //    public override long Position
    //    {
    //        get => _position;
    //        set
    //        {
    //            if (!value.InRange(0, Length))
    //                throw new IndexOutOfRangeException();
    //        }
    //    }


    //    long _position = 0;
    //    readonly Heap<byte> _buffer;
    //    readonly bool _canWrite = true;
    //    readonly long _length;

    //    public HeapStream(long length)
    //    {
    //        _buffer = new Heap<byte>(length);
    //        _length = length;
    //    }
    //    public HeapStream(IntPtr pointer, long length)
    //    {
    //        _buffer = new Heap<byte>(pointer, length);
    //        _length = length;
    //    }
    //    public HeapStream(byte* pointer, long length)
    //    {
    //        _buffer = new Heap<byte>(pointer, length);
    //        _length = length;
    //    }

    //    public override void Write(byte[] buffer, int offset, int count)
    //    {
    //        if (!_canWrite)
    //            throw new NotSupportedException("Unsupport operation because this stream cannot be wrote");

    //        for (int i = offset; i < count; i++)
    //        {
    //            if (_position == Length)
    //                break;
    //            _buffer[_position++] = buffer[i];
    //        }
    //    }
    //    public override void Flush() { }
    //    public override int Read(byte[] buffer, int offset, int count)
    //    {
    //        if (Position == Length)
    //            return 0;
    //        var startAt = _position;
    //        for (int i = offset; i < count; i++)
    //        {
    //            if (_position == Length)
    //                break;
    //            buffer[i] = _buffer[_position++];
    //        }
    //        return (int)(_position - startAt);
    //    }
    //    public override long Seek(long offset, SeekOrigin origin)
    //    {
    //        switch (origin)
    //        {
    //            case SeekOrigin.Begin:
    //                _position = offset.Clamp(0, Length);
    //                break;
    //            case SeekOrigin.Current:
    //                {
    //                    var newPos = _position + offset;
    //                    newPos = newPos.Clamp(0, Length);
    //                    _position = newPos;
    //                }
    //                break;
    //            case SeekOrigin.End:
    //                {
    //                    var newPos = Length - offset;
    //                    newPos = newPos.Clamp(0, Length);
    //                    _position = newPos;
    //                }
    //                break;
    //        }
    //        return _position;
    //    }
    //    public Heap<byte> ToHeap() => _buffer;
    //    public override void SetLength(long value) => throw new NotSupportedException();
    //}
}
