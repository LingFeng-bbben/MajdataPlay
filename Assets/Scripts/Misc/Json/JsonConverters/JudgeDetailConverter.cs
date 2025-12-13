using MajdataPlay.Collections;
using MajdataPlay.Scenes.Game;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace MajdataPlay.Json
{
    public class JudgeDetailConverter : JsonConverter<JudgeDetail>
    {
        public override JudgeDetail ReadJson(JsonReader reader, Type objectType, JudgeDetail? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var obj = JObject.Load(reader);
            var dict = obj.ToObject<Dictionary<ScoreNoteType, JudgeInfo>>(serializer);
            return new JudgeDetail(dict);
        }

        public override void WriteJson(JsonWriter writer, JudgeDetail value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            foreach (var kv in value)
            {
                writer.WritePropertyName(Enum.GetName(typeof(ScoreNoteType), kv.Key));
                serializer.Serialize(writer, kv.Value);
            }
            writer.WriteEndObject();
        }
    }
}
