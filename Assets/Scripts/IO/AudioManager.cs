using UnityEngine;
using System.IO;
using System.Collections.Generic;
using MajdataPlay.Extensions;
using Cysharp.Threading.Tasks;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using MajdataPlay.Utils;

using ManagedBass;
using ManagedBass.Wasapi;
using ManagedBass.Mix;
using ManagedBass.Asio;
using UnityEngine.Profiling;
using UnityEditor;
using System;
using System.Linq;
using MajdataPlay.Collections;
using System.Text;
using MajdataPlay.Settings;
using System.Runtime.InteropServices;
using System.Threading;
using AOT;


#nullable enable
namespace MajdataPlay.IO
{
    public class AudioManager : MonoBehaviour
    {
        string SFXFilePath;
        string VoiceFilePath;
        string[] SFXFileNames = new string[0];
        string[] VoiceFileNames = new string [0];
        private List<AudioSampleWrap> SFXSamples = new();

        readonly static WasapiProcedure _wasapiProcedure;
        readonly static AsioProcedure _asioProcedure;
        private static int BassGlobalMixer = -114514;

        public bool PlayDebug;

        static bool _isInited = false;
        readonly static object _initLock = new();

        unsafe static AudioManager()
        {
#if ENABLE_IL2CPP
            _wasapiProcedure = WasapiProcedure;
            _asioProcedure = AsioProcedure;
            //GCHandle.Alloc(_wasapiProcedure, GCHandleType.Pinned);
            //GCHandle.Alloc(_asioProcedure, GCHandleType.Pinned);
#else
            delegate*<IntPtr, int, IntPtr, int> ptr1 = &WasapiProcedure;
            delegate*<bool, int, IntPtr, int, IntPtr, int> ptr2 = &AsioProcedure;

            _wasapiProcedure = Marshal.GetDelegateForFunctionPointer<WasapiProcedure>((IntPtr)ptr1);
            _asioProcedure = Marshal.GetDelegateForFunctionPointer<AsioProcedure>((IntPtr)ptr2);
#endif
        }
        void Awake()
        {
            if (_isInited)
            {
                return;
            }
            lock (_initLock)
            {
                if (_isInited)
                {
                    return;
                }
                MajInstances.AudioManager = this;
            }
        }
        internal void Init()
        {
            if (_isInited)
            {
                return;
            }
            lock (_initLock)
            {
                if (_isInited)
                {
                    return;
                }
                _isInited = true;
            }
            try
            {
                SFXFilePath = Path.Combine(MajEnv.AssetsPath, "SFX/");
                VoiceFilePath = Path.Combine(MajEnv.AssetsPath, "Voice/");

                DontDestroyOnLoad(this);
                SFXFileNames = new DirectoryInfo(SFXFilePath).GetFiles()
                                                             .AsEnumerable()
                                                             .FindAll(o => !o.Name.EndsWith(".meta"))
                                                             .Select(x => x.Name)
                                                             .ToArray();
                VoiceFileNames = new DirectoryInfo(VoiceFilePath).GetFiles()
                                                                 .AsEnumerable()
                                                                 .FindAll(o => !o.Name.EndsWith(".meta"))
                                                                 .Select(x => x.Name)
                                                                 .ToArray();

                var isExclusiveRequest = MajInstances.Settings.Audio.WasapiExclusive;
                var backend = MajInstances.Settings.Audio.Backend;
                var sampleRate = MajInstances.Settings.Audio.Samplerate;
                var deviceIndex = MajInstances.Settings.Audio.AsioDeviceIndex;
#if !UNITY_EDITOR
            if (MajEnv.Mode == RunningMode.View)
            {
                backend = SoundBackendOption.Wasapi;
                isExclusiveRequest = false;
            }
#endif
#if UNITY_ANDROID
                switch(backend)
                {
                    case SoundBackendOption.BassSimple:
                    case SoundBackendOption.Unity:
                        break;
                    default:
                        MajDebug.LogDebug("Android: Fallback to BassSimple");
                        MajInstances.Settings.Audio.Backend = SoundBackendOption.BassSimple;
                        backend = SoundBackendOption.BassSimple;
                        break;
                }
#endif
                switch (backend)
                {
                    case SoundBackendOption.Asio:
                        {
                            MajDebug.LogInfo("Bass Init: " + Bass.Init(0, sampleRate, Bass.NoSoundDevice));
                            var asioCount = BassAsio.DeviceCount;
                            for (int i = 0; i < asioCount; i++)
                            {
                                BassAsio.GetDeviceInfo(i, out var info);
                                MajDebug.LogInfo("ASIO Device " + i + ": " + info.Name);
                            }

                            MajDebug.LogInfo("Asio Init: " + BassAsio.Init(deviceIndex, AsioInitFlags.Thread));
                            MajDebug.LogInfo(BassAsio.LastError);
                            BassAsio.Rate = sampleRate;
                            BassGlobalMixer = BassMix.CreateMixerStream(sampleRate, 2, BassFlags.MixerNonStop | BassFlags.Decode | BassFlags.Float);
                            Bass.ChannelSetAttribute(BassGlobalMixer, ChannelAttribute.Buffer, 0);
                            //BassAsio.ChannelEnable(false, 0, asioProcedure, IntPtr.Zero);
                            BassAsio.ChannelEnableBass(false, 0, BassGlobalMixer, true);
                            BassAsio.GetInfo(out var asioInfo);
                            BassAsio.ChannelSetFormat(false, 0, AsioSampleFormat.Float);
                            //we dont use Asio.Inputs because we only use stero channels
                            for (int i = 1; i < 2; i++)
                            {
                                if (!BassAsio.ChannelJoin(false, i, 0)) // let channel i follow channel 0
                                {
                                    MajDebug.LogError($"ASIO Channel {i} Join to 0 Failed: " + BassAsio.LastError);
                                }
                                else
                                {
                                    BassAsio.ChannelSetFormat(false, i, AsioSampleFormat.Float);
                                }
                            }

                            BassAsio.Start();
                        }
                        break;
                    case SoundBackendOption.Wasapi:
                        {
                            //Bass.Init(-1, sampleRate);
                            MajDebug.LogInfo("Bass Init: " + Bass.Init(0, sampleRate, Bass.NoSoundDevice));

                            //wasapiProcedure = (buffer, length, user) => GlobalProcedure(buffer, length, user);
                            bool isExclusiveSuccess = false;
                            if (isExclusiveRequest)
                            {
                                isExclusiveSuccess = BassWasapi.Init(
                                    -1, 0, 0,
                                    WasapiInitFlags.Exclusive | WasapiInitFlags.EventDriven | WasapiInitFlags.Async | WasapiInitFlags.Raw,
                                    0.02f, //buffer
                                    0.005f, //peried
                                    _wasapiProcedure);
                                MajDebug.LogInfo($"Wasapi Exclusive Init: {isExclusiveSuccess}");
                            }

                            if (!isExclusiveRequest || !isExclusiveSuccess)
                            {
                                MajDebug.LogInfo("Wasapi Shared Init: " + BassWasapi.Init(
                                    -1, 0, 0,
                                    WasapiInitFlags.Shared | WasapiInitFlags.EventDriven | WasapiInitFlags.Raw,
                                    0, //buffer
                                    0, //peried
                                    _wasapiProcedure));
                            }
                            MajDebug.LogInfo(Bass.LastError);
                            BassWasapi.GetInfo(out var wasapiInfo);
                            BassGlobalMixer = BassMix.CreateMixerStream(wasapiInfo.Frequency, wasapiInfo.Channels, BassFlags.MixerNonStop | BassFlags.Decode | BassFlags.Float);
                            Bass.ChannelSetAttribute(BassGlobalMixer, ChannelAttribute.Buffer, 0);
                            BassWasapi.Start();
                        }
                        break;
                    case SoundBackendOption.BassSimple:
                        MajDebug.LogInfo("Bass Init: " + Bass.Init());
                        MajDebug.LogInfo(Bass.LastError);
                        break;
                }

                InitSFXSample(SFXFileNames, SFXFilePath);
                InitSFXSample(VoiceFileNames, VoiceFilePath);

                if (backend == SoundBackendOption.Wasapi || backend == SoundBackendOption.Asio || backend == SoundBackendOption.BassSimple)
                    MajDebug.LogInfo(Bass.LastError);

                if (PlayDebug)
                {
                    InputManager.BindAnyArea(OnAnyAreaDown);
                }
                ReadVolumeFromSettings();
            }
            catch(Exception e)
            {
                MajDebug.LogException(e);
            }
        }
        [MonoPInvokeCallback(typeof(WasapiProcedure))]
        static int WasapiProcedure(IntPtr buffer, int length, IntPtr user)
        {
            if (BassGlobalMixer == -114514)
            {
                return 0;
            }
            if (Bass.LastError != Errors.OK)
            {
                MajDebug.LogError(Bass.LastError);
            }

            var bytesRead = Bass.ChannelGetData(BassGlobalMixer, buffer, length);

            return bytesRead;
        }
        [MonoPInvokeCallback(typeof(AsioProcedure))]
        static int AsioProcedure(bool input, int channel, IntPtr buffer, int length, IntPtr user)
        {
            if (BassGlobalMixer == -114514)
            {
                return 0;
            }
            if (Bass.LastError != Errors.OK)
            {
                MajDebug.LogError(Bass.LastError);
            }
            var bytesRead = Bass.ChannelGetData(BassGlobalMixer, buffer, length);

            return bytesRead;
        }
        void InitSFXSample(string[] fileNameList,string rootPath)
        {
            foreach (var filePath in fileNameList)
            {
                var path = Path.Combine(rootPath, filePath);
                if (!File.Exists(path))
                {
                    SFXSamples.Add(AudioSampleWrap.Empty);
                    MajDebug.LogWarning(path + " dos not exists");
                    continue;
                }
                AudioSampleWrap sample;
                switch(MajInstances.Settings.Audio.Backend)
                {
                    case SoundBackendOption.Unity:
                        sample = UnityAudioSample.Create($"file://{path}", gameObject);
                        break;
                    case SoundBackendOption.Asio:
                    case SoundBackendOption.Wasapi:
                        sample = BassAudioSample.Create(path, BassGlobalMixer, false, false);
                        break;
                    case SoundBackendOption.BassSimple:
                        sample = BassSimpleAudioSample.Create(path, false, false);
                        break;
                    default:
                        throw new NotImplementedException("Backend not supported");
                }
                sample.Name = filePath;

                //group the samples
                sample.SampleType = filePath switch
                {
                    var _ when rootPath == VoiceFilePath => SFXSampleType.Voice,
                    var p when p.StartsWith("bgm") => SFXSampleType.BGM,
                    var p when p.StartsWith("answer") => SFXSampleType.Answer,
                    var p when p.StartsWith("break") => SFXSampleType.Break,
                    var p when p.StartsWith("slide") => SFXSampleType.Slide,
                    var p when p.StartsWith("tap") => SFXSampleType.Tap,
                    var p when p.StartsWith("touch") => SFXSampleType.Touch,
                    _ => sample.SampleType
                };
                SFXSamples.Add((sample));
            }
        }
        void OnAnyAreaDown(object sender, InputEventArgs e)
        {
            if (e.Status != SwitchStatus.On)
                return;
            if(e.IsButton)
                PlaySFX("answer.wav");
            else
                PlaySFX("touch.wav");
        }

