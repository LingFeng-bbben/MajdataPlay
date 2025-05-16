using MajdataPlay.Collections;
using MajdataPlay.Game;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MajdataPlay.Json
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
}
