using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MajdataPlay.Extensions
{
    internal static class SerialPortExtensions
    {
        public static int Read(this SerialPort serial, Span<byte> buffer)
        {
            var byte2Read = serial.BytesToRead;
            var read = 0;
            for (; read < buffer.Length; read++)
            {
                if (read == byte2Read)
                    break;
                buffer[read] = (byte)serial.ReadByte();
            }
            return read;
        }
        public static void Write(this SerialPort serial, ReadOnlySpan<byte> buffer)
        {
            var baseStream = serial.BaseStream;
            baseStream.Write(buffer);
        }
    }
}
