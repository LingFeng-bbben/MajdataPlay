using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace MajdataPlay.Misc.Editor
{
    public class LangVersionPatcher : AssetPostprocessor
    {
        const string OldVersion = "9.0";
        const string NewVersion = "11.0";

        [InitializeOnLoadMethod]
        public static void Setup()
        {
            var targets = typeof(NamedBuildTarget).GetFields(BindingFlags.Public | BindingFlags.Static)
                                                  .Where(field => field.FieldType == typeof(NamedBuildTarget))
                                                  .Select(field => (NamedBuildTarget)field.GetValue(null));

            bool dirty = false;
            foreach (NamedBuildTarget target in targets)
            {
                const string CscFlag = "-langversion:";

                try
                {
                    var arguments = PlayerSettings.GetAdditionalCompilerArguments(target);
                    if (arguments.Any(argument => argument.StartsWith(CscFlag)))
                        continue;
                    PlayerSettings.SetAdditionalCompilerArguments(target, arguments.Append(CscFlag + "preview").ToArray());
                    dirty = true;
                }
                catch
                {
                    // ignore
                }
            }
            if (!dirty)
                return;
            var projectPath = Path.GetDirectoryName(Path.GetFullPath(Application.dataPath));
            foreach (var file in Directory.GetFiles(projectPath, "*.csproj"))
                File.Delete(file);
            foreach (var file in Directory.GetFiles(projectPath, "*.sln"))
                File.Delete(file);
            Debug.Log($"Patched C# version from {OldVersion} to {NewVersion}!");
        }

        public static string OnGeneratedCSProject(string path, string content)
        {
            const string MsBuildFlag = "LangVersion";
            return content.Replace($"<{MsBuildFlag}>{OldVersion}</{MsBuildFlag}>", $"<{MsBuildFlag}>11.0</{MsBuildFlag}>");
        }
    }
}