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
using System.IO;
using System.Threading.Tasks;
using Unity.VisualScripting;
using MajdataPlay.Utils;
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
            set
            {
                var volume = value.Clamp(0, 2) * _gain * MajInstances.Setting.Audio.Volume.Global.Clamp(0, 1);
                Bass.ChannelSetAttribute(_decode, ChannelAttribute.Volume, volume);
            }
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
        public override bool IsPlaying => !BassMix.ChannelHasFlag(_decode, BassFlags.MixerChanPause);
        public BassAudioSample(int decode, int globalMixer,double gain, bool speedChange = false)
        {
            if(decode is 0 || globalMixer is 0)
                throw new ArgumentException(nameof(decode));
            

            _decode = decode;
            _gain = gain;
            _isSpeedChangeSupported = speedChange;
            _length = Bass.ChannelBytes2Seconds(_decode, Bass.ChannelGetLength(_decode));

            Bass.ChannelSetPosition(_decode, 0, PositionFlags.Decode | PositionFlags.Bytes);
            var reqfreq = (int)Bass.ChannelGetAttribute(globalMixer, ChannelAttribute.Frequency);
            _resampler = BassMix.CreateMixerStream(reqfreq, 2, BassFlags.MixerChanPause | BassFlags.Decode | BassFlags.Float);
            Bass.ChannelSetAttribute(_resampler, ChannelAttribute.Buffer, 0);
            BassMix.MixerAddChannel(_resampler, _decode, BassFlags.Default);
            BassMix.ChannelAddFlag(_decode, BassFlags.MixerChanPause);
            //Bass.ChannelStop(_decode);
            MajDebug.Log(Bass.LastError);
            MajDebug.Log($"Add Channel to Mixer: {BassMix.MixerAddChannel(globalMixer, _resampler, BassFlags.Default)}");
        }
        //public BassAudioSample(string path, int globalMixer, bool normalize = true, bool speedChange = false)
        //{
        //    if (path.StartsWith("http"))
        //    {
        //        MajDebug.Log("Load Online Stream "+ path);
        //        //var client = HttpTransporter.ShareClient;
        //        //var task = client.GetByteArrayAsync(path);
        //        //task.Wait();
        //        //var buf = task.Result;
        //        _decode = Bass.CreateStream(path, 0, BassFlags.Decode | BassFlags.Prescan | BassFlags.AsyncFile, null);
        //        MajDebug.Log(_decode);
        //        MajDebug.Log(Bass.LastError);
        //        var bytelength = Bass.ChannelGetLength(_decode);
        //        _length = Bass.ChannelBytes2Seconds(_decode, bytelength);
        //        Volume = 1;
        //    }
        //    else
        //    {
        //        var buf = System.IO.File.ReadAllBytes(path);
        //        var decode_orig = Bass.CreateStream(buf, 0, buf.LongLength, BassFlags.Decode | BassFlags.Prescan | BassFlags.AsyncFile);
        //        if (speedChange)
        //        {
        //            //this will cause the music sometimes no sound, if press play after immedantly enter the songlist.
        //            _decode = BassFx.TempoCreate(decode_orig, BassFlags.Decode | BassFlags.Prescan | BassFlags.AsyncFile);
        //        }
        //        else
        //        {
        //            _decode = decode_orig;
        //        }
        //        _isSpeedChangeSupported = speedChange;
        //       Bass.ChannelSetAttribute(_decode, ChannelAttribute.Buffer, 0);

        //        //scan the peak here
        //        var bytelength = Bass.ChannelGetLength(_decode);
        //        _length = Bass.ChannelBytes2Seconds(_decode, bytelength);
        //        if (normalize)
        //        {
        //            double channelmax = 0;
        //            while (Bass.ChannelGetPosition(_decode, PositionFlags.Decode | PositionFlags.Bytes) < bytelength)
        //            {
        //                var level = (double)BitHelper.LoWord(Bass.ChannelGetLevel(_decode)) / 32768;
        //                if (level > channelmax) channelmax = level;
        //            }
        //            _gain = 1 / channelmax;
        //            Volume = 1;
        //        }
                
        //    }
        //    Bass.ChannelSetPosition(_decode, 0, PositionFlags.Decode | PositionFlags.Bytes);
        //    var reqfreq = (int)Bass.ChannelGetAttribute(globalMixer, ChannelAttribute.Frequency);
        //    _resampler = BassMix.CreateMixerStream(reqfreq, 2, BassFlags.MixerChanPause | BassFlags.Decode | BassFlags.Float);
        //    Bass.ChannelSetAttribute(_resampler, ChannelAttribute.Buffer, 0);
        //    BassMix.MixerAddChannel(_resampler, _decode , BassFlags.Default);
        //    BassMix.ChannelAddFlag(_decode, BassFlags.MixerChanPause);
        //    //Bass.ChannelStop(_decode);
        //    MajDebug.Log(Bass.LastError);
        //    MajDebug.Log("Mixer Add Channel" + path + BassMix.MixerAddChannel(globalMixer, _resampler, BassFlags.Default));
        //}
        ~BassAudioSample() => Dispose();

        public override void PlayOneShot()
        {
            BassMix.ChannelSetPosition(_decode, 0);
            //Bass.ChannelPlay(_decode);
            BassMix.ChannelRemoveFlag(_decode, BassFlags.MixerChanPause);
        }
        public override void SetVolume(float volume) => Volume = volume;
        public override void Play()
        {
            BassMix.ChannelRemoveFlag(_decode, BassFlags.MixerChanPause);
            //Bass.ChannelPlay(_decode);
        }
        public override void Pause()
        {
            BassMix.ChannelAddFlag(_decode, BassFlags.MixerChanPause);
            //Bass.ChannelPause(_decode);
        }
        public override void Stop()
        {
            BassMix.ChannelAddFlag(_decode, BassFlags.MixerChanPause);
            Bass.ChannelSetPosition(_decode, 0);
            //Bass.ChannelStop(_decode);
        }
        public override void Dispose()
        {
            BassMix.ChannelAddFlag(_decode, BassFlags.MixerChanPause);
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
        static BassAudioSample Create(byte[] data, int globalMixer, bool normalize, bool speedChange)
        {
            var decode = Bass.CreateStream(data, 0, data.LongLength, BassFlags.Decode | BassFlags.Prescan | BassFlags.AsyncFile);
            if (speedChange)
            {
                //this will cause the music sometimes no sound, if press play after immedantly enter the songlist.
                decode = BassFx.TempoCreate(decode, BassFlags.Decode | BassFlags.Prescan | BassFlags.AsyncFile);
            }
            Bass.ChannelSetAttribute(decode, ChannelAttribute.Buffer, 0);

            //scan the peak here
            var bytelength = Bass.ChannelGetLength(decode);
            var gain = 1d;
            if (normalize)
            {
                double channelmax = 0;
                while (Bass.ChannelGetPosition(decode, PositionFlags.Decode | PositionFlags.Bytes) < bytelength)
                {
                    var level = (double)BitHelper.LoWord(Bass.ChannelGetLevel(decode)) / 32768;
                    if (level > channelmax) channelmax = level;
                }
                gain = 1 / channelmax;
            }

            var sample = new BassAudioSample(decode, globalMixer, gain, speedChange);
            sample.Volume = 1;

            return sample;
        }
        public static BassAudioSample Create(string path, int globalMixer, bool normalize = true, bool speedChange = false)
        {
            MajDebug.Log($"Create Channel From: {path}");
            var buf = File.ReadAllBytes(path);

            return Create(buf, globalMixer, normalize, speedChange);
        }
        public static async ValueTask<BassAudioSample> CreateAsync(string path, int globalMixer, bool normalize = true, bool speedChange = false)
        {
            MajDebug.Log($"Create Channel From: {path}");
            var buf = await File.ReadAllBytesAsync(path);

            return Create(buf, globalMixer, normalize, speedChange);
        }
        public static BassAudioSample CreateFromUri(Uri uri, int globalMixer)
        {
            MajDebug.Log($"Create Channel From: {uri}");
            var decode = Bass.CreateStream(uri.OriginalString, 0, BassFlags.Decode | BassFlags.Prescan | BassFlags.AsyncFile, null);
            MajDebug.Log(decode);
            MajDebug.Log(Bass.LastError);

            var sample = new BassAudioSample(decode, globalMixer, 1, false);
            sample.Volume = 1;

            return sample;
        }
    }
}
