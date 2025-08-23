using Cysharp.Threading.Tasks;
using MajdataPlay.IO;
using MajdataPlay.Utils;
using MajSimai;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using NeoSmart.AsyncLock;
using System.IO;
using System.Net.Http;
using System.Buffers;
using System.Threading;
using System.Net;
using MajdataPlay.Settings;
using MajdataPlay.Net;
using Cysharp.Text;
using MajdataPlay.Buffers;

#nullable enable
namespace MajdataPlay
{
    internal class OnlineSongDetail : ISongDetail, IDisposable
    {
        public string Id { get; init; }
        public string Title { get; init; }
        public string Artist { get; init; }
        public string Description { get; init; } = string.Empty;
        public ReadOnlySpan<string> Designers 
        { 
            get
            {
                return _designers;
            }
        }
        public ReadOnlySpan<string> Levels
        {
            get
            {
                return _levels;
            }
        }
        public ChartStorageLocation Location { get; } = ChartStorageLocation.Online;
        public DateTime Timestamp { get; init; }
        public string Hash { get; init; }
        public ApiEndpoint ServerInfo => _serverInfo;

        readonly string _hashHexStr = string.Empty;
        readonly string _cachePath = string.Empty;
        readonly ApiEndpoint _serverInfo;

        readonly Uri _maidataUri;
        readonly Uri _trackUri;
        readonly Uri _videoUri;
        readonly Uri _fullSizeCoverUri;
        readonly Uri _coverUri;

        readonly string[] _designers = new string[7];
        readonly string[] _levels = new string[7];

        static readonly Action _emptyCallback = () => { };

        bool _isDisposed = false;
        bool _isPreloaded = false;

        string? _videoPath = null;
        AudioSampleWrap? _audioTrack = null;
        AudioSampleWrap? _previewAudioTrack = null;
        Sprite? _cover = null;
        Sprite? _fullSizeCover = null;
        SimaiFile? _maidata = null;

        readonly AsyncLock _previewAudioTrackLock = new();
        readonly AsyncLock _audioTrackLock = new();
        readonly AsyncLock _videoPathLock = new();
        readonly AsyncLock _coverLock = new();
        readonly AsyncLock _fullSizeCoverLock = new();
        readonly AsyncLock _maidataLock = new();
        readonly AsyncLock _preloadLock = new();

        ~OnlineSongDetail()
        {
            Dispose();
        }
        public OnlineSongDetail(ApiEndpoint serverInfo, MajnetSongDetail songDetail)
        {
            var apiroot = $"{serverInfo.Url}/maichart";

            Title = songDetail.Title;
            Artist = songDetail.Artist;
            for (var i = 0; i < 7; i++)
            {
                if(i >= songDetail.Levels.Length)
                {
                    break;
                }
                _levels[i] = songDetail.Levels[i];
            }
            var maidataUriStr = $"{apiroot}/{songDetail.Id}/chart";
            var trackUriStr = $"{apiroot}/{songDetail.Id}/track";
            var fullSizeCoverUriStr = $"{apiroot}/{songDetail.Id}/image?fullimage=true";
            var videoUriStr = $"{apiroot}/{songDetail.Id}/video";
            var coverUriStr = $"{apiroot}/{songDetail.Id}/image";

            _maidataUri = new Uri(maidataUriStr);
            _trackUri = new Uri(trackUriStr);
            _fullSizeCoverUri = new Uri(fullSizeCoverUriStr);
            _videoUri = new Uri(videoUriStr);
            _coverUri = new Uri(coverUriStr);

            Hash = songDetail.Hash;
            _hashHexStr = HashHelper.ToHexString(Convert.FromBase64String(Hash));
            _serverInfo = serverInfo;
            _cachePath = Path.Combine(MajEnv.CachePath, $"Net/{_serverInfo.Name}/{_hashHexStr}");
            Id = songDetail.Id;
            Timestamp = songDetail.Timestamp;

            using (var sb = ZString.CreateStringBuilder())
            {
                sb.AppendLine(Description);
                foreach (var tag in songDetail.Tags.Concat(songDetail.PublicTags))
                {
                    sb.AppendLine(tag);
                }
                Description = sb.ToString();
                sb.Clear();
                sb.Append(songDetail.Uploader);
                sb.Append('@');
                sb.Append(songDetail.Designer);
                var designer = sb.ToString();
                for (var i = 0; i < _designers.Length; i++)
                {
                    _designers[i] = designer;
                    
                }
                sb.Clear();
            }

            if (!Directory.Exists(_cachePath))
            {
                Directory.CreateDirectory(_cachePath);
            }
        }

