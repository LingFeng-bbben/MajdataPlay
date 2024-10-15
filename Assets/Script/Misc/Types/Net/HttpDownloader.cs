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
        public double Progress
        {
            get
            {
                if (_downloaders.Length == 0)
                    return 0;
                var totalProgress = _downloaders.Select(x => x.Progress).Sum();
                return totalProgress / _downloaders.Length;
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
        Downloader[] _downloaders = Array.Empty<Downloader>();

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
            bool rangeDlAvailable = rsp.Headers.Contains("Accept-Ranges") && rsp.Headers.GetValues("Accept-Ranges").Any(x => x == "bytes");
            if (MultiThread)
                MultiThread = rangeDlAvailable;
            var fileSize = rsp.Content.Headers.ContentLength ?? throw new HttpRequestException("Invalid http response");
            Length = fileSize;

            if (!MultiThread || fileSize < ThreadCount * 2)
                await SingleThreadDownloadAsync(savePath);
            else
                await MultiThreadDownloadAsync(savePath);
        }
        async ValueTask SingleThreadDownloadAsync(string savePath)
        {
            var downloader = new Downloader(RequestAddress, _httpClient, 0, Length - 1);
            _downloaders = new Downloader[] { downloader };
            await downloader.DownloadAsync(savePath);
        }
        async ValueTask MultiThreadDownloadAsync(string savePath)
        {
            var length4Part = Length / ThreadCount;
            _downloaders = new Downloader[ThreadCount];
            Task[] tasks = new Task[ThreadCount];
            for (int i = 0; i < ThreadCount; i++)
            {
                var isLast = i == ThreadCount - 1;
                var startAt = i * length4Part;
                var length = length4Part;
                if (isLast)
                    length = Length - startAt;
                var downloader = new Downloader(RequestAddress, _httpClient, startAt, length);
                _downloaders[i] = downloader;
                tasks[i] = downloader.DownloadAsync($"{savePath}.{i}");
            }
            await Task.WhenAll(tasks);
            if (File.Exists(savePath))
                File.Delete(savePath);
            var combiners = new FileCombiner[ThreadCount];
            for (int i = 0; i < ThreadCount; i++)
            {
                var chunkedFilePath = $"{savePath}.{i}";
                var isLast = i == ThreadCount - 1;
                var startAt = i * length4Part;
                var length = length4Part;

                if (isLast)
                    length = Length - startAt;

                var combiner = new FileCombiner(savePath, chunkedFilePath, startAt, length);
                combiners[i] = combiner;
                tasks[i] = combiner.CombineAsync();
            }
            await Task.WhenAll(tasks);
            foreach (var combiner in combiners)
                combiner.Close();
        }
        struct FileCombiner
        {
            public long StartAt { get; private set; }
            public long Length { get; private set; }

            FileStream _fileStream;
            FileStream _chunkedStream;

            public FileCombiner(string savePath, string chunkedFilePath, long startAt, long length)
            {
                StartAt = startAt;
                Length = length;
                _fileStream = new FileStream(savePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite, 1024, FileOptions.WriteThrough);
                _chunkedStream = new FileStream(chunkedFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                _fileStream.Position = startAt;
            }

            public async Task CombineAsync()
            {
                int readBytes = 0;
                var buffer = new byte[1024];
                while (readBytes < Length)
                {
                    var read = await _chunkedStream.ReadAsync(buffer, 0, 1024);
                    await _fileStream.WriteAsync(buffer, 0, read);
                    readBytes += read;
                }
            }
            public void Close()
            {
                _fileStream.Close();
                _chunkedStream.Close();
            }
        }
        struct Downloader
        {
            public double Progress { get; private set; }
            public long StartAt { get; private set; }
            public long EndAt { get; private set; }
            public long Length { get; private set; }
            public int MaxRetryCount { get; set; }
            public bool IsCompleted { get; private set; }
            public Uri RequestAddress { get; private set; }

            long _downloadedBytes;
            bool _isDownloading;
            int _retryCount;
            HttpClient _httpClient;
            public Downloader(Uri address, HttpClient httpClient, long startAt, long length)
            {
                _httpClient = httpClient;
                StartAt = startAt;
                EndAt = startAt + length - 1;
                Length = length;
                RequestAddress = address;

                _downloadedBytes = 0;
                MaxRetryCount = 4;
                IsCompleted = false;

                Progress = 0;
                _isDownloading = false;
                _retryCount = 0;
            }
            public async Task<Stream> DownloadAsync()
            {
                ThrowIfDownloadingOrCompleted();
                _isDownloading = true;
                var httpStream = await Connect();
                var heapStream = new HeapStream(Length);
                var buffer = new byte[1024];
                var membuffer = buffer.AsMemory();
                while (_downloadedBytes < Length)
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
                        var newHttpStream = await Retry(_downloadedBytes, EndAt);

                        if (newHttpStream is not null)
                            httpStream = newHttpStream;

                        _retryCount++;
                        continue;
                    }
                    var _memBuffer = read != 1024 ? membuffer.Slice(0, read) : membuffer;
                    await heapStream.WriteAsync(_memBuffer);
                    _downloadedBytes += read;
                }
                httpStream.Dispose();
                _isDownloading = false;
                IsCompleted = true;
                return heapStream;
            }
            public async Task DownloadAsync(string savePath)
            {
                ThrowIfDownloadingOrCompleted();
                _isDownloading = true;
                using var fileStream = File.Create(savePath, 1024, FileOptions.WriteThrough);
                var httpStream = await Connect();
                var buffer = new byte[1024];
                var membuffer = buffer.AsMemory();
                while (_downloadedBytes < Length)
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
                        var newHttpStream = await Retry(_downloadedBytes, EndAt);

                        if (newHttpStream is not null)
                            httpStream = newHttpStream;

                        _retryCount++;
                        continue;
                    }
                    var _memBuffer = read != 1024 ? membuffer.Slice(0, read) : membuffer;
                    await fileStream.WriteAsync(_memBuffer);
                    await fileStream.FlushAsync();
                    _downloadedBytes += read;
                    UpdateProgress();
                }
                httpStream.Dispose();
                _isDownloading = false;
                IsCompleted = true;
            }
            void UpdateProgress()
            {
                if (Length == 0)
                    Progress = 0;
                else
                    Progress = (double)_downloadedBytes / Length;
            }
            async Task<Stream> Connect()
            {
                while (true)
                {
                    try
                    {
                        var req = new HttpRequestMessage(HttpMethod.Get, RequestAddress);
                        req.Headers.Range = new RangeHeaderValue(StartAt, EndAt);
                        var rsp = await _httpClient.SendAsync(req, HttpCompletionOption.ResponseHeadersRead);
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
                    rsp.EnsureSuccessStatusCode();
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
                _downloadedBytes = 0;
            }
        }
    }
}
