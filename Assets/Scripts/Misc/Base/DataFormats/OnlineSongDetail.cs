using Cysharp.Text;
using Cysharp.Threading.Tasks;
using MajdataPlay.Buffers;
using MajdataPlay.IO;
using MajdataPlay.Net;
using MajdataPlay.Numerics;
using MajdataPlay.Utils;
using MajSimai;
using NeoSmart.AsyncLock;
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
        WeakReference<AudioSampleWrap> _audioTrackRef = new(null!);
        WeakReference<AudioSampleWrap> _previewAudioTrackRef = new(null!);
        WeakReference<Sprite> _coverRef = new(null!);
        WeakReference<Sprite> _fullSizeCoverRef = new(null!);
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

            if (!Directory.Exists(_cachePath))
            {
                Directory.CreateDirectory(_cachePath);
            }
        }

        #region Public
        public async ValueTask PreloadAsync(INetProgress? progress = null, CancellationToken token = default)
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
            await Task.WhenAll(GetMaidataAsync(token: token).AsTask(), GetCompressedCoverAsync(false, progress, token).AsTask());
            _isPreloaded = true;
        }
        public async ValueTask<AudioSampleWrap> GetPreviewAudioTrackAsync(INetProgress? progress = null, CancellationToken token = default)
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
                    if (_audioTrackRef.TryGetTarget(out var audioTrack) || _previewAudioTrackRef.TryGetTarget(out audioTrack))
                    {
                        return audioTrack;
                    }
                    var cacheFlagPath = Path.Combine(_cachePath, "track.cache");
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
#if (ENABLE_IL2CPP || MAJDATA_IL2CPP_DEBUG)
                            await using(UniTask.ReturnToCurrentSynchronizationContext())
                            {
                                await UniTask.SwitchToMainThread();
                                using var getReq = UnityWebRequestFactory.Head(_videoUri);
                                var asyncOperation = getReq.SendWebRequest();

                                while (!asyncOperation.isDone)
                                {
                                    if (token.IsCancellationRequested)
                                    {
                                        getReq.Abort();
                                        throw new HttpException(_coverUri.OriginalString, HttpErrorCode.Canceled);
                                    }
                                    await UniTask.Yield();
                                }
                                if(getReq.result is (UnityWebRequest.Result.Success or UnityWebRequest.Result.ProtocolError))
                                {
                                    if(getReq.responseCode != (long)HttpStatusCode.OK)
                                    {
                                        using var _ = File.Create(cacheFlagPath);
                                        _videoPath = string.Empty;
                                        return _videoPath;
                                    }
                                    else if(getReq.responseCode == (long)HttpStatusCode.NotFound)
                                    {
                                        break;
                                    }
                                }
                            }
#else
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
#endif
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
                    var waiting4LockTask = _audioTrackLock.LockAsync(token);
                    await Task.WhenAny(waiting4LockTask, Task.Delay(Timeout.Infinite, token));
                    var @lock = waiting4LockTask.Result;
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
                    var waiting4LockTask = _coverLock.LockAsync(token);
                    await Task.WhenAny(waiting4LockTask, Task.Delay(Timeout.Infinite, token));
                    var @lock = waiting4LockTask.Result;
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
                                cover = await SpriteLoader.LoadAsync(savePath, token);
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
                            if (e.ErrorCode is HttpErrorCode.Unsuccessful)
                            {
                                using var _ = File.Create(cacheFlagPath);
                            }
                            _coverRef.SetTarget(MajEnv.EmptySongCover);
                            return MajEnv.EmptySongCover;
                        }
                        finally
                        {
                            progress?.Report(1);
                        }

                        token.ThrowIfCancellationRequested();
                        cover = await SpriteLoader.LoadAsync(savePath, token);

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
                var waiting4LockTask = _fullSizeCoverLock.LockAsync(token);
                await Task.WhenAny(waiting4LockTask, Task.Delay(Timeout.Infinite, token));
                var @lock = waiting4LockTask.Result;
                using (@lock)
                {
                    token.ThrowIfCancellationRequested();
                    if (_fullSizeCoverRef.TryGetTarget(out var cover) && await cover.IsNativeAliveAsync())
                    {
                        return cover;
                    }
                    var savePath = Path.Combine(_cachePath, "bg_fullSize.jpg");
                    var cacheFlagPath = Path.Combine(_cachePath, $"bg_fullSize.jpg.cache");

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
                            cover = await SpriteLoader.LoadAsync(savePath, token);
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
                        if (e.ErrorCode is HttpErrorCode.Unsuccessful)
                        {
                            using var _ = File.Create(cacheFlagPath);
                        }
                        _fullSizeCoverRef.SetTarget(MajEnv.EmptySongCover);
                        return MajEnv.EmptySongCover;
                    }
                    finally
                    {
                        progress?.Report(1);
                    }
                    token.ThrowIfCancellationRequested();
                    cover = await SpriteLoader.LoadAsync(savePath, token);

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

