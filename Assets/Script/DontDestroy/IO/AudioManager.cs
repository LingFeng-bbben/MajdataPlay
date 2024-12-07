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

#nullable enable
namespace MajdataPlay.IO
{
    public class AudioManager : MonoBehaviour
    {
        readonly string SFXFilePath = Application.streamingAssetsPath + "/SFX/";
        readonly string VoiceFilePath = Application.streamingAssetsPath + "/Voice/";
        string[] SFXFileNames = new string[0];
        string[] VoiceFileNames = new string [0];
        private List<AudioSampleWrap?> SFXSamples = new();

        private WasapiProcedure? wasapiProcedure;
        private AsioProcedure? asioProcedure;
        private WasapiNotifyProcedure? wasapiNotifyProcedure;
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
            var backend = MajInstances.Setting.Audio.Backend;
            var sampleRate = MajInstances.Setting.Audio.Samplerate;
            var deviceIndex = MajInstances.Setting.Audio.AsioDeviceIndex;
            switch (backend)
            {
                case SoundBackendType.Asio:
                    {
                        Debug.Log("Bass Init " + Bass.Init(-1, sampleRate, Bass.NoSoundDevice));
                        asioProcedure = (input, channel, buffer, length, _) =>
                        {
                            if (BassGlobalMixer == -114514)
                                return 0;
                            //Debug.Log("wasapi get");
                            return Bass.ChannelGetData(BassGlobalMixer, buffer, length);
                        };
                        Debug.Log("Asio Init " + BassAsio.Init(deviceIndex, AsioInitFlags.Thread));
                        BassGlobalMixer = BassMix.CreateMixerStream(44100, 2, BassFlags.MixerNonStop | BassFlags.Decode | BassFlags.Float);
                        Bass.ChannelSetAttribute(BassGlobalMixer, ChannelAttribute.Buffer, 0);
                        BassAsio.ChannelEnable(false, 0, asioProcedure);
                        BassAsio.Start();
                    }
                    break;
                case SoundBackendType.Wasapi:
                    {
                        //Bass.Init(-1, sampleRate);
                        Debug.Log("Bass Init " + Bass.Init(-1, sampleRate,Bass.NoSoundDevice));

                        wasapiProcedure = (buffer, length, _) =>
                        {
                            if (BassGlobalMixer == -114514)
                                return 0;
                            //Debug.Log("wasapi get");
                            return Bass.ChannelGetData(BassGlobalMixer, buffer, length);
                        };

                        Debug.Log("Wasapi Init " + BassWasapi.Init(-1, Procedure: wasapiProcedure, Buffer: 0f, Period: 0f));
                        BassWasapi.GetInfo(out var wasapiInfo);
                        BassGlobalMixer = BassMix.CreateMixerStream(wasapiInfo.Frequency, wasapiInfo.Channels, BassFlags.MixerNonStop | BassFlags.Decode | BassFlags.Float);
                        Bass.ChannelSetAttribute(BassGlobalMixer, ChannelAttribute.Buffer, 0);
                        BassWasapi.Start();
                    }
                    break;
            }
            InitSFXSample(SFXFileNames,SFXFilePath);
            InitSFXSample(VoiceFileNames,VoiceFilePath);

            Debug.Log(Bass.LastError);

            if (PlayDebug)
                MajInstances.InputManager.BindAnyArea(OnAnyAreaDown);
            ReadVolumeFromSettings();
        }
        void InitSFXSample(string[] fileNameList,string rootPath)
        {
            foreach (var filePath in fileNameList)
            {
                var path = Path.Combine(rootPath, filePath);
                if (!File.Exists(path))
                {
                    SFXSamples.Add(null);
                    Debug.LogWarning(path + " dos not exists");
                    continue;
                }
                AudioSampleWrap sample;
                switch(MajInstances.Setting.Audio.Backend)
                {
                    case SoundBackendType.Unity:
                        sample = UnityAudioSample.ReadFromFile($"file://{path}", gameObject);
                        break;
                    case SoundBackendType.Asio:
                    case SoundBackendType.Wasapi:
                        sample = new BassAudioSample(path,BassGlobalMixer,false);
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
                if(sample is null) 
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

        public AudioSampleWrap? LoadMusic(string path)
        {
            var backend = MajInstances.Setting.Audio.Backend;
            if (File.Exists(path) || path.StartsWith("http"))
            {
                switch (backend)
                {
                    case SoundBackendType.Unity:
                        if (path.StartsWith("http"))
                            return UnityAudioSample.ReadFromFile(path, gameObject);
                        else
                            return UnityAudioSample.ReadFromFile($"file://{path}", gameObject);
                    case SoundBackendType.Asio:
                    case SoundBackendType.Wasapi:
                        return new BassAudioSample(path, BassGlobalMixer);
                    default:
                        throw new NotImplementedException("Backend not supported");
                }
            }
            else
            {
                Debug.LogWarning(path + " dos not exists");
                return null;
            }
        }
        public async UniTask<AudioSampleWrap?> LoadMusicAsync(string path, bool speedChange=false)
        {
            await UniTask.SwitchToThreadPool();
            var backend = MajInstances.Setting.Audio.Backend;
            if (File.Exists(path) || path.StartsWith("http"))
            {
                switch (backend)
                {
                    case SoundBackendType.Unity:
                        await UniTask.SwitchToMainThread();
                        if (path.StartsWith("http"))
                            return await UnityAudioSample.ReadFromFileAsync(path, gameObject);
                        else
                            return await UnityAudioSample.ReadFromFileAsync($"file://{path}", gameObject);
                    case SoundBackendType.Asio:
                    case SoundBackendType.Wasapi:
                        return new BassAudioSample(path, BassGlobalMixer,speedChange: speedChange);
                    default:
                        throw new NotImplementedException("Backend not supported");
                }
            }
            else
            {
                Debug.LogWarning(path + " dos not exists");
                return null;
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
                Debug.LogError("No such SFX");
        }

        public AudioSampleWrap GetSFX(string name)
        {
            var psp = SFXSamples.FirstOrDefault(o=>o.Name==name);
            if (psp is not null)
            {
                return psp;
            }
            else
            {
                return null;
            }
            
        }

        public void StopSFX(string name)
        {
            var psp = SFXSamples.FirstOrDefault(o => o.Name == name);
            if (psp is not null)
                psp.Stop();
            else
                Debug.LogError("No such SFX");
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