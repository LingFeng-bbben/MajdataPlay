using MajdataPlay.Scenes.Game;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Scripting;

namespace MajdataPlay;
[Preserve]
[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public readonly struct MajNetAccountSongScore
{
    public Accurate Acc { get; init; }
    public uint DXScore { get; init; }
    public ChartLevel ChartLevel { get; init; }
    public ComboState ComboState { get; init; }
    public string Hash { get; init; }
    public DateTime Timestamp { get; init; }
}
