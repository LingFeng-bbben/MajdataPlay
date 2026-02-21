using MajdataPlay.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
#nullable enable
namespace MajdataPlay.Net;
internal readonly struct EndpointResponse
{
    public long Length { get; }
    public required bool IsSuccessfully { get; init; }
    public required bool IsDeserializable { get; init; }
    public HttpStatusCode? StatusCode { get; init; }
    public required HttpErrorCode ErrorCode { get; init; }
    public IReadOnlyDictionary<string, IEnumerable<string>> Headers { get; init; } = EMPTY_HEADERS;
    public required string Message { get; init; }
    
    readonly ReadOnlyMemory<byte> _data;
    readonly JsonSerializer _serializer;
    readonly JsonSerializerSettings _serializerSettings;
    public readonly static IReadOnlyDictionary<string, IEnumerable<string>> EMPTY_HEADERS = new Dictionary<string, IEnumerable<string>>();

    public EndpointResponse(ReadOnlyMemory<byte> data, JsonSerializer serializer, JsonSerializerSettings serializerSettings)
    {
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
    public T? Deserialize<T>()
    {
        return Deserialize<T>(Encoding.UTF8);
    }
    public T? Deserialize<T>(Encoding encoder)
    {
        if(encoder is null)
        {
            throw new ArgumentNullException(nameof(encoder));
        }
        if(!IsDeserializable)
        {
            throw new InvalidOperationException("This response cannot be deserialized.");
        }

        return Serializer.Json.Deserialize<T>(encoder.GetString(_data.Span), _serializerSettings);
    }
    public bool TryDeserialize<T>(out T? result)
    {
        return TryDeserialize(Encoding.UTF8, out result);
    }
    public bool TryDeserialize<T>(Encoding encoder, out T? result)
    {
        if (encoder is null)
        {
            throw new ArgumentNullException(nameof(encoder));
        }
        if (!IsDeserializable)
        {
            result = default;
            return false;
        }
        return Serializer.Json.TryDeserialize<T>(encoder.GetString(_data.Span), out result, _serializerSettings);
    }
    public ValueTask<T?> DeserializeAsync<T>()
    {
        return DeserializeAsync<T>(Encoding.UTF8);
    }
    public async ValueTask<T?> DeserializeAsync<T>(Encoding encoder)
    {
        if (encoder is null)
        {
            throw new ArgumentNullException(nameof(encoder));
        }
        if (!IsDeserializable)
        {
            throw new InvalidOperationException("This response cannot be deserialized.");
        }
        return await Serializer.Json.DeserializeAsync<T>(encoder.GetString(_data.Span), _serializerSettings);
    }
    public ReadOnlySpan<byte> AsSpan()
    {
        return _data.Span;
    }
    public ReadOnlyMemory<byte> AsMemory()
    {
        return _data;
    }
    public IEnumerable<string> TryGetHeader(string header)
    {
        if(string.IsNullOrEmpty(header))
        {
            throw new ArgumentNullException(nameof(header));
        }
        if(Headers.TryGetValue(header, out var values))
        {
            return values;
        }
        return Array.Empty<string>();
    }
    public override string ToString()
    {
        return $"StatusCode: {StatusCode}\nErrorCode: {ErrorCode}\nIsDeserializable: {IsDeserializable}\nMessage:{Message}\nHeaders:\n" + string.Join('\n', Headers.Select(x => $"{x.Key}: {string.Join(';', x.Value)}"));
    }
}
