using NAudio.Wave;
using System;

namespace MajdataPlay.Types
{
    public class CachedSoundSampleProvider : ISampleProvider
    {
        private readonly CachedSound cachedSound;
        public long Position { get; set; } = 0;
        public float Volume { get; set; } = 1f;
        public bool isLoop { get; set; } = false;
        public CachedSoundSampleProvider(CachedSound cachedSound)
        {
            this.cachedSound = cachedSound;
        }
        int lastLoopOffset = 0;
        public int Read(float[] buffer, int offset, int count)
        {
            var availableSamples = cachedSound.AudioData.Length - Position;
            var samplesToCopy = Math.Min(availableSamples, count);

            Console.WriteLine(samplesToCopy);

            if (availableSamples < count && isLoop)
            {
                lastLoopOffset = offset;
                Position = 0;
                Array.Copy(cachedSound.AudioData, Position, buffer, offset - lastLoopOffset, availableSamples);
            }
            else
            {
                Array.Copy(cachedSound.AudioData, Position, buffer, offset, samplesToCopy);
            }
            if (Volume != 1f)
            {
                for (int i = 0; i < buffer.Length; i++)
                {
                    buffer[i] = Volume * buffer[i];
                }
            }
            Position += samplesToCopy;
            return buffer.Length;
        }
        public WaveFormat WaveFormat { get { return cachedSound.WaveFormat; } }
    }
}