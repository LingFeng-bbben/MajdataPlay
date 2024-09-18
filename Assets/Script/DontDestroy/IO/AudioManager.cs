using System.Collections.Generic;
using UnityEngine;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.IO;
using UnityEngine.Networking;
using MajdataPlay.Types;
#nullable enable
namespace MajdataPlay.IO
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance;
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
        private AsioOut asioOut;
        private WaveOutEvent waveOut;
        private MixingSampleProvider mixer;
        public bool PlayDebug;
        private void Awake()
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
        void Start()
        {
            var backend = GameManager.Instance.Setting.Audio.Backend;
            var sampleRate = GameManager.Instance.Setting.Audio.Samplerate;
            if (backend != SoundBackendType.Unity)
            {
                var waveformat = WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, 2);
                mixer = new MixingSampleProvider(waveformat);
                mixer.ReadFully = true;
            }
            InitSFXSample(SFXFileNames,SFXFilePath);
            InitSFXSample(VoiceFileNames,VoiceFilePath);
            switch(backend)
            {
                case SoundBackendType.Asio:
                    var devices = AsioOut.GetDriverNames();
                    foreach (var device in devices) { print(device); }
                    asioOut = new AsioOut(devices[GameManager.Instance.Setting.Audio.AsioDeviceIndex]);
                    print("Starting ASIO...at " + asioOut.DriverName + " as " + sampleRate);
                    asioOut.Init(mixer);
                    asioOut.Play();
                    break;
                case SoundBackendType.WaveOut:
                    print("Starting WaveOut... with " + sampleRate);
                    waveOut = new WaveOutEvent();
                    waveOut.NumberOfBuffers = 12;
                    waveOut.Init(mixer, false);
                    waveOut.Play();
                    break;
            }
            if (PlayDebug)
                InputManager.Instance.BindAnyArea(OnAnyAreaDown);
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

                switch(GameManager.Instance.Setting.Audio.Backend)
                {
                    case SoundBackendType.Unity:
                        SFXSamples.Add(UnityAudioSample.ReadFromFile($"file://{path}", gameObject));
                        break;
                    case SoundBackendType.WaveOut:
                    case SoundBackendType.Asio:
                        var provider = new PausableSoundProvider(new CachedSound(path), mixer);
                        SFXSamples.Add(new NAudioAudioSample(provider));
                        mixer.AddMixerInput(provider);
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
            asioOut?.Stop();
            asioOut?.Dispose();
            waveOut?.Stop();
            waveOut?.Dispose();
        }

        public void ReadVolumeFromSettings()
        {
            var setting = GameManager.Instance.Setting;
            SFXSamples[(int)SFXSampleType.ANSWER]?.SetVolume(setting.Audio.Volume.Anwser);
            SFXSamples[(int)SFXSampleType.ALL_PERFECT]?.SetVolume(setting.Audio.Volume.Voice);
            SFXSamples[(int)SFXSampleType.BREAK]?.SetVolume(setting.Audio.Volume.Break);
            SFXSamples[(int)SFXSampleType.BREAK_SLIDE]?.SetVolume(setting.Audio.Volume.Break);
            SFXSamples[(int)SFXSampleType.BREAK_SLIDE_START]?.SetVolume(setting.Audio.Volume.Slide);
            SFXSamples[(int)SFXSampleType.CLOCK]?.SetVolume(setting.Audio.Volume.Anwser);
            SFXSamples[(int)SFXSampleType.HANABI]?.SetVolume(setting.Audio.Volume.Touch);
            SFXSamples[(int)SFXSampleType.JUDGE]?.SetVolume(setting.Audio.Volume.Judge);
            SFXSamples[(int)SFXSampleType.JUDGE_BREAK]?.SetVolume(setting.Audio.Volume.Judge);
            SFXSamples[(int)SFXSampleType.JUDGE_BREAK_SLIDE]?.SetVolume(setting.Audio.Volume.Judge);
            SFXSamples[(int)SFXSampleType.JUDGE_EX]?.SetVolume(setting.Audio.Volume.Judge);
            SFXSamples[(int)SFXSampleType.SLIDE]?.SetVolume(setting.Audio.Volume.Slide);
            SFXSamples[(int)SFXSampleType.TOUCH]?.SetVolume(setting.Audio.Volume.Touch);
            SFXSamples[(int)SFXSampleType.TOUCH_HOLD_RISER]?.SetVolume(setting.Audio.Volume.Touch);
            SFXSamples[(int)SFXSampleType.TRACK_START]?.SetVolume(setting.Audio.Volume.BGM);
            SFXSamples[(int)SFXSampleType.GOOD]?.SetVolume(setting.Audio.Volume.Judge);
            SFXSamples[(int)SFXSampleType.GREAT]?.SetVolume(setting.Audio.Volume.Judge);
            SFXSamples[(int)SFXSampleType.TITLE_BGM]?.SetVolume(setting.Audio.Volume.BGM);

            SFXSamples[(int)SFXSampleType.MAJDATA_PLAY]?.SetVolume(setting.Audio.Volume.Voice);
            SFXSamples[(int)SFXSampleType.SELECT_SONG]?.SetVolume(setting.Audio.Volume.Voice);
            SFXSamples[(int)SFXSampleType.SUGOI]?.SetVolume(setting.Audio.Volume.Voice);
            SFXSamples[(int)SFXSampleType.DONT_TOUCH_ME]?.SetVolume(setting.Audio.Volume.Voice);
        }

        public AudioSampleWrap? LoadMusic(string path)
        {
            var backend = GameManager.Instance.Setting.Audio.Backend;
            if (File.Exists(path))
            {
                switch(backend)
                {
                    case SoundBackendType.Unity:
                        return UnityAudioSample.ReadFromFile($"file://{path}", gameObject);
                    default:
                        var provider = new PausableSoundProvider(new CachedSound(path), mixer);
                        mixer.AddMixerInput(provider);
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
        public void StopSFX(in SFXSampleType sfxType)
        {
            var psp = SFXSamples[(int)sfxType];
            if (psp is not null)
                psp.Pause();
            else
                Debug.LogError("No such SFX");
        }
        public void OpenAsioPannel()
        {
            asioOut?.ShowControlPanel();
        }
    }
}