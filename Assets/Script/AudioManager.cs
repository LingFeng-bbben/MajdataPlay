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
    PauseSoundSampleProvider AudioFile;
    AsioOut asioOut;
    MixingSampleProvider mixer;
    // Start is called before the first frame update
    void Start()
    {
        AudioFile = new PauseSoundSampleProvider(new CachedSound( SFXFilePath + "answer.wav"));
        mixer = new MixingSampleProvider(WaveFormat.CreateIeeeFloatWaveFormat(48000, 2));
        mixer.ReadFully = true;
        var devices = AsioOut.GetDriverNames();
        asioOut = new AsioOut(devices.FirstOrDefault());
        
        mixer.AddMixerInput(AudioFile);
        asioOut.Init(mixer);
        asioOut.Play();
        IOManager.Instance.OnTouchAreaDown += OnTouchDown;
        IOManager.Instance.OnButtonDown += OnButtonDown;
    }

    void OnTouchDown(object sender, TouchAreaEventArgs e)
    {
       AudioFile.PlayOneShot();
    }
    void OnButtonDown(object sender, ButtonEventArgs e)
    {
        AudioFile.PlayOneShot();
    }

    private void OnApplicationQuit()
    {
        asioOut?.Stop();
        asioOut?.Dispose();
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}

class CachedSound
{
    public float[] AudioData { get; private set; }
    public WaveFormat WaveFormat { get; private set; }
    public CachedSound(string audioFileName)
    {
        using (var audioFileReader = new AudioFileReader(audioFileName))
        {
            // TODO: could add resampling in here if required
            WaveFormat = audioFileReader.WaveFormat;
            var wholeFile = new List<float>((int)(audioFileReader.Length / 4));
            var readBuffer = new float[audioFileReader.WaveFormat.SampleRate * audioFileReader.WaveFormat.Channels];
            int samplesRead;
            while ((samplesRead = audioFileReader.Read(readBuffer, 0, readBuffer.Length)) > 0)
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
        position += samplesToCopy;
        return (int)samplesToCopy;
    }
    public WaveFormat WaveFormat { get { return cachedSound.WaveFormat; } }
}

class PauseSoundSampleProvider : ISampleProvider
{
    private readonly CachedSoundSampleProvider cachedSound;
    public long position => cachedSound.position;
    public bool isPlaying = false;
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
            if (ret < 256)
            {
                isPlaying = false;
                cachedSound.position = 0;
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
    public WaveFormat WaveFormat { get { return cachedSound.WaveFormat; } }
}