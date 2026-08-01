using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MajdataPlay.Diagnostics;
using Newtonsoft.Json;
using UnityEngine;

#nullable enable
namespace MajdataPlay
{
    internal static class ResourceManifestLoader
    {
        private const string V1ManifestResourcePath = "ResourceUpdate/V1ResourceHashes";
        private const string V2ManifestResourcePath = "ResourceUpdate/V2ResourceHashes";
        private const string DiffManifestResourcePath = "ResourceUpdate/V1ToV2Diff";
        private const string HashAlgorithm = "SHA-256";

        private sealed class ResourceHashManifest
        {
            [JsonProperty("version")]
            public int Version { get; set; }

            [JsonProperty("algorithm")]
            public string Algorithm { get; set; } = string.Empty;

            [JsonProperty("files")]
            public Dictionary<string, string>? Files { get; set; } = new(StringComparer.Ordinal);
        }

        private sealed class ResourceDiffManifest
        {
            [JsonProperty("baseVersion")]
            public int BaseVersion { get; set; }

            [JsonProperty("targetVersion")]
            public int TargetVersion { get; set; }

            [JsonProperty("files")]
            public string[]? Files { get; set; } = Array.Empty<string>();
        }

        public static bool TryGetUpdateManifests(
            out Dictionary<string, string> v1Hashes,
            out Dictionary<string, string> v2Hashes,
            out HashSet<string> diffPaths)
        {
            v1Hashes = new Dictionary<string, string>(StringComparer.Ordinal);
            v2Hashes = new Dictionary<string, string>(StringComparer.Ordinal);
            diffPaths = new HashSet<string>(StringComparer.Ordinal);

            if (!TryLoadHashManifest(V1ManifestResourcePath, expectedVersion: 1, out v1Hashes) ||
                !TryLoadHashManifest(V2ManifestResourcePath, expectedVersion: 2, out v2Hashes) ||
                !TryLoadDiffManifest(v2Hashes, out var loadedDiffPaths))
            {
                return false;
            }

            diffPaths = new HashSet<string>(loadedDiffPaths, StringComparer.Ordinal);
            return true;
        }

        public static bool TryGetV2Hashes(out Dictionary<string, string> hashes)
        {
            return TryLoadHashManifest(V2ManifestResourcePath, expectedVersion: 2, out hashes);
        }

        private static bool TryLoadHashManifest(
            string resourcePath,
            int expectedVersion,
            out Dictionary<string, string> hashes)
        {
            hashes = new Dictionary<string, string>(StringComparer.Ordinal);
            if (!TryDeserializeManifest(resourcePath, out ResourceHashManifest? manifest) || manifest == null ||
                manifest.Version != expectedVersion ||
                !string.Equals(manifest.Algorithm, HashAlgorithm, StringComparison.OrdinalIgnoreCase) ||
                manifest.Files == null || manifest.Files.Count == 0)
            {
                MajDebug.LogError($"Invalid v{expectedVersion} resource manifest: {resourcePath}");
                return false;
            }

            foreach (var (path, rawHash) in manifest.Files)
            {
                var normalizedPath = NormalizeRelativePath(path);
                var hash = rawHash?.Trim().ToLowerInvariant();
                if (normalizedPath == null || normalizedPath != path || hash == null || !IsSha256(hash) ||
                    !hashes.TryAdd(normalizedPath, hash))
                {
                    MajDebug.LogError($"Invalid v{expectedVersion} resource entry: {path}");
                    hashes.Clear();
                    return false;
                }
            }

            return true;
        }

        private static bool TryLoadDiffManifest(
            IReadOnlyDictionary<string, string> v2Hashes,
            out string[] diffPaths)
        {
            if (!TryDeserializeManifest(DiffManifestResourcePath, out ResourceDiffManifest? manifest) || manifest == null ||
                manifest.BaseVersion != 1 || manifest.TargetVersion != 2 || manifest.Files == null)
            {
                MajDebug.LogError($"Invalid resource diff manifest: {DiffManifestResourcePath}");
                diffPaths = Array.Empty<string>();
                return false;
            }

            var paths = new HashSet<string>(StringComparer.Ordinal);
            foreach (var path in manifest.Files)
            {
                if (NormalizeRelativePath(path) != path || !v2Hashes.ContainsKey(path) || !paths.Add(path))
                {
                    MajDebug.LogError($"Invalid v2 resource diff entry: {path}");
                    diffPaths = Array.Empty<string>();
                    return false;
                }
            }

            diffPaths = manifest.Files;
            return true;
        }

        private static bool TryDeserializeManifest<T>(string resourcePath, out T? manifest)
            where T : class
        {
            var textAsset = Resources.Load<TextAsset>(resourcePath);
            if (textAsset == null)
            {
                MajDebug.LogError($"Resource manifest not found in Resources: {resourcePath}");
                manifest = null;
                return false;
            }

            try
            {
                manifest = JsonConvert.DeserializeObject<T>(textAsset.text);
                return manifest != null;
            }
            catch (JsonException exception)
            {
                MajDebug.LogError($"Invalid resource manifest JSON: {resourcePath}\n{exception}");
                manifest = null;
                return false;
            }
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
    }
}
