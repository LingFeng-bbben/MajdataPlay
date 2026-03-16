//https://stackoverflow.com/questions/43657461/how-to-find-list-of-files-in-streamingassets-folder-in-android
#if UNITY_EDITOR

using System.Collections.Generic;
using System.IO;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

class BuildProcessor : IPreprocessBuildWithReport
{
    public int callbackOrder { get { return 0; } }
    public void OnPreprocessBuild(BuildReport report)
    {
        Debug.LogWarning("OnPreprocessBuild");

        SaveStreamingAssetPaths();
    }

    private void SaveStreamingAssetPaths(string directory = "", string file_name = "StreamingAssetPaths")
    {
        List<string> paths = StreamingAssetsExtension.GetPathsRecursively(directory); // Gets list of files from StreamingAssets/directory

        string txtPath = Path.Combine(Application.dataPath, "Resources", file_name + ".txt"); // writes the list of file paths to /Assets/Resources/
        if (File.Exists(txtPath))
        {
            File.Delete(txtPath);
        }
        using (FileStream fs = File.Create(txtPath)) { }
        using (StreamWriter writer = new StreamWriter(txtPath, false))
        {
            foreach (string path in paths)
            {
                writer.WriteLine(path);
            }
        }

    }
}

#endif