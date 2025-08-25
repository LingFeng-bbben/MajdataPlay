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
using MajdataPlay.Numerics;
#nullable enable
namespace MajdataPlay.IO
{
    public class BassSimpleAudioSample : AudioSampleWrap
    {
        private int _stream = -1;
        private int _decode = -1;
        private double _length = 0;

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
                if(value)
                    Bass.ChannelAddFlag(_stream,BassFlags.Loop);
                else
                    Bass.ChannelRemoveFlag(_stream, BassFlags.Loop);
            }
        }
        public override bool IsEmpty => false;
        public override double CurrentSec
        {
            get => Bass.ChannelBytes2Seconds(_stream, Bass.ChannelGetPosition(_stream));
            set => Bass.ChannelSetPosition(_stream,Bass.ChannelSeconds2Bytes(_stream, value));
        }
        public override float Volume
        {
            get => (float)Bass.ChannelGetAttribute(_stream, ChannelAttribute.Volume);
            set
            {
                var volume = value.Clamp(0, 2) * _gain * MajInstances.Settings.Audio.Volume.Global.Clamp(0, 1);
                Bass.ChannelSetAttribute(_stream, ChannelAttribute.Volume, volume);
            }
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
        public override bool IsPlaying => Bass.ChannelIsActive(_stream)== PlaybackState.Playing;
        public BassSimpleAudioSample(int stream, double gain, bool speedChange = false)
        {
            if(stream is 0)
                throw new ArgumentException(nameof(stream));

            _stream = stream;
            _gain = gain;
            _isSpeedChangeSupported = speedChange;
            _length = Bass.ChannelBytes2Seconds(_stream, Bass.ChannelGetLength(_stream));

            Bass.ChannelStop(_stream);
            Bass.ChannelSetPosition(_stream, 0);

            MajDebug.LogInfo(Bass.LastError);
        }
        ~BassSimpleAudioSample() => Dispose();

        public override void PlayOneShot()
        {
            Bass.ChannelSetPosition(_stream, 0);
            Bass.ChannelPlay(_stream);
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
            Stop();

            if (_stream != -1)
            {
                Bass.StreamFree(_stream);
            }
            if(_decode != -1)
            {
                Bass.StreamFree(_decode);
            }
        }
        static BassSimpleAudioSample Create(byte[] data, bool normalize, bool speedChange)
        {
            var decode = Bass.CreateStream(data, 0, data.LongLength, BassFlags.Decode | BassFlags.Prescan | BassFlags.AsyncFile);
            if(decode == 0)
            {
                throw new NotSupportedException();
            }
            Bass.LastError.EnsureSuccessStatusCode();
            var stream = 0;
            stream = BassFx.TempoCreate(decode,BassFlags.Default);
            Bass.LastError.EnsureSuccessStatusCode();
            Bass.ChannelSetAttribute(stream, ChannelAttribute.Buffer, 0);
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

            var sample = new BassSimpleAudioSample(stream, gain, speedChange);
            sample.Volume = 1;
            sample._decode = decode;
            return sample;
        }
        public static BassSimpleAudioSample Create(string path, bool normalize = true, bool speedChange = false)
        {
            MajDebug.LogInfo($"Create Channel From: {path}");
            var buf = File.ReadAllBytes(path);

            return Create(buf, normalize, speedChange);
        }
        public static async ValueTask<BassSimpleAudioSample> CreateAsync(string path, bool normalize = true, bool speedChange = false)
        {
            MajDebug.LogInfo($"Create Channel From: {path}");
            var buf = await File.ReadAllBytesAsync(path);

            return Create(buf, normalize, speedChange);
        }
        public static BassSimpleAudioSample CreateFromUri(Uri uri)
        {
            MajDebug.LogInfo($"Create Channel From: {uri}");
            var stream = Bass.CreateStream(uri.OriginalString, 0, BassFlags.Prescan | BassFlags.AsyncFile, null);
            MajDebug.LogInfo(stream);
            MajDebug.LogInfo(Bass.LastError);

            var sample = new BassSimpleAudioSample(stream, 1, false);
            sample.Volume = 1;

            return sample;
        }
    }
}
