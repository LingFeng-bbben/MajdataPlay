using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
#nullable enable

namespace MajdataPlay.IO
{
    public class CachedSampleProvider : INAudioSampleProvider<CachedSound>, IDisposable
    {
        bool isDestroyed = false;
        public CachedSound? Sample { get; private set; }
        public WaveFormat WaveFormat
        {
            get
            {
                if (Sample is null)
                    throw new ObjectDisposedException("");
                return Sample.WaveFormat;
            }
        }
        public TimeSpan TrackLen
        {
            get
            {
                if (Sample is null)
                    throw new ObjectDisposedException("");
                return Sample.TrackLen;
            }
        }
        public int Length { get; private set; }
        public int Position { get; set; } = 0;
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
        bool _isPlaying = false;
        readonly MixingSampleProvider mixer;
        public CachedSampleProvider(CachedSound cachedSound, MixingSampleProvider mixer)
        {
            Sample = cachedSound;
            this.mixer = mixer;
            mixer.AddMixerInput(this);
            Length = cachedSound.Length;
        }
        ~CachedSampleProvider() => Dispose();

        public int Read(float[] buffer, int offset, int count)
        {
            if (Sample is null)
                throw new ObjectDisposedException("");
            var audioData = Sample.AudioData;

            for (int i = 0; i < count; i++)
            {
                if (!_isPlaying)
                {
                    buffer[i + offset] = 0;
                    continue;
                }
                else if (Position == Length) // 到末尾
                {
                    if (IsLoop)
                        Position = 0;
                    else
                    {
                        _isPlaying = false;
                        buffer[i + offset] = 0;
                        continue;
                    }
                }
                buffer[i + offset] = audioData[Position] * Volume;
                Position++;
            }
            return buffer.Length;
        }
        public void Dispose()
        {
            if (isDestroyed || Sample is null)
                return;
            isDestroyed = true;
            Sample.Dispose();
            mixer.RemoveMixerInput(this);
            Sample = null;
        }
    }
}