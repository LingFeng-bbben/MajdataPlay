using MajdataPlay.Extensions;
using MajdataPlay.Numerics;
using MajdataPlay.Utils;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
#nullable enable
namespace MajdataPlay.Net
{
    public partial class HttpTransporter
    {
        public string UserAgent { get; set; } = $"MajdataPlay/{MajInstances.GameVersion}";
        public static TimeSpan Timeout
        {
            get => ShareClient.Timeout;
            set => ShareClient.Timeout = value;
        }
        public static HttpClient ShareClient { get; } = new HttpClient(new HttpClientHandler()
        {
            Proxy = WebRequest.GetSystemWebProxy(),
            UseProxy = true,
            UseCookies = true,
            CookieContainer = new CookieContainer()
        });

        public async ValueTask<PostResult> PostAsync(PostRequest request)
        {
            var httpReq = new HttpRequestMessage(HttpMethod.Post,request.RequestAddress);
            var length = request.Content.Headers.ContentLength;
            var retryCount = 0;
            var maxRetryCount = request.MaxRetryCount;
            var startAt = DateTime.Now;
            httpReq.Headers.UserAgent.ParseAdd(UserAgent);
            httpReq.Content = request.Content;

            while(true)
            {
                try
                {
                    var rsp = await ShareClient.SendAsync(httpReq);
                    rsp.ThrowIfTransmitFailure();
                    return new PostResult()
                    {
                        Length = length ?? 0,
                        StartAt = startAt,
                        StatusCode = rsp.StatusCode,
                        RequestError = HttpRequestError.NoError,
                        ResponseMessage = rsp
                    };
                }
                catch(HttpRequestException ex)
                {
                    if (retryCount >= maxRetryCount)
                    {
                        var errCode = ex.GetErrorCode();
                        return new PostResult()
                        {
                            Length = length ?? 0,
                            StartAt = startAt,
                            StatusCode = HttpStatusCode.BadGateway,
                            RequestError = errCode,
                            ResponseMessage = null
                        };
                    }
                }
                catch(HttpTransmitException e)
                {
                    if (retryCount >= maxRetryCount)
                    {
                        return new PostResult()
                        {
                            Length = length ?? 0,
                            StartAt = startAt,
                            StatusCode = e.StatusCode,
                            RequestError = e.RequestError,
                            ResponseMessage = e.ResponseMessage
                        };
                    }
                }
                retryCount++;
            }
        }
        public async ValueTask<GetResult> GetAsync(GetRequest request, int bufferSize = 4096)
        {
            var threadCount = request.ThreadCount;
            var multiThread = request.MultiThread && threadCount > 1;

            var preCheckResult = await PreCheck(request);
            var fileSize = preCheckResult.Length;
            var startAt = DateTime.Now;

            multiThread = multiThread && preCheckResult.RangeDLAvailable;

            if (fileSize < 0)
            {
                return new GetResult()
                {
                    Length = fileSize,
                    SavePath = request.SavePath,
                    StartAt = startAt,
                    StatusCode = preCheckResult.StatusCode,
                    RequestError = HttpRequestError.InvalidResponse,
                    ResponseMessage = preCheckResult.ResponseMessage
                };
            }


            try
            {
                if (!multiThread || fileSize < threadCount * 2)
                    await SingleThreadGetAsync(request, fileSize, bufferSize);
                else
                    await MultiThreadGetAsync(request, fileSize, bufferSize);
                return new GetResult()
                {
                    Length = fileSize,
                    SavePath = request.SavePath,
                    StartAt = startAt,
                    StatusCode = preCheckResult.StatusCode,
                    RequestError = HttpRequestError.NoError,
                    ResponseMessage = null
                };
            }
            catch (HttpTransmitException e)
            {
                return new GetResult()
                {
                    Length = fileSize,
                    SavePath = request.SavePath,
                    StartAt = startAt,
                    StatusCode = e.StatusCode,
                    RequestError = e.RequestError,
                    ResponseMessage = e.ResponseMessage
                };
            }
        }
        async ValueTask<PreCheckResule> PreCheck(GetRequest request)
        {
            var requestAddress = request.RequestAddress;
            var req = new HttpRequestMessage(HttpMethod.Head, requestAddress);
            var retryCount = 0;
            var rsp = await ShareClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, requestAddress));
            var isSuccess = rsp.IsSuccessStatusCode && rsp.Content.Headers.ContentLength > 0;
            req.Headers.UserAgent.ParseAdd(UserAgent);
            
