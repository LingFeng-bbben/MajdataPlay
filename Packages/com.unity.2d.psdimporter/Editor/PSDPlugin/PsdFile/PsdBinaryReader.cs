/////////////////////////////////////////////////////////////////////////////////
//
// Photoshop PSD FileType Plugin for Paint.NET
// http://psdplugin.codeplex.com/
//
// This software is provided under the MIT License:
//   Copyright (c) 2006-2007 Frank Blumenberg
//   Copyright (c) 2010-2013 Tao Yue
//
// Portions of this file are provided under the BSD 3-clause License:
//   Copyright (c) 2006, Jonas Beckeman
//
// See LICENSE.txt for complete licensing and attribution information.
//
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Text;
using PDNWrapper;

namespace PhotoshopFile
{
    /// <summary>
    /// Reads PSD data types in big-endian byte order.
    /// </summary>
    internal class PsdBinaryReader : IDisposable
    {
        private BinaryReader reader;
        private Encoding encoding;

        public Stream BaseStream
        {
            get { return reader.BaseStream; }
        }

        public PsdBinaryReader(Stream stream, PsdBinaryReader reader)
            : this(stream, reader.encoding)
        {
        }

        public PsdBinaryReader(Stream stream, Encoding encoding)
        {
            this.encoding = encoding;

            // ReadPascalString and ReadUnicodeString handle encoding explicitly.
            // BinaryReader.ReadString() is never called, so it is constructed with
            // ASCII encoding to make accidental usage obvious.
            reader = new BinaryReader(stream, Encoding.ASCII);
        }

        public byte ReadByte()
        {
            return reader.ReadByte();
        }

        public byte[] ReadBytes(int count)
        {
            return reader.ReadBytes(count);
        }

        public bool ReadBoolean()
        {
            return reader.ReadBoolean();
        }

        public Int16 ReadInt16()
        {
            short val = reader.ReadInt16();
            byte[] b = BitConverter.GetBytes(val);
            {
                Util.SwapBytes(b, 0, 2);
            }
            val = BitConverter.ToInt16(b, 0);
            return val;
        }

        public Int32 ReadInt32()
        {
            int val = reader.ReadInt32();
            byte[] b = BitConverter.GetBytes(val);
            {
                Util.SwapBytes(b, 0, 4);
            }
            val = BitConverter.ToInt32(b, 0);
            return val;
        }

        public double ReadDouble()
        {
            double val = reader.ReadDouble();
            byte[] b = BitConverter.GetBytes(val);
            {
                Util.SwapBytes(b, 0, 8);
            }
            val = BitConverter.ToDouble(b, 0);
            return val;
        }

        public Int64 ReadInt64()
        {
            long val = reader.ReadInt64();
            byte[] b = BitConverter.GetBytes(val);
            {
                Util.SwapBytes(b, 0, 8);
            }
            val = BitConverter.ToInt64(b, 0);
            return val;
        }

        public UInt16 ReadUInt16()
        {
            ushort val = reader.ReadUInt16();
            byte[] b = BitConverter.GetBytes(val);
            {
                Util.SwapBytes(b, 0, 2);
            }
            val = BitConverter.ToUInt16(b, 0);
            return val;
        }

        public UInt32 ReadUInt32()
        {
            uint val = reader.ReadUInt32();
            byte[] b = BitConverter.GetBytes(val);
            {
                Util.SwapBytes(b, 0, 4);
            }
            val = BitConverter.ToUInt32(b, 0);
            return val;
        }

        public UInt64 ReadUInt64()
        {
            ulong val = reader.ReadUInt64();
            byte[] b = BitConverter.GetBytes(val);
            {
                Util.SwapBytes(b, 0, 8);
            }
            val = BitConverter.ToUInt64(b, 0);
            return val;
        }

        //////////////////////////////////////////////////////////////////

        /// <summary>
        /// Read padding to get to the byte multiple for the block.
        /// </summary>
        /// <param name="startPosition">Starting position of the padded block.</param>
        /// <param name="padMultiple">Byte multiple that the block is padded to.</param>
        public void ReadPadding(long startPosition, int padMultiple)
        {
            // Pad to specified byte multiple
            long totalLength = reader.BaseStream.Position - startPosition;
            int padBytes = Util.GetPadding((int)totalLength, padMultiple);
            ReadBytes(padBytes);
        }

        public Rectangle ReadRectangle()
        {
            Rectangle rect = new Rectangle();
            rect.Y = ReadInt32();
            rect.X = ReadInt32();
            rect.Height = ReadInt32() - rect.Y;
            rect.Width = ReadInt32() - rect.X;
            return rect;
        }

        /// <summary>
        /// Read a fixed-length ASCII string.
        /// </summary>
        public string ReadAsciiChars(int count)
        {
            byte[] bytes = reader.ReadBytes(count);
            string s = Encoding.ASCII.GetString(bytes);
            return s;
        }

        /// <summary>
        /// Read a Pascal string using the specified encoding.
        /// </summary>
        /// <param name="padMultiple">Byte multiple that the Pascal string is padded to.</param>
        public string ReadPascalString(int padMultiple)
        {
            long startPosition = reader.BaseStream.Position;

            byte stringLength = ReadByte();
            byte[] bytes = ReadBytes(stringLength);
            ReadPadding(startPosition, padMultiple);

            // Default decoder uses best-fit fallback, so it will not throw any
            // exceptions if unknown characters are encountered.
            string str = encoding.GetString(bytes);
            return str;
        }

        public string ReadUnicodeString()
        {
            int numChars = ReadInt32();
            int length = 2 * numChars;
            byte[] data = ReadBytes(length);
            string str = Encoding.BigEndianUnicode.GetString(data, 0, length);

            return str;
        }

        //////////////////////////////////////////////////////////////////

        #region IDisposable

        private bool disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (disposed)
                return;

            if (disposing)
            {
                if (reader != null)
                {
                    // BinaryReader.Dispose() is protected.
                    reader.Close();
                    reader = null;
                }
            }

            disposed = true;
        }

        #endregion
    }
}
