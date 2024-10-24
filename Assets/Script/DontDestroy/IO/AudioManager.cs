using UnityEngine;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System.IO;
using System.Collections.Generic;
using MajdataPlay.Types;
using MajdataPlay.Extensions;
using NAudio.CoreAudioApi;
using Cysharp.Threading.Tasks;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using MajdataPlay.Utils;

using ManagedBass;
using ManagedBass.Wasapi;
using ManagedBass.Mix;
using ManagedBass.Asio;

#nullable enable
namespace MajdataPlay.IO
{
    public class AudioManager : MonoBehaviour
    {
        readonly string SFXFilePath = Application.streamingAssetsPath + "/SFX/";
        readonly string VoiceFilePath = Application.streamingAssetsPath + "/Voice/";
        readonly string[] SFXFileNames = new string[]
        {
            "all_perfect.wav",
            "answer.wav",
            "break.wav",
            "break_slide.wav",
            "break_slide_start.wav",
            "clock.wav",
            "hanabi.wav",
            "judge.wav",
            "judge_break.wav",
            "judge_break_slide.wav",
            "judge_ex.wav",
            "slide.wav",
            "touch.wav",
            "touchHold_riser.wav",
            "track_start.wav",
            "good.wav",
            "great.wav",
            "titlebgm.mp3",
            "resultbgm.mp3",
            "selectbgm.mp3"
        };
        readonly string[] VoiceFileNames = new string[]
        {
            "MajdataPlay.wav",
            "SelectSong.wav",
            "Sugoi.wav",
            "DontTouchMe.wav"
        };
        private List<AudioSampleWrap?> SFXSamples = new();

        IWavePlayer? audioOutputDevice = null;
        private MixingSampleProvider NAudioGlobalMixer;

        private WasapiProcedure? wasapiProcedure;
        private AsioProcedure? asioProcedure;
        private WasapiNotifyProcedure? wasapiNotifyProcedure;
        private int BassGlobalMixer = -114514;

        public bool PlayDebug;
        private void Awake()
        {
            MajInstances.AudioManager = this;
            DontDestroyOnLoad(this);
        }
        void Start()
        {
            var backend = MajInstances.Setting.Audio.Backend;
            var sampleRate = MajInstances.Setting.Audio.Samplerate;
            var deviceIndex = MajInstances.Setting.Audio.AsioDeviceIndex;
            if (backend != SoundBackendType.Unity)
            {
                var waveformat = NAudio.Wave.WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, 2);
                NAudioGlobalMixer = new MixingSampleProvider(waveformat);
                NAudioGlobalMixer.ReadFully = true;
            }
            switch (backend)
            {
                case SoundBackendType.Asio:
                    {
                        var devices = AsioOut.GetDriverNames();
                        if (devices.IsEmpty() || deviceIndex >= devices.Length)
                        {
                            Debug.LogError("No Asio Output Device");
                            return;
                        }
                        var asioOut = new AsioOut(devices[deviceIndex]);
                        print("Starting ASIO...at " + asioOut.DriverName + " as " + sampleRate);
                        audioOutputDevice = asioOut;
                        audioOutputDevice.Init(NAudioGlobalMixer);
                        audioOutputDevice.Play();
                    }
                    break;
                case SoundBackendType.BassAsio:
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
                        BassAsio.ChannelEnable(false, 0, asioProcedure);
                        BassAsio.Start();
                    }
                    break;
                case SoundBackendType.WaveOut:
                    {
                        print("Starting WaveOut... with " + sampleRate);
                        var waveOut = new WaveOutEvent();
                        waveOut.NumberOfBuffers = 12;
                        audioOutputDevice = waveOut;
                        audioOutputDevice.Init(NAudioGlobalMixer, false);
                        audioOutputDevice.Play();
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

                switch(MajInstances.Setting.Audio.Backend)
                {
                    case SoundBackendType.Unity:
                        SFXSamples.Add(UnityAudioSample.ReadFromFile($"file://{path}", gameObject));
                        break;
                    case SoundBackendType.WaveOut:
                    case SoundBackendType.Asio:
                        var provider = new CachedSampleProvider(new CachedSound(path), NAudioGlobalMixer);
                        SFXSamples.Add(new NAudioAudioSample(provider));
                        break;
                    case SoundBackendType.BassAsio:
                    case SoundBackendType.Wasapi:
                        SFXSamples.Add(new BassAudioSample(path,BassGlobalMixer));
                        break;
                }
            }
        }
        void OnAnyAreaDown(object sender, InputEventArgs e)
        {
            if (e.Status != SensorStatus.On)
                return;
            if(e.IsButton)
                PlaySFX(SFXSampleType.ANSWER);
            else
                PlaySFX(SFXSampleType.TOUCH);
        }

