//https://stackoverflow.com/questions/43657461/how-to-find-list-of-files-in-streamingassets-folder-in-android
using MajdataPlay.Editor.Android;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace MajdataPlay.Editor;
class BuildProcessor : IPreprocessBuildWithReport, IPostprocessBuildWithReport
{
    public int callbackOrder { get { return 0; } }

    static AndroidSdkVersions? _originSdkVersion;
    public void OnPreprocessBuild(BuildReport report)
    {
        Debug.LogWarning("OnPreprocessBuild");

        if (report.summary.platform == BuildTarget.Android && EditorUserBuildSettings.exportAsGoogleAndroidProject)
        {
            _originSdkVersion = PlayerSettings.Android.targetSdkVersion;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel36;
        }

        AndroidProcessor.OnPreprocessBuild(report);
    }

    public void OnPostprocessBuild(BuildReport report)
    {
        Debug.LogWarning("OnPostprocessBuild");

        if (_originSdkVersion is AndroidSdkVersions originSdkVersion)
        {
            PlayerSettings.Android.targetSdkVersion = originSdkVersion;
            _originSdkVersion = null;
            AssetDatabase.SaveAssets();
        }
    }
}