        private void OnDestroy()
        {
            if(MajInstances.Settings.Audio.Backend == SoundBackendOption.Wasapi
                || MajInstances.Settings.Audio.Backend == SoundBackendOption.Asio||
                MajInstances.Settings.Audio.Backend == SoundBackendOption.BassSimple)
            {
                foreach (var sample in SFXSamples)
                {
                    if(sample is not null)
                        sample.Dispose();
                }

                Bass.StreamFree(BassGlobalMixer);
                BassAsio.Stop();
                BassAsio.Free();
                BassWasapi.Stop();
                BassWasapi.Free();
                Bass.Stop();
                Bass.Free();
            }
        }

        public void ReadVolumeFromSettings()
        {
            var volume = MajInstances.Settings.Audio.Volume;
            foreach(var sample in SFXSamples)
            {
                if(sample is null || sample.IsEmpty) 
                    continue;
                var vol = sample.SampleType switch
                {
                    SFXSampleType.Answer => volume.Answer,
                    SFXSampleType.Tap => volume.Tap,
                    SFXSampleType.Break => volume.Break,
                    SFXSampleType.Touch => volume.Touch,
                    SFXSampleType.BGM => volume.BGM,
                    SFXSampleType.Slide => volume.Slide,
                    SFXSampleType.Voice => volume.Voice,
                    _ => 1f
                };
                sample.SetVolume(vol);
            }
        }

