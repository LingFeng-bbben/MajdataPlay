#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using MajdataPlay.Diagnostics;
using UnityEngine;
using UnityEngine.Networking;

#nullable enable
namespace MajdataPlay
{
    internal static class MobileResourceUpdater
    {
        private const string PendingManagedAssetMovesMarkerName = ".pending-managed-asset-moves";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitializeOrUpdate()
        {
            if (!Directory.Exists(MajEnv.AssetsPath))
            {
                if (ExtractAssets())
                {
                    var preservedExistingManagedAssets = MoveCharts();
                    preservedExistingManagedAssets |= MoveSkins();
                    ClearPendingManagedAssetMovesMarkerIfCompleted();
                    if (preservedExistingManagedAssets &&
                        ResourceManifestLoader.TryGetUpdateManifests(
                            out var existingManagedV1Hashes,
                            out var existingManagedV2Hashes,
                            out var existingManagedDiffPaths))
                    {
                        // AssetsPath can be missing while player-managed charts/skins still exist.
                        // Run the normal v1 hash gate for those existing files.
                        ApplyV2Diff(
                            existingManagedV1Hashes,
                            existingManagedV2Hashes,
                            existingManagedDiffPaths);
                    }
                }
                else
                {
                    MajDebug.LogError(
                        "Initial resource extraction was incomplete; managed assets were not moved.");
                }
                return;
            }

            CompletePendingManagedAssetMoves();
            if (!ResourceManifestLoader.TryGetUpdateManifests(
                    out var v1Hashes,
                    out var v2Hashes,
                    out var diffPaths))
            {
                return;
            }

            ApplyV2Diff(v1Hashes, v2Hashes, diffPaths);
            SyncMissingAssets(v2Hashes, diffPaths);
        }

        private static void ApplyV2Diff(
            IReadOnlyDictionary<string, string> v1Hashes,
            IReadOnlyDictionary<string, string> v2Hashes,
            IReadOnlyCollection<string> diffPaths)
        {
            var updatedCount = 0;
            var preservedCount = 0;
            foreach (var relativePath in diffPaths)
            {
                var v2Hash = v2Hashes[relativePath];
                var destinationPath = ResolveDestinationPath(relativePath);
                var isRootManagedAsset = IsRootManagedAsset(relativePath);
                if (v1Hashes.TryGetValue(relativePath, out var v1Hash))
                {
                    if (!File.Exists(destinationPath))
                    {
                        if (isRootManagedAsset)
                        {
                            MajDebug.LogInfo(
                                $"Resource update preserved player deletion: {relativePath}");
                            preservedCount++;
                            continue;
                        }
                    }
                    else if (!TryComputeFileSha256(destinationPath, out var localHash) ||
                             !string.Equals(localHash, v1Hash, StringComparison.OrdinalIgnoreCase))
                    {
                        MajDebug.LogInfo(
                            $"Resource update preserved player customization: {relativePath}");
                        preservedCount++;
                        continue;
                    }
                }
                else if (File.Exists(destinationPath))
                {
                    // This path did not exist in v1. An existing local file is player-owned.
                    MajDebug.LogInfo(
                        $"Resource update preserved player file at new v2 path: {relativePath}");
                    preservedCount++;
                    continue;
                }
                else if (isRootManagedAsset)
                {
                    // Do not repopulate player-managed chart/skin trees during an upgrade.
                    MajDebug.LogInfo(
                        $"Resource update skipped new managed asset to preserve player layout: {relativePath}");
                    preservedCount++;
                    continue;
                }

                if (!TryReadPackagedResource(relativePath, out var v2Data))
                {
                    continue;
                }

                var packagedHash = ComputeSha256(v2Data);
                if (!string.Equals(packagedHash, v2Hash, StringComparison.OrdinalIgnoreCase))
                {
                    MajDebug.LogError(
                        $"Resource update skipped (packaged v2 hash mismatch): {relativePath}");
                    continue;
                }

                try
                {
                    var destinationDirectory = Path.GetDirectoryName(destinationPath);
                    if (!string.IsNullOrEmpty(destinationDirectory))
                    {
                        Directory.CreateDirectory(destinationDirectory);
                    }

                    WriteAllBytesAtomically(destinationPath, v2Data);
                    updatedCount++;
                    MajDebug.LogInfo($"Resource updated from official v1 to v2: {relativePath}");
                }
                catch (Exception exception)
                {
                    MajDebug.LogError(
                        $"Resource update failed while writing: {relativePath}\n{exception}");
                }
            }

            MajDebug.LogInfo(
                $"Mobile resource update finished: {updatedCount} updated, " +
                $"{preservedCount} customized or missing file(s) preserved.");
        }