#if ENABLE_IL2CPP || MAJDATA_IL2CPP_DEBUG
        async Task DownloadFile(Uri uri, string savePath, bool forceReDl = false, INetProgress? progress = null, CancellationToken token = default)
        {
            var fileInfo = new FileInfo(savePath);
            var cacheFlagPath = Path.Combine(fileInfo.Directory.FullName, $"{fileInfo.Name}.cache");
            var hashFlagPath = Path.Combine(fileInfo.Directory.FullName, $"{fileInfo.Name}.sha256");
            var fileSHA256 = (string?)null;
            if (File.Exists(cacheFlagPath))
            {
                if (!forceReDl)
                {
                    return;
                }
                else
                {
                    File.Delete(cacheFlagPath);
                    if(fileInfo.Exists)
                    {
                        fileInfo.Delete();
                    }
                }
            }

            for (var i = 0; i <= MajEnv.HTTP_REQUEST_MAX_RETRY; i++)
            {
                try
                {
                    await UniTask.SwitchToMainThread();
                    using var request = UnityWebRequestFactory.Get(uri);
                    var asyncOperation = request.SendWebRequest();

                    while (!asyncOperation.isDone)
                    {
                        if (token.IsCancellationRequested)
                        {
                            request.Abort();
                            throw new HttpException(uri.OriginalString, HttpErrorCode.Canceled);
                        }
                        progress?.Report(asyncOperation.progress);
                        await UniTask.Yield();
                    }
                    request.EnsureSuccessStatusCode();
                    if (File.Exists(hashFlagPath))
                    {
                        fileSHA256 = await File.ReadAllTextAsync(hashFlagPath);
                    }
                    else
                    {
                        var header = request.GetResponseHeader("Hash") ?? request.GetResponseHeader("hash");
                        if (!string.IsNullOrEmpty(header))
                        {
                            var hash = header;
                            fileSHA256 = hash;
                            await File.WriteAllTextAsync(hashFlagPath, fileSHA256);
                        }
                    }
                    using var fileStream = File.Create(savePath);
                    var nativeData = request.downloadHandler.nativeData;
                    await UniTask.SwitchToThreadPool();
                    var fileLen = nativeData.Length;
                    var buffer = Pool<byte>.RentArray(fileLen.Clamp(0, 512 * 1024)); // 512KB
                    var totalRead = 0;
                    try
                    {
                        while(true)
                        {
                            var remaining = fileLen - totalRead;
                            var read = 0;
                            if (remaining == 0)
                            {
                                break;
                            }

                            if (remaining > buffer.Length)
                            {
                                nativeData.AsReadOnlySpan().Slice(totalRead, buffer.Length).CopyTo(buffer);
                                read = buffer.Length;
                            }
                            else
                            {
                                nativeData.AsReadOnlySpan().Slice(totalRead, remaining).CopyTo(buffer);
                                read = remaining;
                            }
                            await fileStream.WriteAsync(buffer.AsMemory(0, read));
                            totalRead += read;
                        }
                    }
                    finally
                    {
                        Pool<byte>.ReturnArray(buffer);
                    }
                    if (string.IsNullOrEmpty(fileSHA256))
                    {
                        if (totalRead < 10)
                        {
                            continue;
                        }
                    }
                    else
                    {
                        fileStream.Position = 0;
                        var currentHashBytes = SHA256.Create().ComputeHash(fileStream);
                        var currentHash = Convert.ToBase64String(currentHashBytes);
                        if (fileSHA256 != currentHash)
                        {
                            MajDebug.LogWarning($"Hash mismatch for online resource\nOrigin: {fileSHA256}\nLocal: {currentHash}");
                            if (i == MajEnv.HTTP_REQUEST_MAX_RETRY)
                            {
                                throw new HttpException(uri.OriginalString, HttpErrorCode.IntegrityCheckFailed);
                            }
                            else
                            {
                                continue;
                            }
                        }
                    }
                    File.Create(cacheFlagPath).Dispose();
                    break;
                }
                catch (Exception e)
                {
                    if (i == MajEnv.HTTP_REQUEST_MAX_RETRY)
                    {
                        MajDebug.LogError($"Failed to request resource: {uri}\n{e}");
                        throw new HttpException(uri.OriginalString, HttpErrorCode.Unreachable);
                    }
                }
            }
        }
