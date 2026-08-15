using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

#if UNITY_SWITCH2
using UnityEditor.TestRunner.CommandLineParser;
#endif

namespace UnityEditor.TestTools.TestRunner
{
    internal class Switch2PlatformSetup : IPlatformSetup
    {
        public void Setup()
        {
            EditorUserBuildSettings.development = true;
#if UNITY_SWITCH2
            var isPerformanceTestRun = GetIsPerformanceTestRun();
            Debug.Log($"[Switch2PlatformSetup] Calling PrepareForTestRun(isPerformanceTestRun={isPerformanceTestRun})");
            UnityEditor.Switch2.EditorUserBuildSettingsUtility.PrepareForTestRun(isPerformanceTestRun);
#endif
        }

#if UNITY_SWITCH2
        // Reads the isPerformanceTestRun flag from the test settings JSON written by UTR.
        // This mirrors the approach used in GameCore's RunPlayerBuildCommand
        // (PlatformDependent/GameCore/Testing/Unity.Automation.Players.GameCore/Starter.cs)
        // which passes build configuration explicitly via the eval system rather than environment variables.
        private bool GetIsPerformanceTestRun()
        {
            var commandLineArgs = Environment.GetCommandLineArgs();
            var testSettingsFilePath = string.Empty;
            var optionSet = new CommandLineOptionSet(
                new CommandLineOption("testSettingsFile", filePath => testSettingsFilePath = filePath));
            optionSet.Parse(commandLineArgs);

            Debug.Log($"[Switch2PlatformSetup] testSettingsFile path from command line: '{testSettingsFilePath}'");

            if (string.IsNullOrEmpty(testSettingsFilePath))
            {
                Debug.Log("[Switch2PlatformSetup] No testSettingsFile argument found; defaulting isPerformanceTestRun=false");
                return false;
            }

            if (!File.Exists(testSettingsFilePath))
            {
                Debug.LogWarning($"[Switch2PlatformSetup] testSettingsFile not found at '{testSettingsFilePath}'; defaulting isPerformanceTestRun=false");
                return false;
            }

            // testSettingsFile is a JSON file written by UTR's EditorStartInfoBuilder
            // (Tests/Unity.UnityTestFramework.PluginBase/EditorStartInfoBuilder.cs) before
            // the editor process is launched. It contains build configuration fields such as
            // scriptingBackend, apiProfile, and isPerformanceTestRun, serialized from
            // Unity.Automation.TestSettings (Tests/Unity.UnityTestFramework.PluginBase/TestSettings.cs).
            var text = File.ReadAllText(testSettingsFilePath);
            var settings = Json.Deserialize(text) as Dictionary<string, object>;

            if (settings == null)
            {
                Debug.LogWarning($"[Switch2PlatformSetup] Failed to parse JSON from '{testSettingsFilePath}'; defaulting isPerformanceTestRun=false");
                return false;
            }

            var hasKey = settings.TryGetValue("isPerformanceTestRun", out var value);
            if (!hasKey)
                Debug.LogWarning($"[Switch2PlatformSetup] 'isPerformanceTestRun' key not found in '{testSettingsFilePath}'. " +
                    "The UTR version that wrote this file may predate the field. Defaulting to false. " +
                    "Ensure UTR is up to date so the field is written explicitly.");
            var result = hasKey && value is bool b && b;
            Debug.Log($"[Switch2PlatformSetup] isPerformanceTestRun key present={hasKey}, raw value='{value}', resolved={result}");
            return result;
        }
#endif

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
        }
    }
}
