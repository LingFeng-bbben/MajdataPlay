using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;

public class SettingManager : MonoBehaviour
{
    
    public static SettingManager Instance;
    readonly string JsonPath = Application.streamingAssetsPath + "/settings.json";
    public SettingFile SettingFile;
    private void Awake()
    {
        Instance = this;
        if (File.Exists(JsonPath))
        {
            var js = File.ReadAllText(JsonPath);
            SettingFile = JsonConvert.DeserializeObject<SettingFile>(js);
        }
        else
        {
            SettingFile = new SettingFile();
            var jsnew = JsonConvert.SerializeObject(SettingFile);
            File.WriteAllText(JsonPath, jsnew);
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(this);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnApplicationQuit()
    {
        var jsnew = JsonConvert.SerializeObject(SettingFile);
        File.WriteAllText(JsonPath, jsnew);
    }
}

public enum SoundBackendType
{
    WaveOut,Asio,Unity
}

public class SettingFile
{
    public SoundBackendType SoundBackend { get; set; } = SoundBackendType.WaveOut;
    public int SoundOutputSamplerate { get; set; } = 44100;
}
