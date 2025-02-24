using MajdataPlay.Collections;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MajdataPlay.Types
{
    public class JudgeDetailConverter : JsonConverter<JudgeDetail>
    {
        public override JudgeDetail Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var dict = JsonSerializer.Deserialize<Dictionary<ScoreNoteType, JudgeInfo>>(ref reader, options);

            return new JudgeDetail(dict);
        }

        public override void Write(Utf8JsonWriter writer, JudgeDetail value, JsonSerializerOptions options) => JsonSerializer.Serialize(writer, value, options);
    }
    public class JudgeInfoConverter : JsonConverter<JudgeInfo>
    {
        public override JudgeInfo Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var data = JsonSerializer.Deserialize<Dictionary<string, int>>(ref reader, options);
            var dict = new Dictionary<JudgeGrade, int>();
            var type = typeof(JudgeGrade);
            foreach (JudgeGrade judgeResult in Enum.GetValues(type))
            {
                if (!data.TryGetValue(Enum.GetName(type, judgeResult), out int i))
                    dict[judgeResult] = 0;
                else 
                    dict[judgeResult] = i;
            }
            foreach (var (k, v) in data)
            {
                switch(k)
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

        public override void Write(Utf8JsonWriter writer, JudgeInfo value, JsonSerializerOptions options) => JsonSerializer.Serialize(writer, value, options);
    }
}
