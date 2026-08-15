using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace MajdataPlay.Editor
{
    class BuildProcessor : IPreprocessBuildWithReport, IPostprocessBuildWithReport
    {
        public int callbackOrder { get { return 0; } }

        static AndroidSdkVersions? _originSdkVersion;
        public void OnPreprocessBuild(BuildReport report)
        {
            Debug.Log("OnPreprocessBuild");

            if (report.summary.platform == BuildTarget.Android && EditorUserBuildSettings.exportAsGoogleAndroidProject)
            {
                _originSdkVersion = PlayerSettings.Android.targetSdkVersion;
                PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel36;
            }

            var generateMobileDiff = report.summary.platform == BuildTarget.Android ||
                                     report.summary.platform == BuildTarget.iOS;
            ResourceManifestBuildProcessor.Generate(report.summary.platform, generateMobileDiff);
        }

        public void OnPostprocessBuild(BuildReport report)
        {
            Debug.Log("OnPostprocessBuild");

            if (_originSdkVersion is AndroidSdkVersions originSdkVersion)
            {
                PlayerSettings.Android.targetSdkVersion = originSdkVersion;
                _originSdkVersion = null;
                AssetDatabase.SaveAssets();
            }
        }
    }
}
