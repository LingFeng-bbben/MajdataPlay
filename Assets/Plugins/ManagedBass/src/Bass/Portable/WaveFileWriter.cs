using System;
using System.IO;
using static System.Text.Encoding;

namespace ManagedBass
{
    /// <summary>
    /// Writes Wave data to a .wav file
    /// </summary>
    public class WaveFileWriter : IDisposable
    {
        #region Fields
        Stream _ofstream;
        BinaryWriter _writer;
        readonly long _dataSizePos, _factSampleCountPos;
        readonly object _locker = new object();
        
        /// <summary>
        /// Number of bytes of audio
        /// </summary>
        public long Length { get; set; }

        readonly WaveFormat _waveFormat;
        #endregion

        #region Factory
        /// <summary>
        /// Creates a <see cref="WaveFileWriter"/> that writes to a <see cref="Stream"/>.
        /// </summary>
        public WaveFileWriter(Stream outStream, WaveFormat format)
        {
            _ofstream = outStream;
            _writer = new BinaryWriter(outStream, UTF8);

            _writer.Write(UTF8.GetBytes("RIFF"));
            _writer.Write(0); // placeholder
            _writer.Write(UTF8.GetBytes("WAVEfmt "));
            _waveFormat = format;

            _writer.Write(18 + format.ExtraSize); // wave format Length
            format.Serialize(_writer);

            // CreateFactChunk
            if (format.Encoding != WaveFormatTag.Pcm)
            {
                _writer.Write(UTF8.GetBytes("fact"));
                _writer.Write(4);
                _factSampleCountPos = outStream.Position;
                _writer.Write(0); // number of samples
            }

            // WriteDataChunkHeader
            _writer.Write(UTF8.GetBytes("data"));
            _dataSizePos = outStream.Position;
            _writer.Write(0); // placeholder

            Length = 0;
        }
        #endregion

        #region Write
        /// <summary>
        /// Writes bytes to the WaveFile
        /// </summary>
        /// <param name="Data">the Buffer containing the wave data</param>
        /// <param name="Length">the number of bytes to write</param>
        public bool Write(byte[] Data, int Length)
        {
            try
            {
                lock (_locker)
                    _writer.Write(Data, 0, Length);

                this.Length += Length;

                return true;
            }
            catch { return false; }
        }

        /// <summary>
        /// Writes 16 bit samples to the Wave file
        /// </summary>
        /// <param name="Data">The Buffer containing the wave data</param>
        /// <param name="Length">The number of bytes to write</param>
        public bool Write(short[] Data, int Length)
        {
            try
            {
                var n = Length / 2;

                lock (_locker)
                    for (var i = 0; i < n; i++)
                        _writer.Write(Data[i]);

                this.Length += Length;

                return true;
            }
            catch { return false; }
        }

        /// <summary>
        /// Writes 32 bit float samples to the Wave file
        /// </summary>
        /// <param name="Data">The Buffer containing the wave data</param>
        /// <param name="Length">The number of bytes to write</param>
        public bool Write(float[] Data, int Length)
        {
            try
            {
                var n = Length / 4;

                lock (_locker)
                    for (var i = 0; i < n; i++)
                        _writer.Write(Data[i]);

                this.Length += Length;

                return true;
            }
            catch { return false; }
        }
        #endregion
        
        #region IDisposable Members
        /// <summary>
        /// Closes this WaveFile
        /// </summary>
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(true);
        }

        /// <summary>
        /// Actually performs the close,making sure the header contains the correct data
        /// </summary>
        /// <param name="Disposing">True if called from <see>Dispose</see></param>
        void Dispose(bool Disposing)
        {
            if (!Disposing || _writer == null)
                return;
            try
            {
                lock (_locker)
                {
                    _writer.Flush();

                    _writer.Seek(4, SeekOrigin.Begin);
                    _writer.Write((int) (_ofstream.Length - 8));

                    if (_waveFormat.Encoding != WaveFormatTag.Pcm)
                    {
                        _writer.Seek((int) _factSampleCountPos, SeekOrigin.Begin);
                        _writer.Write((int) (Length * 8 / _waveFormat.BitsPerSample));
                    }

                    _writer.Seek((int) _dataSizePos, SeekOrigin.Begin);
                    _writer.Write((int) Length);
                }
            }
            finally
            {
                lock (_locker)
                {
                    _writer.Dispose();
                    _writer = null;
                }

                _ofstream.Dispose(); // will close the underlying base stream
                _ofstream = null;
            }
        }

        /// <summary>
        /// Finaliser - should only be called if the User forgot to close this WaveFileWriter
        /// </summary>
        ~WaveFileWriter() { Dispose(false); }
        #endregion
    }
}
