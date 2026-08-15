using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UnityEditor.TestTools.TestRunner
{
    internal class PlayerLauncherBuildOptions
    {
        public BuildPlayerOptions BuildPlayerOptions;

#if UNITY_6000_1_OR_NEWER
        public BuildPlayerWithProfileOptions BuildPlayerWithProfileOptions;
#endif

        public string PlayerDirectory;
        private EditorBuildSettingsScene[] originalScenes = Array.Empty<EditorBuildSettingsScene>();

        public override string ToString()
        {
            var str = new StringBuilder();

#if UNITY_6000_1_OR_NEWER
            if (BuildPlayerWithProfileOptions.buildProfile != null)
            {
                var buildProfile = BuildPlayerWithProfileOptions.buildProfile;
                str.AppendLine(string.Concat("BuildTarget: ", buildProfile.buildTarget.ToString()));
                str.AppendLine(string.Concat("BuildPlayerLocation: ", BuildPlayerWithProfileOptions.locationPathName));
                str.AppendLine(string.Concat("Options: ", BuildPlayerWithProfileOptions.options.ToString()));
                str.AppendLine("assetBundleManifestPath = " + BuildPlayerWithProfileOptions.assetBundleManifestPath);
                str.Append((buildProfile.overrideGlobalScenes) ? "Profile Scenes: " : "Global Scenes: ");
                var scenes = (buildProfile.overrideGlobalScenes)
                    ? buildProfile.scenes
                    : EditorBuildSettings.GetEditorBuildSettingsSceneIgnoreProfile();
                for (int i = 0; i < scenes.Length; i++)
                {
                    str.Append(JsonUtility.ToJson(scenes[i]));
                    str.Append(", ");
                }
                str.AppendLine();

                return str.ToString();
            }
#endif

            str.AppendLine("locationPathName = " + BuildPlayerOptions.locationPathName);
            str.AppendLine("target = " + BuildPlayerOptions.target);
            str.AppendLine("scenes = " + string.Join(", ", BuildPlayerOptions.scenes));
            str.AppendLine("assetBundleManifestPath = " + BuildPlayerOptions.assetBundleManifestPath);
            str.AppendLine("options.Development = " + ((BuildPlayerOptions.options & BuildOptions.Development) != 0));
            str.AppendLine("options.AutoRunPlayer = " + ((BuildPlayerOptions.options & BuildOptions.AutoRunPlayer) != 0));
            return str.ToString();
        }

        public BuildOptions GetCurrentBuildOptions()
        {
#if UNITY_6000_1_OR_NEWER
            if (BuildPlayerWithProfileOptions.buildProfile != null)
                return BuildPlayerWithProfileOptions.options;
#endif

            return BuildPlayerOptions.options;
        }

        public string GetCurrentLocationPath()
        {
#if UNITY_6000_1_OR_NEWER
            if (BuildPlayerWithProfileOptions.buildProfile != null)
                return BuildPlayerWithProfileOptions.locationPathName;
#endif

            return BuildPlayerOptions.locationPathName;
        }

#if UNITY_6000_1_OR_NEWER
        /// <summary>
        /// Prepares to build with a build profile. Caching the profiles scene list
        /// before injecting the test scene path.
        /// </summary>
        public void OnBeforeBuildProfileBuild(string testScenePath)
        {
            var profile = BuildPlayerWithProfileOptions.buildProfile;

            // Initial test scene must be first entry in scene list.
            var scenes = new List<EditorBuildSettingsScene>
            {
                new EditorBuildSettingsScene(testScenePath, true)
            };
            
            if (profile.overrideGlobalScenes)
            {
                originalScenes = profile.scenes;
                scenes.AddRange(profile.scenes);
                profile.scenes = scenes.ToArray();
            }
            else
            {
                originalScenes = EditorBuildSettings.scenes;
                scenes.AddRange(profile.scenes);
                EditorBuildSettings.scenes = scenes.ToArray();
            }
        }

        /// <summary>
        /// After build, cleanup scene list changes.
        /// </summary>
        public void OnAfterBuildProfileBuild()
        {
            var profile = BuildPlayerWithProfileOptions.buildProfile;
            if (profile.overrideGlobalScenes)
            {
                profile.scenes = originalScenes;
            }
            else
            {
                EditorBuildSettings.scenes = originalScenes;
            }
        }

        public bool ShouldBuildWithProfile()
        {
            return BuildPlayerWithProfileOptions.buildProfile != null;
        }
#endif
    }
}
