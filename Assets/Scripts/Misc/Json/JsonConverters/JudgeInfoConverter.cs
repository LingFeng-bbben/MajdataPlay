using MajdataPlay.Collections;
using MajdataPlay.Scenes.Game.Notes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace MajdataPlay.Json
{
    public class JudgeInfoConverter : JsonConverter<JudgeInfo>
    {
        public override JudgeInfo ReadJson(JsonReader reader, Type objectType, JudgeInfo existingValue, bool hasExistingValue, JsonSerializer serializer)
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

        public override void WriteJson(JsonWriter writer, JudgeInfo value, JsonSerializer serializer)
        {
            writer.WriteStartObject();

            foreach (var kvp in value)
            {
                writer.WritePropertyName(kvp.Key.ToString());
                serializer.Serialize(writer, kvp.Value);
            }

            writer.WriteEndObject();
        }
    }
}