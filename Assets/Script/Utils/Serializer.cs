using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using UnityEngine;
#nullable enable
namespace MajdataPlay.Utils
{
    public static class Serializer
    {
        public static class Json
        {
            public static string Serialize<T>(in T obj, in JsonSerializerOptions? option = null)
            {
                if (option is not null)
                    return JsonSerializer.Serialize(obj, option);
                else
                    return JsonSerializer.Serialize(obj);
            }
            public static async ValueTask<string> SerializeAsync<T>(T obj, JsonSerializerOptions? option = null)
            {
                await using (var memStream = new MemoryStream())
                {
                    if (option is not null)
                        await JsonSerializer.SerializeAsync(memStream, obj, option);
                    else
                        await JsonSerializer.SerializeAsync(memStream, obj);
                    memStream.Position = 0;
                    using (var memReader = new StreamReader(memStream))
                        return await memReader.ReadToEndAsync();
                }
            }
            public static async ValueTask SerializeAsync<T>(Stream stream, T obj, JsonSerializerOptions? option = null)
            {
                if (option is not null)
                    await JsonSerializer.SerializeAsync(stream, obj, option);
                else
                    await JsonSerializer.SerializeAsync(stream, obj);
            }

            public static T? Deserialize<T>(in string json, in JsonSerializerOptions? option = null)
            {
                if (option is not null)
                    return JsonSerializer.Deserialize<T>(json, option);
                else
                    return JsonSerializer.Deserialize<T>(json);
            }
            public static bool TryDeserialize<T>(in string json, out T? result, in JsonSerializerOptions? option = null)
            {
                try
                {
                    if (option is not null)
                        result = JsonSerializer.Deserialize<T>(json, option);
                    else
                        result = JsonSerializer.Deserialize<T>(json);
                    return true;
                }
                catch(Exception e)
                {
                    Debug.LogException(e);
                    result = default;
                    return false;
                }
            }
            public static async ValueTask<T?> DeserializeAsync<T>(Stream jsonStream, JsonSerializerOptions? option = null)
            {
                if (option is not null)
                    return await JsonSerializer.DeserializeAsync<T>(jsonStream, option);
                else
                    return await JsonSerializer.DeserializeAsync<T>(jsonStream);
            }
            public static async ValueTask<(bool, T?)> TryDeserializeAsync<T>(Stream jsonStream, JsonSerializerOptions? option = null)
            {
                try
                {
                    T? result = default;
                    if (option is not null)
                        result = await JsonSerializer.DeserializeAsync<T>(jsonStream, option);
                    else
                        result = await JsonSerializer.DeserializeAsync<T>(jsonStream);
                    return (true, result);
                }
                catch
                {
                    return (false, default);
                }
            }
        }
    }
}

