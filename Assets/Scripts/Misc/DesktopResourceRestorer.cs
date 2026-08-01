#if UNITY_STANDALONE_WIN
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MajdataPlay.Diagnostics;
using UnityEngine;

namespace MajdataPlay
{
    internal static class DesktopResourceRestorer
    {
        public static void RestoreManagedAssetsIfMissing()
        {
            var chartRootMissing = !Directory.EnumerateFileSystemEntries(MajEnv.ChartPath).Any();
            var skinRootMissing = !Directory.EnumerateFileSystemEntries(MajEnv.SkinPath).Any();
            if (!chartRootMissing && !skinRootMissing)
            {
                return;
            }

            if (!ResourceManifestLoader.TryGetV2Hashes(out var v2Hashes))
            {
                return;
            }

            if (chartRootMissing)
            {
                RestoreGroup("MaiCharts/", MajEnv.ChartPath, v2Hashes);
            }

            if (skinRootMissing)
            {
                RestoreGroup("Skins/", MajEnv.SkinPath, v2Hashes);
            }
        }

        private static void RestoreGroup(
            string sourcePrefix,
            string destinationRoot,
            IReadOnlyDictionary<string, string> v2Hashes)
        {
            Directory.CreateDirectory(destinationRoot);
            foreach (var relativePath in v2Hashes.Keys
                         .Where(path => path.StartsWith(sourcePrefix, StringComparison.OrdinalIgnoreCase))
                         .OrderBy(path => path, StringComparer.Ordinal))
            {
                var sourcePath = Path.Combine(
                    Application.streamingAssetsPath,
                    relativePath.Replace('/', Path.DirectorySeparatorChar));
                var destinationPath = Path.Combine(
                    destinationRoot,
                    relativePath.Substring(sourcePrefix.Length)
                                .Replace('/', Path.DirectorySeparatorChar));
                var destinationDirectory = Path.GetDirectoryName(destinationPath);
                if (!string.IsNullOrEmpty(destinationDirectory))
                {
                    Directory.CreateDirectory(destinationDirectory);
                }

                try
                {
                    File.Copy(sourcePath, destinationPath, overwrite: true);
                    MajDebug.LogInfo($"Restore managed asset(Windows): {sourcePath} -> {destinationPath}");
                }
                catch (Exception exception)
                {
                    MajDebug.LogError(
                        $"Restore managed asset failed(Windows): {relativePath}\n{exception}");
                }
            }
        }
    }
}
#endif
