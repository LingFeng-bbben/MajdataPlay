using MajdataPlay.Extensions;
using MajdataPlay.Numerics;
using ManagedBass;
using ManagedBass.Fx;
using ManagedBass.Mix;
using System;
using System.Drawing.Drawing2D;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using static UnityEngine.Rendering.VirtualTexturing.Debugging;
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
                ThrowIfDisposed();
                return Bass.ChannelHasFlag(_decode, BassFlags.Loop);
            }
            set
            {
                ThrowIfDisposed();
                if (value)
                {
                    if (!Bass.ChannelHasFlag(_decode, BassFlags.Loop))
                    {
                        Bass.ChannelAddFlag(_decode, BassFlags.Loop);
                    }
                }
                else
                {
                    if (Bass.ChannelHasFlag(_decode, BassFlags.Loop))
                    {
                        Bass.ChannelRemoveFlag(_decode, BassFlags.Loop);
                    }
                }
            }
        }
        public override bool IsEmpty
        {
            get
            {
                return false;
            }
        }
        public override double CurrentSec
        {
            get
            {
                ThrowIfDisposed();

                return Bass.ChannelBytes2Seconds(_decode, Bass.ChannelGetPosition(_decode));
            }
            set
            {
                ThrowIfDisposed();

                Bass.ChannelSetPosition(_decode, Bass.ChannelSeconds2Bytes(_decode, value));
            }
        }
        public override float Volume
        {
            get
            {
                ThrowIfDisposed();

                return (float)Bass.ChannelGetAttribute(_decode, ChannelAttribute.Volume);
            }
            set
            {
                ThrowIfDisposed();

                var volume = value.Clamp(0, 2) * _gain * MajInstances.Settings.Audio.Volume.Global.Clamp(0, 1);
                Bass.ChannelSetAttribute(_decode, ChannelAttribute.Volume, volume);
            }
        }
        public override float Speed 
        {
            
            get
            {
                ThrowIfDisposed();
                if (_isSpeedChangeSupported)
                {
                    return (float)Bass.ChannelGetAttribute(_decode, ChannelAttribute.Tempo) / 100f + 1f;
                }
                else
                {
                    return 1f;
                }
            }
            set
            {
                ThrowIfDisposed();
                if (_isSpeedChangeSupported)
                {
                    Bass.ChannelSetAttribute(_decode, ChannelAttribute.Tempo, (value - 1) * 100f);
                }
                else
                {
                    return;
                }
            }
        }

        public override TimeSpan Length
        {
            get
            {
                return TimeSpan.FromSeconds(_length);
            }
        }
        public override bool IsPlaying
        {
            get
            {
                ThrowIfDisposed();
                var state = Bass.ChannelIsActive(_decode);
                return state == PlaybackState.Playing && !BassMix.ChannelHasFlag(_decode, BassFlags.MixerChanPause);
            }
        }
        readonly GCHandle? _dataHandle = null;

        BassAudioSample(int decode, int globalMixer,double gain, bool speedChange = false, GCHandle? dataHandle = null)
        {
            if(decode is 0 || globalMixer is 0)
            {
                throw new ArgumentException(nameof(decode));
            }
            

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
            Bass.ChannelStop(_decode);
            MajDebug.LogInfo(Bass.LastError);
            MajDebug.LogInfo($"Add Channel to Mixer: {BassMix.MixerAddChannel(globalMixer, _resampler, BassFlags.MixerChanMatrix)}");
            BassMix.ChannelSetMatrix(_resampler, AudioManager.MixingMatrix);
        }
        public BassAudioSample(int decode, int globalMixer, double gain, bool speedChange = false) : this(decode, globalMixer, gain, speedChange, null)
        {

        }
        ~BassAudioSample()
        {
            Dispose();
        }

        public override void PlayOneShot()
        {
            ThrowIfDisposed();
            BassMix.ChannelSetPosition(_decode, 0);
            //Bass.ChannelPlay(_decode);
            BassMix.ChannelRemoveFlag(_decode, BassFlags.MixerChanPause);
        }
        public override void SetVolume(float volume)
        {
            ThrowIfDisposed();
            Volume = volume;
        }
        public override void Play()
        {
            ThrowIfDisposed();
            BassMix.ChannelRemoveFlag(_decode, BassFlags.MixerChanPause);
            //Bass.ChannelPlay(_decode);
        }
        public override void Pause()
        {
            ThrowIfDisposed();
            BassMix.ChannelAddFlag(_decode, BassFlags.MixerChanPause);
            //Bass.ChannelPause(_decode);
        }
        public override void Stop()
        {
            ThrowIfDisposed();
            BassMix.ChannelAddFlag(_decode, BassFlags.MixerChanPause);
            Bass.ChannelSetPosition(_decode, 0);
            //Bass.ChannelStop(_decode);
        }
        public override void Dispose()
        {
            if(_isDisposed)
            {
                return;
            }
            _isDisposed = true;

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
            if (_dataHandle is not null)
            {
                var handle = (GCHandle)_dataHandle;
                handle.Free();
            }
        }
        public override ValueTask DisposeAsync()
        {
            Dispose();

            return new ValueTask(Task.CompletedTask);
        }
        static BassAudioSample Create(byte[] data, int globalMixer, bool normalize, bool speedChange)
        {
            var handle = (GCHandle?)null;
#if ENABLE_IL2CPP || MAJDATA_IL2CPP_DEBUG
            handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            var decode = Bass.CreateStream(((GCHandle)handle).AddrOfPinnedObject(), 0, data.LongLength, BassFlags.Decode | BassFlags.Prescan | BassFlags.AsyncFile);
#else
            var decode = Bass.CreateStream(data, 0, data.LongLength, BassFlags.Decode | BassFlags.Prescan | BassFlags.AsyncFile);
#endif
            try
            {
                if (decode == 0)
                {
                    throw new NotSupportedException();
                }
                Bass.LastError.EnsureSuccessStatusCode();
                if (speedChange)
                {
                    //this will cause the music sometimes no sound, if press play after immedantly enter the songlist.
                    decode = BassFx.TempoCreate(decode, BassFlags.Decode | BassFlags.Prescan | BassFlags.AsyncFile);
                    Bass.LastError.EnsureSuccessStatusCode();
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
            catch
            {
                if (handle is not null)
                {
                    ((GCHandle)handle).Free();
                }
                throw;
            }
        }
        public static BassAudioSample Create(string path, int globalMixer, bool normalize = true, bool speedChange = false)
        {
            MajDebug.LogInfo($"Create Channel From: {path}");
            var buf = File.ReadAllBytes(path);

            return Create(buf, globalMixer, normalize, speedChange);
        }
        public static async ValueTask<BassAudioSample> CreateAsync(string path, int globalMixer, bool normalize = true, bool speedChange = false)
        {
            MajDebug.LogInfo($"Create Channel From: {path}");
            var buf = await File.ReadAllBytesAsync(path);

            return Create(buf, globalMixer, normalize, speedChange);
        }
        public static BassAudioSample CreateFromUri(Uri uri, int globalMixer)
        {
            MajDebug.LogInfo($"Create Channel From: {uri}");
            var decode = Bass.CreateStream(uri.OriginalString, 0, BassFlags.Decode | BassFlags.Prescan | BassFlags.AsyncFile, null);
            MajDebug.LogInfo(decode);
            MajDebug.LogInfo(Bass.LastError);

            var sample = new BassAudioSample(decode, globalMixer, 1, false);
            sample.Volume = 1;

            return sample;
        }
    }
}
