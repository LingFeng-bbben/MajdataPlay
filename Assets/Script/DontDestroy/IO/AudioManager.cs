using System.Collections.Generic;
using UnityEngine;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.IO;
using UnityEngine.Networking;
using MajdataPlay.Types;

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
        private Dictionary<string, AudioSampleWrap> SFXSamples = new Dictionary<string, AudioSampleWrap>();
        private AsioOut asioOut;
        private WaveOutEvent waveOut;
        private MixingSampleProvider mixer;
        public bool PlayDebug;
        private void Awake()
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
        // Start is called before the first frame update
        void Start()
        {
            var backend = GameManager.Instance.Setting.Audio.Backend;
            if (backend == SoundBackendType.Unity)
            {
                foreach (var file in SFXFileNames)
                {
                    var path = "file://" + SFXFilePath + file;
                    if (File.Exists(SFXFilePath + file))
                    {
                        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(path, AudioType.UNKNOWN))
                        {
                            www.SendWebRequest();
                            while (!www.isDone) ;
                            AudioClip myClip = DownloadHandlerAudioClip.GetContent(www);
                            SFXSamples.Add(file, new UnityAudioSample(myClip, gameObject));

                        }
                    }
                    else
                    {
                        Debug.LogWarning(path + " dos not exists");
                    }
                }
                foreach (var file in VoiceFileNames)
                {
                    var path = "file://" + VoiceFilePath + file;
                    if (File.Exists(VoiceFilePath + file))
                    {
                        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(path, AudioType.UNKNOWN))
                        {
                            www.SendWebRequest();
                            while (!www.isDone) ;
                            AudioClip myClip = DownloadHandlerAudioClip.GetContent(www);
                            SFXSamples.Add(file, new UnityAudioSample(myClip, gameObject));

                        }
                    }
                    else
                    {
                        Debug.LogWarning(path + " dos not exists");
                    }
                }
            }
            else
            {
                var sampleRate = GameManager.Instance.Setting.Audio.Samplerate;
                var waveformat = WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, 2);
                mixer = new MixingSampleProvider(waveformat);
                mixer.ReadFully = true;
                foreach (var file in SFXFileNames)
                {
                    var path = SFXFilePath + file;
                    if (File.Exists(path))
                    {
                        var provider = new PausableSoundProvider(new CachedSound(path));
                        SFXSamples.Add(file, new NAudioAudioSample(provider));
                        mixer.AddMixerInput(provider);
                    }
                    else
                    {
                        Debug.LogWarning(path + " dos not exists");
                    }
                }

                foreach (var file in VoiceFileNames)
                {
                    var path = VoiceFilePath + file;
                    if (File.Exists(path))
                    {
                        var provider = new PausableSoundProvider(new CachedSound(path));
                        SFXSamples.Add(file, new NAudioAudioSample(provider));
                        mixer.AddMixerInput(provider);
                    }
                    else
                    {
                        Debug.LogWarning(path + " dos not exists");
                    }
                }
                if (backend == SoundBackendType.Asio)
                {
                    var devices = AsioOut.GetDriverNames();
                    foreach(var device in devices) { print(device); }
                    asioOut = new AsioOut(devices[GameManager.Instance.Setting.Audio.AsioDeviceIndex]);
                    print("Starting ASIO...at " + asioOut.DriverName + " as " + sampleRate);
                    asioOut.Init(mixer);
                    asioOut.Play();
                }
                if (backend == SoundBackendType.WaveOut)
                {
                    print("Starting WaveOut... with " + sampleRate);
                    waveOut = new WaveOutEvent();
                    waveOut.NumberOfBuffers = 12;
                    waveOut.Init(mixer, false);
                    waveOut.Play();
                }
            }
            if (PlayDebug)
                InputManager.Instance.BindAnyArea(OnAnyAreaDown);
            ReadVolumeFromSettings();
        }
        void OnAnyAreaDown(object sender, InputEventArgs e)
        {
            if (e.Status != SensorStatus.On)
                return;
            if(e.IsButton)
                SFXSamples["answer.wav"].PlayOneShot();
            else
                SFXSamples["touch.wav"].PlayOneShot();
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
            SFXSamples["answer.wav"].SetVolume(setting.Audio.Volume.Anwser);
            SFXSamples["all_perfect.wav"].SetVolume(setting.Audio.Volume.Voice);
            SFXSamples["break.wav"].SetVolume(setting.Audio.Volume.Break);
            SFXSamples["break_slide.wav"].SetVolume(setting.Audio.Volume.Break);
            SFXSamples["break_slide_start.wav"].SetVolume(setting.Audio.Volume.Slide);
            SFXSamples["clock.wav"].SetVolume(setting.Audio.Volume.Anwser);
            SFXSamples["hanabi.wav"].SetVolume(setting.Audio.Volume.Touch);
            SFXSamples["judge.wav"].SetVolume(setting.Audio.Volume.Judge);
            SFXSamples["judge_break.wav"].SetVolume(setting.Audio.Volume.Judge);
            SFXSamples["judge_break_slide.wav"].SetVolume(setting.Audio.Volume.Judge);
            SFXSamples["judge_ex.wav"].SetVolume(setting.Audio.Volume.Judge);
            SFXSamples["slide.wav"].SetVolume(setting.Audio.Volume.Slide);
            SFXSamples["touch.wav"].SetVolume(setting.Audio.Volume.Touch);
            SFXSamples["touchHold_riser.wav"].SetVolume(setting.Audio.Volume.Touch);
            SFXSamples["track_start.wav"].SetVolume(setting.Audio.Volume.BGM);
            SFXSamples["good.wav"].SetVolume(setting.Audio.Volume.Judge);
            SFXSamples["great.wav"].SetVolume(setting.Audio.Volume.Judge);
            SFXSamples["titlebgm.mp3"].SetVolume(setting.Audio.Volume.BGM);

            SFXSamples["MajdataPlay.wav"].SetVolume(setting.Audio.Volume.Voice);
            SFXSamples["SelectSong.wav"].SetVolume(setting.Audio.Volume.Voice);
            SFXSamples["Sugoi.wav"].SetVolume(setting.Audio.Volume.Voice);
            SFXSamples["DontTouchMe.wav"].SetVolume(setting.Audio.Volume.Voice);
        }

        public AudioSampleWrap LoadMusic(string path)
        {
            var backend = GameManager.Instance.Setting.Audio.Backend;
            if (File.Exists(path))
            {
                if (backend == SoundBackendType.Unity)
                {
                    if (File.Exists(path))
                    {
                        path = "file://" + path;
                        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(path, AudioType.UNKNOWN))
                        {
                            var handle = www.SendWebRequest();
                            while (!handle.isDone) ;
                            AudioClip myClip = DownloadHandlerAudioClip.GetContent(www);
                            return new UnityAudioSample(myClip, gameObject);
                        }
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    var provider = new PausableSoundProvider(new CachedSound(path));
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
        public void UnLoadMusic(AudioSampleWrap sample)
        {
            throw new NotImplementedException();
        }

        public void PlaySFX(string name,bool isLoop=false)
        {
            AudioSampleWrap psp = null;
            if (SFXSamples.TryGetValue(name, out psp)) { 
                psp.PlayOneShot();
                psp.IsLoop = isLoop;
            }   
            else
                Debug.LogError("No such SFX");
        }
        public void StopSFX(string name)
        {
            AudioSampleWrap psp = null;
            if (SFXSamples.TryGetValue(name, out psp))
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