        private static bool ExtractAssets()
        {
            if (!ResourceManifestLoader.TryGetV2Hashes(out var v2Hashes))
            {
                return false;
            }

            var extractionRoot = MajEnv.AssetsPath.TrimEnd(
                                     Path.DirectorySeparatorChar,
                                     Path.AltDirectorySeparatorChar) + ".extracting-v2";
            try
            {
                if (Directory.Exists(extractionRoot))
                {
                    Directory.Delete(extractionRoot, recursive: true);
                }
                else if (File.Exists(extractionRoot))
                {
                    File.Delete(extractionRoot);
                }

                Directory.CreateDirectory(extractionRoot);
            }
            catch (Exception exception)
            {
                MajDebug.LogError($"Failed to prepare resource extraction directory:\n{exception}");
                return false;
            }

            var succeeded = true;
            foreach (var (relativePath, hash) in v2Hashes.OrderBy(pair => pair.Key, StringComparer.Ordinal))
            {
                var destinationPath = Path.Combine(
                    extractionRoot,
                    relativePath.Replace('/', Path.DirectorySeparatorChar));
                if (!CopyPackagedResource(relativePath, destinationPath, hash, "Extract"))
                {
                    succeeded = false;
                }
            }

            if (!succeeded)
            {
                return false;
            }

            try
            {
                File.WriteAllText(
                    Path.Combine(extractionRoot, PendingManagedAssetMovesMarkerName),
                    string.Empty);
                Directory.Move(extractionRoot, MajEnv.AssetsPath);
                return true;
            }
            catch (Exception exception)
            {
                MajDebug.LogError($"Failed to finish initial resource extraction:\n{exception}");
                return false;
            }
        }

        private static void SyncMissingAssets(
            IReadOnlyDictionary<string, string> v2Hashes,
            IReadOnlyCollection<string> diffPaths)
        {
            foreach (var (relativePath, hash) in v2Hashes.OrderBy(pair => pair.Key, StringComparer.Ordinal))
            {
                if (IsRootManagedAsset(relativePath) || diffPaths.Contains(relativePath))
                {
                    continue;
                }

                var destinationPath = Path.Combine(
                    MajEnv.AssetsPath,
                    relativePath.Replace('/', Path.DirectorySeparatorChar));
                if (!File.Exists(destinationPath))
                {
                    CopyPackagedResource(relativePath, destinationPath, hash, "Sync missing");
                }
            }
        }

        private static bool MoveCharts()
        {
            return MoveExtractedDirectory(
                Path.Combine(MajEnv.AssetsPath, "MaiCharts", "Original"),
                Path.Combine(MajEnv.ChartPath, "Original"));
        }

        private static bool MoveSkins()
        {
            return MoveExtractedDirectory(
                Path.Combine(MajEnv.AssetsPath, "Skins", "default"),
                Path.Combine(MajEnv.SkinPath, "default"));
        }

        private static bool MoveExtractedDirectory(string sourcePath, string destinationPath)
        {
            if (!Directory.Exists(sourcePath))
            {
                MajDebug.LogError($"Move failed: source not found: {sourcePath}");
                return false;
            }

            try
            {
                if (Directory.Exists(destinationPath) || File.Exists(destinationPath))
                {
                    // Never replace a pre-existing player-managed directory during extraction.
                    Directory.Delete(sourcePath, recursive: true);
                    MajDebug.LogInfo(
                        $"Preserved existing player-managed path during extraction: {destinationPath}");
                    return true;
                }

                Directory.Move(sourcePath, destinationPath);
                MajDebug.LogInfo($"Moved: {sourcePath} -> {destinationPath}");
                return false;
            }
            catch (Exception exception)
            {
                MajDebug.LogError($"Move failed: {sourcePath} -> {destinationPath}\n{exception}");
                return false;
            }
        }

        private static void CompletePendingManagedAssetMoves()
        {
            var markerPath = Path.Combine(
                MajEnv.AssetsPath,
                PendingManagedAssetMovesMarkerName);
            if (!File.Exists(markerPath))
            {
                return;
            }

            var chartSourcePath = Path.Combine(MajEnv.AssetsPath, "MaiCharts", "Original");
            if (Directory.Exists(chartSourcePath))
            {
                MoveCharts();
            }

            var skinSourcePath = Path.Combine(MajEnv.AssetsPath, "Skins", "default");
            if (Directory.Exists(skinSourcePath))
            {
                MoveSkins();
            }

            ClearPendingManagedAssetMovesMarkerIfCompleted();
        }

        private static void ClearPendingManagedAssetMovesMarkerIfCompleted()
        {
            var chartSourcePath = Path.Combine(MajEnv.AssetsPath, "MaiCharts", "Original");
            var skinSourcePath = Path.Combine(MajEnv.AssetsPath, "Skins", "default");
            if (Directory.Exists(chartSourcePath) || Directory.Exists(skinSourcePath))
            {
                return;
            }

            var markerPath = Path.Combine(
                MajEnv.AssetsPath,
                PendingManagedAssetMovesMarkerName);
            try
            {
                if (File.Exists(markerPath))
                {
                    File.Delete(markerPath);
                }
            }
            catch (Exception exception)
            {
                MajDebug.LogError(
                    $"Failed to clean pending managed asset move marker: {markerPath}\n{exception}");
            }
        }

