using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Scripting;

namespace MajdataPlay
{
    [Preserve]
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public readonly struct MajNetOnlineDanInfo
    {
        public required string Id { get; init; }
        public required string Name { get; init; }
        public required string CreatedBy { get; init; }
        public required string Description { get; init; }
        public required HashSet<string> SongHashs { get; init; }
        public bool IsPlayList { get; init; }
        public bool IsForceGameover { get; init; }
    }
}