        public async UniTask<AudioSampleWrap> GetPreviewAudioTrackAsync(INetProgress? progress = null, CancellationToken token = default)
        {
            ThrowIfDisposed();
            try
            {
                await UniTask.SwitchToThreadPool();
                var waiting4LockTask = _previewAudioTrackLock.LockAsync(token);
                await Task.WhenAny(waiting4LockTask, Task.Delay(Timeout.Infinite, token));
                var @lock = waiting4LockTask.Result;
                using (@lock)
                {
                    token.ThrowIfCancellationRequested();
                    if(_audioTrack is not null)
                    {
                        _previewAudioTrack = _audioTrack;
                        return _previewAudioTrack;
                    }
                    else if (_previewAudioTrack is not null)
                    {
                        return _previewAudioTrack;
                    }
                    var cacheFlagPath = Path.Combine(_cachePath, "track.cache");
                    var audioManager = MajInstances.AudioManager;
                    var sample = await audioManager.LoadMusicFromUriAsync(_trackUri);

                    _previewAudioTrack = sample;

                    return _previewAudioTrack;
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                throw;
            }
        }
        public async UniTask<AudioSampleWrap> GetAudioTrackAsync(INetProgress? progress = null, CancellationToken token = default)
        {
            ThrowIfDisposed();
            try
            {
                await UniTask.SwitchToThreadPool();
                var waiting4LockTask = _audioTrackLock.LockAsync(token);
                await Task.WhenAny(waiting4LockTask, Task.Delay(Timeout.Infinite, token));
                var @lock = waiting4LockTask.Result;
                using (@lock)
                {
                    token.ThrowIfCancellationRequested();
                    if (_audioTrack is not null)
                    {
                        return _audioTrack;
                    }
                    var savePath = Path.Combine(_cachePath, "track.mp3");
                    var cacheFlagPath = Path.Combine(_cachePath, "track.cache");

                    await DownloadFile(_trackUri, savePath, progress, token);
                    var sampleWarp = await MajInstances.AudioManager.LoadMusicAsync(savePath, true);
                    if (sampleWarp.IsEmpty)
                    {
                        if (File.Exists(cacheFlagPath))
                        {
                            File.Delete(cacheFlagPath);
                        }
                        await DownloadFile(_trackUri, savePath, progress, token);
                    }
                    _audioTrack = sampleWarp;

                    return sampleWarp;
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                _audioTrack = null;
                throw new Exception("Music track Load Failed");
            }
        }
        public async UniTask PreloadAsync(INetProgress? progress = null, CancellationToken token = default)
        {
            ThrowIfDisposed();
            if (_isPreloaded)
            {
                return;
            }
            await UniTask.SwitchToThreadPool();
            if (!await _preloadLock.TryLockAsync(_emptyCallback, TimeSpan.Zero))
            {
                return;
            }
            await UniTask.WhenAll(GetMaidataAsync(token: token), GetCoverAsync(true, token: token));
            _isPreloaded = true;
        }
        public async UniTask<string> GetVideoPathAsync(INetProgress? progress = null, CancellationToken token = default)
        {
            ThrowIfDisposed();
            try
            {
                if (_videoPath is not null)
                {
                    return _videoPath;
                }
                await UniTask.SwitchToThreadPool();
                var waiting4LockTask = _videoPathLock.LockAsync(token);
                await Task.WhenAny(waiting4LockTask, Task.Delay(Timeout.Infinite, token));
                var @lock = waiting4LockTask.Result;
                using (@lock)
                {
                    token.ThrowIfCancellationRequested();

                    if (_videoPath is not null)
                    {
                        return _videoPath;
                    }
                    var savePath = Path.Combine(_cachePath, "bg.mp4");
                    var cacheFlagPath = Path.Combine(_cachePath, $"bg.mp4.cache");

                    if (File.Exists(cacheFlagPath) && !File.Exists(savePath))
                    {
                        _videoPath = string.Empty;
                        return _videoPath;
                    }
                    for (var i = 0; i <= MajEnv.HTTP_REQUEST_MAX_RETRY; i++)
                    {
                        try
                        {
                            var httpClient = MajEnv.SharedHttpClient;
                            using var rsp = await httpClient.GetAsync(_videoUri, HttpCompletionOption.ResponseHeadersRead, token);

                            if (rsp.StatusCode != HttpStatusCode.OK)
                            {
                                using var _ = File.Create(cacheFlagPath);
                                _videoPath = string.Empty;
                                return _videoPath;
                            }
                            else
                            {
                                break;
                            }
                        }
                        catch (Exception e)
                        {
                            if (i == MajEnv.HTTP_REQUEST_MAX_RETRY)
                            {
                                MajDebug.LogError($"Failed to request resource: {_coverUri}\n{e}");
                                throw;
                            }
                        }
                    }
                    await DownloadFile(_videoUri, savePath, progress, token);
                    _videoPath = savePath;
                    return _videoPath;
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                throw;
            }
        }
        public async UniTask<Sprite> GetCoverAsync(bool isCompressed, INetProgress? progress = null, CancellationToken token = default)
        {
            ThrowIfDisposed();
            if (isCompressed)
            {
                return await GetCompressedCoverAsync(progress, token);
            }
            else
            {
                return await GetFullSizeCoverAsync(progress, token);
            }
        }
        public async UniTask<SimaiFile> GetMaidataAsync(bool ignoreCache = false, INetProgress? progress = null, CancellationToken token = default)
        {
            ThrowIfDisposed();
            if (!ignoreCache && _maidata is not null)
            {
                return _maidata;
            }
            try
            {
                await UniTask.SwitchToThreadPool();
                var waiting4LockTask = _maidataLock.LockAsync(token);
                await Task.WhenAny(waiting4LockTask, Task.Delay(Timeout.Infinite, token));
                //while (!waiting4LockTask.IsCompleted)
                //{
                //    await Task.Yield();
                //}
                var @lock = waiting4LockTask.Result;
                using (@lock)
                {
                    token.ThrowIfCancellationRequested();
                    var savePath = Path.Combine(_cachePath, "maidata.txt");

                    await DownloadFile(_maidataUri, savePath, progress, token);

                    _maidata = await SimaiParser.ParseAsync(File.OpenRead(savePath));

                    return _maidata;
                }
            }
            catch(Exception e)
            {
                Debug.LogException(e);
                _maidata = null;
                throw new Exception("Maidata Load Failed");
            }
        }
        async UniTask<Sprite> GetCompressedCoverAsync(INetProgress? progress = null, CancellationToken token = default)
        {
            try
            {
                if (_cover is not null)
                {
                    return _cover;
                }
                await UniTask.SwitchToThreadPool();
                var waiting4LockTask = _coverLock.LockAsync(token);
                await Task.WhenAny(waiting4LockTask, Task.Delay(Timeout.Infinite, token));
                var @lock = waiting4LockTask.Result;
                using (@lock)
                {
                    token.ThrowIfCancellationRequested();
                    if (_cover is not null)
                    {
                        return _cover;
                    }
                    var savePath = Path.Combine(_cachePath, "bg.jpg");
                    var cacheFlagPath = Path.Combine(_cachePath, $"bg.jpg.cache");

                    if (File.Exists(cacheFlagPath))
                    {
                        if (!File.Exists(savePath))
                        {
                            _cover = MajEnv.EmptySongCover;
                        }
                        else
                        {
                            _cover = await SpriteLoader.LoadAsync(savePath, token);
                        }
                        return _cover;
                    }
                    try
                    {
                        await DownloadFile(_coverUri, savePath, progress, token);
                    }
                    catch (InternalHttpRequestException e)
                    {
                        if (e.ErrorCode is HttpErrorCode.Unsuccessful)
                        {
                            using var _ = File.Create(cacheFlagPath);
                            _cover = MajEnv.EmptySongCover;
                            return _cover;
                        }
                        else
                        {
                            _cover = MajEnv.EmptySongCover;
                            return _cover;
                        }
                    }

                    token.ThrowIfCancellationRequested();
                    _cover = await SpriteLoader.LoadAsync(savePath, token);

                    return _cover;
                }
            }
            catch(Exception e)
            {
                Debug.LogException(e);
                throw;
            }
        }
        async UniTask<Sprite> GetFullSizeCoverAsync(INetProgress? progress = null, CancellationToken token = default)
        {
            try
            {
                if (_fullSizeCover is not null)
                {
                    return _fullSizeCover;
                }
                await UniTask.SwitchToThreadPool();
                var waiting4LockTask = _fullSizeCoverLock.LockAsync(token);
                await Task.WhenAny(waiting4LockTask, Task.Delay(Timeout.Infinite, token));
                var @lock = waiting4LockTask.Result;
                using (@lock)
                {
                    token.ThrowIfCancellationRequested();
                    if (_fullSizeCover is not null)
                    {
                        return _fullSizeCover;
                    }
                    var savePath = Path.Combine(_cachePath, "bg_fullSize.jpg");
                    var cacheFlagPath = Path.Combine(_cachePath, $"bg_fullSize.jpg.cache");

                    if (File.Exists(cacheFlagPath))
                    {
                        if (!File.Exists(savePath))
                        {
                            _fullSizeCover = MajEnv.EmptySongCover;
                        }
                        else
                        {
                            _fullSizeCover = await SpriteLoader.LoadAsync(savePath, token);
                        }
                        return _fullSizeCover;
                    }
                    try
                    {
                        await DownloadFile(_fullSizeCoverUri, savePath, progress, token);
                    }
                    catch (InternalHttpRequestException e)
                    {
                        if (e.ErrorCode is HttpErrorCode.Unsuccessful)
                        {
                            using var _ = File.Create(cacheFlagPath);
                            _fullSizeCover = MajEnv.EmptySongCover;
                            return _fullSizeCover;
                        }
                        else
                        {
                            _fullSizeCover = MajEnv.EmptySongCover;
                            return _fullSizeCover;
                        }
                    }
                    token.ThrowIfCancellationRequested();
                    _fullSizeCover = await SpriteLoader.LoadAsync(savePath, token);

                    return _fullSizeCover;
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                throw;
            }
        }
        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }
            _isDisposed = true;
            _audioTrack?.Dispose();
            _previewAudioTrack?.Dispose();
            UniTask.Post(() =>
            {
                GameObject.DestroyImmediate(_cover, true);
                GameObject.DestroyImmediate(_fullSizeCover, true);
            });
            _maidata = null;
            _audioTrack = null;
            _previewAudioTrack = null;
            _cover = null;
            _fullSizeCover = null;
            _videoPath = null;
        }
        void ThrowIfDisposed()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(OnlineSongDetail));
            }
        }
        async Task DownloadFile(Uri uri, string savePath, INetProgress? progress = null, CancellationToken token = default)
        {
            var bufferSize = MajEnv.HTTP_BUFFER_SIZE;
            var fileInfo = new FileInfo(savePath);
            var httpClient = MajEnv.SharedHttpClient;
            var rentBuffer = Pool<byte>.RentArray(bufferSize, true);
            var buffer = rentBuffer.AsMemory();
            var cacheFlagPath = Path.Combine(fileInfo.Directory.FullName, $"{fileInfo.Name}.cache");

            try
            {
                for (var i = 0; i <= MajEnv.HTTP_REQUEST_MAX_RETRY; i++)
                {
                    try
                    {
                        if (File.Exists(cacheFlagPath))
                        {
                            return;
                        }
                        using var rsp = await httpClient.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, token);
                        if (!rsp.IsSuccessStatusCode)
                        {
                            throw new InternalHttpRequestException(HttpErrorCode.Unsuccessful, rsp.StatusCode);
                        }
                        token.ThrowIfCancellationRequested();
                        MajDebug.LogInfo($"Received http response header from: {uri}");

                        if (!rsp.IsSuccessStatusCode) throw new Exception($"HTTP Req Failed: {uri}");

                        if (progress is not null)
                        {
                            progress.TotalBytes = rsp.Content.Headers.ContentLength ?? 0;
                        }
                        using var fileStream = File.Create(savePath);
                        using var httpStream = await rsp.Content.ReadAsStreamAsync();
                        var read = 0;
                        var totalRead = 0;
                        do
                        {
                            token.ThrowIfCancellationRequested();
                            read = await httpStream.ReadAsync(buffer, token);
                            token.ThrowIfCancellationRequested();
                            await fileStream.WriteAsync(buffer.Slice(0, read), token);
                            totalRead += read;
                            if (progress is not null)
                            {
                                var percent = 0f;
                                progress.ReadBytes = totalRead;
                                if (progress.TotalBytes != 0)
                                {
                                    percent = (float)progress.ReadBytes / progress.TotalBytes;
                                }
                                percent = Mathf.Clamp01(percent);
                                progress.Report(percent);
                            }
                        }
                        while (read > 0);
                        if (totalRead < 10)
                        {
                            continue;
                        }
                        File.Create(cacheFlagPath).Dispose();
                        break;
                    }
                    catch (InternalHttpRequestException)
                    {
                        throw;
                    }
                    catch (InvalidOperationException)
                    {
                        throw new InternalHttpRequestException(HttpErrorCode.InvalidRequest, null);
                    }
                    catch (OperationCanceledException)
                    {
                        if (token.IsCancellationRequested)
                        {
                            MajDebug.LogWarning($"Request for resource \"{uri}\" was canceled");
                            throw new InternalHttpRequestException(HttpErrorCode.Canceled, null);
                        }
                        else if (i == MajEnv.HTTP_REQUEST_MAX_RETRY)
                        {
                            MajDebug.LogError($"Failed to request resource: {uri}\nTimeout");
                            throw new InternalHttpRequestException(HttpErrorCode.Timeout, null);
                        }
                    }
                    catch (Exception e)
                    {
                        if (i == MajEnv.HTTP_REQUEST_MAX_RETRY)
                        {
                            MajDebug.LogError($"Failed to request resource: {uri}\n{e}");
                            throw new InternalHttpRequestException(HttpErrorCode.Unreachable, null);
                        }
                    }
                }
            }
            finally
            {
                Pool<byte>.ReturnArray(rentBuffer, true);
            }
        }
        class InternalHttpRequestException : Exception
        {
            public HttpErrorCode ErrorCode { get; init; }
            public HttpStatusCode? StatusCode { get; init; }
            public InternalHttpRequestException(HttpErrorCode errorCode, HttpStatusCode? statusCode) : base()
            {
                ErrorCode = errorCode;
                StatusCode = statusCode;
            }
        }
        enum HttpErrorCode
        {
            Unreachable,
            InvalidRequest,
            Unsuccessful,
            Timeout,
            Canceled
        }
    }
}