        private void OnDestroy()
        {
            if(audioOutputDevice is not null)
            {
                audioOutputDevice.Stop();
                audioOutputDevice.Dispose();
            }
            if(MajInstances.Setting.Audio.Backend == SoundBackendType.Wasapi
                || MajInstances.Setting.Audio.Backend == SoundBackendType.BassAsio)
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
            var setting = MajInstances.Setting;
            SFXSamples[(int)SFXSampleType.ANSWER]?.SetVolume(setting.Audio.Volume.Answer);
            SFXSamples[(int)SFXSampleType.ALL_PERFECT]?.SetVolume(setting.Audio.Volume.Voice);
            SFXSamples[(int)SFXSampleType.BREAK]?.SetVolume(setting.Audio.Volume.Break);
            SFXSamples[(int)SFXSampleType.BREAK_SLIDE]?.SetVolume(setting.Audio.Volume.Break);
            SFXSamples[(int)SFXSampleType.BREAK_SLIDE_START]?.SetVolume(setting.Audio.Volume.Slide);
            SFXSamples[(int)SFXSampleType.CLOCK]?.SetVolume(setting.Audio.Volume.Answer);
            SFXSamples[(int)SFXSampleType.HANABI]?.SetVolume(setting.Audio.Volume.Touch);
            SFXSamples[(int)SFXSampleType.JUDGE]?.SetVolume(setting.Audio.Volume.Tap);
            SFXSamples[(int)SFXSampleType.JUDGE_BREAK]?.SetVolume(setting.Audio.Volume.Tap);
            SFXSamples[(int)SFXSampleType.JUDGE_BREAK_SLIDE]?.SetVolume(setting.Audio.Volume.Tap);
            SFXSamples[(int)SFXSampleType.JUDGE_EX]?.SetVolume(setting.Audio.Volume.Tap);
            SFXSamples[(int)SFXSampleType.SLIDE]?.SetVolume(setting.Audio.Volume.Slide);
            SFXSamples[(int)SFXSampleType.TOUCH]?.SetVolume(setting.Audio.Volume.Touch);
            SFXSamples[(int)SFXSampleType.TOUCH_HOLD_RISER]?.SetVolume(setting.Audio.Volume.Touch);
            SFXSamples[(int)SFXSampleType.TRACK_START]?.SetVolume(setting.Audio.Volume.BGM);
            SFXSamples[(int)SFXSampleType.GOOD]?.SetVolume(setting.Audio.Volume.Tap);
            SFXSamples[(int)SFXSampleType.GREAT]?.SetVolume(setting.Audio.Volume.Tap);
            SFXSamples[(int)SFXSampleType.TITLE_BGM]?.SetVolume(setting.Audio.Volume.BGM);

            SFXSamples[(int)SFXSampleType.MAJDATA_PLAY]?.SetVolume(setting.Audio.Volume.Voice);
            SFXSamples[(int)SFXSampleType.SELECT_SONG]?.SetVolume(setting.Audio.Volume.Voice);
            SFXSamples[(int)SFXSampleType.SUGOI]?.SetVolume(setting.Audio.Volume.Voice);
            SFXSamples[(int)SFXSampleType.DONT_TOUCH_ME]?.SetVolume(setting.Audio.Volume.Voice);
        }

        public AudioSampleWrap? LoadMusic(string path)
        {
            var backend = MajInstances.Setting.Audio.Backend;
            if (File.Exists(path))
            {
                switch(backend)
                {
                    case SoundBackendType.Unity:
                        return UnityAudioSample.ReadFromFile($"file://{path}", gameObject);
                    case SoundBackendType.BassAsio:
                    case SoundBackendType.Wasapi:
                        return new BassAudioSample(path, BassGlobalMixer);
                    default:
                        var provider = new UncachedSampleProvider(path, NAudioGlobalMixer);
                        return new NAudioAudioSample(provider);
                }
            }
            else
            {
                Debug.LogWarning(path + " dos not exists");
                return null;
            }
        }
        public async UniTask<AudioSampleWrap?> LoadMusicAsync(string path)
        {
            await UniTask.SwitchToThreadPool();
            var backend = MajInstances.Setting.Audio.Backend;
            if (File.Exists(path))
            {
                switch (backend)
                {
                    case SoundBackendType.Unity:
                        await UniTask.SwitchToMainThread();
                        return await UnityAudioSample.ReadFromFileAsync($"file://{path}", gameObject);
                    case SoundBackendType.BassAsio:
                    case SoundBackendType.Wasapi:
                        return new BassAudioSample(path, BassGlobalMixer);
                    default:
                        //var provider = new CachedSampleProvider(new CachedSound(path), mixer);
                        var provider = new UncachedSampleProvider(path,NAudioGlobalMixer);
                        return new NAudioAudioSample(provider);
                }
            }
            else
            {
                Debug.LogWarning(path + " dos not exists");
                return null;
            }
        }
        public void PlaySFX(in SFXSampleType sfxType, bool isLoop = false)
        {
            var psp = SFXSamples[(int)sfxType];
            if (psp is not null) 
            { 
                psp.PlayOneShot();
                psp.IsLoop = isLoop;
            }   
            else
                Debug.LogError("No such SFX");
        }

        public AudioSampleWrap GetSFX(in SFXSampleType sfxType)
        {
            var psp = SFXSamples[(int)sfxType];
            if (psp is not null)
            {
                return psp;
            }
            else
            {
                return null;
            }
            
        }

        public void StopSFX(in SFXSampleType sfxType)
        {
            var psp = SFXSamples[(int)sfxType];
            if (psp is not null)
                psp.Stop();
            else
                Debug.LogError("No such SFX");
        }
        public void OpenAsioPannel()
        {
            if (MajInstances.Setting.Audio.Backend == SoundBackendType.Asio)
            {
                if (audioOutputDevice is AsioOut asioOut)
                    asioOut.ShowControlPanel();
            }
            if(MajInstances.Setting.Audio.Backend == SoundBackendType.BassAsio)
            {
                BassAsio.ControlPanel();
            }
        }
    }
}