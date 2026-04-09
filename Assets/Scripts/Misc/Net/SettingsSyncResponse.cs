using MajdataPlay.Settings;
using Newtonsoft.Json;
#nullable enable
namespace MajdataPlay.Net
{
    internal class SettingsSyncResponse
    {
        [JsonProperty("version")]
        public long Version { get; set; }

        [JsonProperty("updatedAt")]
        public string? UpdatedAt { get; set; }

        [JsonProperty("game")]
        public GameOptions? Game { get; set; }

        [JsonProperty("judge")]
        public JudgeOptions? Judge { get; set; }

        [JsonProperty("display")]
        public DisplayOptions? Display { get; set; }

        [JsonProperty("audio")]
        public SoundOptions? Audio { get; set; }

        [JsonProperty("debug")]
        public DebugOptions? Debug { get; set; }
    }

    internal class SettingsSyncRequest
    {
        [JsonProperty("version")]
        public long Version { get; set; }

        [JsonProperty("game")]
        public GameOptions? Game { get; set; }

        [JsonProperty("judge")]
        public JudgeOptions? Judge { get; set; }

        [JsonProperty("display")]
        public DisplayOptions? Display { get; set; }

        [JsonProperty("audio")]
        public SoundOptions? Audio { get; set; }

        [JsonProperty("debug")]
        public DebugOptions? Debug { get; set; }
    }

    internal class SettingsPutResponse
    {
        [JsonProperty("version")]
        public long Version { get; set; }

        [JsonProperty("updatedAt")]
        public string? UpdatedAt { get; set; }
    }
}
