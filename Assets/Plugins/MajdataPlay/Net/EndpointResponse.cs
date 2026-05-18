using MajdataPlay.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
#nullable enable
namespace MajdataPlay.Net
{
    public readonly struct EndpointResponse
    {
        public required Uri Endpoint { get; init; }
        public long Length { get; }
        public required bool IsSuccessfully { get; init; }
        public required bool IsDeserializable { get; init; }
        public HttpStatusCode? StatusCode { get; init; }
        public required HttpErrorCode ErrorCode { get; init; }
        public IReadOnlyDictionary<string, IEnumerable<string>> Headers
        {
            get
            {
                return _headers;
            }
            init
            {
                _headers = value ?? EMPTY_HEADERS;
            }
        }
        public required string Message { get; init; }

        readonly ReadOnlyMemory<byte> _data;
        readonly JsonSerializer _serializer;
        readonly JsonSerializerSettings _serializerSettings;
        readonly IReadOnlyDictionary<string, IEnumerable<string>> _headers = EMPTY_HEADERS;
        public readonly static IReadOnlyDictionary<string, IEnumerable<string>> EMPTY_HEADERS = new Dictionary<string, IEnumerable<string>>();

        public EndpointResponse()
        {
            _headers = EMPTY_HEADERS;
        }
        public EndpointResponse(ReadOnlyMemory<byte> data, JsonSerializer serializer, JsonSerializerSettings serializerSettings)
        {
            if (serializer is null)
            {
                throw new ArgumentNullException(nameof(serializer));
            }
            if (serializerSettings is null)
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
            if (encoder is null)
            {
                throw new ArgumentNullException(nameof(encoder));
            }
            if (!IsDeserializable)
            {
                throw new InvalidOperationException("This response cannot be deserialized.");
            }

            return Serializer.Json.Deserialize<T>(encoder.GetString(_data.Span), _serializerSettings);
        }
        public bool TryDeserialize<T>([NotNullWhen(true)] out T? result, [NotNullWhen(false)] out Exception? exception)
        {
            return TryDeserialize(Encoding.UTF8, out result, out exception);
        }
        public bool TryDeserialize<T>(Encoding encoder, [NotNullWhen(true)] out T? result, [NotNullWhen(false)] out Exception? exception)
        {
            if (encoder is null)
            {
                throw new ArgumentNullException(nameof(encoder));
            }
            if (!IsDeserializable)
            {
                result = default;
                exception = new InvalidOperationException();
                return false;
            }
            return Serializer.Json.TryDeserialize<T>(encoder.GetString(_data.Span), out result, out exception, _serializerSettings);
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
            if (string.IsNullOrEmpty(header))
            {
                throw new ArgumentNullException(nameof(header));
            }
            if (Headers.TryGetValue(header, out var values))
            {
                return values;
            }
            return Array.Empty<string>();
        }
        public override string ToString()
        {
            return $"Endpoint: {Endpoint}\nStatusCode: {StatusCode}\nErrorCode: {ErrorCode}\nIsDeserializable: {IsDeserializable}\nMessage:{Message}\nHeaders:\n" + string.Join('\n', Headers.Select(x => $"{x.Key}: {string.Join(';', x.Value)}"));
        }
    }
    public readonly struct EndpointResponse<T>
    {
        public long Length => _endpointResponse.Length;
        public bool IsSuccessfully => _endpointResponse.IsSuccessfully;
        public bool IsDeserializable => _endpointResponse.IsDeserializable;
        public HttpStatusCode? StatusCode => _endpointResponse.StatusCode;
        public HttpErrorCode ErrorCode => _endpointResponse.ErrorCode;
        public IReadOnlyDictionary<string, IEnumerable<string>> Headers => _endpointResponse.Headers;
        public string Message => _endpointResponse.Message;

        readonly EndpointResponse _endpointResponse;

        public EndpointResponse(EndpointResponse endpointResponse)
        {
            _endpointResponse = endpointResponse;
        }
        public T? Deserialize()
        {
            return _endpointResponse.Deserialize<T>();
        }
        public T? Deserialize(Encoding encoder)
        {
            return _endpointResponse.Deserialize<T>(encoder);
        }
        public bool TryDeserialize([NotNullWhen(true)] out T? result, [NotNullWhen(false)] out Exception? exception)
        {
            return _endpointResponse.TryDeserialize(out result, out exception);
        }
        public bool TryDeserialize(Encoding encoder, [NotNullWhen(true)] out T? result, [NotNullWhen(false)] out Exception? exception)
        {
            return _endpointResponse.TryDeserialize(encoder, out result, out exception);
        }
        public ValueTask<T?> DeserializeAsync()
        {
            return _endpointResponse.DeserializeAsync<T>();
        }
        public ValueTask<T?> DeserializeAsync(Encoding encoder)
        {
            return _endpointResponse.DeserializeAsync<T>(encoder);
        }
        public ReadOnlySpan<byte> AsSpan()
        {
            return _endpointResponse.AsSpan();
        }
        public ReadOnlyMemory<byte> AsMemory()
        {
            return _endpointResponse.AsMemory();
        }
        public IEnumerable<string> TryGetHeader(string header)
        {
            return _endpointResponse.TryGetHeader(header);
        }
        public override string ToString()
        {
            return _endpointResponse.ToString();
        }
    }
}