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
        [Preserve]
        public bool? AutoLogin { get; init; } = false;
        // 确保只自动登录一次，避免用户退出后继续自动登录
        [Preserve, JsonIgnore]
        public bool? IsLoggedOnce { get; set; } = false;
        [Preserve, JsonIgnore]
        public ApiRuntimeConfig RuntimeConfig { get; init; } = new();
    }
}