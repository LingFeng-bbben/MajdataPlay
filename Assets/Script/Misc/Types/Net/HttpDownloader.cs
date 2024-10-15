using MajdataPlay.Collections;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
#nullable enable
namespace MajdataPlay.Net
{
    public class HttpDownloader
    {
        public float Progress
        {
            get
            {
                if (Length == 0)
                    return 0;
                return _downloadedBytes / Length;
            }
        }
        public long Length { get; private set; }
        public bool MultiThread { get; set; } = false;
        public int ThreadCount { get; set; } = 4;
        public int MaxRetryCount { get; set; } = 4;
        public Uri RequestAddress { get; private set; }
        public static TimeSpan Timeout
        {
            get => _httpClient.Timeout;
            set => _httpClient.Timeout = value;
        }

        int _retryCount = 0;
        long _downloadedBytes = 0;

        static HttpClient _httpClient = new HttpClient(new HttpClientHandler()
        {
            Proxy = WebRequest.GetSystemWebProxy(),
            UseProxy = true
        });
        public HttpDownloader(string uri)
        {
            RequestAddress = new Uri(uri);
        }
        public HttpDownloader(Uri uri)
        {
            RequestAddress = uri;
        }

        public async Task DownloadAsync(string savePath)
        {
            var rsp = await _httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, RequestAddress));
            rsp.EnsureSuccessStatusCode();
            var fileSize = rsp.Content.Headers.ContentLength ?? throw new HttpRequestException("Invalid http response");
            Length = fileSize;
            //await DownloadWithSingleThread(savePath);
        }

        struct Downloader
        {
            public int StartAt { get; private set; }
            public int EndAt { get; private set; }
            public long Length { get; private set; }
            public long DownloadedBytes { get; private set; }
            public int MaxRetryCount { get; set; }
            public bool IsCompleted { get; private set; }
            public Uri RequestAddress { get; private set; }

            bool _isDownloading;
            int _retryCount;
            HttpClient _httpClient;
            public Downloader(Uri address, HttpClient httpClient, int startAt, int length)
            {
                _httpClient = httpClient;
                StartAt = startAt;
                EndAt = startAt + length;
                Length = length;
                RequestAddress = address;

                DownloadedBytes = 0;
                MaxRetryCount = 4;
                IsCompleted = false;

                _isDownloading = false;
                _retryCount = 0;
            }

            async Task<Stream> DownloadAsync()
            {
                ThrowIfDownloadingOrCompleted();
                _isDownloading = true;
                var httpStream = await Connect();
                var heapStream = new HeapStream(Length);
                var buffer = new byte[1024];
                var membuffer = buffer.AsMemory();
                while (DownloadedBytes < Length)
                {
                    int read = 0;
                    try
                    {
                        read = await httpStream.ReadAsync(membuffer);
                    }
                    catch
                    {
                        if (_retryCount >= MaxRetryCount)
                        {
                            _isDownloading = false;
                            throw;
                        }

                        await httpStream.DisposeAsync();
                        var newHttpStream = await Retry(DownloadedBytes, EndAt);

                        if (newHttpStream is not null)
                            httpStream = newHttpStream;

                        _retryCount++;
                        continue;
                    }
                    var _memBuffer = read != 1024 ? membuffer.Slice(0, read) : membuffer;
                    await heapStream.WriteAsync(_memBuffer);
                    DownloadedBytes += read;
                }
                httpStream.Dispose();
                _isDownloading = false;
                IsCompleted = true;
                return heapStream;
            }
            async Task DownloadAsync(string savePath)
            {
                ThrowIfDownloadingOrCompleted();
                _isDownloading = true;
                using var fileStream = File.OpenWrite(savePath);
                var httpStream = await Connect();
                var buffer = new byte[1024];
                var membuffer = buffer.AsMemory();
                while (DownloadedBytes < Length)
                {
                    int read = 0;
                    try
                    {
                        read = await httpStream.ReadAsync(membuffer);
                    }
                    catch
                    {
                        if (_retryCount >= MaxRetryCount)
                        {
                            _isDownloading = false;
                            throw;
                        }

                        await httpStream.DisposeAsync();
                        var newHttpStream = await Retry(DownloadedBytes, EndAt);

                        if (newHttpStream is not null)
                            httpStream = newHttpStream;

                        _retryCount++;
                        continue;
                    }
                    var _memBuffer = read != 1024 ? membuffer.Slice(0, read) : membuffer;
                    await fileStream.WriteAsync(_memBuffer);
                    DownloadedBytes += read;
                }
                httpStream.Dispose();
                _isDownloading = false;
                IsCompleted = true;
            }
            async Task<Stream> Connect()
            {
                while (true)
                {
                    try
                    {
                        var rsp = await _httpClient.GetAsync(RequestAddress, HttpCompletionOption.ResponseHeadersRead);
                        rsp.EnsureSuccessStatusCode();
                        return await rsp.Content.ReadAsStreamAsync();
                    }
                    catch
                    {
                        if (_retryCount >= MaxRetryCount)
                            throw;
                        _retryCount++;
                    }
                }
            }
            async Task<Stream?> Retry(long startAt, long endAt)
            {
                try
                {
                    var req = new HttpRequestMessage(HttpMethod.Get, RequestAddress);
                    req.Headers.Range = new RangeHeaderValue(startAt, endAt);

                    var rsp = await _httpClient.SendAsync(req, HttpCompletionOption.ResponseHeadersRead);
                    return await rsp.Content.ReadAsStreamAsync();
                }
                catch
                {
                    return null;
                }
            }
            void ThrowIfDownloadingOrCompleted()
            {
                if (_isDownloading || IsCompleted)
                    throw new InvalidOperationException("");
                _retryCount = 0;
                DownloadedBytes = 0;
            }
        }

    }
    public unsafe class HeapStream : Stream
    {
        public override bool CanRead => _canRead;
        public override bool CanSeek => _canSeek;
        public override bool CanWrite => _canWrite;
        public override long Length => _length;
        public override long Position { get; set; }


        long _position = 0;
        readonly Heap<byte> _buffer;
        readonly bool _canRead = true;
        readonly bool _canSeek = true;
        readonly bool _canWrite = true;
        readonly long _length;

        public HeapStream(long length, bool canRead, bool canWrite, bool canSeek) : this(length, canRead, canWrite)
        {
            _canSeek = canSeek;
        }
        public HeapStream(long length, bool canRead, bool canWrite) : this(length, canRead)
        {
            _canWrite = canWrite;
        }
        public HeapStream(long length, bool canRead) : this(length)
        {
            _canRead = canRead;
        }
        public HeapStream(long length)
        {
            _buffer = new Heap<byte>(length);
            _length = length;
        }
        public HeapStream(IntPtr pointer, long length, bool canRead, bool canWrite, bool canSeek) : this(pointer, length, canRead, canWrite)
        {
            _canSeek = canSeek;
        }
        public HeapStream(IntPtr pointer, long length, bool canRead, bool canWrite) : this(pointer, length, canRead)
        {
            _canWrite = canWrite;
        }
        public HeapStream(IntPtr pointer, long length, bool canRead) : this(pointer, length)
        {
            _canRead = canRead;
        }
        public HeapStream(IntPtr pointer, long length)
        {
            _buffer = new Heap<byte>(pointer, length);
            _length = length;
        }
        public HeapStream(byte* pointer, long length, bool canRead, bool canWrite, bool canSeek) : this(pointer, length, canRead, canWrite)
        {
            _canSeek = canSeek;
        }
        public HeapStream(byte* pointer, long length, bool canRead, bool canWrite) : this(pointer, length, canRead)
        {
            _canWrite = canWrite;
        }
        public HeapStream(byte* pointer, long length, bool canRead) : this(pointer, length)
        {
            _canRead = canRead;
        }
        public HeapStream(byte* pointer, long length)
        {
            _buffer = new Heap<byte>(pointer, length);
            _length = length;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (!_canWrite)
                throw new NotSupportedException("Unsupport operation because this stream cannot be wrote");

            for (int i = offset; i < count; i++)
            {
                if (_position == Length)
                    break;
                _buffer[_position++] = buffer[i];
            }
        }
        public override void Flush() => throw new NotImplementedException();
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (!_canWrite)
                throw new NotSupportedException("Unsupport operation because this stream cannot be read");

            var startAt = _position;
            for (int i = offset; i < count; i++)
            {
                if (_position == Length)
                    break;
                buffer[i] = _buffer[_position++];
            }
            return (int)(_position - startAt);
        }
        public override long Seek(long offset, SeekOrigin origin)
        {
            if (!_canSeek)
                throw new NotSupportedException("Unsupport operation because this stream cannot be seeked");
            switch (origin)
            {
                case SeekOrigin.Begin:
                    var newPos = offset > _length ? _length : offset;
                    _position = newPos;
                    return _position;
                case SeekOrigin.Current:
                    break;
                case SeekOrigin.End:
                    break;
            }
            return 0;
        }
        public Heap<byte> ToHeap() => _buffer;
        public override void SetLength(long value) => throw new NotImplementedException();
    }
}