            if(!isSuccess)
            {
                while (true)
                {
                    rsp = await ShareClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, requestAddress));
                    isSuccess = rsp.IsSuccessStatusCode && rsp.Content.Headers.ContentLength > 0;
                    if (!isSuccess)
                    {
                        if (retryCount < request.MaxRetryCount)
                        {
                            retryCount++;
                            continue;
                        }
                        return new PreCheckResule()
                        {
                            ResponseMessage = rsp,
                            StatusCode = rsp.StatusCode,
                            Length = 0,
                            RangeDLAvailable = false
                        };
                    }
                }
            }

            bool rangeDlAvailable = rsp.Headers.Contains("Accept-Ranges") && rsp.Headers.GetValues("Accept-Ranges").Any(x => x == "bytes");
            var fileSize = rsp.Content.Headers.ContentLength ?? -1;

            return new PreCheckResule()
            {
                ResponseMessage = rsp,
                StatusCode = rsp.StatusCode,
                Length = fileSize,
                RangeDLAvailable = rangeDlAvailable
            };
        }
        async ValueTask SingleThreadGetAsync(GetRequest request, long fileSize, int bufferSize)
        {
            Progress<ReportEventArgs> reporter = new();
            var info = new RangeDownloadInfo()
            {
                RequestAddress = request.RequestAddress,
                Reporter = reporter,
                StartAt = 0,
                UserAgent = UserAgent,
                BufferSize = bufferSize,
                SegmentLength = fileSize,
                SavePath = request.SavePath
            };
            var downloader = new Downloader(info);
            var progressReporter = request.ProgressReporter;
            EventHandler<ReportEventArgs> progressUpdateHandler = (sender, args) =>
            {
                if (progressReporter is not null)
                {
                    var progress = new DLProgress()
                    {
                        Length = fileSize,
                        Progress = args.Progress
                    };
                    progressReporter.OnProgressChanged(progress);
                }
            };
            reporter.ProgressChanged += progressUpdateHandler;

            try
            {
                await downloader.DownloadAsync();
                reporter.ProgressChanged -= progressUpdateHandler;
            }
            catch(Exception e)
            {
                reporter.ProgressChanged -= progressUpdateHandler;
                throw;
            }
        }
        async ValueTask MultiThreadGetAsync(GetRequest request, long fileSize, int bufferSize)
        {
            var reporter = new Progress<ReportEventArgs>();
            var threadCount = request.ThreadCount;
            var savePath = request.SavePath;
            var length4Part = fileSize / threadCount;
            var progresses = new double[threadCount];
            var tasks = new Task[threadCount];
            var progressReporter = request.ProgressReporter;
            EventHandler<ReportEventArgs> progressUpdateHandler = (sender, args) =>
            {
                if (progressReporter is not null)
                {
                    double progress = 0;
                    if (progresses.Length == 0)
                        progress = 0;
                    else
                    {
                        progresses[args.Index] = args.Progress;
                        var totalProgress = progresses.Sum();
                        progress = totalProgress / progresses.Length;
                    }
                    progressReporter.OnProgressChanged(new DLProgress()
                    {
                        Length = fileSize,
                        Progress = progress
                    });
                }
            };
            reporter.ProgressChanged += progressUpdateHandler;
            try
            {
                for (int i = 0; i < threadCount; i++)
                {
                    var isLast = i == threadCount - 1;
                    var startAt = i * length4Part;
                    var length = length4Part;
                    if (isLast)
                        length = fileSize - startAt;
                    var info = new RangeDownloadInfo()
                    {
                        Index = i,
                        Reporter = reporter,
                        StartAt = startAt,
                        BufferSize = bufferSize,
                        UserAgent = UserAgent,
                        SegmentLength = length,
                        SavePath = $"{savePath}.{i}",
                    };

                    var downloader = new Downloader(info);
                    tasks[i] = downloader.DownloadAsync();
                }
                await Task.WhenAll(tasks);
                reporter.ProgressChanged -= progressUpdateHandler;
            }
            catch
            {
                reporter.ProgressChanged -= progressUpdateHandler;
                throw;
            }

            if (File.Exists(savePath))
                File.Delete(savePath);
            var combiners = new FileCombiner[threadCount];
            for (int i = 0; i < threadCount; i++)
            {
                var chunkedFilePath = $"{savePath}.{i}";
                var isLast = i == threadCount - 1;
                var startAt = i * length4Part;
                var length = length4Part;

                if (isLast)
                    length = fileSize - startAt;

                var combiner = new FileCombiner(savePath, chunkedFilePath, startAt, length);
                combiners[i] = combiner;
                tasks[i] = combiner.CombineAsync();
            }
            await Task.WhenAll(tasks);
            foreach (var combiner in combiners)
                combiner.CloseAndDelete();
        }
        struct FileCombiner
        {
            public long StartAt { get; private set; }
            public long Length { get; private set; }

            string _chunkedFilePath;

            FileStream _fileStream;
            FileStream _chunkedStream;

            public FileCombiner(string savePath, string chunkedFilePath, long startAt, long length)
            {
                StartAt = startAt;
                Length = length;
                _fileStream = new FileStream(savePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite, 1024, FileOptions.WriteThrough);
                _chunkedStream = new FileStream(chunkedFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                _fileStream.Position = startAt;
                _chunkedFilePath = chunkedFilePath;
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
                await _fileStream.FlushAsync();
            }
            public void CloseAndDelete()
            {
                _fileStream.Close();
                _chunkedStream.Close();
                File.Delete(_chunkedFilePath);
            }
        }
        struct ReportEventArgs
        {
            public long Index { get; init; }
            public double Progress { get; init; }
        }
        struct Downloader
        {
            public double Progress { get; private set; }
            public long StartAt { get; private set; }
            public long EndAt { get; private set; }
            public long Length { get; private set; }
            public int MaxRetryCount { get; private set; }
            public bool IsCompleted { get; private set; }
            public Uri RequestAddress { get; private set; }

            int _bufferSize;
            long _index;
            long _downloadedBytes;
            bool _isDownloading;
            int _retryCount;
            string _savePath;
            string _userAgent;
            IProgress<ReportEventArgs> _reporter;
            HttpClient _httpClient;
            public Downloader(RangeDownloadInfo info)
            {
                _httpClient = ShareClient;
                StartAt = info.StartAt;
                EndAt = info.StartAt + info.SegmentLength - 1;
                Length = info.SegmentLength;
                RequestAddress = info.RequestAddress;

                _downloadedBytes = 0;
                MaxRetryCount = 4;
                Progress = 0;
                IsCompleted = false;

                _bufferSize = info.BufferSize;
                _userAgent = info.UserAgent;
                _reporter = info.Reporter;
                _isDownloading = false;
                _retryCount = 0;
                _index = info.Index;
                _savePath = info.SavePath;
            }
            public async Task DownloadAsync()
            {
                ThrowIfDownloadingOrCompleted();
                _isDownloading = true;
                using var fileStream = File.Create(_savePath, 1024, FileOptions.WriteThrough);
                var httpStream = await Connect();
                var buffer = new byte[_bufferSize];
                var membuffer = buffer.AsMemory();
                while (true)
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
                        var newHttpStream = await Retry();

                        if (newHttpStream is not null)
                            httpStream = newHttpStream;

                        _retryCount++;
                        continue;
                    }
                    if (read > 0)
                    {
                        var _memBuffer = read != _bufferSize ? membuffer.Slice(0, read) : membuffer;
                        await fileStream.WriteAsync(_memBuffer);
                        await fileStream.FlushAsync();
                        _downloadedBytes += read;
                        UpdateProgress();
                    }
                    else if (_downloadedBytes >= Length || (Length == 0 && read == 0))
                        break;
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
                _reporter.Report(new ReportEventArgs()
                {
                    Index = _index,
                    Progress = Progress,
                });
            }
            async ValueTask<Stream> Connect()
            {
                while (true)
                {
                    try
                    {
                        var req = new HttpRequestMessage(HttpMethod.Get, RequestAddress);
                        if (Length != 0)
                            req.Headers.Range = new RangeHeaderValue(StartAt, EndAt);
                        req.Headers.UserAgent.ParseAdd(_userAgent);
                        var rsp = await _httpClient.SendAsync(req, HttpCompletionOption.ResponseHeadersRead);
                        if(((int)rsp.StatusCode).InRange(400,499))
                            rsp.ThrowIfTransmitFailure();
                        else
                            rsp.EnsureSuccessStatusCode();
                        return await rsp.Content.ReadAsStreamAsync();
                    }
                    catch(HttpRequestException)
                    {
                        if (_retryCount >= MaxRetryCount)
                            throw;
                        _retryCount++;
                    }
                }
            }
            async ValueTask<Stream?> Retry()
            {
                try
                {
                    var req = new HttpRequestMessage(HttpMethod.Get, RequestAddress);
                    if (Length != 0)
                        req.Headers.Range = new RangeHeaderValue(_downloadedBytes, EndAt);
                    else
                        _downloadedBytes = 0;
                    req.Headers.UserAgent.ParseAdd(_userAgent);

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
        readonly struct PreCheckResule
        {
            public HttpResponseMessage ResponseMessage { get; init; }
            public HttpStatusCode StatusCode { get; init; }
            public long Length { get; init; }
            public bool RangeDLAvailable { get; init; }
        }
        readonly struct RangeDownloadInfo
        {
            public int Index { get; init; }
            public long StartAt { get; init; }
            public long SegmentLength { get; init; }
            public int MaxRetryCount { get; init; }
            public string SavePath { get; init; }
            public string UserAgent { get; init; }
            public int BufferSize { get; init; }
            public IProgress<ReportEventArgs> Reporter { get; init; }
            public Uri RequestAddress { get; init; }
        }
    }
}
