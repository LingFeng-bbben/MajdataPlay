using Cysharp.Threading.Tasks;
using MajdataPlay.Diagnostics;
using Newtonsoft.Json;
using Semver;
using System;
using System.Threading;

#nullable enable
namespace MajdataPlay
{
    internal static class VersionChecker
    {
        const string TAGS_API_URL = "https://api.github.com/repos/TeamMajdata/MajdataPlay_Build/tags?per_page=100";
        static readonly TimeSpan UPDATE_CHECK_TIMEOUT = TimeSpan.FromSeconds(5);

        internal static async UniTask<bool> IsNewVersionAvailableAsync()
        {
            try
            {
                using var timeoutCts = new CancellationTokenSource(UPDATE_CHECK_TIMEOUT);
                using var response = await MajEnv.SharedHttpClient.GetAsync(TAGS_API_URL, timeoutCts.Token);
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                var tags = JsonConvert.DeserializeObject<GitHubTagInfo[]>(content);
                if (tags is null || tags.Length == 0)
                {
                    MajDebug.LogWarning("Update check failed: no tags returned from GitHub.");
                    return false;
                }

                var currentVersion = MajInstances.GameVersion;
                SemVersion? latestVersion = null;
                foreach (var tag in tags)
                {
                    var parsedVersion = TryParseVersion(tag.Name);
                    if (parsedVersion is null)
                    {
                        continue;
                    }

                    if (latestVersion is null || CompareMajorMinorPatch(parsedVersion, latestVersion) > 0)
                    {
                        latestVersion = parsedVersion;
                    }
                }

                if (latestVersion is null)
                {
                    MajDebug.LogWarning("Update check failed: no valid semantic version tags returned from GitHub.");
                    return false;
                }

                return CompareMajorMinorPatch(latestVersion, currentVersion) > 0;
            }
            catch (OperationCanceledException)
            {
                MajDebug.LogWarning("Update check failed: request timed out.");
                return false;
            }
            catch (Exception e)
            {
                MajDebug.LogWarning($"Update check failed: {e.Message}");
                return false;
            }
        }

        static int CompareMajorMinorPatch(SemVersion left, SemVersion right)
        {
            var majorComparison = left.Major.CompareTo(right.Major);
            if (majorComparison != 0)
            {
                return majorComparison;
            }

            var minorComparison = left.Minor.CompareTo(right.Minor);
            if (minorComparison != 0)
            {
                return minorComparison;
            }

            return left.Patch.CompareTo(right.Patch);
        }

        static SemVersion? TryParseVersion(string tagName)
        {
            if (string.IsNullOrWhiteSpace(tagName))
            {
                return null;
            }

            var normalized = tagName.Trim();
            if (normalized.StartsWith("v", StringComparison.OrdinalIgnoreCase))
            {
                normalized = normalized.Substring(1);
            }

            try
            {
                return SemVersion.Parse(normalized, SemVersionStyles.Strict);
            }
            catch
            {
                return null;
            }
        }

        sealed class GitHubTagInfo
        {
            [JsonProperty("name")]
            public string Name { get; set; } = string.Empty;
        }
    }
}
