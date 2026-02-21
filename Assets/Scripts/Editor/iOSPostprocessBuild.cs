#if UNITY_IOS
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using UnityEngine;

namespace MajdataPlay.Editor
{
    public static class iOSPostprocessBuild
    {
        private static readonly string[] RunpathAdditions =
        {
            "@executable_path/Frameworks/bass.framework",
            "@executable_path/Frameworks/bass_fx.framework",
            "@executable_path/Frameworks/bassopus.framework",
        };

        private const string SettingsBundleSourcePath = "Assets/Plugins/iOS/Settings.bundle";
        private const string SettingsBundleName = "Settings.bundle";

        [PostProcessBuild(45)]
        public static void OnPostprocessBuild(BuildTarget buildTarget, string path)
        {
            if (buildTarget != BuildTarget.iOS)
            {
                return;
            }

            UpdateRunpathSearchPaths(path);
            UpdateInfoPlist(path);
            AddSettingsBundle(path);
        }

        private static void UpdateRunpathSearchPaths(string path)
        {
            string projPath = PBXProject.GetPBXProjectPath(path);
            if (!File.Exists(projPath))
            {
                Debug.LogWarning($"Xcode project not found at {projPath}");
                return;
            }

            var proj = new PBXProject();
            proj.ReadFromString(File.ReadAllText(projPath));

            string mainTarget = proj.GetUnityMainTargetGuid();
            string frameworkTarget = proj.GetUnityFrameworkTargetGuid();

            AddRunpathSearchPaths(proj, mainTarget);
            if (!string.IsNullOrEmpty(frameworkTarget))
            {
                AddRunpathSearchPaths(proj, frameworkTarget);
            }

            File.WriteAllText(projPath, proj.WriteToString());
        }

        private static void AddRunpathSearchPaths(PBXProject proj, string target)
        {
            if (string.IsNullOrEmpty(target))
            {
                return;
            }

            string current = proj.GetBuildPropertyForAnyConfig(target, "LD_RUNPATH_SEARCH_PATHS") ?? string.Empty;
            var existing = new HashSet<string>(current.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));

            foreach (string runpath in RunpathAdditions)
            {
                if (!existing.Contains(runpath))
                {
                    proj.AddBuildProperty(target, "LD_RUNPATH_SEARCH_PATHS", runpath);
                }
            }
        }

        private static void UpdateInfoPlist(string path)
        {
            string projPath = PBXProject.GetPBXProjectPath(path);
            if (!File.Exists(projPath))
            {
                Debug.LogWarning($"Xcode project not found at {projPath}");
                return;
            }

            var proj = new PBXProject();
            proj.ReadFromString(File.ReadAllText(projPath));

            string mainTarget = proj.GetUnityMainTargetGuid();

            string plistPath = ResolveInfoPlistPath(path, proj, mainTarget);
            if (!File.Exists(plistPath))
            {
                Debug.LogWarning($"Info.plist not found at {plistPath}");
                return;
            }

            var plist = new PlistDocument();
            plist.ReadFromString(File.ReadAllText(plistPath));
            var root = plist.root;
            
            root.SetBoolean("UIFileSharingEnabled", true);
            root.SetBoolean("LSSupportsOpeningDocumentsInPlace", true);
            root.SetBoolean("UISupportsDocumentBrowser", true);

            
            string bundleId = PlayerSettings.applicationIdentifier;
            string adxUti = $"{bundleId}.adx";

            EnsureDocumentType(root,
                typeName: "Zip Archive",
                role: "Viewer",
                contentTypes: new[] { "public.zip-archive" }
            );

            EnsureDocumentType(root,
                typeName: "ADX Package",
                role: "Viewer",
                contentTypes: new[] { adxUti }
            );

            
            EnsureExportedTypeDeclaration(root,
                identifier: adxUti,
                description: "ADX Level Package",
                conformsTo: new[] { "public.zip-archive" },
                filenameExtensions: new[] { "adx" }
            );

            File.WriteAllText(plistPath, plist.WriteToString());
        }

