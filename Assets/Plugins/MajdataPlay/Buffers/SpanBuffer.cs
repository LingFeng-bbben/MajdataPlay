using System;
using System.Collections.Generic;
using System.Text;

namespace MajdataPlay.Buffers
{
    public ref struct SpanBuffer
    {
        public ReadOnlySpan<byte> Data
        {
            get => _buffer.Slice(0, _writeIndex);
        }
        public bool IsEmpty
        {
            get => _writeIndex == 0;
        }


        private int _writeIndex;
        private readonly Span<byte> _buffer;

        public SpanBuffer(Span<byte> buffer)
        {
            _buffer = buffer;
            _writeIndex = 0;
        }

        public int Write(scoped ReadOnlySpan<byte> data)
        {
            if (data.IsEmpty)
            {
                return 0;
            }
            if (data.Length + _writeIndex > _buffer.Length)
            {
                data = data.Slice(0, _buffer.Length - _writeIndex);
            }
            data.CopyTo(_buffer.Slice(_writeIndex));
            _writeIndex += data.Length;

            return data.Length;
        }

        public void Skip(int count)
        {
            if (count <= 0)
            {
                return;
            }
            else if (count >= _writeIndex)
            {
                Clear();
                return;
            }

            var b2 = _buffer.Slice(count, _writeIndex - count);
            b2.CopyTo(_buffer);
            _writeIndex -= count;
        }

        public void Clear()
        {
            _writeIndex = 0;
        }
    }
}
