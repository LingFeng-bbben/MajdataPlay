using Cysharp.Threading.Tasks;
using MajdataPlay.Diagnostics;
using MajdataPlay.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
#nullable enable
namespace MajdataPlay.Settings
{
    internal static class ChartSettingStorage
    {
        static bool _isInited = false;

        readonly static Dictionary<string, ChartSetting> _storage = new(1024);
        static string STORAGE_PATH = string.Empty;

        static SpinLock _lock = new();
        
        public static async ValueTask InitAsync()
        {
            if(_isInited)
            {
                return;
            }
            try
            {
                if(string.IsNullOrEmpty(STORAGE_PATH))
                {
                    STORAGE_PATH = Path.Combine(MajEnv.RootPath, "ChartSetting.db");
                }
                await UniTask.SwitchToThreadPool();
                GameManager.OnSave += OnSave;
                if (!File.Exists(STORAGE_PATH))
                {
                    return;
                }
                using var fileStream = File.OpenRead(STORAGE_PATH);
                var (isSuccess, data, exception) = await Serializer.Json.TryDeserializeAsync<ChartSetting[]>(fileStream);
                if (!isSuccess)
                {
                    var path = Path.Combine(STORAGE_PATH, $"{DateTime.Now:yyyy-MM-dd-HH-mm-ss}.bak");
                    File.Copy(STORAGE_PATH, path);
                    MajDebug.LogError($"Failed to load chart settings\nPath: {STORAGE_PATH}\nException: {exception}");
                }
                else
                {
                    for (var i = 0; i < data.Length; i++)
                    {
                        var setting = data[i];
                        if(string.IsNullOrEmpty(setting.Hash) || _storage.TryGetValue(setting.Hash, out _))
                        {
                            continue;
                        }
                        if (setting.Unit != MajEnv.Settings.Debug.OffsetUnit)
                        {
                            if(setting.Unit == OffsetUnitOption.Second) // Second => Frame
                            {
                                setting.AudioOffset /= MajEnv.FRAME_LENGTH_SEC;
                            }
                            else if(setting.Unit == OffsetUnitOption.Frame) // Frame => Second
                            {
                                setting.AudioOffset *= MajEnv.FRAME_LENGTH_SEC;
                            }
                            setting.Unit = MajEnv.Settings.Debug.OffsetUnit;
                        }
                        _storage.Add(setting.Hash, setting);
                    }
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
            ref var @lock = ref _lock;
            var isLocked = false;

            try
            {
                @lock.Enter(ref isLocked);

                if (!_storage.TryGetValue(hash, out var setting))
                {
                    setting = new()
                    {
                        Hash = hash,
                        Unit = MajEnv.Settings.Debug.OffsetUnit
                    };
                    _storage.Add(hash, setting);
                }
                return setting;
            }
            finally
            {
                if(isLocked)
                {
                    @lock.Exit();
                }
            }
        }
        public static void ConvertUnitToFrame()
        {
            ref var @lock = ref _lock;
            var isLocked = false;
            try
            {
                @lock.Enter(ref isLocked);
                foreach(var setting in _storage.Values)
                {
                    if (setting.Unit != OffsetUnitOption.Frame)
                    {
                        setting.Unit = OffsetUnitOption.Frame;
                        setting.AudioOffset /= MajEnv.FRAME_LENGTH_SEC;
                    }
                }
            }
            finally
            {
                if (isLocked)
                {
                    @lock.Exit();
                }
            }
        }
        public static void ConvertUnitToSecond()
        {
            ref var @lock = ref _lock;
            var isLocked = false;
            try
            {
                @lock.Enter(ref isLocked);
                foreach(var setting in _storage.Values)
                {
                    if (setting.Unit != OffsetUnitOption.Second)
                    {
                        setting.Unit = OffsetUnitOption.Second;
                        setting.AudioOffset *= MajEnv.FRAME_LENGTH_SEC;
                    }
                }
            }
            finally
            {
                if (isLocked)
                {
                    @lock.Exit();
                }
            }
        }
        static void OnSave(object? sender, EventArgs? args)
        {
            if (!_isInited)
            {
                return;
            }
            ref var @lock = ref _lock;
            var isLocked = false;
            try
            {
                @lock.Enter(ref isLocked);
                var json = Serializer.Json.Serialize(_storage.Values);
                File.WriteAllText(STORAGE_PATH, json);                
            }
            catch(Exception e)
            {
                MajDebug.LogException(e);
            }
            finally
            {
                if (isLocked)
                {
                    @lock.Exit();
                }
            }
        }
    }
}
