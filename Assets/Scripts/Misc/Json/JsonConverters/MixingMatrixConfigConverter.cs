using MajdataPlay.Settings;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

#nullable enable

namespace MajdataPlay.Json
{
    public class MixingMatrixConfigConverter : JsonConverter<MixingMatrixConfig>
    {
        const int INPUT_CHANNELS = 2;

        public override MixingMatrixConfig? ReadJson(JsonReader reader, Type objectType, MixingMatrixConfig? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var config = new MixingMatrixConfig();
            var obj = JObject.Load(reader);

            foreach (var prop in obj.Properties())
            {
                var key = prop.Name;
                if (prop.Value is not JArray array)
                {
                    continue;
                }

                int rowCount = array.Count;
                if (rowCount == 0)
                {
                    continue;
                }

                var matrix = new float[rowCount, INPUT_CHANNELS];
                for (int i = 0; i < rowCount; i++)
                {
                    if (array[i] is not JArray row)
                    {
                        continue;
                    }
                    for (int j = 0; j < Math.Min(row.Count, INPUT_CHANNELS); j++)
                    {
                        var val = row[j]?.Value<float>() ?? 0f;
                        matrix[i, j] = Math.Clamp(val, -1f, 1f);
                    }
                }

                config.ByChannelCount[key] = matrix;
            }

            return config;
        }

        public override void WriteJson(JsonWriter writer, MixingMatrixConfig? value, JsonSerializer serializer)
        {
            writer.WriteStartObject();

            foreach (var kvp in value!.ByChannelCount)
            {
                writer.WritePropertyName(kvp.Key);
                writer.WriteStartArray();
                var matrix = kvp.Value;
                int rows = matrix.GetLength(0);
                int cols = matrix.GetLength(1);
                for (int i = 0; i < rows; i++)
                {
                    writer.WriteStartArray();
                    for (int j = 0; j < cols; j++)
                    {
                        writer.WriteValue(matrix[i, j]);
                    }
                    writer.WriteEndArray();
                }
                writer.WriteEndArray();
            }

            writer.WriteEndObject();
        }
    }
}
