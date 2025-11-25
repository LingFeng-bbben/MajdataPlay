using MajdataPlay.Utils;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MajdataPlay.Net;
internal readonly struct EndpointResponse
{
    public long Length { get; }
    public required bool IsSuccessfully { get; init; }
    public required bool IsDeserializable { get; init; }
    public HttpStatusCode? StatusCode { get; init; }
    public EndpointResponseCode ResponseCode { get; init; }
    public required HttpErrorCode ErrorCode { get; init; }
    public required string Message { get; init; }
    
    readonly byte[] _data;
    readonly JsonSerializer _serializer;
    readonly JsonSerializerSettings _serializerSettings;

    public EndpointResponse(byte[] data, JsonSerializer serializer, JsonSerializerSettings serializerSettings)
    {
        if(data is null)
        {
            throw new ArgumentNullException(nameof(data));
        }
        if (serializer is null)
        {
            throw new ArgumentNullException(nameof(serializer));
        }
        if(serializerSettings is null)
        {
            throw new ArgumentNullException(nameof(serializerSettings));
        }
        _data = data;
        _serializer = serializer;
        _serializerSettings = serializerSettings;
        Length = data.Length;
    }
    public T Deserialize<T>()
    {
        return Deserialize<T>(Encoding.UTF8);
    }
    public T Deserialize<T>(Encoding encoder)
    {
        if(encoder is null)
        {
            throw new ArgumentNullException(nameof(encoder));
        }
        if(!IsDeserializable)
        {
            throw new InvalidOperationException("This response cannot be deserialized.");
        }

        return Serializer.Json.Deserialize<T>(encoder.GetString(_data), _serializerSettings);
    }
    public ValueTask<T> DeserializeAsync<T>()
    {
        return DeserializeAsync<T>(Encoding.UTF8);
    }
    public async ValueTask<T> DeserializeAsync<T>(Encoding encoder)
    {
        if (encoder is null)
        {
            throw new ArgumentNullException(nameof(encoder));
        }
        if (!IsDeserializable)
        {
            throw new InvalidOperationException("This response cannot be deserialized.");
        }
        return await Serializer.Json.DeserializeAsync<T>(encoder.GetString(_data), _serializerSettings);
    }
    public ReadOnlySpan<byte> AsSpan()
    {
        return _data;
    }
    public ReadOnlyMemory<byte> AsMemory()
    {
        return _data;
    }
}