#else
        async Task DownloadFile(Uri uri, string savePath, bool forceReDl = false, INetProgress? progress = null, CancellationToken token = default)
        {
            var bufferSize = MajEnv.HTTP_BUFFER_SIZE;
            var fileInfo = new FileInfo(savePath);
            var httpClient = MajEnv.SharedHttpClient;
            var rentBuffer = Pool<byte>.RentArray(bufferSize, true);
            var buffer = rentBuffer.AsMemory();
            var cacheFlagPath = Path.Combine(fileInfo.Directory.FullName, $"{fileInfo.Name}.cache");
            var hashFlagPath = Path.Combine(fileInfo.Directory.FullName, $"{fileInfo.Name}.sha256");
            var fileSHA256 = (string?)null;

            try
            {
                for (var i = 0; i <= MajEnv.HTTP_REQUEST_MAX_RETRY; i++)
                {
                    try
                    {
                        if (File.Exists(cacheFlagPath))
                        {
                            if (!forceReDl)
                            {
                                return;
                            }
                            else
                            {
                                File.Delete(cacheFlagPath);
                                if(fileInfo.Exists)
                                {
                                    fileInfo.Delete();
                                }
                            }
                        }
                        using var rsp = await httpClient.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, token);
                        if (!rsp.IsSuccessStatusCode)
                        {
                            throw new HttpException(uri.OriginalString, HttpErrorCode.Unsuccessful, rsp.StatusCode);
                        }
                        token.ThrowIfCancellationRequested();
                        MajDebug.LogInfo($"Received http response header from: {uri}");

                        if (!rsp.IsSuccessStatusCode) throw new Exception($"HTTP Req Failed: {uri}");

                        if (progress is not null)
                        {
                            progress.TotalBytes = rsp.Content.Headers.ContentLength ?? 0;
                        }
                        if(File.Exists(hashFlagPath))
                        {
                            fileSHA256 = await File.ReadAllTextAsync(hashFlagPath);
                        }
                        else
                        {
                            if (rsp.Headers.TryGetValues("hash", out var values) || rsp.Headers.TryGetValues("Hash", out values))
                            {
                                var hash = values.FirstOrDefault();
                                if (!string.IsNullOrEmpty(hash))
                                {
                                    fileSHA256 = hash;
                                    await File.WriteAllTextAsync(hashFlagPath, fileSHA256);
                                }
                            }
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
                        if(string.IsNullOrEmpty(fileSHA256))
                        {
                            if (totalRead < 10)
                            {
                                continue;
                            }
                        }
                        else
                        {
                            fileStream.Position = 0;
                            var currentHashBytes = SHA256.Create().ComputeHash(fileStream);
                            var currentHash = Convert.ToBase64String(currentHashBytes);
                            if (fileSHA256 != currentHash)
                            {
                                MajDebug.LogWarning($"Hash mismatch for online resource\nOrigin: {fileSHA256}\nLocal: {currentHash}");
                                if (i == MajEnv.HTTP_REQUEST_MAX_RETRY)
                                {
                                    throw new HttpException(uri.OriginalString, HttpErrorCode.IntegrityCheckFailed);
                                }
                                else
                                {
                                    continue;
                                }
                            }
                        }
                        
                        File.Create(cacheFlagPath).Dispose();
                        break;
                    }
                    catch (HttpException)
                    {
                        throw;
                    }
                    catch (InvalidOperationException)
                    {
                        throw new HttpException(uri.OriginalString, HttpErrorCode.InvalidRequest);
                    }
                    catch (OperationCanceledException)
                    {
                        if (token.IsCancellationRequested)
                        {
                            MajDebug.LogWarning($"Request for resource \"{uri}\" was canceled");
                            throw new HttpException(uri.OriginalString, HttpErrorCode.Canceled);
                        }
                        else if (i == MajEnv.HTTP_REQUEST_MAX_RETRY)
                        {
                            MajDebug.LogError($"Failed to request resource: {uri}\nTimeout");
                            throw new HttpException(uri.OriginalString, HttpErrorCode.Timeout);
                        }
                    }
                    catch (Exception e)
                    {
                        if (i == MajEnv.HTTP_REQUEST_MAX_RETRY)
                        {
                            MajDebug.LogError($"Failed to request resource: {uri}\n{e}");
                            throw new HttpException(uri.OriginalString, HttpErrorCode.Unreachable);
                        }
                    }
                }
            }
            finally
            {
                Pool<byte>.ReturnArray(rentBuffer, true);
            }
        }
#endif
    }
}
