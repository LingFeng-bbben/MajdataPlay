using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
#nullable enable
namespace MajdataPlay.Utils
{
    public static class Serializer
    {
        public static class Json
        {
            readonly static JsonSerializer DEFAULT_JSON_SERIALIZER = JsonSerializer.Create(new()
            {
                //DefaultValueHandling = DefaultValueHandling.Populate
                ObjectCreationHandling = ObjectCreationHandling.Replace
            });
            static Json()
            {
                JsonConvert.DefaultSettings = () => new JsonSerializerSettings
                {
                    //DefaultValueHandling = DefaultValueHandling.Populate
                    ObjectCreationHandling = ObjectCreationHandling.Replace
                };
            }
            public static string Serialize<T>(in T obj, JsonSerializerSettings? settings = null)
            {
                return JsonConvert.SerializeObject(obj, settings);
            }
            public static Task<string> SerializeAsync<T>(T obj, JsonSerializerSettings? settings)
            {
                if(settings is null)
                {
                    return SerializeAsync<T>(obj, DEFAULT_JSON_SERIALIZER);
                }
                else
                {
                    return SerializeAsync<T>(obj, JsonSerializer.Create(settings));
                } 
            }
            public static async Task<string> SerializeAsync<T>(T obj, JsonSerializer? serializer = null)
            {
                using var memStream = new MemoryStream();
                using (var streamWriter = new StreamWriter(memStream))
                using (var jsonWriter = new JsonTextWriter(streamWriter))
                {
                    serializer ??= DEFAULT_JSON_SERIALIZER;
                    serializer.Serialize(jsonWriter, obj);
                    await jsonWriter.FlushAsync();
                    memStream.Position = 0;
                    using var reader = new StreamReader(memStream);
                    return await reader.ReadToEndAsync();
                }
            }
            public static Task SerializeAsync<T>(Stream stream, T obj, JsonSerializerSettings? settings)
            {
                if (settings is null)
                {
                    return SerializeAsync<T>(stream, obj, DEFAULT_JSON_SERIALIZER);
                }
                else
                {
                    return SerializeAsync<T>(stream, obj, JsonSerializer.Create(settings));
                }
            }
            public static async Task SerializeAsync<T>(Stream stream, T obj, JsonSerializer? serializer = null)
            {
                using var streamWriter = new StreamWriter(stream, Encoding.UTF8, 4096, true);
                using var jsonWriter = new JsonTextWriter(streamWriter);
                serializer ??= DEFAULT_JSON_SERIALIZER;
                serializer.Serialize(jsonWriter, obj);
                await jsonWriter.FlushAsync();
            }

            public static T? Deserialize<T>(in string json, JsonSerializerSettings? settings = null)
            {
                return JsonConvert.DeserializeObject<T>(json, settings);
            }

            public static bool TryDeserialize<T>(in string json, out T? result, JsonSerializerSettings? settings = null)
            {
                try
                {
                    result = JsonConvert.DeserializeObject<T>(json, settings);
                    return true;
                }
                catch (Exception e)
                {
                    MajDebug.LogException(e);
                    result = default;
                    return false;
                }
            }
            public static Task<T?> DeserializeAsync<T>(Stream jsonStream, JsonSerializerSettings? settings)
            {
                if (settings is null)
                {
                    return DeserializeAsync<T>(jsonStream, DEFAULT_JSON_SERIALIZER);
                }
                else
                {
                    return DeserializeAsync<T>(jsonStream, JsonSerializer.Create(settings));
                }
            }
            public static Task<T?> DeserializeAsync<T>(Stream jsonStream, JsonSerializer? serializer = null)
            {
                return Task.Run(() =>
                {
                    using var streamReader = new StreamReader(jsonStream);
                    using var jsonReader = new JsonTextReader(streamReader);
                    serializer ??= DEFAULT_JSON_SERIALIZER;
                    return serializer.Deserialize<T>(jsonReader);
                });
            }

            public static async Task<(bool, T?)> TryDeserializeAsync<T>(Stream jsonStream, JsonSerializerSettings? settings)
            {
                try
                {
                    var result = await DeserializeAsync<T>(jsonStream, settings);
                    return (true, result);
                }
                catch (Exception e)
                {
                    MajDebug.LogError($"{nameof(T)}: {e}");
                    return (false, default);
                }
            }
            public static async Task<(bool, T?)> TryDeserializeAsync<T>(Stream jsonStream, JsonSerializer? serializer = null)
            {
                try
                {
                    var result = await DeserializeAsync<T>(jsonStream, serializer);
                    return (true, result);
                }
                catch (Exception e)
                {
                    MajDebug.LogError($"{nameof(T)}: {e}");
                    return (false, default);
                }
            }
            public static Task<T?> DeserializeAsync<T>(string json, JsonSerializerSettings? settings = null)
            {
                return Task.Run(() => JsonConvert.DeserializeObject<T>(json, settings));
            }

            public static async Task<(bool, T?)> TryDeserializeAsync<T>(string json, JsonSerializerSettings? settings = null)
            {
                try
                {
                    var result = await DeserializeAsync<T>(json, settings);
                    return (true, result);
                }
                catch (Exception e)
                {
                    MajDebug.LogError($"{nameof(T)}: {e}");
                    return (false, default);
                }
            }
        }
    }
}