        public AudioSampleWrap LoadMusic(string path, bool speedChange = false)
        {
            var backend = MajInstances.Settings.Audio.Backend;
            if (File.Exists(path))
            {
                switch (backend)
                {
                    case SoundBackendOption.Unity:
                        return UnityAudioSample.Create($"file://{path}", gameObject);
                    case SoundBackendOption.Asio:
                    case SoundBackendOption.Wasapi:
                        return BassAudioSample.Create(path, BassGlobalMixer, true, speedChange);
                    case SoundBackendOption.BassSimple:
                        return BassSimpleAudioSample.Create(path, true, speedChange);
                    default:
                        throw new NotImplementedException("Backend not supported");
                }
            }
            else
            {
                MajDebug.LogWarning(path + " dos not exists");
                return AudioSampleWrap.Empty;
            }
        }
        public AudioSampleWrap LoadMusicFromUri(Uri uri)
        {
            var backend = MajInstances.Settings.Audio.Backend;
            switch (backend)
            {
                case SoundBackendOption.Unity:
                    return UnityAudioSample.Create(uri.OriginalString, gameObject);
                case SoundBackendOption.Asio:
                case SoundBackendOption.Wasapi:
                    return BassAudioSample.CreateFromUri(uri, BassGlobalMixer);
                case SoundBackendOption.BassSimple:
                    return BassSimpleAudioSample.CreateFromUri(uri);
                default:
                    throw new NotImplementedException("Backend not supported");
            }
        }
        public async UniTask<AudioSampleWrap> LoadMusicAsync(string path, bool speedChange = false)
        {
            await UniTask.SwitchToThreadPool();
            var backend = MajInstances.Settings.Audio.Backend;
            if (File.Exists(path))
            {
                switch (backend)
                {
                    case SoundBackendOption.Unity:
                        await UniTask.SwitchToMainThread();
                        return await UnityAudioSample.CreateAsync($"file://{path}", gameObject);
                    case SoundBackendOption.Asio:
                    case SoundBackendOption.Wasapi:
                        return await BassAudioSample.CreateAsync(path, BassGlobalMixer, true, speedChange);
                    case SoundBackendOption.BassSimple:
                        return await BassSimpleAudioSample.CreateAsync(path, true, speedChange);
                    default:
                        throw new NotImplementedException("Backend not supported");
                }
            }
            else
            {
                MajDebug.LogWarning(path + " dos not exists");
                return AudioSampleWrap.Empty;
            }
        }
        public async UniTask<AudioSampleWrap> LoadMusicFromUriAsync(Uri uri)
        {
            await UniTask.SwitchToThreadPool();
            var backend = MajInstances.Settings.Audio.Backend;
            switch (backend)
            {
                case SoundBackendOption.Unity:
                    await UniTask.SwitchToMainThread();
                    return await UnityAudioSample.CreateAsync(uri.OriginalString, gameObject);
                case SoundBackendOption.Asio:
                case SoundBackendOption.Wasapi:
                    return BassAudioSample.CreateFromUri(uri, BassGlobalMixer);
                case SoundBackendOption.BassSimple:
                    return BassSimpleAudioSample.CreateFromUri(uri);
                default:
                    throw new NotImplementedException("Backend not supported");
            }
        }
        public AudioSampleWrap? PlaySFX(string name, bool isLoop = false)
        {
            var psp = SFXSamples.FirstOrDefault(o => o.Name == name);
            if (psp is not null)
            {
                if (psp.SampleType == SFXSampleType.Voice)
                {
                    foreach (var voice in SFXSamples.FindAll(o => o.SampleType == SFXSampleType.Voice))
                    {
                        if (voice is not null)
                            voice.Stop();
                    }
                }
                psp.PlayOneShot();
                psp.IsLoop = isLoop;
                return psp;
            }
            else
            {
                MajDebug.LogError("No such SFX");
                return null;
            }
        }

        public AudioSampleWrap GetSFX(string name)
        {
            var psp = SFXSamples.FirstOrDefault(o => o.Name == name);
            if (psp is not null)
            {
                return psp;
            }
            else
            {
                return AudioSampleWrap.Empty;
            }
        }

        public void StopSFX(string name)
        {
            var psp = SFXSamples.FirstOrDefault(o => o.Name == name);
            if (psp is not null)
                psp.Stop();
            else
                MajDebug.LogError("No such SFX");
        }
        public void OpenAsioPannel()
        {
            if(MajInstances.Settings.Audio.Backend == SoundBackendOption.Asio)
            {
                BassAsio.ControlPanel();
            }
        }
    }
}