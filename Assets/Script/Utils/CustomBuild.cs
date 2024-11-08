using UnityEditor;

namespace MajdataPlay.Utils
{
    public static class CustomBuild
    {
        public static void BuildDebug()
        {
            var buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = GetScenesToBuild(),
                options = BuildOptions.Development | BuildOptions.AllowDebugging
            };
            BuildPipeline.BuildPlayer(buildPlayerOptions);
        }
        static string[] GetScenesToBuild()
        {
            var sceneCount = EditorBuildSettings.scenes.Length;
            var scenes = new string[sceneCount];

            for (int i = 0; i < sceneCount; i++)
                scenes[i] = EditorBuildSettings.scenes[i].path;

            return scenes;
        }
    }
}
