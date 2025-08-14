using MajdataPlay.Buffers;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MajdataPlay.IO
{
    internal static class SerialPortExtensions
    {
        public static int Read(this SerialPort serial, Span<byte> buffer)
        {
            if(buffer.IsEmpty || serial.BytesToRead == 0)
            {
                return 0;
            }
            var byte2Read = Math.Min(serial.BytesToRead , buffer.Length);
            var rentedBuffer = Pool<byte>.RentArray(byte2Read);
            try
            {
                serial.Read(rentedBuffer, 0, byte2Read);
                rentedBuffer.AsSpan(0, byte2Read)
                            .CopyTo(buffer);
            }
            finally
            {
                Pool<byte>.ReturnArray(rentedBuffer);
            }

            return byte2Read;
        }
        public static void Write(this SerialPort serial, ReadOnlySpan<byte> buffer)
        {
            if(buffer.IsEmpty)
            {
                return;
            }
            var rentedBuffer = Pool<byte>.RentArray(buffer.Length);
            try
            {
                buffer.CopyTo(rentedBuffer);
                serial.Write(rentedBuffer, 0, buffer.Length);
            }
            finally
            {
                Pool<byte>.ReturnArray(rentedBuffer);
            }
        }
    }
}
