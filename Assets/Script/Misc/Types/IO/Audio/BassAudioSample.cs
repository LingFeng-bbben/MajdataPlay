using MajdataPlay.Extensions;
using System;
using UnityEngine;
using ManagedBass;
using ManagedBass.Mix;
using Live2D.Cubism.Framework.Json;
using ManagedBass.Fx;
using System.Threading;

namespace MajdataPlay.IO
{
    public class BassAudioSample : AudioSampleWrap
    {
        private int stream = -1;
        private int decode = -1;
        private double length;
        private int resampler = -1;
        private double gain = 1f;
        private bool isSpeedChangeSupported = false;
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
            set => Bass.ChannelSetAttribute(stream, ChannelAttribute.Volume, value.Clamp(0, 2)*gain) ;
        }
        public override float Speed {
            
            get{
                if (isSpeedChangeSupported)
                    return (float)Bass.ChannelGetAttribute(stream, ChannelAttribute.Tempo) / 100f + 1f;
                else
                    throw new NotImplementedException();
            }
            set{
                if (isSpeedChangeSupported)
                    Bass.ChannelSetAttribute(stream, ChannelAttribute.Tempo, (value - 1) * 100f);
                else
                    throw new NotImplementedException();
            }
        }

        public override TimeSpan Length => TimeSpan.FromSeconds(length);
        public override bool IsPlaying => Bass.ChannelIsActive(stream) == PlaybackState.Playing;
        public BassAudioSample(string path, int globalMixer, bool normalize = true, bool speedChange = false)
        {
            if (path.StartsWith("http"))
            {
                Debug.Log("Load Online Stream "+ path);
                stream = Bass.CreateStream(path, 0, 0, null);
                var bytelength = Bass.ChannelGetLength(stream);
                length = Bass.ChannelBytes2Seconds(stream, bytelength);
                Volume = 1;
            }
            else
            {
                decode = Bass.CreateStream(path, 0, 0, BassFlags.Decode|BassFlags.Prescan);
                if (speedChange)
                {
                    //this will cause the music sometimes no sound, if press play after immedantly enter the songlist.
                    stream = BassFx.TempoCreate(decode,BassFlags.Default);
                }
                else
                {
                    stream = Bass.CreateStream(path, 0, 0, BassFlags.Prescan);
                }
                isSpeedChangeSupported = speedChange;
                Bass.ChannelSetAttribute(stream, ChannelAttribute.Buffer, 0);

                //scan the peak here
                var bytelength = Bass.ChannelGetLength(decode);
                length = Bass.ChannelBytes2Seconds(decode, bytelength);
                if (normalize)
                {
                    double channelmax = 0;
                    while (Bass.ChannelGetPosition(decode, PositionFlags.Decode | PositionFlags.Bytes) < bytelength)
                    {
                        var level = (double)BitHelper.LoWord(Bass.ChannelGetLevel(decode)) / 32768;
                        if (level > channelmax) channelmax = level;
                    }
                    gain = 1 / channelmax;
                    Volume = 1;
                }
                Bass.ChannelSetPosition(decode, 0, PositionFlags.Decode | PositionFlags.Bytes);
            }
            var reqfreq = (int)Bass.ChannelGetAttribute(globalMixer, ChannelAttribute.Frequency);
            resampler = BassMix.CreateMixerStream(reqfreq, 2, BassFlags.MixerNonStop | BassFlags.Decode | BassFlags.Float);
            Bass.ChannelSetAttribute(resampler, ChannelAttribute.Buffer, 0);
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
            if(resampler != -1)
            {
                BassMix.MixerRemoveChannel(resampler);
                Bass.ChannelStop(resampler);
                Bass.StreamFree(resampler);
            }
            
            if(stream != -1)
            {
                BassMix.MixerRemoveChannel(stream);
                Bass.ChannelStop(stream);
                Bass.StreamFree(stream);
            }

            if (decode != -1)
            {
                Bass.StreamFree(decode);
            }
        }
    }
}
