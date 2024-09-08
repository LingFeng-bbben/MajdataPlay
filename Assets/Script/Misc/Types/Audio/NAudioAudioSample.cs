﻿using MajdataPlay.IO;
using System;

namespace MajdataPlay.Types
{
    public class NAudioAudioSample : AudioSampleWrap
    {
        public override bool IsLoop
        {
            get
            {
                return soundProvider.IsLoop;
            }
            set { soundProvider.IsLoop = value; }
        }

        private PausableSoundProvider soundProvider;
        public NAudioAudioSample(PausableSoundProvider pausableSound)
        {
            soundProvider = pausableSound;
        }
        public override bool GetPlayState()
        {
            return soundProvider.IsPlaying;
        }
        public override void Play()
        {
            soundProvider.Play();
        }
        public override void Pause()
        {
            soundProvider.Pause();
        }
        public override void PlayOneShot()
        {
            soundProvider.PlayOneShot();
        }
        public override double GetCurrentTime()
        {
            return soundProvider.GetCurrentTime();
        }
        public override void SetCurrentTime(float time)
        {
            //TODO: time to sample
            //soundProvider.position = time;
            throw new NotImplementedException();
        }
        public override void SetVolume(float volume)
        {
            soundProvider.SetVolume(volume);
        }
    }
}