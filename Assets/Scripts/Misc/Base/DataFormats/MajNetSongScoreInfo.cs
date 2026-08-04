using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using UnityEngine.Scripting;

namespace MajdataPlay
{
    [Preserve]
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public struct MajNetSongScoreInfo
    {
        public string Hash { get; init; }
        public MajNetSongScore[][] Scores { get; init; }
    }
}
