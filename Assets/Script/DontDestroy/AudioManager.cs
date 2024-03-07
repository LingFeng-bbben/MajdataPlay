using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NAudio;
using NAudio.Wave;
using System.Linq;
using NAudio.Wave.SampleProviders;
using System;
using System.IO;
using Unity.VisualScripting;
using UnityEngine.UIElements;
using NAudio.CoreAudioApi;
using System.Threading;
using System.Runtime.CompilerServices;
using UnityEngine.Networking;

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
        "titlebgm.mp3",
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
    }
    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(this);
        var backend = SettingManager.Instance.SettingFile.SoundBackend;
        if (backend == SoundBackendType.Unity) {
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
                        SFXSamples.Add(file, new UnityAudioSample(myClip,gameObject));
                        
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
        } else {
            var sampleRate = SettingManager.Instance.SettingFile.SoundOutputSamplerate;
            var waveformat = WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, 2);
            mixer = new MixingSampleProvider(waveformat);
            mixer.ReadFully = true;
            foreach (var file in SFXFileNames)
            {
                var path = SFXFilePath + file;
                if (File.Exists(path))
                {
                    var provider = new PausableSoundProvider(new CachedSound(path));
                    SFXSamples.Add(file, new NAudioAudioSample( provider));
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
                asioOut = new AsioOut(devices.FirstOrDefault());
                print("Starting ASIO...at " + devices.FirstOrDefault() + " as " + sampleRate);
                asioOut.Init(mixer);
                asioOut.Play();
            }
            if (backend == SoundBackendType.WaveOut)
            {
                print("Starting WaveOut... with " + sampleRate);
                waveOut = new WaveOutEvent();
                waveOut.Init(mixer, false);
                waveOut.Play();
            }
        }
        if (PlayDebug) {
            IOManager.Instance.OnTouchAreaDown += OnTouchDown;
            IOManager.Instance.OnButtonDown += OnButtonDown;
        }
        
    }
    void OnTouchDown(object sender, TouchAreaEventArgs e)
    {
        SFXSamples["touch.wav"].PlayOneShot();
    }
    void OnButtonDown(object sender, ButtonEventArgs e)
    {
        //SFXSamples["answer.wav"].SetVolume(e.ButtonIndex / 8f);
        SFXSamples["answer.wav"].PlayOneShot();
    }

    private void OnApplicationQuit()
    {
        asioOut?.Stop();
        asioOut?.Dispose();
        waveOut?.Stop();
        waveOut?.Dispose();
    }
    
    public AudioSampleWrap LoadMusic(string path)
    {
        var backend = SettingManager.Instance.SettingFile.SoundBackend;
        if (File.Exists(path))
        {
            if (backend == SoundBackendType.Unity) {
                path = "file://" + path;
                if (File.Exists(path))
                {
                    using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(path, AudioType.UNKNOWN))
                    {
                        www.SendWebRequest();
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

    public void PlaySFX(string name)
    {
        AudioSampleWrap psp = null;
        if (SFXSamples.TryGetValue(name, out psp))
            psp.PlayOneShot();
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

public class CachedSound
{
    public float[] AudioData { get; private set; }
    public WaveFormat WaveFormat { get; private set; }
    //this might take time
    public CachedSound(string audioFileName)
    {
        using (var audioFileReader = new AudioFileReader(audioFileName))
        {
            var resampler = new WdlResamplingSampleProvider(audioFileReader, SettingManager.Instance.SettingFile.SoundOutputSamplerate);
            WaveFormat = resampler.WaveFormat;
            var wholeFile = new List<float>();
            var readBuffer = new float[resampler.WaveFormat.SampleRate * resampler.WaveFormat.Channels];
            int samplesRead;
            while ((samplesRead = resampler.Read(readBuffer, 0, readBuffer.Length)) > 0)
            {
                wholeFile.AddRange(readBuffer.Take(samplesRead));
            }
            AudioData = wholeFile.ToArray();
        }
    }
}

public class CachedSoundSampleProvider : ISampleProvider
{
    private readonly CachedSound cachedSound;
    public long position;
    public float volume = 1f;
    public CachedSoundSampleProvider(CachedSound cachedSound)
    {
        this.cachedSound = cachedSound;
    }

    public int Read(float[] buffer, int offset, int count)
    {
        var availableSamples = cachedSound.AudioData.Length - position;
        var samplesToCopy = Math.Min(availableSamples, count);

        Console.WriteLine(samplesToCopy);
        Array.Copy(cachedSound.AudioData, position, buffer, offset, samplesToCopy);
        if (volume != 1f)
        {
            for(int i = 0; i<buffer.Length; i++)
            {
                buffer[i] = volume * buffer[i];
            }
        }
        position += samplesToCopy;
        return (int)samplesToCopy;
    }
    public WaveFormat WaveFormat { get { return cachedSound.WaveFormat; } }
}

public class PausableSoundProvider : ISampleProvider
{
    private readonly CachedSoundSampleProvider cachedSound;
    public long position => cachedSound.position;
    public float volume => cachedSound.volume;
    public bool isPlaying = false;
    public PausableSoundProvider(CachedSoundSampleProvider cachedSound) {
        this.cachedSound = cachedSound;
    }
    public PausableSoundProvider(CachedSound cachedSound)
    {
        this.cachedSound = new CachedSoundSampleProvider(cachedSound);
    }
    public int Read(float[] buffer, int offset, int count)
    {
        if(isPlaying)
        {
            var ret = cachedSound.Read(buffer, offset, count);
            if (ret < buffer.Length)
            {
                isPlaying = false;
                cachedSound.position = 0;
                //PlayStopped?.Invoke(this, EventArgs.Empty);
                for (var n = 0; n < count; n++)
                    buffer[offset + n] = 0;
                return count;
            }
            return ret;
        }
        else
        {
            for (var n = 0; n < count; n++)
                buffer[offset + n] = 0;
            return count;
        }
    }
    public void PlayOneShot()
    {
        isPlaying = true;
        cachedSound.position = 0;
    }
    public void Play()
    {
        isPlaying = true;
    }
    public void Pause()
    {
        isPlaying = false;
    }
    public double GetCurrentTime()
    {
        return cachedSound.position / (double)WaveFormat.SampleRate / 2d;
    }
    public void SetVolume(float volume)
    {
        cachedSound.volume = volume;
    }
    public WaveFormat WaveFormat { get { return cachedSound.WaveFormat; } }
}

public abstract class AudioSampleWrap
{
    public abstract bool GetPlayState();
    public abstract void Play() ;
    public abstract void Pause();
    public abstract void PlayOneShot() ;
    public abstract double GetCurrentTime();
    public abstract void SetCurrentTime(float time);
    public abstract void SetVolume(float volume);
}

public class NAudioAudioSample : AudioSampleWrap
{
    private PausableSoundProvider soundProvider;
    public NAudioAudioSample(PausableSoundProvider pausableSound)
    {
        soundProvider = pausableSound;
    }
    public override bool GetPlayState()
    {
        return soundProvider.isPlaying;
    }
    public override void Play()
    {
        soundProvider.Play();
    }
    public override void Pause()
    {
        soundProvider.Pause();
    }
    public override void PlayOneShot()
    {
        soundProvider.PlayOneShot();
    }
    public override double GetCurrentTime()
    {
        return soundProvider.GetCurrentTime();
    }
    public override void SetCurrentTime(float time)
    {
        //TODO: time to sample
        //soundProvider.position = time;
        throw new NotImplementedException();
    }
    public override void SetVolume(float volume)
    {
        soundProvider.SetVolume(volume);
    }
}

public class UnityAudioSample : AudioSampleWrap
{
    private AudioClip audioClip;
    private AudioSource audioSource;
    private GameObject gameObject;
    public UnityAudioSample(AudioClip audioClip, GameObject gameObject)
    {
        this.audioClip = audioClip;
        this.gameObject = gameObject;
        audioSource = this.gameObject.AddComponent<AudioSource>();
        audioSource.clip = audioClip;
        audioSource.loop = false;
        audioSource.bypassEffects = true;
    }

    public override void PlayOneShot()
    {
        audioSource.time = 0;
        audioSource.Play();
    }
    public override bool GetPlayState()
    {
        return audioSource.isPlaying;
    }
    public override void SetVolume(float volume)
    {
        audioSource.volume = volume;
    }
    public override double GetCurrentTime()
    {
        return audioSource.time;
    }
    public override void SetCurrentTime(float time)
    {
        audioSource.time = time;
    }
    public override void Play()
    {
        audioSource.Play();
    }
    public override void Pause()
    {
        audioSource.Pause();
    }
}