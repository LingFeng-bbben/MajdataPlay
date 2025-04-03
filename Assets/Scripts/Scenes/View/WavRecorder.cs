using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using MajdataPlay.Types;
using MajdataPlay.Utils;
using ManagedBass.Asio;
using ManagedBass.Wasapi;

namespace MajdataPlay.Assets.Scripts.Scenes.View
{
    public class WavRecorder : IDisposable
    {
        private FileStream _fileStream;
        private readonly int _sampleRate;
        private readonly int _channels;
        private readonly int _bitsPerSample;
        private bool _isRecording;
        private int _dataSize;

        public WavRecorder(string filePath, int bitsPerSample)
        {
            if (MajInstances.GameManager.Setting.Audio.Backend == SoundBackendType.Wasapi)
            {
                BassWasapi.GetInfo(out var wasapiInfo);
                _sampleRate = wasapiInfo.Frequency;
                _channels = wasapiInfo.Channels;
            }
            else if (MajInstances.GameManager.Setting.Audio.Backend == SoundBackendType.Asio)
            {
                _channels = BassAsio.Info.Inputs;
                _sampleRate = (int)BassAsio.Rate;
            }
            _bitsPerSample = bitsPerSample;

            try
            {
                _fileStream = new FileStream(filePath, FileMode.Create);
                WriteHeader();
                _isRecording = true;
            }
            catch (Exception e)
            {
                MajDebug.LogError($"Failed to create WAV file: {e.Message}");
                Dispose();
            }
        }

        private void WriteHeader()
        {
            // RIFF header
            WriteString("RIFF");
            WriteInt(0); // Placeholder for file size
            WriteString("WAVE");

            // fmt chunk
            WriteString("fmt ");
            WriteInt(16); // PCM chunk size
            WriteShort((ushort)(_bitsPerSample == 32 ? 3 : 1)); // Audio format (3 = IEEE float)
            WriteShort((ushort)_channels);
            WriteInt(_sampleRate);
            WriteInt(_sampleRate * _channels * (_bitsPerSample / 8)); // Byte rate
            WriteShort((ushort)(_channels * (_bitsPerSample / 8))); // Block align
            WriteShort((ushort)_bitsPerSample);

            // data chunk header
            WriteString("data");
            WriteInt(0); // Placeholder for data size
        }

        public void HandleData(IntPtr buffer, int length, IntPtr user)
        {
            if (!_isRecording || _fileStream == null) return;

            try
            {
                byte[] data = new byte[length];
                Marshal.Copy(buffer, data, 0, length);
                _fileStream.Write(data, 0, length);
                _dataSize += length;
            }
            catch (Exception e)
            {
                MajDebug.LogError($"Error writing audio data: {e.Message}");
                Dispose();
            }
        }

        public void Finish()
        {
            if (!_isRecording || _fileStream == null) return;

            try
            {
                _isRecording = false;
                _fileStream.Flush();

                // Update RIFF size
                _fileStream.Seek(4, SeekOrigin.Begin);
                WriteInt(_dataSize + 36); // 36 = total header size

                // Update data chunk size
                _fileStream.Seek(40, SeekOrigin.Begin);
                WriteInt(_dataSize);
            }
            catch (Exception e)
            {
                MajDebug.LogError($"Error finalizing WAV file: {e.Message}");
            }
            finally
            {
                Dispose();
            }
        }

        private void WriteShort(ushort value)
        {
            _fileStream.Write(BitConverter.GetBytes(value), 0, 2);
        }

        private void WriteInt(int value)
        {
            _fileStream.Write(BitConverter.GetBytes(value), 0, 4);
        }

        private void WriteString(string value)
        {
            _fileStream.Write(Encoding.ASCII.GetBytes(value), 0, value.Length);
        }

        public void Dispose()
        {
            _fileStream?.Dispose();
            _fileStream = null;
        }
    }
}
