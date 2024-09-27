using System.Collections.Generic;
using NAudio.Wave;
using System.Linq;
using NAudio.Wave.SampleProviders;
using System.Runtime.InteropServices;
using System;

namespace MajdataPlay.Types
{
    public unsafe class CachedSound: IDisposable
    {
        bool isDestroyed = false;
        public int Length { get; private set; }
        public Span<float> AudioData => new Span<float>(audioData, Length);
        float* audioData;
        public WaveFormat WaveFormat { get; private set; }
        //this might take time
        public CachedSound(string audioFileName)
        {
            using (var audioFileReader = new AudioFileReader(audioFileName))
            {
                var resampler = new WdlResamplingSampleProvider(audioFileReader, GameManager.Instance.Setting.Audio.Samplerate);
                WaveFormat = resampler.WaveFormat;
                var wholeFile = new List<float>();
                var readBuffer = new float[resampler.WaveFormat.SampleRate * resampler.WaveFormat.Channels];
                int samplesRead;
                while ((samplesRead = resampler.Read(readBuffer, 0, readBuffer.Length)) > 0)
                {
                    wholeFile.AddRange(readBuffer.Take(samplesRead));
                }
                Length = wholeFile.Count;
                audioData = (float*)Marshal.AllocHGlobal(sizeof(float) * Length);
                var dataSpan = new Span<float>(audioData, Length);
                for (var i = 0; i < Length; i++)
                    dataSpan[i] = wholeFile[i];
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