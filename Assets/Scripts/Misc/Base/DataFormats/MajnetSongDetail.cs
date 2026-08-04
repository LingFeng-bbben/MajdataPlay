using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

namespace MajdataPlay
{
    [Preserve]
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class MajnetSongDetail
    {
        [Preserve]
        public string Id { get; set; }
        [Preserve]
        public string Title { get; set; }
        [Preserve]
        public string Artist { get; set; }
        [Preserve]
        public string Designer { get; set; }
        [Preserve]
        public string Description { get; set; }
        [Preserve]
        public string[] Levels { get; set; }
        [Preserve]
        public string Uploader { get; set; }
        [Preserve]
        public DateTime Timestamp { get; set; }
        [Preserve]
        public string Hash { get; set; }
        [Preserve]
        public string[] Tags { get; set; } = Array.Empty<string>();
        [Preserve]
        public string[] PublicTags { get; set; } = Array.Empty<string>();
    }
}
