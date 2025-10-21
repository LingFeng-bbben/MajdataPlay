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
        public static float[,] MixingMatrix { get; private set; } = new float[0, 0];
        public static BassFlags Speaker { get; private set; } = BassFlags.SpeakerFront;

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

                var wasapiOptions = MajInstances.Settings.Audio.Wasapi;
                var asioOptions = MajInstances.Settings.Audio.Asio;
                var isExclusiveRequest = wasapiOptions.Exclusive;
                var backend = MajInstances.Settings.Audio.Backend;
                var deviceIndex = asioOptions.DeviceIndex;
                var mainChannel = MajInstances.Settings.Audio.Channel.Main;
                var isValidCh = mainChannel is ("Front" or "Rear" or "Side" or "CenterAndLFE");
                var isRawMode = wasapiOptions.RawMode;
                if (!isValidCh)
                {
                    MajDebug.LogWarning($"Invalid sound card channel: \"{mainChannel}\"");
                    mainChannel = "Front";
                    MajInstances.Settings.Audio.Channel.Main = mainChannel;
                }
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
                            GenerateMixingMatrix(asioInfo.Outputs, mainChannel);

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
                            GenerateMixingMatrix(wasapiInfo.Channels, mainChannel);
                            BassWasapi.Start();
                        }
                        break;
                    case SoundBackendOption.BassSimple:
                        MajDebug.LogInfo("Bass Init: " + Bass.Init());
                        MajDebug.LogInfo(Bass.LastError);
                        GenerateMixingMatrix(Bass.Info.SpeakerCount, mainChannel);
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
        static void GenerateMixingMatrix(int chCount, string main)
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

            var isForceMono = MajInstances.Settings.Audio.ForceMono;
            switch (chCount)
            {
                case 2: // 2.0
                    matrix = new float[2,2]
                    {
                        { 0f, 0f },
                        { 0f, 0f }
                    };
                    break;
                case 3: // 3.0
                    matrix = new float[3, 2]
                    {
                        { 0f, 0f },
                        { 0f, 0f },
                        { 0f, 0f },
                    };
                    break;
                case 4: // 4.0
                    matrix = new float[4, 2]
                    {
                        { 0f, 0f },
                        { 0f, 0f },
                        { 0f, 0f },
                        { 0f, 0f },
                    };
                    break;
                case 5: // 5.1
                    matrix = new float[5, 2]
                    {
                        { 0f, 0f },
                        { 0f, 0f },
                        { 0f, 0f },
                        { 0f, 0f },
                        { 0f, 0f },
                    };
                    break;
                case 8: // 7.1
                    matrix = new float[8, 2]
                    {
                        { 0f, 0f },
                        { 0f, 0f },
                        { 0f, 0f },
                        { 0f, 0f },
                        { 0f, 0f },
                        { 0f, 0f },
                        { 0f, 0f },
                        { 0f, 0f },
                    };
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(chCount));
            }

            switch(main)
            {
                case "Rear":
                    Speaker = BassFlags.SpeakerRear;
                    if(chCount < 4)
                    {
                        goto default;
                    }
                    if(chCount == 4)
                    {
                        if(isForceMono)
                        {
                            matrix[2, 0] = .5f;
                            matrix[2, 1] = .5f;
                            matrix[3, 0] = .5f;
                            matrix[3, 1] = .5f;
                        }
                        else
                        {
                            matrix[2, 0] = 1f;
                            matrix[3, 1] = 1f;
                        } 
                    }
                    else if(chCount == 6)
                    {
                        if (isForceMono)
                        {
                            matrix[4, 0] = .5f;
                            matrix[4, 1] = .5f;
                            matrix[5, 0] = .5f;
                            matrix[5, 1] = .5f;
                        }
                        else
                        {
                            matrix[4, 0] = 1f;
                            matrix[5, 1] = 1f;
                        }
                    }
                    else if(chCount == 8)
                    {
                        if (isForceMono)
                        {
                            matrix[4, 0] = .5f;
                            matrix[4, 1] = .5f;
                            matrix[5, 0] = .5f;
                            matrix[5, 1] = .5f;
                        }
                        else
                        {
                            matrix[4, 0] = 1f;
                            matrix[5, 1] = 1f;
                        }
                    }
                    else
                    {
                        MajDebug.LogWarning($"Not support channel count \"{chCount}\", fallback to \"Front\"");
                        goto default;
                    }
                    break;
                case "Side":
                    Speaker = BassFlags.SpeakerRearCenter;
                    if (chCount < 8)
                    {
                        goto default;
                    }
                    if (isForceMono)
                    {
                        matrix[6, 0] = .5f;
                        matrix[6, 1] = .5f;
                        matrix[7, 0] = .5f;
                        matrix[7, 1] = .5f;
                    }
                    else
                    {
                        matrix[6, 0] = 1f;
                        matrix[7, 1] = 1f;
                    }
                    break;
                case "CenterAndLFE":
                    Speaker = BassFlags.SpeakerCenterLFE;
                    if (chCount is not (3 or 6 or 8))
                    {
                        goto default;
                    }
                    if (chCount == 3)
                    {
                        Speaker = BassFlags.SpeakerCenter;
                        matrix[2, 0] = 0.5f;
                        matrix[2, 1] = 0.5f;
                    }
                    else if (chCount is (6 or 8))
                    {
                        if (isForceMono)
                        {
                            matrix[3, 0] = .5f;
                            matrix[3, 1] = .5f;
                            matrix[2, 0] = .5f;
                            matrix[2, 1] = .5f;
                        }
                        else
                        {
                            matrix[3, 0] = 1f;
                            matrix[2, 1] = 1f;
                        }
                    }
                    else
                    {
                        MajDebug.LogWarning($"Not support channel count \"{chCount}\", fallback to \"Front\"");
                        goto default;
                    }
                    break;
                case "Front":
                default:
                    Speaker = BassFlags.SpeakerFront;
                    if (isForceMono)
                    {
                        matrix[0, 0] = .5f;
                        matrix[0, 1] = .5f;
                        matrix[1, 0] = .5f;
                        matrix[1, 1] = .5f;
                    }
                    else
                    {
                        matrix[0, 0] = 1f;
                        matrix[1, 1] = 1f;
                    }
                    break;
            }

            MixingMatrix = matrix;
        }
    }
}