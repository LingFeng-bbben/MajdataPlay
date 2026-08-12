using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MajdataPlay.IO
{
    internal abstract class SerialStream : Stream
    {
        public sealed override bool CanRead => true;
        public sealed override bool CanSeek => false;
        public sealed override bool CanWrite => true;
        public sealed override long Length => throw new NotSupportedException();
        public sealed override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public sealed override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public sealed override void SetLength(long value) => throw new NotSupportedException();
    }
}
