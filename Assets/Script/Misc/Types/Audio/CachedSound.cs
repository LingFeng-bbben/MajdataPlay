using System.Collections.Generic;
using NAudio.Wave;
using System.Linq;
using NAudio.Wave.SampleProviders;
using System;
using UnityEngine;
using System.Runtime.InteropServices;

namespace MajdataPlay.Types
{
    public class CachedSound: IDisposable
    {
        bool isDisposed = false;
        private IntPtr AudioData;
        public WaveFormat WaveFormat { get; private set; }
        public int Length { get; private set; }
        //this might take time
        public CachedSound(string audioFileName)
        {
            using (var audioFileReader = new AudioFileReader(audioFileName))
            {
                
                var resampler = new WdlResamplingSampleProvider(audioFileReader, GameManager.Instance.Setting.Audio.Samplerate);
                WaveFormat = resampler.WaveFormat;
                var readBuffer = new float[resampler.WaveFormat.SampleRate * resampler.WaveFormat.Channels];
                var datasize = (int)((audioFileReader.TotalTime.TotalSeconds+1) * resampler.WaveFormat.SampleRate * resampler.WaveFormat.Channels * 4); //byte count
                Length = datasize;
                AudioData = Marshal.AllocHGlobal(datasize);
                
                int samplesRead;
                int pos = 0;
                while ((samplesRead = resampler.Read(readBuffer, 0, readBuffer.Length)) > 0)
                {
                    Marshal.Copy(readBuffer, 0, IntPtr.Add(AudioData,pos), samplesRead);
                    pos += samplesRead*4;
                }
                audioFileReader.Dispose();
            }
        }
        public int Read(float[] buffer, int offset, int count)
        {
            for(int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = 1f;
            }
            //Marshal.Copy(AudioData,buffer,offset, count);
            return buffer.Length;
        }

        ~CachedSound() => Dispose();

        public void Dispose()
        {
            if(isDisposed) return;
            Marshal.FreeHGlobal(AudioData);
            Debug.LogWarning("Dispose Audio");
            isDisposed = true;
        }
    }
}