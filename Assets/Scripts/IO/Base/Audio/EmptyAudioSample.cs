using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MajdataPlay.IO
{
    public class EmptyAudioSample : AudioSampleWrap
    {
        public override bool IsEmpty => true;

        public override bool IsPlaying => true;

        public override float Volume 
        { 
            get => 1; 
            set
            {

            }
        }
        public override float Speed 
        { 
            get => 1;
            set
            {

            }
        }
        public override double CurrentSec 
        {
            get => 0;
            set
            {

            }
        }

        public override TimeSpan Length => TimeSpan.Zero;

        public override bool IsLoop 
        {
            get => false;
            set
            {

            }
        }
        public static EmptyAudioSample Shared { get; } = new EmptyAudioSample();
        public override void Dispose() { }
        public override void Pause() { }
        public override void Play() { }
        public override void PlayOneShot() { }
        public override void SetVolume(float volume) { }
        public override void Stop() { }
    }
}
