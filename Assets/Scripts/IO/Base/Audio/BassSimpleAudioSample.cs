using MajdataPlay.Extensions;
using MajdataPlay.Numerics;
using MajdataPlay.Utils;
using ManagedBass;
using ManagedBass.Fx;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
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
                using(Lock)
                {
                    ThrowIfDisposed();
                    return Bass.ChannelHasFlag(_stream, BassFlags.Loop);
                }
            }
            set
            {
                using(Lock)
                {
                    ThrowIfDisposed();
                    if (value)
                    {
                        Bass.ChannelAddFlag(_stream, BassFlags.Loop);
                    }
                    else
                    {
                        Bass.ChannelRemoveFlag(_stream, BassFlags.Loop);
                    }
                }
            }
        }
        public override bool IsEmpty => false;
        public override double CurrentSec
        {
            get
            {
                using(Lock)
                {
                    ThrowIfDisposed();
                    ThrowIfCanSeekNotSupported();
                    return Bass.ChannelBytes2Seconds(_stream, Bass.ChannelGetPosition(_stream));
                }
            }
            set
            {
                using(Lock)
                {
                    ThrowIfDisposed();
                    Bass.ChannelSetPosition(_stream, Bass.ChannelSeconds2Bytes(_stream, value));
                }
            }
        }
        public override float Volume
        {
            get
            {
                using (Lock)
                {
                    ThrowIfDisposed();
                    return (float)Bass.ChannelGetAttribute(_stream, ChannelAttribute.Volume);
                }
            }
            set
            {
                using (Lock)
                {
                    ThrowIfDisposed();
                    var volume = value.Clamp(0, 2) * _gain * MajInstances.Settings.Audio.Volume.Global.Clamp(0, 1);
                    Bass.ChannelSetAttribute(_stream, ChannelAttribute.Volume, volume);
                }
            }
        }
        public override float Speed 
        {
            get
            {
                using (Lock)
                {
                    ThrowIfDisposed();
                    if (_isSpeedChangeSupported)
                    {
                        return (float)Bass.ChannelGetAttribute(_stream, ChannelAttribute.Tempo) / 100f + 1f;
                    }
                    else
                    {
                        return 1f;
                    }
                }
            }
            set
            {
                using (Lock)
                {
                    ThrowIfDisposed();
                    if (_isSpeedChangeSupported)
                    {
                        Bass.ChannelSetAttribute(_stream, ChannelAttribute.Tempo, (value - 1) * 100f);
                    }
                    else
                    {
                        return;
                    }
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
                using(Lock)
                {
                    ThrowIfDisposed();
                    return Bass.ChannelIsActive(_stream) == PlaybackState.Playing;
                }
            }
        }
        readonly GCHandle _dataHandle;
        BassSimpleAudioSample(int stream, double gain, GCHandle dataHandle, bool speedChange = false)
        {
            if (stream is 0)
            {
                throw new ArgumentException(nameof(stream));
            }

            _stream = stream;
            _gain = gain;
            _isSpeedChangeSupported = speedChange;
            _length = Bass.ChannelBytes2Seconds(_stream, Bass.ChannelGetLength(_stream));
            Bass.ChannelStop(_stream);
            Bass.ChannelSetPosition(_stream, 0);

            MajDebug.LogInfo(Bass.LastError);
            _dataHandle = dataHandle;
        }
        public BassSimpleAudioSample(int stream, double gain, bool speedChange = false): this(stream, gain, default, speedChange)
        {

        }
        ~BassSimpleAudioSample()
        {
            Dispose();
        }

        public override void PlayOneShot()
        {
            using (Lock)
            {
                ThrowIfDisposed();
                Bass.ChannelSetPosition(_stream, 0);
                Bass.ChannelPlay(_stream);
            }
        }
        public override void SetVolume(float volume)
        {
            Volume = volume;
        }
        public override void Play()
        {
            using (Lock)
            {
                ThrowIfDisposed();
                Bass.ChannelPlay(_stream);
            }
        }
        public override void Pause()
        {
            using (Lock)
            {
                ThrowIfDisposed();
                Bass.ChannelPause(_stream);
            }
        }
        public override void Stop()
        {
            using(Lock)
            {
                ThrowIfDisposed();
                Bass.ChannelStop(_stream);
                Bass.ChannelSetPosition(_stream, 0);
            }
        }
        public override void Dispose()
        {
            using(Lock)
            {
                if (_isDisposed)
                {
                    return;
                }
                _isDisposed = true;
                Bass.ChannelStop(_stream);
                Bass.ChannelSetPosition(_stream, 0);
                if (_stream != -1)
                {
                    Bass.StreamFree(_stream);
                }
                if (_decode != -1)
                {
                    Bass.StreamFree(_decode);
                }
                if (_dataHandle.IsAllocated)
                {
                    _dataHandle.Free();
                }
            }
        }
        public override ValueTask DisposeAsync()
        {
            Dispose();

            return new ValueTask(Task.CompletedTask);
        }
        static BassSimpleAudioSample Create(byte[] data, bool normalize, bool speedChange)
        {
            var handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            var addr = handle.AddrOfPinnedObject();
            try
            {
                var decode = BassHelper.CreateStream(addr, 0, data.LongLength, BassFlags.Decode | BassFlags.Prescan | BassFlags.AsyncFile);
                var stream = 0;
                stream = BassFx.TempoCreate(decode, BassFlags.Default);
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
                        if (level > channelmax)
                        {
                            channelmax = level;
                        }
                    }
                    gain = 1 / channelmax;
                }

                var sample = new BassSimpleAudioSample(stream, gain, handle, speedChange)
                {
                    CanSeek = true,
                };
                sample.Volume = 1;
                sample._decode = decode;
                return sample;
            }
            catch (Exception e)
            {
                MajDebug.LogException(e);
                if (handle.IsAllocated)
                {
                    handle.Free();
                }
                throw;
            }
        }
        public static BassSimpleAudioSample Create(string path, bool normalize = true, bool speedChange = false)
        {
            var buf = File.ReadAllBytes(path);

            return Create(buf, normalize, speedChange);
        }
        public static async ValueTask<BassSimpleAudioSample> CreateAsync(string path, bool normalize = true, bool speedChange = false)
        {
            var buf = await File.ReadAllBytesAsync(path);

            return Create(buf, normalize, speedChange);
        }
        public static BassSimpleAudioSample CreateFromUri(Uri uri)
        {
            var stream = Bass.CreateStream(uri.OriginalString, 0, BassFlags.Prescan | BassFlags.AsyncFile, null);
            MajDebug.LogInfo(stream);
            MajDebug.LogInfo(Bass.LastError);

            var sample = new BassSimpleAudioSample(stream, 1, false)
            {
                CanSeek = false,
            };
            sample.Volume = 1;

            return sample;
        }
    }
}
