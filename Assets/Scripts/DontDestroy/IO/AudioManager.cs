using UnityEngine;
using System.IO;
using System.Collections.Generic;
using MajdataPlay.Types;
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

#nullable enable
namespace MajdataPlay.IO
{
    public class AudioManager : MonoBehaviour
    {
        readonly string SFXFilePath = Application.streamingAssetsPath + "/SFX/";
        readonly string VoiceFilePath = Application.streamingAssetsPath + "/Voice/";
        string[] SFXFileNames = new string[0];
        string[] VoiceFileNames = new string [0];
        private List<AudioSampleWrap> SFXSamples = new();

        private WasapiProcedure? wasapiProcedure;
        private int BassGlobalMixer = -114514;

        public bool PlayDebug;
        private void Awake()
        {
            MajInstances.AudioManager = this;
            DontDestroyOnLoad(this);
            SFXFileNames =  new DirectoryInfo(SFXFilePath).GetFiles().FindAll(o=>!o.Name.EndsWith(".meta")).Select(x => x.Name).ToArray();
            VoiceFileNames = new DirectoryInfo(VoiceFilePath).GetFiles().FindAll(o => !o.Name.EndsWith(".meta")).Select(x => x.Name).ToArray();
        }
        void Start()
        {
            var isExclusiveRequest = MajInstances.Setting.Audio.WasapiExclusive;
            var backend = MajInstances.Setting.Audio.Backend;
            var sampleRate = MajInstances.Setting.Audio.Samplerate;
            var deviceIndex = MajInstances.Setting.Audio.AsioDeviceIndex;

#if !UNITY_EDITOR
            if (MajEnv.Mode == RunningMode.View)
            {
                backend = SoundBackendType.Wasapi;
                isExclusiveRequest = false;
            }
#endif

            switch (backend)
            {
                case SoundBackendType.Asio:
                    {
                        MajDebug.Log("Bass Init: " + Bass.Init(0, sampleRate, Bass.NoSoundDevice));
                        var asioCount = BassAsio.DeviceCount;
                        for (int i = 0; i < asioCount; i++) 
                        {
                            BassAsio.GetDeviceInfo(i, out var info);
                            MajDebug.Log("ASIO Device " + i + ": " + info.Name);
                        }
                        
                        MajDebug.Log("Asio Init: " + BassAsio.Init(deviceIndex, AsioInitFlags.Thread));
                        MajDebug.Log(BassAsio.LastError);
                        BassAsio.Rate = sampleRate;
                        BassGlobalMixer = BassMix.CreateMixerStream(sampleRate, 2, BassFlags.MixerNonStop | BassFlags.Decode | BassFlags.Float);
                        Bass.ChannelSetAttribute(BassGlobalMixer, ChannelAttribute.Buffer, 0);
                        BassAsio.ChannelEnableBass(false, 0, BassGlobalMixer, true);

                        BassAsio.Start();
                    }
                    break;
                case SoundBackendType.Wasapi:
                    {
                        //Bass.Init(-1, sampleRate);
                        MajDebug.Log("Bass Init: " + Bass.Init(0, sampleRate,Bass.NoSoundDevice));

                        wasapiProcedure = (buffer, length, _) =>
                        {
                            if (BassGlobalMixer == -114514)
                                return 0;
                            if(Bass.LastError != Errors.OK)
                                MajDebug.LogError(Bass.LastError);
                            return Bass.ChannelGetData(BassGlobalMixer, buffer, length);
                        };
                        bool isExclusiveSuccess = false;
                        if (isExclusiveRequest)
                        {
                            isExclusiveSuccess = BassWasapi.Init(
                                -1, 0, 0,
                                WasapiInitFlags.Exclusive | WasapiInitFlags.EventDriven | WasapiInitFlags.Async | WasapiInitFlags.Raw,
                                0.02f, //buffer
                                0.005f, //peried
                                wasapiProcedure);
                            MajDebug.Log($"Wasapi Exclusive Init: {isExclusiveSuccess}");
                        }

                        if(!isExclusiveRequest || !isExclusiveSuccess)
                        {
                            MajDebug.Log("Wasapi Shared Init: " + BassWasapi.Init(
                                -1, 0, 0,
                                WasapiInitFlags.Shared | WasapiInitFlags.EventDriven | WasapiInitFlags.Raw,
                                0, //buffer
                                0, //peried
                                wasapiProcedure));
                        }
                        MajDebug.Log(Bass.LastError);
                        BassWasapi.GetInfo(out var wasapiInfo);
                        BassGlobalMixer = BassMix.CreateMixerStream(wasapiInfo.Frequency, wasapiInfo.Channels, BassFlags.MixerNonStop | BassFlags.Decode | BassFlags.Float);
                        Bass.ChannelSetAttribute(BassGlobalMixer, ChannelAttribute.Buffer, 0);
                        BassWasapi.Start();
                    }
                    break;
            }
            InitSFXSample(SFXFileNames,SFXFilePath);
            InitSFXSample(VoiceFileNames,VoiceFilePath);

            MajDebug.Log(Bass.LastError);

            if (PlayDebug)
                InputManager.BindAnyArea(OnAnyAreaDown);
            ReadVolumeFromSettings();
        }
        void InitSFXSample(string[] fileNameList,string rootPath)
        {
            foreach (var filePath in fileNameList)
            {
                var path = Path.Combine(rootPath, filePath);
                if (!File.Exists(path))
                {
                    SFXSamples.Add(EmptyAudioSample.Shared);
                    MajDebug.LogWarning(path + " dos not exists");
                    continue;
                }
                AudioSampleWrap sample;
                switch(MajInstances.Setting.Audio.Backend)
                {
                    case SoundBackendType.Unity:
                        sample = UnityAudioSample.Create($"file://{path}", gameObject);
                        break;
                    case SoundBackendType.Asio:
                    case SoundBackendType.Wasapi:
                        sample = BassAudioSample.Create(path, BassGlobalMixer, false, false);
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
            if (e.Status != SensorStatus.On)
                return;
            if(e.IsButton)
                PlaySFX("answer.wav");
            else
                PlaySFX("touch.wav");
        }

        private void OnDestroy()
        {
            if(MajInstances.Setting.Audio.Backend == SoundBackendType.Wasapi
                || MajInstances.Setting.Audio.Backend == SoundBackendType.Asio)
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
            var volume = MajInstances.Setting.Audio.Volume;
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
            var backend = MajInstances.Setting.Audio.Backend;
            if (File.Exists(path))
            {
                switch (backend)
                {
                    case SoundBackendType.Unity:
                        return UnityAudioSample.Create($"file://{path}", gameObject);
                    case SoundBackendType.Asio:
                    case SoundBackendType.Wasapi:
                        return BassAudioSample.Create(path, BassGlobalMixer, true, speedChange);
                    default:
                        throw new NotImplementedException("Backend not supported");
                }
            }
            else
            {
                MajDebug.LogWarning(path + " dos not exists");
                return EmptyAudioSample.Shared;
            }
        }
        public AudioSampleWrap LoadMusicFromUri(Uri uri)
        {
            var backend = MajInstances.Setting.Audio.Backend;
            switch (backend)
            {
                case SoundBackendType.Unity:
                    return UnityAudioSample.Create(uri.OriginalString, gameObject);
                case SoundBackendType.Asio:
                case SoundBackendType.Wasapi:
                    return BassAudioSample.CreateFromUri(uri, BassGlobalMixer);
                default:
                    throw new NotImplementedException("Backend not supported");
            }
        }
        public async UniTask<AudioSampleWrap> LoadMusicAsync(string path, bool speedChange = false)
        {
            await UniTask.SwitchToThreadPool();
            var backend = MajInstances.Setting.Audio.Backend;
            if (File.Exists(path))
            {
                switch (backend)
                {
                    case SoundBackendType.Unity:
                        await UniTask.SwitchToMainThread();
                        return await UnityAudioSample.CreateAsync($"file://{path}", gameObject);
                    case SoundBackendType.Asio:
                    case SoundBackendType.Wasapi:
                        return await BassAudioSample.CreateAsync(path, BassGlobalMixer, true, speedChange);
                    default:
                        throw new NotImplementedException("Backend not supported");
                }
            }
            else
            {
                MajDebug.LogWarning(path + " dos not exists");
                return EmptyAudioSample.Shared;
            }
        }
        public async UniTask<AudioSampleWrap> LoadMusicFromUriAsync(Uri uri)
        {
            await UniTask.SwitchToThreadPool();
            var backend = MajInstances.Setting.Audio.Backend;
            switch (backend)
            {
                case SoundBackendType.Unity:
                    await UniTask.SwitchToMainThread();
                    return await UnityAudioSample.CreateAsync(uri.OriginalString, gameObject);
                case SoundBackendType.Asio:
                case SoundBackendType.Wasapi:
                    return BassAudioSample.CreateFromUri(uri, BassGlobalMixer);
                default:
                    throw new NotImplementedException("Backend not supported");
            }
        }
        public void PlaySFX(string name, bool isLoop = false)
        {
            var psp = SFXSamples.FirstOrDefault(o => o.Name == name);
            if (psp is not null) 
            {
                if (psp.SampleType == SFXSampleType.Voice)
                {
                    foreach(var voice in SFXSamples.FindAll(o => o.SampleType == SFXSampleType.Voice))
                    {
                        if(voice is not null)
                            voice.Stop();
                    }
                }
                psp.PlayOneShot();
                psp.IsLoop = isLoop;
            }   
            else
                MajDebug.LogError("No such SFX");
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
                return EmptyAudioSample.Shared;
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
            if(MajInstances.Setting.Audio.Backend == SoundBackendType.Asio)
            {
                BassAsio.ControlPanel();
            }
        }
    }
}