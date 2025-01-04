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
            var dict = JsonSerializer.Deserialize<Dictionary<JudgeGrade, int>>(ref reader, options);
            foreach(JudgeGrade judgeResult in Enum.GetValues(typeof(JudgeGrade)))
            {
                if (!dict.TryGetValue(judgeResult, out int i))
                    dict[judgeResult] = 0;
            }
            return new JudgeInfo(dict);
        }

        public override void Write(Utf8JsonWriter writer, JudgeInfo value, JsonSerializerOptions options) => JsonSerializer.Serialize(writer, value, options);
    }
}
