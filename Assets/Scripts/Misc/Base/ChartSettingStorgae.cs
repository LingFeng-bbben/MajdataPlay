using Cysharp.Threading.Tasks;
using MajdataPlay.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace MajdataPlay.Settings
{
    internal static class ChartSettingStorgae
    {
        static bool _isInited = false;

        readonly static List<ChartSetting> _storage = new(1024);
        readonly static string STORAGE_PATH = Path.Combine(MajEnv.RootPath, "ChartSetting.db");
        
        public static async ValueTask InitAsync()
        {
            if(_isInited)
            {
                return;
            }
            try
            {
                await UniTask.SwitchToThreadPool();
                MajEnv.OnApplicationQuit += OnApplicationQuit;
                if (!File.Exists(STORAGE_PATH))
                {
                    return;
                }
                using var fileStream = File.OpenRead(STORAGE_PATH);
                var (isSuccess, data) = await Serializer.Json.TryDeserializeAsync<ChartSetting[]>(fileStream);
                if (!isSuccess)
                {
                    var path = Path.Combine(STORAGE_PATH, $"{DateTime.Now:yyyy-MM-dd-HH-mm-ss}.bak");
                    File.Copy(STORAGE_PATH, path);
                }
                else
                {
                    for (var i = 0; i < _storage.Count; i++)
                    {
                        var setting = data[i];
                        if (setting.Unit != MajEnv.UserSettings.Debug.OffsetUnit)
                        {
                            if(setting.Unit == OffsetUnitOption.Second) // Second => Frame
                            {
                                setting.AudioOffset /= MajEnv.FRAME_LENGTH_SEC;
                            }
                            else if(setting.Unit == OffsetUnitOption.Frame) // Frame => Second
                            {
                                setting.AudioOffset *= MajEnv.FRAME_LENGTH_SEC;
                            }
                            setting.Unit = MajEnv.UserSettings.Debug.OffsetUnit;
                        }
                    }
                    _storage.AddRange(data);
                }
            }
            finally
            {
                _isInited = true;
            }
        }
        public static ChartSetting GetSetting(ISongDetail chartInfo)
        {
            return GetSetting(chartInfo.Hash);
        }
        public static ChartSetting GetSetting(string hash)
        {
            var setting = _storage.Find(x => x.Hash == hash);

            if (setting is null)
            {
                setting = new()
                {
                    Hash = hash,
                    Unit = MajEnv.UserSettings.Debug.OffsetUnit
                };
                _storage.Add(setting);
            }

            return setting;
        }
        public static void ConvertUnitToFrame()
        {
            for (var i = 0; i < _storage.Count; i++)
            {
                var setting = _storage[i];
                if(setting.Unit != OffsetUnitOption.Frame)
                {
                    setting.Unit = OffsetUnitOption.Frame;
                    setting.AudioOffset /= MajEnv.FRAME_LENGTH_SEC;
                }
            }
        }
        public static void ConvertUnitToSecond()
        {
            for (var i = 0; i < _storage.Count; i++)
            {
                var setting = _storage[i];
                if (setting.Unit != OffsetUnitOption.Second)
                {
                    setting.Unit = OffsetUnitOption.Second;
                    setting.AudioOffset *= MajEnv.FRAME_LENGTH_SEC;
                }
            }
        }
        static void OnApplicationQuit()
        {
            try
            {
                if (!_isInited)
                {
                    return;
                }
                var json = Serializer.Json.Serialize(_storage);
                File.WriteAllText(STORAGE_PATH, json);
            }
            finally
            {
                MajEnv.OnApplicationQuit -= OnApplicationQuit;
            }
        }
    }
}
