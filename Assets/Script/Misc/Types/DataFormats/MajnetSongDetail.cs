using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MajdataPlay.Types
{
    public partial class MajnetSongDetail
    {
        [JsonPropertyName("Id")]
        public Guid Id { get; set; }

        [JsonPropertyName("Title")]
        public string Title { get; set; }

        [JsonPropertyName("Artist")]
        public string Artist { get; set; }

        [JsonPropertyName("Designer")]
        public string Designer { get; set; }

        [JsonPropertyName("Description")]
        public object Description { get; set; }

        [JsonPropertyName("Levels")]
        public List<string> Levels { get; set; }

        [JsonPropertyName("Uploader")]
        public string Uploader { get; set; }

        [JsonPropertyName("Timestamp")]
        public long Timestamp { get; set; }
    }
}