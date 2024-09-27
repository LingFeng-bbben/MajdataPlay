using NAudio.Wave;
using System;
using UnityEngine.UIElements;

namespace MajdataPlay.Types
{
    public class CachedSoundSampleProvider : ISampleProvider, IDisposable
    {
        private CachedSound cachedSound;
        bool isDisposed = false;
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
            var availableSamples = cachedSound.Length - Position;
            var samplesToCopy = Math.Min(availableSamples, count);

            Console.WriteLine(samplesToCopy);

            if (availableSamples < count && isLoop)
            {
                lastLoopOffset = offset;
                Position = 0;
                var buf = new float[availableSamples];
                cachedSound.Read(buf, (int)Position, (int)availableSamples);
                Array.Copy(buf, 0, buffer, offset - lastLoopOffset, availableSamples);
            }
            else
            {
                var buf = new float[availableSamples];
                cachedSound.Read(buf, (int)Position, (int)availableSamples);
                Array.Copy(buf, 0, buffer, offset, samplesToCopy);
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
        ~CachedSoundSampleProvider() => Dispose();
        public void Dispose()
        {
            if(isDisposed) return;
            cachedSound.Dispose();
            cachedSound = null;
            isDisposed = true;
        }
    }
}