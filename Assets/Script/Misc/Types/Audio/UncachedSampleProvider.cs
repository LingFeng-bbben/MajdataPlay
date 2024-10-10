using MajdataPlay.Interfaces;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
#nullable enable
namespace MajdataPlay.Types
{
    public class UncachedSampleProvider : INAudioSampleProvider, IDisposable
    {
        public int BuffSize { get; private set; } = 4096;
        public int BufferRemaining { get; private set; } = 4096;
        public TimeSpan TrackLen { get; private set; }
        public int Length { get; private set; }
        public int Position { get; set; }
        //public int Position { get; set; }
        public float Volume { get; set; } = 1f;
        public bool IsLoop { get; set; } = false;
        public bool IsPlaying
        {
            get => _isPlaying;
            set
            {
                if (Position == Length && !IsLoop)
                    return;
                _isPlaying = value;
            }
        }
        public WaveFormat WaveFormat { get; private set; }

        bool _isPlaying = false;
        int streamPosition = 0;
        float[] buffer;
        AudioFileReader audioFileReader;
        WdlResamplingSampleProvider resampler;
        MixingSampleProvider mixer;

        public UncachedSampleProvider(string audioFileName, MixingSampleProvider mixer,int bufferSize)
        {
            audioFileReader = new AudioFileReader(audioFileName);
            resampler = new WdlResamplingSampleProvider(audioFileReader, GameManager.Instance.Setting.Audio.Samplerate);
            WaveFormat = resampler.WaveFormat;
            TrackLen = audioFileReader.TotalTime;
            Length = (int)audioFileReader.Length;
            buffer = Array.Empty<float>();
            this.mixer = mixer;
            mixer.AddMixerInput(this);
            //buffer = new float[Math.Min(Length, bufferSize)];
            //BuffSize = buffer.Length;
            //BufferRemaining = buffer.Length;

            //streamPosition += resampler.Read(buffer, 0, buffer.Length);
        }
        public UncachedSampleProvider(string audioFileName, MixingSampleProvider mixer) : this(audioFileName,mixer, 4096) { }
        ~UncachedSampleProvider() => Dispose();
        public int Read(float[] buffer, int offset, int count)
        {
            if (!_isPlaying)
            {
                for (int i = 0; i < count; i++)
                    buffer[i + offset] = 0;
                return count;
            }
            
            int readCount = 0;

            while(readCount < count)
            {
                if (Position >= Length)
                {
                    if (IsLoop)
                    {
                        Position = 0;
                        resampler = new WdlResamplingSampleProvider(audioFileReader, GameManager.Instance.Setting.Audio.Samplerate);
                    }
                    else
                    {
                        _isPlaying = false;
                        break;
                    }
                }
                readCount += resampler.Read(buffer, readCount, count - readCount);
                Position += readCount;
            }

            for (int i = 0; i < count; i++)
                buffer[i + offset] = buffer[i + offset] * Volume;

            return readCount;
        }
        public void Dispose()
        {
            mixer.RemoveMixerInput(this);
            audioFileReader.Dispose();
        }
        //public unsafe void PaddingBuffer()
        //{
        //    if (BuffSize == Length || BufferRemaining == BuffSize)
        //        return;
        //    var offset = BuffSize - BufferRemaining;
        //    Span<float> oldData = stackalloc float[BuffSize];

        //    if (BufferRemaining != 0)
        //    {
        //        for (int i = offset; offset < buffer.Length; i++)
        //            oldData[i - offset] = buffer[i];
        //    }
        //    streamPosition += resampler.Read(oldData.ToArray(), BufferRemaining, offset);
        //}
    }
}
