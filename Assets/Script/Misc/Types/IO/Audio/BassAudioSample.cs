using MajdataPlay.Extensions;
using System;
using UnityEngine;
using ManagedBass;
using ManagedBass.Mix;
using Live2D.Cubism.Framework.Json;
using ManagedBass.Fx;
using System.Threading;
#nullable enable
namespace MajdataPlay.IO
{
    public class BassAudioSample : AudioSampleWrap
    {
        private int _stream = -1;
        private int _decode = -1;
        private double _length = 0;
        private int _resampler = -1;
        private double _gain = 1f;
        private bool _isSpeedChangeSupported = false;
        public override bool IsLoop
        {
            get
            {
                return Bass.ChannelHasFlag(_stream, BassFlags.Loop);
            }
            set
            {
                if (value)
                {
                    if (!Bass.ChannelHasFlag(_stream, BassFlags.Loop))
                        Bass.ChannelAddFlag(_stream, BassFlags.Loop);
                }else
                {
                    if (Bass.ChannelHasFlag(_stream, BassFlags.Loop))
                        Bass.ChannelRemoveFlag(_stream, BassFlags.Loop);
                }
            }
        }
        public override double CurrentSec
        {
            get => Bass.ChannelBytes2Seconds(_stream, Bass.ChannelGetPosition(_stream));
            set => Bass.ChannelSetPosition(_stream,Bass.ChannelSeconds2Bytes(_stream,value));
        }
        public override float Volume
        {
            get => (float)Bass.ChannelGetAttribute(_stream, ChannelAttribute.Volume);
            set => Bass.ChannelSetAttribute(_stream, ChannelAttribute.Volume, value.Clamp(0, 2)*_gain) ;
        }
        public override float Speed 
        {
            
            get
            {
                if (_isSpeedChangeSupported)
                    return (float)Bass.ChannelGetAttribute(_stream, ChannelAttribute.Tempo) / 100f + 1f;
                else
                    return 1f;
            }
            set
            {
                if (_isSpeedChangeSupported)
                    Bass.ChannelSetAttribute(_stream, ChannelAttribute.Tempo, (value - 1) * 100f);
                else
                    return;
            }
        }

        public override TimeSpan Length => TimeSpan.FromSeconds(_length);
        public override bool IsPlaying => Bass.ChannelIsActive(_stream) == PlaybackState.Playing;
        public BassAudioSample(string path, int globalMixer, bool normalize = true, bool speedChange = false)
        {
            if (path.StartsWith("http"))
            {
                Debug.Log("Load Online Stream "+ path);
                _stream = Bass.CreateStream(path, 0, 0, null);
                var bytelength = Bass.ChannelGetLength(_stream);
                _length = Bass.ChannelBytes2Seconds(_stream, bytelength);
                Volume = 1;
            }
            else
            {
                _decode = Bass.CreateStream(path, 0, 0, BassFlags.Decode|BassFlags.Prescan);
                if (speedChange)
                {
                    //this will cause the music sometimes no sound, if press play after immedantly enter the songlist.
                    _stream = BassFx.TempoCreate(_decode,BassFlags.Default);
                }
                else
                {
                    _stream = Bass.CreateStream(path, 0, 0, BassFlags.Prescan);
                }
                _isSpeedChangeSupported = speedChange;
                Bass.ChannelSetAttribute(_stream, ChannelAttribute.Buffer, 0);

                //scan the peak here
                var bytelength = Bass.ChannelGetLength(_decode);
                _length = Bass.ChannelBytes2Seconds(_decode, bytelength);
                if (normalize)
                {
                    double channelmax = 0;
                    while (Bass.ChannelGetPosition(_decode, PositionFlags.Decode | PositionFlags.Bytes) < bytelength)
                    {
                        var level = (double)BitHelper.LoWord(Bass.ChannelGetLevel(_decode)) / 32768;
                        if (level > channelmax) channelmax = level;
                    }
                    _gain = 1 / channelmax;
                    Volume = 1;
                }
                Bass.ChannelSetPosition(_decode, 0, PositionFlags.Decode | PositionFlags.Bytes);
            }
            var reqfreq = (int)Bass.ChannelGetAttribute(globalMixer, ChannelAttribute.Frequency);
            _resampler = BassMix.CreateMixerStream(reqfreq, 2, BassFlags.MixerNonStop | BassFlags.Decode | BassFlags.Float);
            Bass.ChannelSetAttribute(_resampler, ChannelAttribute.Buffer, 0);
            BassMix.MixerAddChannel(_stream, _resampler, BassFlags.Default);
            Debug.Log("Mixer Add Channel" + path + BassMix.MixerAddChannel(globalMixer, _resampler, BassFlags.Default));
        }
        ~BassAudioSample() => Dispose();

        public override void PlayOneShot()
        {
            Bass.ChannelPlay(_stream,true);
        }
        public override void SetVolume(float volume) => Volume = volume;
        public override void Play()
        {
            Bass.ChannelPlay(_stream);
        }
        public override void Pause()
        {
            Bass.ChannelPause(_stream);
        }
        public override void Stop()
        {
            Bass.ChannelStop(_stream);
            Bass.ChannelSetPosition(_stream, 0);
        }
        public override void Dispose()
        {
            if(_resampler != -1)
            {
                BassMix.MixerRemoveChannel(_resampler);
                Bass.ChannelStop(_resampler);
                Bass.StreamFree(_resampler);
            }
            
            if(_stream != -1)
            {
                BassMix.MixerRemoveChannel(_stream);
                Bass.ChannelStop(_stream);
                Bass.StreamFree(_stream);
            }

            if (_decode != -1)
            {
                Bass.StreamFree(_decode);
            }
        }
    }
}