        private static bool CopyPackagedResource(
            string relativePath,
            string destinationPath,
            string expectedHash,
            string operation)
        {
            if (!TryReadPackagedResource(relativePath, out var data))
            {
                return false;
            }

            if (!string.Equals(ComputeSha256(data), expectedHash, StringComparison.OrdinalIgnoreCase))
            {
                MajDebug.LogError($"{operation} failed (packaged hash mismatch): {relativePath}");
                return false;
            }

            try
            {
                var destinationDirectory = Path.GetDirectoryName(destinationPath);
                if (!string.IsNullOrEmpty(destinationDirectory))
                {
                    Directory.CreateDirectory(destinationDirectory);
                }

                WriteAllBytesAtomically(destinationPath, data);
                MajDebug.LogInfo($"{operation}: {relativePath} -> {destinationPath}");
                return true;
            }
            catch (Exception exception)
            {
                MajDebug.LogError($"{operation} failed: {relativePath}\n{exception}");
                return false;
            }
        }

        private static void WriteAllBytesAtomically(string destinationPath, byte[] data)
        {
            var temporaryPath = $"{destinationPath}.{Guid.NewGuid():N}.tmp";
            try
            {
                File.WriteAllBytes(temporaryPath, data);
                if (File.Exists(destinationPath))
                {
                    File.Replace(temporaryPath, destinationPath, null);
                }
                else
                {
                    File.Move(temporaryPath, destinationPath);
                }
            }
            finally
            {
                if (File.Exists(temporaryPath))
                {
                    try
                    {
                        File.Delete(temporaryPath);
                    }
                    catch (Exception exception)
                    {
                        MajDebug.LogError(
                            $"Failed to clean temporary resource file: {temporaryPath}\n{exception}");
                    }
                }
            }
        }

        private static bool IsRootManagedAsset(string path)
        {
            return path.StartsWith("MaiCharts/", StringComparison.OrdinalIgnoreCase) ||
                   path.StartsWith("Skins/", StringComparison.OrdinalIgnoreCase);
        }

        private static string ResolveDestinationPath(string relativePath)
        {
            var platformPath = relativePath.Replace('/', Path.DirectorySeparatorChar);
            return IsRootManagedAsset(relativePath)
                ? Path.Combine(MajEnv.RootPath, platformPath)
                : Path.Combine(MajEnv.AssetsPath, platformPath);
        }

        private static bool TryReadPackagedResource(string relativePath, out byte[] data)
        {
#if UNITY_IOS
            var sourcePath = Path.Combine(
                Application.streamingAssetsPath,
                relativePath.Replace('/', Path.DirectorySeparatorChar));
            try
            {
                data = File.ReadAllBytes(sourcePath);
                return true;
            }
            catch (Exception exception)
            {
                MajDebug.LogError(
                    $"Resource update failed while reading packaged iOS resource: " +
                    $"{relativePath}\n{exception}");
                data = Array.Empty<byte>();
                return false;
            }
#elif UNITY_ANDROID
            var sourceUrl = Path.Combine(Application.streamingAssetsPath, relativePath).Replace("\\", "/");
            try
            {
                using var request = UnityWebRequest.Get(sourceUrl);
                request.downloadHandler = new DownloadHandlerBuffer();
                var operation = request.SendWebRequest();
                while (!operation.isDone)
                {
                    System.Threading.Thread.Sleep(1);
                }

                if (request.result != UnityWebRequest.Result.Success)
                {
                    MajDebug.LogError(
                        $"Resource update failed while reading packaged Android resource: " +
                        $"{relativePath}\n{request.error}");
                    data = Array.Empty<byte>();
                    return false;
                }

                data = request.downloadHandler.data ?? Array.Empty<byte>();
                return true;
            }
            catch (Exception exception)
            {
                MajDebug.LogError(
                    $"Resource update failed while reading packaged Android resource: " +
                    $"{relativePath}\n{exception}");
                data = Array.Empty<byte>();
                return false;
            }
#endif
        }

        private static bool TryComputeFileSha256(string path, out string hash)
        {
            try
            {
                using var stream = File.OpenRead(path);
                using var sha256 = SHA256.Create();
                hash = ToHexString(sha256.ComputeHash(stream));
                return true;
            }
            catch (Exception exception)
            {
                MajDebug.LogError($"Failed to hash local resource: {path}\n{exception}");
                hash = string.Empty;
                return false;
            }
        }

        private static string ComputeSha256(byte[] data)
        {
            using var sha256 = SHA256.Create();
            return ToHexString(sha256.ComputeHash(data));
        }

        private static string ToHexString(byte[] hash)
        {
            return BitConverter.ToString(hash).Replace("-", string.Empty).ToLowerInvariant();
        }
    }
}
#endif
