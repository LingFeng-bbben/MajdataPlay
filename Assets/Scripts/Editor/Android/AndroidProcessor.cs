using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace MajdataPlay.Editor.Android;
internal static class AndroidProcessor
{
    public static void OnPreprocessBuild(BuildReport report)
    {
        Debug.LogWarning("OnPreprocessBuild");

        SaveStreamingAssetPaths();
    }

    static void SaveStreamingAssetPaths(string directory = "", string file_name = "StreamingAssetPaths")
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
