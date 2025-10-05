using MajdataPlay.Collections;
using MajdataPlay.Scenes.Game.Notes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MajdataPlay.Json
{
    public class JudgeInfoConverter : System.Text.Json.Serialization.JsonConverter<JudgeInfo>
    {
        public override JudgeInfo Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var data = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, int>>(ref reader, options);
            var dict = new Dictionary<JudgeGrade, int>();
            var type = typeof(JudgeGrade);
            foreach (JudgeGrade judgeResult in Enum.GetValues(type))
            {
                if (!data.TryGetValue(Enum.GetName(type, judgeResult), out var i))
                {
                    dict[judgeResult] = 0;
                }
                else
                {
                    dict[judgeResult] = i;
                }
            }
            foreach (var (k, v) in data)
            {
                switch (k)
                {
                    case "FastGreat1":
                        dict[JudgeGrade.FastGreat2nd] = v;
                        break;
                    case "FastGreat2":
                        dict[JudgeGrade.FastGreat3rd] = v;
                        break;
                    case "LateGreat1":
                        dict[JudgeGrade.LateGreat2nd] = v;
                        break;
                    case "LateGreat2":
                        dict[JudgeGrade.LateGreat3rd] = v;
                        break;
                    case "FastPerfect1":
                        dict[JudgeGrade.FastPerfect2nd] = v;
                        break;
                    case "FastPerfect2":
                        dict[JudgeGrade.FastPerfect3rd] = v;
                        break;
                    case "LatePerfect1":
                        dict[JudgeGrade.LatePerfect2nd] = v;
                        break;
                    case "LatePerfect2":
                        dict[JudgeGrade.LatePerfect3rd] = v;
                        break;
                }
            }
            return new JudgeInfo(dict);
        }

        public override void Write(Utf8JsonWriter writer, JudgeInfo value, JsonSerializerOptions options)
        {
            return System.Text.Json.JsonSerializer.Serialize(writer, value, options);
        }
    }
}
namespace MajdataPlay.Json.Migrating
{
    public class JudgeInfoConverter : Newtonsoft.Json.JsonConverter<JudgeInfo>
    {
        public override JudgeInfo ReadJson(JsonReader reader, Type objectType, JudgeInfo existingValue, bool hasExistingValue, Newtonsoft.Json.JsonSerializer serializer)
        {
            var data = JObject.Load(reader).ToObject<Dictionary<string, int>>(serializer);

            var dict = new Dictionary<JudgeGrade, int>();
            var type = typeof(JudgeGrade);

            foreach (JudgeGrade judgeResult in Enum.GetValues(type))
            {
                if (!data.TryGetValue(Enum.GetName(type, judgeResult), out var i))
                {
                    dict[judgeResult] = 0;
                }
                else
                {
                    dict[judgeResult] = i;
                }
            }

            foreach (var kv in data)
            {
                switch (kv.Key)
                {
                    case "FastGreat1":
                        dict[JudgeGrade.FastGreat2nd] = kv.Value;
                        break;
                    case "FastGreat2":
                        dict[JudgeGrade.FastGreat3rd] = kv.Value;
                        break;
                    case "LateGreat1":
                        dict[JudgeGrade.LateGreat2nd] = kv.Value;
                        break;
                    case "LateGreat2":
                        dict[JudgeGrade.LateGreat3rd] = kv.Value;
                        break;
                    case "FastPerfect1":
                        dict[JudgeGrade.FastPerfect2nd] = kv.Value;
                        break;
                    case "FastPerfect2":
                        dict[JudgeGrade.FastPerfect3rd] = kv.Value;
                        break;
                    case "LatePerfect1":
                        dict[JudgeGrade.LatePerfect2nd] = kv.Value;
                        break;
                    case "LatePerfect2":
                        dict[JudgeGrade.LatePerfect3rd] = kv.Value;
                        break;
                }
            }

            return new JudgeInfo(dict);
        }

        public override void WriteJson(JsonWriter writer, JudgeInfo value, Newtonsoft.Json.JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }
    }
}