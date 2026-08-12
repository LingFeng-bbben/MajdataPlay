using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Text;
using System.Threading;

namespace MajdataPlay.IO.Ports
{
    public abstract class SerialStream : Stream
    {
        public abstract string PortName { get; }
        public abstract int BaudRate { get; set; }

        public abstract int DataBits { get; set; }

        public abstract Parity Parity { get; set; }

        public abstract StopBits StopBits { get; set; }

        public abstract Handshake Handshake { get; set; }

        public abstract byte ParityReplace { get; set; }

        public abstract bool DtrEnable { get; set; }

        public abstract bool CtsHolding { get; }

        public abstract bool DsrHolding { get; }

        public abstract bool CDHolding { get; }

        public abstract bool BreakState { get; set; }

        public new abstract int ReadTimeout { get; set; }

        public new abstract int WriteTimeout { get; set; }

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
