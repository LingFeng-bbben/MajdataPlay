using UnityEngine;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System.IO;
using System.Collections.Generic;
using MajdataPlay.Types;
using MajdataPlay.Extensions;
using NAudio.CoreAudioApi;
#nullable enable
namespace MajdataPlay.IO
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }
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
            var deviceIndex = GameManager.Instance.Setting.Audio.AsioDeviceIndex;
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
                        audioOutputDevice.Init(mixer);
                        audioOutputDevice.Play();
                    }
                    break;
                case SoundBackendType.WaveOut:
                    {
                        print("Starting WaveOut... with " + sampleRate);
                        var waveOut = new WaveOutEvent();
                        waveOut.NumberOfBuffers = 12;
                        audioOutputDevice = waveOut;
                        audioOutputDevice.Init(mixer, false);
                        audioOutputDevice.Play();
                    }
                    break;
                case SoundBackendType.Wasapi:
                    {
                        print("Starting Wasapi... with " + sampleRate);
                        var wasapi = new WasapiOut(AudioClientShareMode.Shared, 0);
                        wasapi.Init(mixer);
                        audioOutputDevice = wasapi;
                        audioOutputDevice.Play();
                    }
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
                    case SoundBackendType.Wasapi:
                    case SoundBackendType.WaveOut:
                    case SoundBackendType.Asio:
                        var provider = new CachedSampleProvider(new CachedSound(path), mixer);
                        SFXSamples.Add(new NAudioAudioSample(provider));
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
        }

        public void ReadVolumeFromSettings()
        {
            var setting = GameManager.Instance.Setting;
            SFXSamples[(int)SFXSampleType.ANSWER]?.SetVolume(setting.Audio.Volume.Answer);
            SFXSamples[(int)SFXSampleType.ALL_PERFECT]?.SetVolume(setting.Audio.Volume.Voice);
            SFXSamples[(int)SFXSampleType.BREAK]?.SetVolume(setting.Audio.Volume.Break);
            SFXSamples[(int)SFXSampleType.BREAK_SLIDE]?.SetVolume(setting.Audio.Volume.Break);
            SFXSamples[(int)SFXSampleType.BREAK_SLIDE_START]?.SetVolume(setting.Audio.Volume.Slide);
            SFXSamples[(int)SFXSampleType.CLOCK]?.SetVolume(setting.Audio.Volume.Answer);
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
                        var provider = new CachedSampleProvider(new CachedSound(path), mixer);
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
            if(audioOutputDevice is AsioOut asioOut)
                asioOut.ShowControlPanel();
        }
    }
}