        private static void EnsureDocumentType(PlistElementDict root, string typeName, string role, string[] contentTypes)
        {
            var arr = root.values.ContainsKey("CFBundleDocumentTypes")
                ? root["CFBundleDocumentTypes"].AsArray()
                : root.CreateArray("CFBundleDocumentTypes");

            
            foreach (var el in arr.values)
            {
                var d = el.AsDict();
                if (d == null) continue;

                if (!d.values.TryGetValue("LSItemContentTypes", out var ctEl)) continue;
                var ctArr = ctEl.AsArray();
                if (ctArr == null) continue;

                var existing = new HashSet<string>(StringComparer.Ordinal);
                foreach (var v in ctArr.values)
                {
                    var s = v.AsString();
                    if (s != null) existing.Add(s);
                }

                bool match = false;
                foreach (var ct in contentTypes)
                {
                    if (existing.Contains(ct)) { match = true; break; }
                }

                if (match) return; 
            }

            
            var entry = arr.AddDict();
            entry.SetString("CFBundleTypeName", typeName);
            entry.SetString("CFBundleTypeRole", role);

            var ctNew = entry.CreateArray("LSItemContentTypes");
            foreach (var ct in contentTypes) ctNew.AddString(ct);
        }

        private static void EnsureExportedTypeDeclaration(
            PlistElementDict root,
            string identifier,
            string description,
            string[] conformsTo,
            string[] filenameExtensions)
        {
            var arr = root.values.ContainsKey("UTExportedTypeDeclarations")
                ? root["UTExportedTypeDeclarations"].AsArray()
                : root.CreateArray("UTExportedTypeDeclarations");
            
            foreach (var el in arr.values)
            {
                var d = el.AsDict();
                if (d == null) continue;
                if (d.values.TryGetValue("UTTypeIdentifier", out var idEl))
                {
                    var s = idEl.AsString();
                    if (string.Equals(s, identifier, StringComparison.Ordinal))
                        return;
                }
            }

            var entry = arr.AddDict();
            entry.SetString("UTTypeIdentifier", identifier);
            entry.SetString("UTTypeDescription", description);

            var conformsArr = entry.CreateArray("UTTypeConformsTo");
            foreach (var c in conformsTo) conformsArr.AddString(c);

            var tagSpec = entry.CreateDict("UTTypeTagSpecification");
            var extArr = tagSpec.CreateArray("public.filename-extension");
            foreach (var ext in filenameExtensions) extArr.AddString(ext);
        }

        private static string ResolveInfoPlistPath(string buildPath, PBXProject proj, string target)
        {
            string plistPath = proj.GetBuildPropertyForAnyConfig(target, "INFOPLIST_FILE");
            if (string.IsNullOrEmpty(plistPath))
            {
                return Path.Combine(buildPath, "Info.plist");
            }

            plistPath = plistPath.Trim().Trim('"');
            if (plistPath.Contains("$(SRCROOT)"))
            {
                plistPath = plistPath.Replace("$(SRCROOT)", buildPath);
            }

            if (Path.IsPathRooted(plistPath))
            {
                return plistPath;
            }

            return Path.Combine(buildPath, plistPath);
        }

        private static void AddSettingsBundle(string buildPath)
        {
            if (!Directory.Exists(SettingsBundleSourcePath))
            {
                return;
            }

            string destPath = Path.Combine(buildPath, SettingsBundleName);

            if (Directory.Exists(destPath))
            {
                FileUtil.DeleteFileOrDirectory(destPath);
            }

            FileUtil.CopyFileOrDirectory(SettingsBundleSourcePath, destPath);

            string projPath = PBXProject.GetPBXProjectPath(buildPath);
            if (!File.Exists(projPath))
            {
                return;
            }

            var proj = new PBXProject();
            proj.ReadFromString(File.ReadAllText(projPath));

            string mainTarget = proj.GetUnityMainTargetGuid();
            string frameworkTarget = proj.GetUnityFrameworkTargetGuid();

            string fileGuid = proj.AddFile(SettingsBundleName, SettingsBundleName, PBXSourceTree.Source);

            if (!string.IsNullOrEmpty(mainTarget))
            {
                proj.AddFileToBuild(mainTarget, fileGuid);
            }

            if (!string.IsNullOrEmpty(frameworkTarget))
            {
                proj.AddFileToBuild(frameworkTarget, fileGuid);
            }

            File.WriteAllText(projPath, proj.WriteToString());
        }
    }
}
#endif