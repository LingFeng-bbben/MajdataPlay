using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using UnityEngine.Scripting;
#nullable enable
namespace MajdataPlay.Net
{
    public enum EndpointRole
    {
        Shared = 0,
        Player = 1,
    }

    [Preserve]
    public class ApiEndpoint
    {
        [Preserve]
        public string Name { get; init; } = string.Empty;
        [Preserve]
        public required Uri Url { get; init; }
        [Preserve]
        public string? Username { get; init; }
        [Preserve]
        public string? Password { get; init; }
        [Preserve]
        public bool AutoLogin { get; set; }
        [Preserve]
        [JsonConverter(typeof(StringEnumConverter))]
        public EndpointRole Role { get; init; } = EndpointRole.Shared;
        [Preserve, JsonIgnore]
        public ApiRuntimeConfig RuntimeConfig { get; init; } = new();
    }
}