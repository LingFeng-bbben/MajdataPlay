using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NAudio;
using NAudio.Wave;
using System.Linq;
using NAudio.Wave.SampleProviders;
using UnityEngine.Windows;
using System;
using Unity.VisualScripting;
using UnityEngine.UIElements;

public class AudioManager : MonoBehaviour
{
    readonly string SFXFilePath = Application.streamingAssetsPath + "/SFX/";
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
    };
    private Dictionary<string, PauseSoundSampleProvider> SFXSamples = new Dictionary<string, PauseSoundSampleProvider>();
    private AsioOut asioOut;
    private MixingSampleProvider mixer;
    public bool PlayDebug;
    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(this);
        mixer = new MixingSampleProvider(WaveFormat.CreateIeeeFloatWaveFormat(44100, 2));
        mixer.ReadFully = true;
        foreach(var file in SFXFileNames)
        {
            var path = SFXFilePath + file;
            if (File.Exists(path))
            {
                var provider =  new PauseSoundSampleProvider(new CachedSound(path));
                SFXSamples.Add(file, provider);
                mixer.AddMixerInput(provider);
            }
            else
            {
                Debug.LogWarning(path + " dos not exists");
            }
        }
        
        var devices = AsioOut.GetDriverNames();
        asioOut = new AsioOut(devices.FirstOrDefault());
        asioOut.Init(mixer);
        asioOut.Play();

        if(PlayDebug) {
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
    }
    // Update is called once per frame
    void Update()
    {
        //print(AudioFile.GetCurrentTime());
    }
}

class CachedSound
{
    public float[] AudioData { get; private set; }
    public WaveFormat WaveFormat { get; private set; }
    //this might take time
    public CachedSound(string audioFileName)
    {
        using (var audioFileReader = new AudioFileReader(audioFileName))
        {
            var resampler = new WdlResamplingSampleProvider(audioFileReader, 44100);
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

class CachedSoundSampleProvider : ISampleProvider
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

class PauseSoundSampleProvider : ISampleProvider
{
    private readonly CachedSoundSampleProvider cachedSound;
    public long position => cachedSound.position;
    public float volume => cachedSound.volume;
    public bool isPlaying = false;
    public event EventHandler PlayStopped;
    public PauseSoundSampleProvider(CachedSoundSampleProvider cachedSound) {
        this.cachedSound = cachedSound;
    }
    public PauseSoundSampleProvider(CachedSound cachedSound)
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
                PlayStopped?.Invoke(this, EventArgs.Empty);
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