using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor.Build;
using UnityEngine;

#if UNITY_GAMECORE
using UnityEditor.GameCore;
using UnityEditor.TestRunner.CommandLineParser;
#endif

namespace UnityEditor.TestTools.TestRunner
{
    internal class GameCorePlatformSetup : IPlatformSetup
    {
#if UNITY_GAMECORE
        private GameCoreDeployMethod gameCoreDeploymentMethod = GameCoreDeployMethod.Push;
        private const string k_PlsSize = "512";
        private const string k_PlsGrow = "1024";
        private const string k_TitleID = "73ECA03C";
        private const string k_MSAAppId = "0000000040283475";
        private const string k_SCID = "00000000-0000-0000-0000-000073ECA03C";

        private const string k_GameCorePackageDeployment = "GameCoreDeploymentMethod";

        private string m_Warning;

        private static GameCoreSettings GetGameCoreSettings()
        {
#if UNITY_GAMECORE_XBOXSERIES
            return GameCoreScarlettSettings.GetInstance();
#elif UNITY_GAMECORE_XBOXONE
            return GameCoreXboxOneSettings.GetInstance();
#endif
        }

        bool IsValidMsaAppId(string id)
        {
            if (id == null || id.Length != 16 || id == "0000000000000000")
                return false;

            for (int i = 0; i < id.Length; i++)
            {
                if (!Uri.IsHexDigit(id[i]))
                    return false;
            }

            return true;
        }

        bool IsValidScidAndTitleId(string scid, string titleId)
        {
            if (titleId == null || titleId.Length != 8 || titleId == "00000000")
                return false;

            for (int i = 0; i < titleId.Length; i++)
            {
                if (!Uri.IsHexDigit(titleId[i]))
                    return false;
            }

            if (!Guid.TryParse(scid, out _) || scid == "00000000-0000-0000-0000-000000000000")
                return false;

            if (!string.IsNullOrEmpty(titleId) && !scid.EndsWith(titleId, StringComparison.OrdinalIgnoreCase))
                return false;

            return true;
        }

        bool IsValidPersistentLocalStorage(GameCore.GameConfig.CT_PersistentLocalStorage pls)
        {
            return pls != null && !string.IsNullOrEmpty(pls.SizeMB) && !string.IsNullOrEmpty(pls.GrowableToMB);
        }

        private Dictionary<string, object> GetTestSettings()
        {
            var commandLineArgs = Environment.GetCommandLineArgs();
            var testSettingsFilePath = string.Empty;
            var optionSet = new CommandLineOptionSet(new CommandLineOption("testSettingsFile", settingsFilePath => { testSettingsFilePath = settingsFilePath; }));
            optionSet.Parse(commandLineArgs);
            Dictionary<string, object> settingsDictionary = null;
            if (!string.IsNullOrEmpty(testSettingsFilePath))
            {
                var text = File.ReadAllText(testSettingsFilePath);
                settingsDictionary = Json.Deserialize(text) as Dictionary<string, object>;
            }

            return settingsDictionary;
        }

        private bool TrySetDeploymentMethod(Dictionary<string, object> settingsDictionary)
        {
            if (settingsDictionary.ContainsKey(k_GameCorePackageDeployment))
            {
                settingsDictionary.TryGetValue(k_GameCorePackageDeployment, out var argumentValue);
                gameCoreDeploymentMethod = (GameCoreDeployMethod)Enum.Parse(typeof(GameCoreDeployMethod), argumentValue.ToString());
                return true;
            }

            return false;
        }
#endif

        public void Setup()
        {
#if UNITY_GAMECORE
            GameCoreSettings settings = GetGameCoreSettings();

            if (settings == null)
                return;

            bool changed = false;
            var settingsDictionary = GetTestSettings();
            if (settingsDictionary != null)
            {
                if (TrySetDeploymentMethod(settingsDictionary))
                {
                    settings.DeploymentMethod = gameCoreDeploymentMethod;
                    changed = true;
                }
            }

            if (!IsValidMsaAppId(settings.GameConfig.MSAAppId))
            {
                settings.GameConfig.MSAAppId = k_MSAAppId;
                changed = true;
            }

            if (!IsValidScidAndTitleId(settings.SCID, settings.GameConfig.TitleId))
            {
                m_Warning = $"Invalid SCID '{settings.SCID}' or TitleId '{settings.GameConfig.TitleId}'. Setting to default values (TitleId: {k_TitleID}, SCID: {k_SCID}).";
                settings.GameConfig.TitleId = k_TitleID;
                settings.SCID = k_SCID;
                changed = true;
            }

            if (!IsValidPersistentLocalStorage(settings.GameConfig.PersistentLocalStorage))
            {
                if (settings.GameConfig.PersistentLocalStorage == null)
                {
                    settings.GameConfig.PersistentLocalStorage = new GameCore.GameConfig.CT_PersistentLocalStorage();
                }
                settings.GameConfig.PersistentLocalStorage.SizeMB = k_PlsSize;
                settings.GameConfig.PersistentLocalStorage.GrowableToMB = k_PlsGrow;
                changed = true;
            }

            if (changed)
            {
                settings.SaveGameConfig();
            }
#endif
        }

        public void PostBuildAction()
        {
        }

        public void PostSuccessfulBuildAction()
        {
        }

        public void PostSuccessfulLaunchAction()
        {
        }

        public void CleanUp()
        {
#if UNITY_GAMECORE
            if (!string.IsNullOrEmpty(m_Warning))
            {
                Debug.LogWarning(m_Warning);
                m_Warning = null;
            }
#endif
        }
    }
}
