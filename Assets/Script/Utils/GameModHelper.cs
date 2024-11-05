using MajdataPlay.Extensions;
using MajdataPlay.Types.Mods;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
#nullable enable
namespace MajdataPlay.Utils
{
    public static class GameModHelper
    {
        static MajGameMod[] _gameMods = Array.Empty<MajGameMod>();
        
        public static void Initialize()
        {
            //var types = Assembly.GetExecutingAssembly().GetTypes()
            //                    .Where(t => t.IsSubclassOf(typeof(MajGameMod)) && !t.IsAbstract);
            //if (types.IsEmpty() || !_gameMods.IsEmpty())
            //    return;
            //_gameMods = new MajGameMod[types.Count()];
            //foreach(var (i,type) in types.WithIndex())
            //    _gameMods[i] = (MajGameMod)Activator.CreateInstance(type);
            var modPath = Path.Combine(GameManager.ModPath, "mod.json");
            var modSettingsPath = Path.Combine(GameManager.AssestsPath, "ModSettings.json");
            if (!File.Exists(modPath))
                return;
            if (Serializer.Json.TryDeserialize(
                    File.ReadAllText(modPath), 
                    out MajGameMod[]? gameMods, 
                    GameManager.JsonReaderOption) && gameMods is not null)
            {
                _gameMods = gameMods;
            }
            else
                Debug.LogError("Fail to load mods");

            if (!File.Exists(modSettingsPath))
                return;
            if (Serializer.Json.TryDeserialize(
                    File.ReadAllText(modSettingsPath),
                    out ModSetting[]? modSettings,
                    GameManager.JsonReaderOption) && modSettings is not null)
            {
                foreach(var setting in modSettings)
                {
                    if (setting is null)
                        continue;
                    var mod = GetGameMod(setting.Type);
                    if (mod is null)
                        continue;
                    mod.Value = setting.Value;
                    mod.Active = setting.Active;
                }
            }
            else
                Debug.LogError("Fail to load mod setting");
        }
        public static void Refresh()
        {
            var modSettingsPath = Path.Combine(GameManager.AssestsPath, "ModSettings.json");
            if (!File.Exists(modSettingsPath))
                return;
            if (Serializer.Json.TryDeserialize(
                    File.ReadAllText(modSettingsPath),
                    out ModSetting[]? modSettings,
                    GameManager.JsonReaderOption) && modSettings is not null)
            {
                foreach (var setting in modSettings)
                {
                    if (setting is null)
                        continue;
                    var mod = GetGameMod(setting.Type);
                    if (mod is null)
                        continue;
                    mod.Value = setting.Value;
                    mod.Active = setting.Active;
                }
            }
            else
                Debug.LogError("Fail to load mod setting");
        }
        public static void Save()
        {
            if (_gameMods.IsEmpty())
                return;
            var settings = new ModSetting[_gameMods.Length];
            foreach(var (i,mod) in _gameMods.WithIndex())
            {
                settings[i] = new ModSetting()
                {
                    Type = mod.Type,
                    Value = mod.Value,
                    Active = mod.Active,
                };
            }
            var modSettingsPath = Path.Combine(GameManager.AssestsPath, "ModSettings.json");
            var json = Serializer.Json.Serialize(settings,GameManager.JsonReaderOption);
            File.WriteAllText(modSettingsPath, json);
        }
        //static MajGameMod? GetGameMod<T>() where T : MajGameMod
        //{
        //    var mod = _gameMods.OfType<T>().FirstOrDefault();

        //    return mod;
        //}
        //public static bool IsActive<T>() where T : MajGameMod
        //{
        //    var mod = GetGameMod<T>();

        //    return mod?.Active ?? false;
        //}
        public static MajGameMod? GetGameMod(ModType modType)
        {
            var mod = _gameMods.Where(x => x.Type == modType).FirstOrDefault();

            return mod;
        }
        public static bool IsActive(ModType modType)
        {
            var mod = GetGameMod(modType);

            return mod?.Active ?? false;
        }
        class ModSetting
        {
            public ModType Type { get; init; }
            public bool Active { get; init; }
            public float Value { get; init; }
        }
    }
}
