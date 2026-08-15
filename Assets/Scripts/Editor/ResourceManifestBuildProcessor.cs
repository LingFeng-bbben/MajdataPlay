using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

#nullable enable
namespace MajdataPlay.Editor
{
    internal static class ResourceManifestBuildProcessor
    {
        private const string V1ManifestAssetPath = "Assets/Resources/ResourceUpdate/V1ResourceHashes.json";
        private const string V2ManifestAssetPath = "Assets/Resources/ResourceUpdate/V2ResourceHashes.json";
        private const string DiffManifestAssetPath = "Assets/Resources/ResourceUpdate/V1ToV2Diff.json";
        private const string HashAlgorithm = "SHA-256";

        private sealed class V1ResourceHashManifest
        {
            [JsonProperty("version")]
            public int Version { get; set; }

            [JsonProperty("algorithm")]
            public string Algorithm { get; set; } = string.Empty;

            [JsonProperty("files")]
            public Dictionary<string, string>? Files { get; set; } = new(StringComparer.Ordinal);
        }

        private sealed class V2ResourceHashManifest
        {
            [JsonProperty("version")]
            public int Version { get; set; } = 2;

            [JsonProperty("algorithm")]
            public string Algorithm { get; set; } = HashAlgorithm;

            [JsonProperty("files")]
            public SortedDictionary<string, string> Files { get; set; } = new(StringComparer.Ordinal);
        }

        private sealed class ResourceDiffManifest
        {
            [JsonProperty("baseVersion")]
            public int BaseVersion { get; set; } = 1;

            [JsonProperty("targetVersion")]
            public int TargetVersion { get; set; } = 2;

            [JsonProperty("files")]
            public string[] Files { get; set; } = Array.Empty<string>();
        }

        public static void Generate(BuildTarget platform, bool generateDiff)
        {
            Debug.Log($"Generating resource manifest for {platform}.");

            var streamingAssetPaths = GetStreamingAssetPaths();
            var v2Hashes = new SortedDictionary<string, string>(StringComparer.Ordinal);

            foreach (var relativePath in streamingAssetPaths)
            {
                var absolutePath = Path.Combine(
                    Application.streamingAssetsPath,
                    relativePath.Replace('/', Path.DirectorySeparatorChar));
                var v2Hash = ComputeSha256(absolutePath);
                v2Hashes.Add(relativePath, v2Hash);
            }

            var v2Manifest = new V2ResourceHashManifest
            {
                Files = v2Hashes,
            };
            WriteTextAsset(
                V2ManifestAssetPath,
                JsonConvert.SerializeObject(v2Manifest, Formatting.Indented) + "\n");

            if (!generateDiff)
            {
                Debug.Log($"Generated v2 resource manifest: {streamingAssetPaths.Length} file(s).");
                return;
            }

            var v1Hashes = LoadV1HashManifest();
            var diffPaths = v2Hashes
                            .Where(pair =>
                                !v1Hashes.TryGetValue(pair.Key, out var v1Hash) ||
                                !string.Equals(v1Hash, pair.Value, StringComparison.OrdinalIgnoreCase))
                            .Select(pair => pair.Key)
                            .ToArray();
            var diffManifest = new ResourceDiffManifest
            {
                Files = diffPaths,
            };
            WriteTextAsset(
                DiffManifestAssetPath,
                JsonConvert.SerializeObject(diffManifest, Formatting.Indented) + "\n");

            Debug.Log(
                $"Generated mobile resource diff: {streamingAssetPaths.Length} total, " +
                $"{diffPaths.Length} changed or added file(s).");
        }

        private static string[] GetStreamingAssetPaths()
        {
            var root = Application.streamingAssetsPath.TrimEnd(
                Path.DirectorySeparatorChar,
                Path.AltDirectorySeparatorChar);

            return Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories)
                            .Select(path => path.Substring(root.Length + 1).Replace('\\', '/'))
                            .Where(IsPackagedResource)
                            .OrderBy(path => path, StringComparer.Ordinal)
                            .ToArray();
        }

