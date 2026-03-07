using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using UnityEngine.Scripting;

namespace MajdataPlay
{
    [Preserve]
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public readonly struct ChartCommentSummary
    {
        [Preserve]
        public string Sender { get; init; }
        [Preserve]
        public string Content { get; init; }
        [Preserve]
        public DateTime Timestamp { get; init; }
        [Preserve]
        public ChartCommentSummary[] Replies { get; init; }
    }
}
