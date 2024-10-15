using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System.Runtime.InteropServices;
using System;
using MajdataPlay.Interfaces;
using MajdataPlay.Utils;

namespace MajdataPlay.IO
{
    public unsafe class CachedSound : IDisposable, INAudioSample
    {
        bool isDestroyed = false;
        public TimeSpan TrackLen { get; private set; }
        public int Length { get; private set; }
        public Span<float> AudioData => new Span<float>(audioData, Length);
        float* audioData;
        public WaveFormat WaveFormat { get; private set; }
        //this might take time
        public CachedSound(string audioFileName)
        {
            using (var audioFileReader = new AudioFileReader(audioFileName))
            {

                var resampler = new WdlResamplingSampleProvider(audioFileReader, MajInstances.Setting.Audio.Samplerate);
                WaveFormat = resampler.WaveFormat;
                Length = 0;
                var readBuffer = new float[resampler.WaveFormat.SampleRate * resampler.WaveFormat.Channels];
                var buffer = new float[resampler.WaveFormat.SampleRate * resampler.WaveFormat.Channels];
                int totalRead = 0;
                int samplesRead;
                while ((samplesRead = resampler.Read(readBuffer, 0, readBuffer.Length)) > 0)
                {
                    var startIndex = totalRead;
                    totalRead += samplesRead;
                    if (totalRead > buffer.Length)
                    {
                        var _buffer = new float[totalRead];
                        buffer.CopyTo(_buffer.AsSpan());
                        buffer = _buffer;
                    }
                    for (int i = startIndex; i < totalRead; i++)
                        buffer[i] = readBuffer[i - startIndex];
                }
                Length = buffer.Length;
                audioData = (float*)Marshal.AllocHGlobal(sizeof(float) * Length);
                buffer.CopyTo(AudioData);
                TrackLen = audioFileReader.TotalTime;
            }
        }
        ~CachedSound() => Dispose();
        public void Dispose()
        {
            if (isDestroyed)
                return;
            isDestroyed = true;
            Marshal.FreeHGlobal((IntPtr)audioData);
        }
    }
}