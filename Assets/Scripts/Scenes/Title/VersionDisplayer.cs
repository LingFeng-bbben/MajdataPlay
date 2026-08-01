using Cysharp.Text;
using Cysharp.Threading.Tasks;
using MajdataPlay.Diagnostics;
using MajdataPlay.i18n;
using MajdataPlay.Utils;
using Newtonsoft.Json;
using Semver;
using System;
using System.Net.Http;
using System.Threading;
using UnityEngine;

namespace MajdataPlay.Scenes.Title
{
    public class VersionDisplayer : MonoBehaviour
    {
        const string TAGS_API_URL = "https://api.github.com/repos/TeamMajdata/MajdataPlay_Build/tags?per_page=100";
        static readonly TimeSpan UPDATE_CHECK_TIMEOUT = TimeSpan.FromSeconds(5);

        TMPro.TextMeshProUGUI _text;
        bool _hasNewVersionAvailable;

        void Awake()
        {
            _text = GetComponent<TMPro.TextMeshProUGUI>();
            Localization.OnLanguageChanged += OnLanguageChanged;
            RefreshText();
        }

        void Start()
        {
//#if !UNITY_EDITOR
            CheckForUpdateAsync().Forget();
//#endif
        }

        void OnDestroy()
        {
            Localization.OnLanguageChanged -= OnLanguageChanged;
        }

        void OnLanguageChanged(object sender, Language language)
        {
            RefreshText();
        }

        void RefreshText()
        {
            var versionText = MajInstances.GameVersion.ToString();
            if (_hasNewVersionAvailable)
            {
                versionText += $" ({"MAJTEXT_NEW_VERSION_AVAILABLE".i18n()})";
            }

            _text.text = ZString.Format("MAJTEXT_VERSION_FORMAT".i18n(), versionText);
        }

        async UniTaskVoid CheckForUpdateAsync()
        {
            try
            {
                using var timeoutCts = new CancellationTokenSource(UPDATE_CHECK_TIMEOUT);
                using var response = await MajEnv.SharedHttpClient.GetAsync(TAGS_API_URL, timeoutCts.Token);
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                var tags = JsonConvert.DeserializeObject<GitHubTagInfo[]>(content);
                if (tags == null || tags.Length == 0)
                {
                    MajDebug.LogWarning("Update check failed: no tags returned from GitHub.");
                    return;
                }

                var currentVersion = MajInstances.GameVersion;
                SemVersion latestVersion = null;
                foreach (var tag in tags)
                {
                    var parsedVersion = TryParseVersion(tag.Name);
                    if (parsedVersion == null)
                    {
                        continue;
                    }

                    if (latestVersion == null || CompareMajorMinorPatch(parsedVersion, latestVersion) > 0)
                    {
                        latestVersion = parsedVersion;
                    }
                }

                if (latestVersion == null)
                {
                    MajDebug.LogWarning("Update check failed: no valid semantic version tags returned from GitHub.");
                    return;
                }

                if (CompareMajorMinorPatch(latestVersion, currentVersion) > 0)
                {
                    await UniTask.SwitchToMainThread();
                    _hasNewVersionAvailable = true;
                    RefreshText();
                }
            }
            catch (OperationCanceledException)
            {
                MajDebug.LogWarning("Update check failed: request timed out.");
            }
            catch (Exception e)
            {
                MajDebug.LogWarning($"Update check failed: {e.Message}");
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

        static SemVersion TryParseVersion(string tagName)
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
