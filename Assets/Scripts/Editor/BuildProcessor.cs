//https://stackoverflow.com/questions/43657461/how-to-find-list-of-files-in-streamingassets-folder-in-android
using MajdataPlay.Editor.Android;
using System.Collections.Generic;
using System.IO;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace MajdataPlay.Editor;
class BuildProcessor : IPreprocessBuildWithReport
{
    public int callbackOrder { get { return 0; } }
    public void OnPreprocessBuild(BuildReport report)
    {
        Debug.LogWarning("OnPreprocessBuild");

        AndroidProcessor.OnPreprocessBuild(report);
    }
}