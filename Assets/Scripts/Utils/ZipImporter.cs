using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MajdataPlay.Utils
{
#if UNITY_IOS //&& !UNITY_EDITOR
    public sealed class ZipImporter : MonoBehaviour
    {
        private static ZipImporter _instance;
        private static readonly Queue<string> Pending = new Queue<string>();
        private static readonly object Gate = new object();
        
        public static event Action<string> OnPackageExtracted;

        [Header("Options")]
        [SerializeField] private bool deleteTempAfterSuccess = true;

        private static string ImportRoot =>
            Path.Combine(Application.persistentDataPath, "MaiCharts", "Import");

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            if (_instance != null) return;

            var go = new GameObject("ZipImporter");
            _instance = go.AddComponent<ZipImporter>();
            DontDestroyOnLoad(go);
        }

        /// <summary>
        /// UnitySendMessage("ZipImporter","OnIncomingPackageReady", "/var/.../tmp/xxx.zip")
        /// </summary>
        public void OnIncomingPackageReady(string tempFilePath)
        {
            if (string.IsNullOrWhiteSpace(tempFilePath))
            {
                Debug.LogError("[ZipImporter] tempFilePath is null/empty");
                return;
            }

            lock (Gate)
            {
                Pending.Enqueue(tempFilePath);
            }
        }

        private void Update()
        {
            while (true)
            {
                string path;
                lock (Gate)
                {
                    if (Pending.Count == 0) break;
                    path = Pending.Dequeue();
                }

                TryImport(path);
            }
        }

        private void TryImport(string tempFilePath)
        {
            MajDebug.LogDebug("[ZipImporter] Got file: " + tempFilePath);
            
            if (!File.Exists(tempFilePath))
            {
                MajDebug.LogError("[ZipImporter] File not found: " + tempFilePath);
                return;
            }
            
            var ext = Path.GetExtension(tempFilePath).ToLowerInvariant();
            if (ext != ".zip" && ext != ".adx")
            {
                MajDebug.LogError("[ZipImporter] Unsupported extension: " + ext);
                return;
            }
            
            Directory.CreateDirectory(ImportRoot);
            
            var folderName = Path.GetFileNameWithoutExtension(tempFilePath);
            var outDir = Path.Combine(ImportRoot, folderName);
            Directory.CreateDirectory(outDir);

            try
            {
                UnzipToDirectorySafe(tempFilePath, outDir);

                MajDebug.LogDebug("[ZipImporter] Extract OK -> " + outDir);

                
                if (deleteTempAfterSuccess)
                {
                    try { File.Delete(tempFilePath); }
                    catch (Exception e) { MajDebug.LogWarning("[ZipImporter] Delete temp failed: " + e.Message); }
                }
                ReloadList(folderName).Forget();
                OnPackageExtracted?.Invoke(outDir);
                
            }
            catch (Exception e)
            {
                MajDebug.LogError("[ZipImporter] Extract FAILED: " + e);
                
                try { Directory.Delete(outDir, true); } catch { /* ignore */ }
            }
        }

        private async static UniTask ReloadList(string folderName)
        {
            await MajInstances.SceneSwitcher.FadeInAsync();
            MajInstances.SceneSwitcher.SwitchScene("Empty", false);
            await UniTask.Delay(100);
            // var bTasks = WaitForBackgroundTaskSuspendAsync();
            // while(!bTasks.IsCompleted)
            // {
            //     await UniTask.Yield();
            // }
            var progress = new Progress<string>();
            progress.ProgressChanged += (o, e) =>
            {
                MajInstances.SceneSwitcher.SetLoadingText($"Importing {folderName}...\n"+e);
            };
            var task = SongStorage.RefreshLocalAsync(progress);
            while(!task.IsCompleted)
            {
                await UniTask.Yield();
            }
            if (!task.IsCompletedSuccessfully)
            {
                MajInstances.SceneSwitcher.SetLoadingText("MAJTEXT_SCAN_CHARTS_FAILED".i18n(), Color.red);
            }
            else
            {
                MajInstances.SceneSwitcher.SetLoadingText(string.Empty);
            }
            await UniTask.Delay(3000);
            MajInstances.SceneSwitcher.SwitchScene("List");
        }

        
        private static void UnzipToDirectorySafe(string zipPath, string destinationDirectory)
        {
            string destRootFull = Path.GetFullPath(destinationDirectory);
            if (!destRootFull.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
                destRootFull += Path.DirectorySeparatorChar;

            using var archive = ZipFile.OpenRead(zipPath);

            foreach (var entry in archive.Entries)
            {
                
                if (string.IsNullOrEmpty(entry.Name))
                    continue;
                
                string combinedPath = Path.Combine(destinationDirectory, entry.FullName);
                string fullPath = Path.GetFullPath(combinedPath);
                
                if (!fullPath.StartsWith(destRootFull, StringComparison.Ordinal))
                    throw new IOException("Zip entry escapes destination: " + entry.FullName);

                var dir = Path.GetDirectoryName(fullPath);
                if (!string.IsNullOrEmpty(dir))
                    Directory.CreateDirectory(dir);
                
                if (File.Exists(fullPath))
                    File.Delete(fullPath);

                entry.ExtractToFile(fullPath);
            }
        }
    }
#endif
}