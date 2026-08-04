using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using UnityEngine.Scripting;

namespace MajdataPlay
{
    [Preserve]
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public readonly struct UserSummary
    {
        [Preserve]
        public string Username { get; init; }
        [Preserve]
        public string Email { get; init; }
        [Preserve]
        public DateTime JoinDate { get; init; }
    }
}
