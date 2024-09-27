using NAudio.Wave;
using System;
#nullable enable

namespace MajdataPlay.Types
{
    public class CachedSoundSampleProvider : ISampleProvider, IDisposable
    {
        bool isDestroyed = false;
        CachedSound? cachedSound;
        public long Position { get; set; } = 0;
        public float Volume { get; set; } = 1f;
        public bool isLoop { get; set; } = false;
        public CachedSoundSampleProvider(CachedSound cachedSound)
        {
            this.cachedSound = cachedSound;
        }
        ~CachedSoundSampleProvider() => Dispose();
        int lastLoopOffset = 0;
        public int Read(float[] buffer, int offset, int count)
        {
            var availableSamples = cachedSound.Length - Position;
            var samplesToCopy = Math.Min(availableSamples, count);

            Console.WriteLine(samplesToCopy);

            if (availableSamples < count && isLoop)
            {
                lastLoopOffset = offset;
                Position = 0;
                var data = cachedSound.AudioData;
                for(int copyedLen = 0;copyedLen < availableSamples;copyedLen++)
                {
                    buffer[(offset-lastLoopOffset) + copyedLen] = data[(int)Position + copyedLen];
                }
                //Array.Copy(cachedSound.AudioData, Position, buffer, offset - lastLoopOffset, availableSamples);
            }
            else
            {
                var data = cachedSound.AudioData;
                for (int copyedLen = 0; copyedLen < samplesToCopy; copyedLen++)
                {
                    buffer[offset + copyedLen] = data[(int)Position + copyedLen];
                }
                //Array.Copy(cachedSound.AudioData, Position, buffer, offset, samplesToCopy);
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
        public void Dispose()
        {
            if (isDestroyed)
                return;
            isDestroyed = true;
            cachedSound.Dispose();
            cachedSound = null;
        }
    }
}