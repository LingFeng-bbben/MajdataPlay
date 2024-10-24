using Cysharp.Threading.Tasks;
using MajdataPlay.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Networking;
using UnityEngine;
using ManagedBass;
using NAudio.Wave.Compression;
using static UnityEngine.Rendering.VirtualTexturing.Debugging;
using Unity.Collections.LowLevel.Unsafe;
using ManagedBass.Mix;
using ManagedBass.Wasapi;

namespace MajdataPlay.IO
{
    public class BassAudioSample : AudioSampleWrap
    {
        private int stream;
        private double length;
        private int resampler;
        public override bool IsLoop
        {
            get
            {
                return Bass.ChannelHasFlag(stream, BassFlags.Loop);
            }
            set
            {
                if (value)
                {
                    if (!Bass.ChannelHasFlag(stream, BassFlags.Loop))
                        Bass.ChannelAddFlag(stream, BassFlags.Loop);
                }else
                {
                    if (Bass.ChannelHasFlag(stream, BassFlags.Loop))
                        Bass.ChannelRemoveFlag(stream, BassFlags.Loop);
                }
            }
        }
        public override double CurrentSec
        {
            get => Bass.ChannelBytes2Seconds(stream, Bass.ChannelGetPosition(stream));
            set => Bass.ChannelSetPosition(stream,Bass.ChannelSeconds2Bytes(stream,value));
        }
        public override float Volume
        {
            get => (float)Bass.ChannelGetAttribute(stream, ChannelAttribute.Volume);
            set => Bass.ChannelSetAttribute(stream, ChannelAttribute.Volume, value.Clamp(0, 1)) ;
        }
        public override TimeSpan Length => TimeSpan.FromSeconds(length);
        public override bool IsPlaying => Bass.ChannelIsActive(stream) == PlaybackState.Playing;
        public BassAudioSample(string path , int globalMixer)
        {
            stream = Bass.CreateStream(path,0,0, BassFlags.Prescan);
            Debug.Log(Bass.LastError);
            //Bass.ChannelSetAttribute(stream,ChannelAttribute.Buffer,0);

            var decode = Bass.CreateStream(path, 0, 0, BassFlags.Decode);
            length = Bass.ChannelBytes2Seconds(decode,
                Bass.ChannelGetLength(decode));
            Bass.StreamFree(decode);

            var reqfreq = (int)Bass.ChannelGetAttribute(globalMixer, ChannelAttribute.Frequency);
            resampler = BassMix.CreateMixerStream(reqfreq,2 , BassFlags.MixerNonStop | BassFlags.Decode | BassFlags.Float);
            BassMix.MixerAddChannel(stream, resampler, BassFlags.Default);
            Debug.Log("Mixer Add Channel" + path + BassMix.MixerAddChannel(globalMixer, resampler, BassFlags.Default));
        }
        ~BassAudioSample() => Dispose();

        public override void PlayOneShot()
        {
            Bass.ChannelPlay(stream,true);
        }
        public override void SetVolume(float volume) => Volume = volume;
        public override void Play()
        {
            Bass.ChannelPlay(stream);
        }
        public override void Pause()
        {
            Bass.ChannelPause(stream);
        }
        public override void Stop()
        {
            Bass.ChannelStop(stream);
            Bass.ChannelSetPosition(stream, 0);
        }
        public override void Dispose()
        {
            BassMix.MixerRemoveChannel(resampler);
            BassMix.MixerRemoveChannel(stream);
            Bass.ChannelStop(stream);
            Bass.StreamFree(stream);
        }
    }
}
