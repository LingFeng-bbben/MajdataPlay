using MajdataPlay.Extensions;
using MajdataPlay.Interfaces;
using System;
using UnityEngine;

namespace MajdataPlay.Types
{
    public class NAudioAudioSample : AudioSampleWrap, IPausableSoundProvider
    {
        public override bool IsLoop
        {
            get => sampleProvider.IsLoop;
            set => sampleProvider.IsLoop = value;
        }
        public override double CurrentSec 
        { 
            get => sampleProvider.Position / (double)sampleProvider.WaveFormat.SampleRate / 2d; 
            set => throw new NotImplementedException(); 
        }
        public override float Volume 
        { 
            get => sampleProvider.Volume; 
            set => sampleProvider.Volume = value.Clamp(0,1); 
        }
        public override TimeSpan Length => sampleProvider.TrackLen;
        public override bool IsPlaying => sampleProvider.IsPlaying;
        bool isDestroyed = false;

        INAudioSampleProvider sampleProvider;
        public NAudioAudioSample(INAudioSampleProvider pausableSound)
        {
            sampleProvider = pausableSound;
            Debug.Log($"TrackLen: {Length}");
        }
        ~NAudioAudioSample() => Dispose();
        public override void Play()
        {
            sampleProvider.IsPlaying = true;
        }
        public override void Pause()
        {
            sampleProvider.IsPlaying = false;
        }
        public override void PlayOneShot()
        {
            sampleProvider.IsLoop = false;
            sampleProvider.Position = 0;
            sampleProvider.IsPlaying = true;
        }
        public override void Stop()
        {
            sampleProvider.IsLoop = false;
            sampleProvider.Position = 0;
            sampleProvider.IsPlaying = false;
        }
        public override void SetVolume(float volume) => Volume = volume;
        public override void Dispose()
        {
            if(isDestroyed)
                return;
            if(sampleProvider is IDisposable disposable)
                disposable.Dispose();
            isDestroyed = true;
        }
    }
}