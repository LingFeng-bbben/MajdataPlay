using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;

namespace MajdataPlay.Types
{
    public class PausableSoundProvider : ISampleProvider, IDisposable
    {
        bool isDestroyed = false;
        private CachedSoundSampleProvider cachedSound;
        readonly MixingSampleProvider mixer;
        public long Position => cachedSound.Position;
        public float Volume => cachedSound.Volume;
        public bool IsLoop
        {
            get
            {
                return cachedSound.isLoop;
            }
            set { cachedSound.isLoop = value; }
        }
        public bool IsPlaying { get; set; } = false;
        public PausableSoundProvider(CachedSoundSampleProvider cachedSound, MixingSampleProvider mixer)
        {
            this.cachedSound = cachedSound;
            this.mixer = mixer;
            mixer.AddMixerInput(this);
        }
        public PausableSoundProvider(CachedSound cachedSound, MixingSampleProvider mixer)
        {
            this.cachedSound = new CachedSoundSampleProvider(cachedSound);
            this.mixer = mixer;
            mixer.AddMixerInput(this);
        }
        ~PausableSoundProvider() => Dispose();
        public int Read(float[] buffer, int offset, int count)
        {
            if (IsPlaying)
            {
                var ret = cachedSound.Read(buffer, offset, count);
                if (ret < buffer.Length)
                {
                    IsPlaying = false;
                    cachedSound.Position = 0;
                    //PlayStopped?.Invoke(this, EventArgs.Empty);
                    for (var n = 0; n < count; n++)
                        buffer[offset + n] = 0;
                    return count;
                }
                return ret;
            }
            else
            {
                for (var n = 0; n < count; n++)
                    buffer[offset + n] = 0;
                return count;
            }
        }
        public void PlayOneShot()
        {
            IsPlaying = true;
            cachedSound.Position = 0;
        }
        public void Play()
        {
            IsPlaying = true;
        }
        public void Pause()
        {
            IsPlaying = false;
        }
        public double GetCurrentTime()
        {
            return cachedSound.Position / (double)WaveFormat.SampleRate / 2d;
        }
        public void SetVolume(float volume)
        {
            cachedSound.Volume = volume;
        }
        public WaveFormat WaveFormat { get { return cachedSound.WaveFormat; } }
        public void Dispose()
        {
            if (isDestroyed)
                return;
            mixer.RemoveMixerInput(this);
            cachedSound.Dispose();
            cachedSound =null;
            isDestroyed = true;

        }
    }
}