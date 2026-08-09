using Cysharp.Text;
using Cysharp.Threading.Tasks;
using MajdataPlay.Buffers;
using MajdataPlay.IO;
using MajdataPlay.Net;
using MajdataPlay.Numerics;
using MajdataPlay.Utils;
using MajdataPlay.Drawing;
using MajSimai;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using MajdataPlay.Settings;
using Nito.AsyncEx;
using MajdataPlay.Diagnostics;
using System.Diagnostics.CodeAnalysis;

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

        public bool IsVideoLoaded { get => _videoPath != null; }
        public bool IsCoverLoaded { get => _fullSizeCoverRef.TryGetTarget(out _); }
        public bool IsCompressedCoverLoaded { get => _coverRef.TryGetTarget(out _); }
        public bool IsAudioTrackLoaded { get => _audioTrackRef.TryGetTarget(out _); }
        public bool IsPreviewAudioTrackLoaded { get => _previewAudioTrackRef.TryGetTarget(out _); }
        public bool IsMaidataLoaded { get => _maidata != null; }

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

        bool _isDisposed = false;
        bool _isPreloaded = false;

        string? _videoPath = null;
        WeakReference<AudioSampleWrap> _audioTrackRef = new(null!);
        WeakReference<AudioSampleWrap> _previewAudioTrackRef = new(null!);
        WeakReference<Sprite> _coverRef = new(null!);
        WeakReference<Sprite> _fullSizeCoverRef = new(null!);
        SimaiFile? _maidata = null;

        volatile int _preloadJoinState = 0;

        readonly AsyncLock _previewAudioTrackLock = new();
        readonly AsyncLock _audioTrackLock = new();
        readonly AsyncLock _videoPathLock = new();
        readonly AsyncLock _coverLock = new();
        readonly AsyncLock _fullSizeCoverLock = new();
        readonly AsyncLock _maidataLock = new();

        ~OnlineSongDetail()
        {
            Dispose(false);
        }
        public OnlineSongDetail(ApiEndpoint serverInfo, MajnetSongDetail songDetail)
        {
            var apiroot = serverInfo.Url.Combine($"maichart/{songDetail.Id}/");
            
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
            _maidataUri = apiroot.Combine("chart");
            _trackUri = apiroot.Combine("track");
            _fullSizeCoverUri = apiroot.Combine("image?fullimage=true");
            _videoUri = apiroot.Combine("video");
            _coverUri = apiroot.Combine("image");

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
        }

        #region Public
        public async ValueTask PreloadAsync(INetProgress? progress = null, CancellationToken token = default)
        {
            ThrowIfDisposed();
            if (Interlocked.CompareExchange(ref _preloadJoinState, 1, 0) != 0)
            {
                return;
            }
            try
            {
                if (_isPreloaded)
                {
                    return;
                }
                await UniTask.SwitchToThreadPool();
                await Task.WhenAll(GetMaidataAsync(token: token).AsTask(), GetCompressedCoverAsync(false, progress, token).AsTask());
                _isPreloaded = true;
            }
            finally
            {
                _preloadJoinState = 0;
            }
        }
        public async ValueTask<AudioSampleWrap> GetPreviewAudioTrackAsync(INetProgress? progress = null, CancellationToken token = default)
        {
            ThrowIfDisposed();
            try
            {
                await UniTask.SwitchToThreadPool();
                var @lock = await _previewAudioTrackLock.LockAsync(token);
                using (@lock)
                {
                    token.ThrowIfCancellationRequested();
                    if (_audioTrackRef.TryGetTarget(out var audioTrack) || _previewAudioTrackRef.TryGetTarget(out audioTrack))
                    {
                        return audioTrack;
                    }

                    var audioManager = MajInstances.AudioManager;
                    var sample = await audioManager.LoadMusicFromUriAsync(_trackUri);

                    _previewAudioTrackRef.SetTarget(sample);

                    return sample;
                }
            }
            catch (Exception e)
            {
                MajDebug.LogException(e);
                throw;
            }
        }
        public ValueTask<AudioSampleWrap> GetAudioTrackAsync(INetProgress? progress = null, CancellationToken token = default)
        {
            return GetAudioTrackAsync(true, progress, token);
        }
        public async ValueTask<string> GetVideoPathAsync(INetProgress? progress = null, CancellationToken token = default)
        {
            ThrowIfDisposed();
            try
            {
                if (_videoPath is not null)
                {
                    return _videoPath;
                }
                await UniTask.SwitchToThreadPool();
                var @lock = await _videoPathLock.LockAsync(token);
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

                            if (rsp.StatusCode == HttpStatusCode.NotFound)
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
                    await DownloadFile(_videoUri, savePath, false, progress, token);
                    progress?.Report(1);
                    _videoPath = savePath;
                    return _videoPath;
                }
            }
            catch (Exception e)
            {
                if (e is not OperationCanceledException)
                {
                    MajDebug.LogException(e);
                }
                throw;
            }
        }
        public ValueTask<Sprite> GetCoverAsync(bool isCompressed, INetProgress? progress = null, CancellationToken token = default)
        {
            ThrowIfDisposed();
            if (isCompressed)
            {
                return GetCompressedCoverAsync(true, progress, token);
            }
            else
            {
                return GetFullSizeCoverAsync(true, progress, token);
            }
        }
        public async ValueTask<SimaiFile> GetMaidataAsync(bool ignoreCache = false, INetProgress? progress = null, CancellationToken token = default)
        {
            ThrowIfDisposed();
            if (!ignoreCache && _maidata is not null)
            {
                return _maidata;
            }
            try
            {
                await UniTask.SwitchToThreadPool();
                var @lock = await _maidataLock.LockAsync(token);
                using (@lock)
                {
                    token.ThrowIfCancellationRequested();
                    var savePath = Path.Combine(_cachePath, "maidata.txt");
                    var metadata = default(SimaiMetadata);
                    var forceReDl = false;

                    for (var i = 0; i <= MajEnv.HTTP_REQUEST_MAX_RETRY; i++)
                    {
                        await DownloadFile(_maidataUri, savePath, forceReDl, progress, token);
                        progress?.Report(1);
                        using var fileStream = File.OpenRead(savePath);
                        metadata = await SimaiParser.ParseMetadataAsync(fileStream);

                        if (metadata.Hash != Hash)
                        {
                            MajDebug.LogWarning($"Hash mismatch for maidata of {Id}, re-download");
                            forceReDl = true;
                            continue;
                        }
                        else
                        {
                            break;
                        }
                    }
                    if (metadata.Hash != Hash)
                    {
                        throw new HttpException(_maidataUri.OriginalString, HttpErrorCode.Unsuccessful);
                    }
                    _maidata = await SimaiParser.ParseAsync(File.OpenRead(savePath));

                    return _maidata;
                }
            }
            catch (HttpException ex)
            {
                MajDebug.LogException(ex);
                throw;
            }
            catch (Exception e)
            {
                if (e is not OperationCanceledException)
                {
                    MajDebug.LogException(e);
                }
                _maidata = null;
                throw new Exception("Maidata Load Failed");
            }
        }
        public void Dispose()
        {
            Dispose(true);
        }
        void Dispose(bool disposing)
        {
            if (_isDisposed)
            {
                return;
            }
            _isDisposed = true;
            if (_audioTrackRef.TryGetTarget(out var audioTrack))
            {
                audioTrack.Dispose();
            }
            if (_previewAudioTrackRef.TryGetTarget(out audioTrack))
            {
                audioTrack.Dispose();
            }
            UniTask.Post(() =>
            {
                if (_coverRef.TryGetTarget(out var cover) && cover.IsNativeAlive())
                {
                    var tex = cover.texture;
                    GameObject.DestroyImmediate(cover, true);
                    GameObject.DestroyImmediate(tex, true);
                }
                if (_fullSizeCoverRef.TryGetTarget(out cover) && cover.IsNativeAlive())
                {
                    var tex = cover.texture;
                    GameObject.DestroyImmediate(cover, true);
                    GameObject.DestroyImmediate(tex, true);
                }
            });
            _maidata = null;
            if(disposing)
            {
                _audioTrackRef.SetTarget(null!);
                _previewAudioTrackRef.SetTarget(null!);
                _coverRef.SetTarget(null!);
                _fullSizeCoverRef.SetTarget(null!);
            }
            _videoPath = null;
        }
        public async ValueTask DisposeAsync()
        {
            if (_isDisposed)
            {
                return;
            }
            _isDisposed = true;
            if (_audioTrackRef.TryGetTarget(out var audioTrack))
            {
                await audioTrack.DisposeAsync();
            }
            if (_previewAudioTrackRef.TryGetTarget(out audioTrack))
            {
                await audioTrack.DisposeAsync();
            }
            await using (UniTask.ReturnToCurrentSynchronizationContext())
            {
                await UniTask.SwitchToMainThread();
                if (_coverRef.TryGetTarget(out var cover) && cover.IsNativeAlive())
                {
                    var tex = cover.texture;
                    GameObject.DestroyImmediate(cover, true);
                    GameObject.DestroyImmediate(tex, true);
                }
                if (_fullSizeCoverRef.TryGetTarget(out cover) && cover.IsNativeAlive())
                {
                    var tex = cover.texture;
                    GameObject.DestroyImmediate(cover, true);
                    GameObject.DestroyImmediate(tex, true);
                }
            }
            _maidata = null;
            _audioTrackRef.SetTarget(null!);
            _previewAudioTrackRef.SetTarget(null!);
            _coverRef.SetTarget(null!);
            _fullSizeCoverRef.SetTarget(null!);
            _videoPath = null;
        }
        //public async ValueTask UnloadUnityAssetsAsync(CancellationToken token = default)
        //{
        //    await using (UniTask.ReturnToCurrentSynchronizationContext())
        //    {
        //        var waiting4LockTask = _coverLock.LockAsync(token);
        //        await Task.WhenAny(waiting4LockTask, Task.Delay(Timeout.Infinite, token));
        //        var @lock = waiting4LockTask.Result;
        //        using (@lock)
        //        {
        //            if(_cover is not null)
        //            {
        //                var texture = _cover.texture;
        //                UnityEngine.Object.DestroyImmediate(_cover, true);
        //                UnityEngine.Object.DestroyImmediate(texture, true);
        //                _cover = null;
        //            }
        //        }
        //        token.ThrowIfCancellationRequested();
        //        var waiting4LockTask2 = _fullSizeCoverLock.LockAsync(token);
        //        await Task.WhenAny(waiting4LockTask2, Task.Delay(Timeout.Infinite, token));
        //        var @lock2 = waiting4LockTask2.Result;
        //        using (@lock2)
        //        {
        //            if (_fullSizeCover is not null)
        //            {
        //                var texture = _fullSizeCover.texture;
        //                UnityEngine.Object.DestroyImmediate(_fullSizeCover, true);
        //                UnityEngine.Object.DestroyImmediate(texture, true);
        //                _fullSizeCover = null;
        //            }
        //        }
        //    }
        //}
        #endregion

        async ValueTask<AudioSampleWrap> GetAudioTrackAsync(bool loadIntoMemory, INetProgress? progress = null, CancellationToken token = default)
        {
            ThrowIfDisposed();
            await using (UniTask.ReturnToCurrentSynchronizationContext())
            {
                try
                {
                    await UniTask.SwitchToThreadPool();
                    var @lock = await _audioTrackLock.LockAsync(token);
                    using (@lock)
                    {
                        token.ThrowIfCancellationRequested();
                        if (_audioTrackRef.TryGetTarget(out var audioTrack))
                        {
                            return audioTrack;
                        }
                        var savePath = Path.Combine(_cachePath, "track.mp3");
                        var cacheFlagPath = Path.Combine(_cachePath, "track.cache");

                        if (!File.Exists(cacheFlagPath))
                        {
                            await DownloadFile(_trackUri, savePath, false, progress, token);
                        }
                        progress?.Report(1);
                        if (!loadIntoMemory)
                        {
                            return AudioSampleWrap.Empty;
                        }
                        var sampleWarp = await MajInstances.AudioManager.LoadMusicAsync(savePath, true, true);
                        if (sampleWarp.IsEmpty)
                        {
                            if (File.Exists(cacheFlagPath))
                            {
                                File.Delete(cacheFlagPath);
                            }
                            await DownloadFile(_trackUri, savePath, false, progress, token);
                        }
                        _audioTrackRef.SetTarget(sampleWarp);

                        return sampleWarp;
                    }
                }
                catch (Exception e)
                {
                    _audioTrackRef.SetTarget(null!);
                    if (e is not OperationCanceledException)
                    {
                        MajDebug.LogException(e);
                        throw e;
                    }
                    
                    throw new InvalidAudioTrackException("Music track Load Failed", Path.Combine(_cachePath, "track.mp3"));
                }
            }
        }
        async ValueTask<Sprite> GetCompressedCoverAsync(bool loadIntoMemory, INetProgress? progress = null, CancellationToken token = default)
        {
            await using (UniTask.ReturnToCurrentSynchronizationContext())
            {
                try
                {
                    await UniTask.SwitchToThreadPool();
                    var @lock = await _coverLock.LockAsync(token);
                    using (@lock)
                    {
                        token.ThrowIfCancellationRequested();
                        if (_coverRef.TryGetTarget(out var cover) && await cover.IsNativeAliveAsync())
                        {
                            return cover;
                        }
                        var savePath = Path.Combine(_cachePath, "bg.jpg");
                        var cacheFlagPath = Path.Combine(_cachePath, $"bg.jpg.cache");

                        if (File.Exists(cacheFlagPath))
                        {
                            if (!File.Exists(savePath))
                            {
                                _coverRef.SetTarget(MajEnv.EmptySongCover);
                            }
                            else if(!loadIntoMemory)
                            {
                                progress?.Report(1);
                                return MajEnv.EmptySongCover;
                            }
                            else
                            {
                                progress?.Report(1);
                                cover = await SpriteLoader.LoadFromFileAsync(savePath, true, token);
                            }
                            _coverRef.SetTarget(cover);
                            return cover;
                        }
                        try
                        {
                            await DownloadFile(_coverUri, savePath, false, progress, token);
                            if (!loadIntoMemory)
                            {
                                return MajEnv.EmptySongCover;
                            }
                        }
                        catch (HttpException e)
                        {
                            var @return = default(Sprite);
                            _fullSizeCoverRef.TryGetTarget(out @return);
                            @return ??= MajEnv.EmptySongCover;
                            if (e.ErrorCode is HttpErrorCode.Unsuccessful)
                            {
                                using var _ = File.Create(cacheFlagPath);
                                _coverRef.SetTarget(@return);
                            }
                            return @return;
                        }
                        finally
                        {
                            progress?.Report(1);
                        }

                        token.ThrowIfCancellationRequested();
                        cover = await SpriteLoader.LoadFromFileAsync(savePath, true, token);

                        _coverRef.SetTarget(cover);
                        return cover;
                    }
                }
                catch (Exception e)
                {
                    if (e is not OperationCanceledException)
                    {
                        MajDebug.LogException(e);
                    }
                    throw;
                }
            }  
        }
        async ValueTask<Sprite> GetFullSizeCoverAsync(bool loadIntoMemory, INetProgress? progress = null, CancellationToken token = default)
        {
            try
            {
                await UniTask.SwitchToThreadPool();
                var @lock = await _fullSizeCoverLock.LockAsync(token);
                using (@lock)
                {
                    token.ThrowIfCancellationRequested();
                    if (_fullSizeCoverRef.TryGetTarget(out var cover) && await cover.IsNativeAliveAsync())
                    {
                        return cover;
                    }
                    var savePath = Path.Combine(_cachePath, "bg_fullSize.jpg");
                    var cacheFlagPath = Path.Combine(_cachePath, "bg_fullSize.jpg.cache");

                    if (File.Exists(cacheFlagPath))
                    {
                        if (!File.Exists(savePath))
                        {
                            _fullSizeCoverRef.SetTarget(MajEnv.EmptySongCover);
                            return MajEnv.EmptySongCover;
                        }
                        else if (!loadIntoMemory)
                        {
                            progress?.Report(1);
                            return MajEnv.EmptySongCover;
                        }
                        else
                        {
                            progress?.Report(1);
                            cover = await SpriteLoader.LoadFromFileAsync(savePath, true, token);
                        }
                        _fullSizeCoverRef.SetTarget(cover);
                        return cover;
                    }
                    try
                    {
                        await DownloadFile(_fullSizeCoverUri, savePath, false, progress, token);
                        if (!loadIntoMemory)
                        {
                            return MajEnv.EmptySongCover;
                        }
                    }
                    catch (HttpException e)
                    {
                        var @return = default(Sprite);
                        _coverRef.TryGetTarget(out @return);
                        @return ??= MajEnv.EmptySongCover;
                        if (e.ErrorCode is HttpErrorCode.Unsuccessful)
                        {
                            using var _ = File.Create(cacheFlagPath);
                            _fullSizeCoverRef.SetTarget(@return);
                        }
                        return @return;
                    }
                    finally
                    {
                        progress?.Report(1);
                    }
                    token.ThrowIfCancellationRequested();
                    cover = await SpriteLoader.LoadFromFileAsync(savePath, true, token);

                    _fullSizeCoverRef.SetTarget(cover);
                    return cover;
                }
            }
            catch (Exception e)
            {
                if (e is not OperationCanceledException)
                {
                    MajDebug.LogException(e);
                }
                throw;
            }
        }
        
        void ThrowIfDisposed()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(OnlineSongDetail));
            }
        }
        private void EnsureCachePath()
        {
            if (!Directory.Exists(_cachePath))
            {
                Directory.CreateDirectory(_cachePath);
            }
        }


        async Task DownloadFile(DownloadOption options, CancellationToken token = default)
        {
            EnsureCachePath();

            var savePath = Path.Combine(options.SaveTo, options.Filename);
            var chunkPath = Path.Combine(options.SaveTo, $"{options.Filename}.chunk");
            var hashPath = Path.Combine(options.SaveTo, $"{options.Filename}.sha256");

            var saveFileStream = default(FileStream?);
            var chunkFileStream = default(FileStream?);

            var bufferSize = MajEnv.HTTP_BUFFER_SIZE;
            var httpClient = MajEnv.SharedHttpClient;
            var rentBuffer = Pool<byte>.RentArray(bufferSize, true);
            var buffer = rentBuffer.AsMemory();
            var fileSHA256 = (string?)null;

            if (options.ForceReDownload)
            {
                DeleteFileIfExists(savePath);
                DeleteFileIfExists(chunkPath);
                DeleteFileIfExists(hashPath);
                fileSHA256 = null;
            }
            else
            {
                if (File.Exists(hashPath))
                {
                    fileSHA256 = await File.ReadAllTextAsync(hashPath);
                }
                if (File.Exists(savePath))
                {
                    EnsureFileStreamIsOpened(chunkPath, ref saveFileStream);
                    var currentFileHash = GetHashFromStream(saveFileStream);
                    if (!string.IsNullOrEmpty(fileSHA256)) // hash metadata found
                    {
                        if(!CheckFileIntegrity(fileSHA256, currentFileHash)) // integrity check failed
                        {
                            MajDebug.LogDebug($"[{nameof(OnlineSongDetail)}] ReDownload online resource: {options.RequestedUri}");
                            await saveFileStream.DisposeAsync();
                            DeleteFileIfExists(savePath);
                            DeleteFileIfExists(hashPath);
                            fileSHA256 = null;
                        }
                        else
                        {
                            DeleteFileIfExists(chunkPath);
                            return;
                        }
                    }
                    else // hash metadata not found
                    {
                        DeleteFileIfExists(chunkPath);
                        return;
                    }
                }
                else if (File.Exists(chunkPath))
                {
                    EnsureFileStreamIsOpened(chunkPath, ref chunkFileStream);
                    var currentFileHash = GetHashFromStream(chunkFileStream);
                    if (!string.IsNullOrEmpty(fileSHA256)) // hash metadata found
                    {
                        if (!CheckFileIntegrity(fileSHA256, currentFileHash)) // integrity check failed
                        {
                            MajDebug.LogDebug($"[{nameof(OnlineSongDetail)}] ReDownload online resource: {options.RequestedUri}");
                            await chunkFileStream.DisposeAsync();
                            DeleteFileIfExists(chunkPath);
                            DeleteFileIfExists(hashPath);
                            fileSHA256 = null;
                        }
                        else
                        {
                            File.Move(chunkPath, savePath);
                            return;
                        }
                    }
                    else // hash metadata not found
                    {
                        DeleteFileIfExists(chunkPath);
                    }
                }
            }
            var requestUri = options.RequestedUri;
            var progress = options.Progress;
            try
            {
                EnsureFileStreamIsOpened(chunkPath, ref chunkFileStream);
                for (var i = 0; i <= MajEnv.HTTP_REQUEST_MAX_RETRY; i++)
                {
                    try
                    {
                        using var req = (i == 0 ? options.CurrentRequest : default) ?? new HttpRequestMessage(HttpMethod.Get, requestUri);
                        using var rsp = await httpClient.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, token);
                        if (!rsp.IsSuccessStatusCode)
                        {
                            throw new HttpException(requestUri.OriginalString, HttpErrorCode.Unsuccessful, rsp.StatusCode);
                        }
                        token.ThrowIfCancellationRequested();
                        MajDebug.LogDebug($"[{nameof(OnlineSongDetail)}] Received http response header from: {requestUri}");

                        if (progress is not null)
                        {
                            progress.TotalBytes = rsp.Content.Headers.ContentLength ?? 0;
                        }
                        if(string.IsNullOrEmpty(fileSHA256)) // write resource hash into disk
                        {
                            if (rsp.Headers.TryGetValues("hash", out var values) || rsp.Headers.TryGetValues("Hash", out values))
                            {
                                foreach(var hash in values)
                                {
                                    if (!string.IsNullOrEmpty(hash))
                                    {
                                        fileSHA256 = hash;
                                        await File.WriteAllTextAsync(hashPath, fileSHA256);
                                        break;
                                    }
                                }                                
                            }
                        }
                        using var httpStream = await rsp.Content.ReadAsStreamAsync();
                        var read = 0;
                        var totalRead = 0;
                        do
                        {
                            read = await httpStream.ReadAsync(buffer, token);
                            await chunkFileStream.WriteAsync(buffer.Slice(0, read), token);
                            totalRead += read;
                            if (progress is not null)
                            {
                                var percent = 0f;
                                progress.TransferredBytes = totalRead;
                                if (progress.TotalBytes != 0)
                                {
                                    percent = (float)progress.TransferredBytes / progress.TotalBytes;
                                }
                                percent = Mathf.Clamp01(percent);
                                progress.Report(percent);
                            }
                        }
                        while (read > 0);
                        await chunkFileStream.FlushAsync();

                        if(!string.IsNullOrEmpty(fileSHA256))
                        {
                            chunkFileStream.Position = 0;
                            var currentHash = GetHashFromStream(chunkFileStream);
                            if (!CheckFileIntegrity(fileSHA256, currentHash))
                            {
                                MajDebug.LogWarning($"[{nameof(OnlineSongDetail)}]Hash mismatch for online resource\nOrigin: {fileSHA256}\nLocal: {currentHash}");
                                if (i == MajEnv.HTTP_REQUEST_MAX_RETRY)
                                {
                                    throw new HttpException(requestUri.OriginalString, HttpErrorCode.IntegrityCheckFailed);
                                }
                                else
                                {
                                    continue;
                                }
                            }
                        }
                        break;
                    }
                    catch (HttpException e)
                    {
                        throw;
                    }
                    catch (InvalidOperationException e)
                    {
                        throw new HttpException(requestUri.OriginalString, HttpErrorCode.InvalidRequest);
                    }
                    catch (OperationCanceledException e)
                    {
                        if (token.IsCancellationRequested)
                        {
                            MajDebug.LogWarning($"Request for resource \"{requestUri}\" was canceled");
                            throw new HttpException(requestUri.OriginalString, HttpErrorCode.Canceled);
                        }
                        else if (i == MajEnv.HTTP_REQUEST_MAX_RETRY)
                        {
                            MajDebug.LogError($"Failed to request resource: {requestUri}\nTimeout");
                            throw new HttpException(requestUri.OriginalString, HttpErrorCode.Timeout);
                        }
                    }
                    catch (Exception e)
                    {
                        if (i == MajEnv.HTTP_REQUEST_MAX_RETRY)
                        {
                            MajDebug.LogError($"Failed to request resource: {requestUri}\n{e}");
                            throw new HttpException(requestUri.OriginalString, HttpErrorCode.Unreachable);
                        }
                    }
                }
            }
            finally
            {
                Pool<byte>.ReturnArray(rentBuffer, true);
            }
        }

        string GetHashFromStream(Stream? stream)
        {
            var currentHashBytes = SHA256.Create().ComputeHash(stream);
            return Convert.ToBase64String(currentHashBytes);
        }
        void DeleteFileIfExists(string path)
        {
            if(File.Exists(path))
            {
                File.Delete(path);
            }
        }

        bool CheckFileIntegrity(string targetHash, string currentHash)
        {
            if(targetHash != currentHash)
            {
                MajDebug.LogWarning($"[{nameof(OnlineSongDetail)}] Hash mismatch for online resource\nOrigin: {targetHash}\nLocal: {currentHash}");
                return false;
            }
            return true;
        }
        void EnsureFileStreamIsOpened(string filePath, [NotNull] ref FileStream? fileStream)
        {
            if(fileStream is null)
            {
                fileStream = File.Open(filePath, FileMode.OpenOrCreate);
            }
        }

        readonly struct DownloadOption
        {
            public Uri RequestedUri { get; init; }
            public string SaveTo { get; init; }
            /// <summary>
            /// e.g: maidata.txt
            /// </summary>
            public string Filename { get; init; }
            /// <summary>
            /// e.g: maidata
            /// </summary>
            public string Name { get; init; }
            /// <summary>
            /// e.g: .txt
            /// </summary>
            public string Extension { get; init; }
            public bool ForceReDownload { get; init; }
            public INetProgress? Progress { get; init; }
            public HttpRequestMessage? CurrentRequest { get; init; }
        }
    }
}
