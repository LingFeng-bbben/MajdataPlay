using MajdataPlay.Collections;
using MajdataPlay.Scenes.Game;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace MajdataPlay.Json
{
    public class JudgeDetailConverter : System.Text.Json.Serialization.JsonConverter<JudgeDetail>
    {
        public override JudgeDetail Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var dict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<ScoreNoteType, JudgeInfo>>(ref reader, options);

            return new JudgeDetail(dict);
        }

        public override void Write(Utf8JsonWriter writer, JudgeDetail value, JsonSerializerOptions options) => System.Text.Json.JsonSerializer.Serialize(writer, value, options);
    }
}
namespace MajdataPlay.Json.Migrating
{
    public class JudgeDetailConverter : Newtonsoft.Json.JsonConverter<JudgeDetail>
    {
        public override JudgeDetail ReadJson(JsonReader reader, Type objectType, JudgeDetail? existingValue, bool hasExistingValue, Newtonsoft.Json.JsonSerializer serializer)
        {
            var obj = JObject.Load(reader);
            var dict = obj.ToObject<Dictionary<ScoreNoteType, JudgeInfo>>(serializer);
            return new JudgeDetail(dict);
        }

        public override void WriteJson(JsonWriter writer, JudgeDetail value, Newtonsoft.Json.JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }
    }
}
