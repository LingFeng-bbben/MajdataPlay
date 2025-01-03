using MajdataPlay.Extensions;
using System;
using UnityEngine;
using ManagedBass;
using ManagedBass.Mix;
using Live2D.Cubism.Framework.Json;
using ManagedBass.Fx;
using System.Threading;
using System.Drawing.Text;
using MajdataPlay.Net;
using Cysharp.Threading.Tasks;
#nullable enable
namespace MajdataPlay.IO
{
    public class BassAudioSample : AudioSampleWrap
    {
        private int _decode = -1;
        private double _length = 0;
        private int _resampler = -1;
        private double _gain = 1f;
        private bool _isSpeedChangeSupported = false;
        public override bool IsLoop
        {
            get
            {
                return Bass.ChannelHasFlag(_decode, BassFlags.Loop);
            }
            set
            {
                if (value)
                {
                    if (!Bass.ChannelHasFlag(_decode, BassFlags.Loop))
                        Bass.ChannelAddFlag(_decode, BassFlags.Loop);
                }else
                {
                    if (Bass.ChannelHasFlag(_decode, BassFlags.Loop))
                        Bass.ChannelRemoveFlag(_decode, BassFlags.Loop);
                }
            }
        }
        public override double CurrentSec
        {
            get => Bass.ChannelBytes2Seconds(_decode, Bass.ChannelGetPosition(_decode));
            set => Bass.ChannelSetPosition(_decode,Bass.ChannelSeconds2Bytes(_decode, value));
        }
        public override float Volume
        {
            get => (float)Bass.ChannelGetAttribute(_decode, ChannelAttribute.Volume);
            set => Bass.ChannelSetAttribute(_decode, ChannelAttribute.Volume, value.Clamp(0, 2)*_gain) ;
        }
        public override float Speed 
        {
            
            get
            {
                if (_isSpeedChangeSupported)
                    return (float)Bass.ChannelGetAttribute(_decode, ChannelAttribute.Tempo) / 100f + 1f;
                else
                    return 1f;
            }
            set
            {
                if (_isSpeedChangeSupported)
                    Bass.ChannelSetAttribute(_decode, ChannelAttribute.Tempo, (value - 1) * 100f);
                else
                    return;
            }
        }

        public override TimeSpan Length => TimeSpan.FromSeconds(_length);
        public override bool IsPlaying => Bass.ChannelIsActive(_decode) == PlaybackState.Playing;
        private bool _isPlaying = false;
        public BassAudioSample(string path, int globalMixer, bool normalize = true, bool speedChange = false)
        {
            if (path.StartsWith("http"))
            {
                Debug.Log("Load Online Stream "+ path);
                var client = HttpTransporter.ShareClient;
                var task = client.GetByteArrayAsync(path);
                task.Wait();
                var buf = task.Result;
                _decode = Bass.CreateStream(buf, 0, buf.LongLength, BassFlags.Decode | BassFlags.Prescan | BassFlags.AsyncFile);
                Debug.Log(_decode);
                Debug.Log(Bass.LastError);
                var bytelength = Bass.ChannelGetLength(_decode);
                _length = Bass.ChannelBytes2Seconds(_decode, bytelength);
                Volume = 1;
            }
            else
            {
                var buf = System.IO.File.ReadAllBytes(path);
                var decode_orig = Bass.CreateStream(buf, 0, buf.LongLength, BassFlags.Decode | BassFlags.Prescan | BassFlags.AsyncFile);
                if (speedChange)
                {
                    //this will cause the music sometimes no sound, if press play after immedantly enter the songlist.
                    _decode = BassFx.TempoCreate(decode_orig, BassFlags.Decode | BassFlags.Prescan | BassFlags.AsyncFile);
                }
                else
                {
                    _decode = decode_orig;
                }
                _isSpeedChangeSupported = speedChange;
               Bass.ChannelSetAttribute(_decode, ChannelAttribute.Buffer, 0);

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
                
            }
            Bass.ChannelSetPosition(_decode, 0, PositionFlags.Decode | PositionFlags.Bytes);
            var reqfreq = (int)Bass.ChannelGetAttribute(globalMixer, ChannelAttribute.Frequency);
            _resampler = BassMix.CreateMixerStream(reqfreq, 2, BassFlags.MixerChanPause | BassFlags.Decode | BassFlags.Float);
            Bass.ChannelSetAttribute(_resampler, ChannelAttribute.Buffer, 0);
            BassMix.MixerAddChannel(_resampler, _decode , BassFlags.Default);
            Bass.ChannelStop(_decode);
            Debug.Log(Bass.LastError);
            Debug.Log("Mixer Add Channel" + path + BassMix.MixerAddChannel(globalMixer, _resampler, BassFlags.Default));
        }
        ~BassAudioSample() => Dispose();

        public override void PlayOneShot()
        {
            BassMix.ChannelSetPosition(_decode, 0);
            Bass.ChannelPlay(_decode);
            //BassMix.ChannelRemoveFlag(_decode, BassFlags.MixerChanPause);
        }
        public override void SetVolume(float volume) => Volume = volume;
        public override void Play()
        {
            //BassMix.ChannelRemoveFlag(_decode, BassFlags.MixerChanPause);
            Bass.ChannelPlay(_decode);
        }
        public override void Pause()
        {
            //BassMix.ChannelAddFlag(_decode, BassFlags.MixerChanPause);
            Bass.ChannelPause(_decode);
        }
        public override void Stop()
        {
            //BassMix.ChannelAddFlag(_decode, BassFlags.MixerChanPause);
            Bass.ChannelSetPosition(_decode, 0);
            Bass.ChannelStop(_decode);
        }
        public override void Dispose()
        {
            //BassMix.ChannelAddFlag(_decode, BassFlags.MixerChanPause);
            if (_resampler != -1)
            {
                BassMix.MixerRemoveChannel(_resampler);
                Bass.ChannelStop(_resampler);
                Bass.StreamFree(_resampler);
            }

            if (_decode != -1)
            {
                Bass.StreamFree(_decode);
            }
        }
    }
}
