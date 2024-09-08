using System.Collections.Generic;
using NAudio.Wave;
using System.Linq;
using NAudio.Wave.SampleProviders;

namespace MajdataPlay.Types
{
    public class CachedSound
    {
        public float[] AudioData { get; private set; }
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
                AudioData = wholeFile.ToArray();
            }
        }
    }
}