        private static bool IsPackagedResource(string relativePath)
        {
            if (relativePath.EndsWith(".meta", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return !relativePath.Split('/').Any(segment => segment.StartsWith(".", StringComparison.Ordinal));
        }

        private static Dictionary<string, string> LoadV1HashManifest()
        {
            var absolutePath = ToAbsoluteProjectPath(V1ManifestAssetPath);
            if (!File.Exists(absolutePath))
            {
                throw new BuildFailedException(
                    $"The v1 resource hash manifest is missing: {V1ManifestAssetPath}. " +
                    "Generate it once from the external official v1_assets directory before building.");
            }

            V1ResourceHashManifest? manifest;
            try
            {
                manifest = JsonConvert.DeserializeObject<V1ResourceHashManifest>(
                    File.ReadAllText(absolutePath));
            }
            catch (JsonException exception)
            {
                throw new BuildFailedException(
                    $"Invalid JSON in v1 resource hash manifest: {V1ManifestAssetPath}\n{exception}");
            }

            if (manifest == null || manifest.Version != 1 ||
                !string.Equals(manifest.Algorithm, HashAlgorithm, StringComparison.OrdinalIgnoreCase) ||
                manifest.Files == null || manifest.Files.Count == 0)
            {
                throw new BuildFailedException(
                    $"Invalid v1 resource hash manifest metadata: {V1ManifestAssetPath}");
            }

            var hashes = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var (path, rawHash) in manifest.Files)
            {
                var normalizedPath = NormalizeRelativePath(path);
                var hash = rawHash?.Trim().ToLowerInvariant();
                if (normalizedPath == null || normalizedPath != path || hash == null || !IsSha256(hash))
                {
                    throw new BuildFailedException(
                        $"Invalid v1 resource entry in {V1ManifestAssetPath}: {path}");
                }

                if (!hashes.TryAdd(normalizedPath, hash))
                {
                    throw new BuildFailedException(
                        $"Duplicate v1 resource path in {V1ManifestAssetPath}: {normalizedPath}");
                }
            }

            return hashes;
        }

        private static string? NormalizeRelativePath(string? path)
        {
            if (path == null)
            {
                return null;
            }

            var normalized = path.Trim().Replace('\\', '/');
            if (normalized.Length == 0 || Path.IsPathRooted(normalized))
            {
                return null;
            }

            var segments = normalized.Split('/');
            if (segments.Any(segment => segment.Length == 0 || segment == "." || segment == ".."))
            {
                return null;
            }

            return normalized;
        }

        private static bool IsSha256(string? value)
        {
            return value != null && value.Length == 64 && value.All(character =>
                character >= '0' && character <= '9' ||
                character >= 'a' && character <= 'f');
        }

        private static string ComputeSha256(string path)
        {
            using var stream = File.OpenRead(path);
            using var sha256 = SHA256.Create();
            return BitConverter.ToString(sha256.ComputeHash(stream))
                               .Replace("-", string.Empty)
                               .ToLowerInvariant();
        }

        private static void WriteTextAsset(string assetPath, string content)
        {
            var absolutePath = ToAbsoluteProjectPath(assetPath);
            Directory.CreateDirectory(
                Path.GetDirectoryName(absolutePath) ??
                throw new InvalidOperationException($"Output directory could not be resolved: {assetPath}"));

            if (!File.Exists(absolutePath) ||
                !string.Equals(File.ReadAllText(absolutePath), content, StringComparison.Ordinal))
            {
                File.WriteAllText(absolutePath, content, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            }

            AssetDatabase.ImportAsset(
                assetPath,
                ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
        }

        private static string ToAbsoluteProjectPath(string assetPath)
        {
            var projectRoot = Path.GetDirectoryName(Application.dataPath) ??
                              throw new InvalidOperationException("Unity project root could not be resolved.");
            return Path.Combine(projectRoot, assetPath.Replace('/', Path.DirectorySeparatorChar));
        }
    }
}
