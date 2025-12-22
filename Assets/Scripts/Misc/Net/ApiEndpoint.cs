using MajdataPlay.Settings;
using Newtonsoft.Json;
using System;
using UnityEngine.Scripting;
#nullable enable
namespace MajdataPlay.Net
{
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
        [Preserve, JsonIgnore]
        public ApiRuntimeConfig RuntimeConfig { get; init; } = new();
    }
}