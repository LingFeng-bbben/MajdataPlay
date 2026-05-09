using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using ManagedBass;
using ManagedBass.Mix;
#if UNITY_STANDALONE_WIN
using ManagedBass.Wasapi;
using ManagedBass.Asio;
#endif
using UnityEngine;
using UnityEngine.Profiling;
using System;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;
using AOT;
using MajdataPlay.Extensions;
using MajdataPlay.Utils;
using MajdataPlay.Collections;
using MajdataPlay.Settings;
using MajdataPlay.Numerics;
using Cysharp.Threading.Tasks;


#nullable enable
namespace MajdataPlay.IO
{
    public class AudioManager : MonoBehaviour
    {
        public static float[,] MixingMatrix { get; private set; } = new float[0, 0];

        string SFXFilePath;
        string VoiceFilePath;
        string[] SFXFileNames = new string[0];
        string[] VoiceFileNames = new string[0];
        private List<AudioSampleWrap> SFXSamples = new();

#if UNITY_STANDALONE_WIN
        readonly static WasapiProcedure _wasapiProcedure;
        readonly static AsioProcedure _asioProcedure;
#endif
        private static int BassGlobalMixer = -114514;

        public bool PlayDebug;

        static bool _isInited = false;
        readonly static object _initLock = new();

        unsafe static AudioManager()
        {
#if UNITY_STANDALONE_WIN
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

                var backend = MajEnv.Settings.Audio.Backend;
                var isBass = backend is (SoundBackendOption.BassSimple or SoundBackendOption.Asio or SoundBackendOption.Wasapi);
#if UNITY_STANDALONE
                var wasapiOptions = MajEnv.Settings.Audio.Wasapi;
                var asioOptions = MajEnv.Settings.Audio.Asio;
                var isExclusiveRequest = wasapiOptions.Exclusive;
                var deviceIndex = asioOptions.DeviceIndex;
                var isRawMode = wasapiOptions.RawMode;
#endif
#if !UNITY_EDITOR && UNITY_STANDALONE
                if (MajEnv.Mode == RunningMode.View)
                {
                    backend = SoundBackendOption.Wasapi;
                    isExclusiveRequest = false;
                }
#endif
#if UNITY_ANDROID || UNITY_IOS || !UNITY_STANDALONE_WIN
                switch (backend)
                {
                    case SoundBackendOption.BassSimple:
                    case SoundBackendOption.Unity:
                        break;
                    default:

#if UNITY_STANDALONE || UNITY_IOS || UNITY_ANDROID
                        backend = SoundBackendOption.BassSimple;
#else
                        backend = SoundBackendOption.Unity;
#endif
                        MajEnv.Settings.Audio.Backend = backend;
                        MajDebug.LogDebug($"Fallback to {backend}");
                        break;
                }
#endif
                switch (backend)
                {
#if UNITY_STANDALONE_WIN
                    case SoundBackendOption.Asio:
                        {
                            MajDebug.LogInfo("Bass Init: " + Bass.Init(Bass.NoSoundDevice));
                            var asioCount = BassAsio.DeviceCount;
                            for (int i = 0; i < asioCount; i++)
                            {
                                BassAsio.GetDeviceInfo(i, out var info);
                                MajDebug.LogInfo("ASIO Device " + i + ": " + info.Name);
                            }

                            MajDebug.LogInfo("Asio Init: " + BassAsio.Init(deviceIndex, AsioInitFlags.Thread));
                            MajDebug.LogInfo(BassAsio.LastError);
                            var asioInfo = BassAsio.Info;
                            var deviceInfo = BassAsio.GetDeviceInfo(BassAsio.CurrentDevice);
                            BassAsio.Rate = asioOptions.SampleRate;
                            BassGlobalMixer = BassMix.CreateMixerStream((int)BassAsio.Rate, asioInfo.Outputs, BassFlags.MixerNonStop | BassFlags.Decode | BassFlags.Float);
                            Bass.ChannelSetAttribute(BassGlobalMixer, ChannelAttribute.Buffer, 0);
                            Bass.ChannelSetAttribute(BassGlobalMixer, (ChannelAttribute)86017, 8);
                            //BassAsio.ChannelEnable(false, 0, asioProcedure, IntPtr.Zero);
                            BassAsio.ChannelEnableBass(false, 0, BassGlobalMixer, true);
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
                            MajDebug.LogInfo($"[BassAsio] Channel count: {asioInfo.Outputs}");
                            GenerateMixingMatrix(asioInfo.Outputs);

                            BassAsio.Start();
                        }
                        break;
                    case SoundBackendOption.Wasapi:
                        {
                            //Bass.Init(-1, sampleRate);
                            MajDebug.LogInfo("Bass Init: " + Bass.Init(Bass.NoSoundDevice));

                            bool isExclusiveSuccess = false;
                            var rawFlag = WasapiInitFlags.Raw;
                            if(!isRawMode)
                            {
                                rawFlag = 0;
                            }
                            if (isExclusiveRequest)
                            {
                                isExclusiveSuccess = BassWasapi.Init(
                                    -1, 0, 0,
                                    WasapiInitFlags.Exclusive | WasapiInitFlags.EventDriven | WasapiInitFlags.Async | rawFlag,
                                    wasapiOptions.BufferSize, //buffer
                                    wasapiOptions.Period, //peried
                                    _wasapiProcedure);
                                MajDebug.LogInfo($"Wasapi Exclusive Init: {isExclusiveSuccess}");
                            }

                            if (!isExclusiveRequest || !isExclusiveSuccess)
                            {
                                MajDebug.LogInfo("Wasapi Shared Init: " + BassWasapi.Init(
                                    -1, 0, 0,
                                    WasapiInitFlags.Shared | WasapiInitFlags.EventDriven | rawFlag,
                                    0, //buffer
                                    0, //peried
                                    _wasapiProcedure));
                            }
                            MajDebug.LogInfo(Bass.LastError);
                            BassWasapi.GetInfo(out var wasapiInfo);
                            BassGlobalMixer = BassMix.CreateMixerStream(wasapiInfo.Frequency, wasapiInfo.Channels, BassFlags.MixerNonStop | BassFlags.Decode | BassFlags.Float);
                            Bass.ChannelSetAttribute(BassGlobalMixer, ChannelAttribute.Buffer, 0);
                            Bass.ChannelSetAttribute(BassGlobalMixer, (ChannelAttribute)86017, 8);
                            MajDebug.LogInfo($"[BassWasapi] Channel count: {wasapiInfo.Channels}");
                            GenerateMixingMatrix(wasapiInfo.Channels);
                            BassWasapi.Start();
                        }
                        break;
#endif
                    case SoundBackendOption.BassSimple:
                        {
#if UNITY_ANDROID || UNITY_IOS
                            var mobileOptions = MajEnv.Settings.Audio.Mobile;
                            mobileOptions.UpdatePeriodMs = mobileOptions.UpdatePeriodMs.Clamp(5, 100);
                            mobileOptions.BufferLengthMs = mobileOptions.BufferLengthMs.Clamp(mobileOptions.UpdatePeriodMs + 1, 5000);
                            mobileOptions.DeviceUpdatePeriodMs = mobileOptions.DeviceUpdatePeriodMs.Clamp(1, int.MaxValue);
                            mobileOptions.DeviceBufferLengthMs = mobileOptions.DeviceBufferLengthMs.Clamp(mobileOptions.DeviceUpdatePeriodMs * 2, int.MaxValue);
                            var @return = default(bool);
#if UNITY_ANDROID // Android Only (AAudio Config)
                            @return = Bass.Configure(Configuration.AndroidAAudio, mobileOptions.EnableAAudio);
                            MajDebug.LogInfo($"[Bass] Set AndroidAAudio: {@return}");
#endif
                            @return = Bass.Configure(Configuration.UpdatePeriod, mobileOptions.UpdatePeriodMs);
                            MajDebug.LogInfo($"[Bass] Set UpdatePeriod: {@return}");
                            @return = Bass.Configure(Configuration.PlaybackBufferLength, mobileOptions.BufferLengthMs);
                            MajDebug.LogInfo($"[Bass] Set PlaybackBufferLength: {@return}");
                            @return = Bass.Configure(Configuration.DevicePeriod, mobileOptions.DeviceUpdatePeriodMs);
                            MajDebug.LogInfo($"[Bass] Set DevicePeriod: {@return}");
                            @return = Bass.Configure(Configuration.DeviceBufferLength, mobileOptions.DeviceBufferLengthMs);
                            MajDebug.LogInfo($"[Bass] Set DeviceBufferLength: {@return}");
#endif
                            MajDebug.LogInfo("Bass Init: " + Bass.Init());
                            MajDebug.LogInfo(Bass.LastError);
                            var info = Bass.Info;
                            MajDebug.LogInfo($"[Bass] Min playback buffer length: {info.MinBufferLength}");
                            MajDebug.LogInfo($"[Bass] Current device buffer length: {Bass.GetConfig(Configuration.DeviceBufferLength)}");
                            MajDebug.LogInfo($"[Bass] Current device period: {Bass.GetConfig(Configuration.DevicePeriod)}");
                            MajDebug.LogInfo($"[Bass] Channel count: {Bass.Info.SpeakerCount}");
                            GenerateMixingMatrix(Bass.Info.SpeakerCount);
                        }
                        break;
                }
                if (isBass)
                {
                    unsafe
                    {
                        var ua = MajEnv.HTTP_USER_AGENT;
                        fixed (char* ptr = &MemoryMarshal.GetReference(ua.AsSpan()))
                        {
                            var isSuccess = Bass.Configure(Configuration.NetAgent, (IntPtr)ptr);
                            MajDebug.LogInfo($"[Bass] Set user-agent: {isSuccess}");
                        }
                    }
                }
                InitSFXSample(SFXFileNames, SFXFilePath);
                InitSFXSample(VoiceFileNames, VoiceFilePath);

                if (backend == SoundBackendOption.Wasapi || backend == SoundBackendOption.Asio || backend == SoundBackendOption.BassSimple)
                {
                    MajDebug.LogInfo(Bass.LastError);
                }

                if (PlayDebug)
                {
                    InputManager.BindAnyArea(OnAnyAreaDown);
                }
                ReadVolumeFromSettings();
                GameManager.OnAppFocus += OnAppFocus;
            }
            catch (Exception e)
            {
                MajDebug.LogException(e);
            }
        }
#if UNITY_STANDALONE_WIN
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
#endif
        void InitSFXSample(string[] fileNameList, string rootPath)
        {
            foreach (var filePath in fileNameList)
            {
                var path = Path.Combine(rootPath, filePath);
                if (!File.Exists(path))
                {
                    SFXSamples.Add(AudioSampleWrap.Empty);
                    MajDebug.LogWarning(path + " does not exists");
                    continue;
                }
                var sample = LoadMusic(path, false, false);
                sample.Name = filePath;

                //group the samples
                sample.SampleType = filePath switch
                {
                    var _ when rootPath == VoiceFilePath => SFXSampleType.Voice,
                    "tap_ex.wav" => SFXSampleType.Ex,
                    "touch_hanabi.wav" => SFXSampleType.Hanabi,
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
            {
                return;
            }
            if (e.IsButton)
            {
                PlaySFX("answer.wav");
            }
            else
            {
                PlaySFX("touch.wav");
            }
        }

        private void OnDestroy()
        {
            GameManager.OnAppFocus -= OnAppFocus;
            if (MajEnv.Settings.Audio.Backend == SoundBackendOption.Wasapi
                || MajEnv.Settings.Audio.Backend == SoundBackendOption.Asio ||
                MajEnv.Settings.Audio.Backend == SoundBackendOption.BassSimple)
            {
                foreach (var sample in SFXSamples)
                {
                    if (sample is not null)
                        sample.Dispose();
                }

                Bass.StreamFree(BassGlobalMixer);
#if UNITY_STANDALONE_WIN
                BassAsio.Stop();
                BassAsio.Free();
                BassWasapi.Stop();
                BassWasapi.Free();
#endif
                Bass.Stop();
                Bass.Free();
            }
        }

        void OnAppFocus(object? sender, bool isFocus)
        {
#if UNITY_ANDROID || UNITY_IOS
            if(BassGlobalMixer == -114514)
            {
                return;
            }
            if (isFocus)
            {
                MajDebug.LogDebug("Application regained focus, attempting to restore mixer volume");
                Bass.ChannelSetAttribute(BassGlobalMixer, ChannelAttribute.Volume, 1f);
                MajDebug.LogDebug($"[Bass] {Bass.LastError}");
            }
            else
            {
                MajDebug.LogDebug("Application lost focus, attempting to mute the mixer");
                Bass.ChannelSetAttribute(BassGlobalMixer, ChannelAttribute.Volume, 0f);
                MajDebug.LogDebug($"[Bass] {Bass.LastError}");
            }
#endif
        }
        public void ReadVolumeFromSettings()
        {
            var volume = MajEnv.Settings.Audio.Volume;
            foreach (var sample in SFXSamples)
            {
                if (sample is null || sample.IsEmpty)
                {
                    continue;
                }
                var vol = sample.SampleType switch
                {
                    SFXSampleType.Answer => volume.Answer,
                    SFXSampleType.Tap => volume.Tap,
                    SFXSampleType.Ex => volume.Ex,
                    SFXSampleType.Break => volume.Break,
                    SFXSampleType.Touch => volume.Touch,
                    SFXSampleType.Hanabi => volume.Hanabi,
                    SFXSampleType.BGM => volume.BGM,
                    SFXSampleType.Slide => volume.Slide,
                    SFXSampleType.Voice => volume.Voice,
                    _ => 1f
                };
                sample.SetVolume(vol);
            }
        }

        public AudioSampleWrap LoadMusic(string path, bool normalize = true, bool speedChange = false)
        {
            MajDebug.LogInfo($"Try creating channel from file: {path}");
            var backend = MajEnv.Settings.Audio.Backend;
            if (File.Exists(path))
            {
                var sample = default(AudioSampleWrap);
                switch (backend)
                {
                    case SoundBackendOption.Unity:
                        sample = UnityAudioSample.Create($"file://{path}", gameObject);
                        break;
                    case SoundBackendOption.Asio:
                    case SoundBackendOption.Wasapi:
                        sample = BassAudioSample.Create(path, BassGlobalMixer, normalize, speedChange);
                        break;
                    case SoundBackendOption.BassSimple:
                        sample = BassSimpleAudioSample.Create(path, normalize, speedChange);
                        break;
                    default:
                        MajDebug.LogError("Backend not supported");
                        return AudioSampleWrap.Empty;
                }
                MajDebug.LogInfo("Channel created");
                return sample;
            }
            else
            {
                MajDebug.LogWarning(path + " dos not exists");
                return AudioSampleWrap.Empty;
            }
        }
        public AudioSampleWrap LoadMusicFromUri(Uri uri)
        {
            MajDebug.LogInfo($"Try creating channel from uri: {uri}");
            var backend = MajEnv.Settings.Audio.Backend;
            var sample = default(AudioSampleWrap);
            switch (backend)
            {
                case SoundBackendOption.Unity:
                    sample = UnityAudioSample.Create(uri.OriginalString, gameObject);
                    break;
                case SoundBackendOption.Asio:
                case SoundBackendOption.Wasapi:
                    sample = BassAudioSample.CreateFromUri(uri, BassGlobalMixer);
                    break;
                case SoundBackendOption.BassSimple:
                    sample = BassSimpleAudioSample.CreateFromUri(uri);
                    break;
                default:
                    MajDebug.LogError("Backend not supported");
                    return AudioSampleWrap.Empty;
            }
            MajDebug.LogInfo("Channel created");
            return sample;
        }
        public async UniTask<AudioSampleWrap> LoadMusicAsync(string path, bool normalize = true, bool speedChange = false)
        {
            MajDebug.LogInfo($"Try creating channel from file: {path}");
            await UniTask.SwitchToThreadPool();
            var backend = MajEnv.Settings.Audio.Backend;
            if (File.Exists(path))
            {
                var sample = default(AudioSampleWrap);
                switch (backend)
                {
                    case SoundBackendOption.Unity:
                        await UniTask.SwitchToMainThread();
                        sample = await UnityAudioSample.CreateAsync($"file://{path}", gameObject);
                        break;
                    case SoundBackendOption.Asio:
                    case SoundBackendOption.Wasapi:
                        sample = await BassAudioSample.CreateAsync(path, BassGlobalMixer, normalize, speedChange);
                        break;
                    case SoundBackendOption.BassSimple:
                        sample = await BassSimpleAudioSample.CreateAsync(path, normalize, speedChange);
                        break;
                    default:
                        MajDebug.LogError("Backend not supported");
                        return AudioSampleWrap.Empty;
                }
                MajDebug.LogInfo("Channel created");
                return sample;
            }
            else
            {
                MajDebug.LogWarning(path + " dos not exists");
                return AudioSampleWrap.Empty;
            }
        }
        public async UniTask<AudioSampleWrap> LoadMusicFromUriAsync(Uri uri)
        {
            MajDebug.LogInfo($"Try creating channel from uri: {uri}");
            await UniTask.SwitchToThreadPool();
            var backend = MajEnv.Settings.Audio.Backend;
            var sample = default(AudioSampleWrap);
            switch (backend)
            {
                case SoundBackendOption.Unity:
                    await UniTask.SwitchToMainThread();
                    sample = await UnityAudioSample.CreateAsync(uri.OriginalString, gameObject);
                    break;
                case SoundBackendOption.Asio:
                case SoundBackendOption.Wasapi:
                    sample = BassAudioSample.CreateFromUri(uri, BassGlobalMixer);
                    break;
                case SoundBackendOption.BassSimple:
                    sample = BassSimpleAudioSample.CreateFromUri(uri);
                    break;
                default:
                    MajDebug.LogError("Backend not supported");
                    return AudioSampleWrap.Empty;
            }
            MajDebug.LogInfo("Channel created");
            return sample;
        }
        public AudioSampleWrap PlaySFX(string name, bool isLoop = false)
        {
            var psp = SFXSamples.FirstOrDefault(o => o.Name == name);
            if (psp is not null)
            {
                if (psp.SampleType == SFXSampleType.Voice)
                {
                    foreach (var voice in SFXSamples.FindAll(o => o.SampleType == SFXSampleType.Voice))
                    {
                        if (voice is not null)
                        {
                            voice.Stop();
                        }
                    }
                }
                psp.PlayOneShot();
                psp.IsLoop = isLoop;
                return psp;
            }
            else
            {
                MajDebug.LogError($"No such SFX\nName: {name}");
                return AudioSampleWrap.Empty;
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
            {
                psp.Stop();
            }
            else
            {
                MajDebug.LogError($"No such SFX\nName: {name}");
            }
        }
        public void OpenAsioPannel()
        {
#if UNITY_STANDALONE_WIN
            if(MajEnv.Settings.Audio.Backend == SoundBackendOption.Asio)
            {
                BassAsio.ControlPanel();
            }
#endif
        }
        static void GenerateMixingMatrix(int chCount)
        {
            //        var matrix = new float[8, 2]
            //        {
            //// Input      L   R
            //            { 1f, 0f }, // LF
            //            { 0f, 1f }, // RF
            //            { 0f, 0f }, // Center
            //            { 0f, 0f }, // LFE
            //            { 0f, 0f }, // LR
            //            { 0f, 0f }, // RR
            //            { 0f, 0f }, // LR Center
            //            { 0f, 0f }, // RR Center
            //        };
            // 3 channels      left - front, right - front, center.
            // 4 channels      left - front, right - front, left - rear / side, right - rear / side.
            // 6 channels(5.1) left - front, right - front, center, LFE, left - rear / side, right - rear / side.
            // 8 channels(7.1) left - front, right - front, center, LFE, left - rear / side, right - rear / side, left - rear center, right - rear center.

            // LFE = left
            // Center = right

            float[,] matrix;

            var isForceMono = MajEnv.Settings.Audio.ForceMono;
#if UNITY_ANDROID || UNITY_IOS
            var volumeSettings = new
            {
                FrontVolume = 1f,
                CenterAndLFEVolume = 1f,
                SideVolume = 1f,
                RearVolume = 1f
            };
#else
            var volumeSettings = MajEnv.Settings.Audio.Channel;
#endif
            if (isForceMono)
            {
                switch (chCount)
                {
                    case 1:// Mono
                        matrix = new float[1, 2]
                        {
                            { 0.5f, 0.5f }
                        };
                        break;
                    case 2: // 2.0
                        matrix = new float[2, 2]
                        {
                            { 0.5f, 0.5f },
                            { 0.5f, 0.5f }
                        };
                        break;
                    case 3: // 3.0
                        matrix = new float[3, 2]
                        {
                            { 0.5f, 0.5f },
                            { 0.5f, 0.5f },
                            { 0.5f, 0.5f },
                        };
                        break;
                    case 4: // 4.0
                        matrix = new float[4, 2]
                        {
                            { 0.5f, 0.5f },
                            { 0.5f, 0.5f },
                            { 0.5f, 0.5f },
                            { 0.5f, 0.5f },
                        };
                        break;
                    case 6: // 5.1
                        matrix = new float[6, 2]
                        {
                            { 0.5f, 0.5f },
                            { 0.5f, 0.5f },
                            { 0.5f, 0.5f },
                            { 0.5f, 0.5f },
                            { 0.5f, 0.5f },
                            { 0.5f, 0.5f },
                        };
                        break;
                    case 8: // 7.1
                        matrix = new float[8, 2]
                        {
                            { 0.5f, 0.5f },
                            { 0.5f, 0.5f },
                            { 0.5f, 0.5f },
                            { 0.5f, 0.5f },
                            { 0.5f, 0.5f },
                            { 0.5f, 0.5f },
                            { 0.5f, 0.5f },
                            { 0.5f, 0.5f },
                        };
                        break;
                    default:
                        matrix = new float[1, 2]
                        {
                            { 0.5f, 0.5f }
                        };
                        break;
                }
            }
            else
            {
                switch (chCount)
                {
                    case 1:// Mono
                        matrix = new float[1, 2]
                        {
                            { 0.5f, 0.5f }
                        };
                        break;
                    case 2: // 2.0
                        matrix = new float[2, 2]
                        {
                            { volumeSettings.FrontVolume, 0f },
                            { 0f, volumeSettings.FrontVolume }
                        };
                        break;
                    case 3: // 3.0
                        matrix = new float[3, 2]
                        {
                            { volumeSettings.FrontVolume, 0f },
                            { volumeSettings.CenterAndLFEVolume/2f, volumeSettings.CenterAndLFEVolume/2f },
                            { 0f, volumeSettings.FrontVolume },
                        };
                        break;
                    case 4: // 4.0
                        matrix = new float[4, 2]
                        {
                            { volumeSettings.FrontVolume, 0f },
                            { 0f, volumeSettings.FrontVolume },
                            { volumeSettings.RearVolume, 0f },
                            { 0f, volumeSettings.RearVolume },
                        };
                        break;
                    case 6: // 5.1
                        matrix = new float[6, 2]
                        {
                            { volumeSettings.FrontVolume, 0f },
                            { 0f, volumeSettings.FrontVolume },
                            { volumeSettings.CenterAndLFEVolume, 0f },
                            { 0f, volumeSettings.CenterAndLFEVolume },
                            { volumeSettings.SideVolume, 0f },
                            { 0f, volumeSettings.SideVolume }
                        };
                        break;
                    case 8: // 7.1
                        matrix = new float[8, 2]
                        {
                            { volumeSettings.FrontVolume, 0f },
                            { 0f, volumeSettings.FrontVolume },
                            { volumeSettings.CenterAndLFEVolume, 0f },
                            { 0f, volumeSettings.CenterAndLFEVolume },
                            { volumeSettings.SideVolume, 0f },
                            { 0f, volumeSettings.SideVolume },
                            { volumeSettings.RearVolume, 0f },
                            { 0f, volumeSettings.RearVolume },
                        };
                        break;
                    default:
                        matrix = new float[1, 2]
                        {
                            { 0.5f, 0.5f }
                        };
                        break;
                }
            }


            MixingMatrix = matrix;
        }
    }
}
