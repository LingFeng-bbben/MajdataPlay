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
using Unity.VisualScripting.Antlr3.Runtime;
#nullable enable
namespace MajdataPlay.Types
{
    internal class OnlineSongDetail: ISongDetail
    {
        public string Id { get; init; }
        public string Title { get; init; }
        public string Artist { get; init; }
        public string Description { get; init; } = string.Empty;
        public string[] Designers { get; init; } = new string[7];
        public string[] Levels { get; init; }
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
        readonly string _maidataUriStr = string.Empty;
        readonly string _trackUriStr = string.Empty;
        readonly string _videoUriStr = string.Empty;
        readonly string _fullSizeCoverUriStr = string.Empty;
        readonly string _coverUriStr = string.Empty;

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

        readonly Func<Task> _preloadCallback;

        public OnlineSongDetail(ApiEndpoint serverInfo, MajnetSongDetail songDetail)
        {
            var apiroot = $"{serverInfo.Url}/maichart";

            Title = songDetail.Title;
            Artist = songDetail.Artist;
            Levels = songDetail.Levels;
            _maidataUriStr = $"{apiroot}/{songDetail.Id}/chart";
            _trackUriStr = $"{apiroot}/{songDetail.Id}/track";
            _fullSizeCoverUriStr = $"{apiroot}/{songDetail.Id}/image?fullimage=true";
            _videoUriStr = $"{apiroot}/{songDetail.Id}/video";
            _coverUriStr = $"{apiroot}/{songDetail.Id}/image";

            _maidataUri = new Uri(_maidataUriStr);
            _trackUri = new Uri(_trackUriStr);
            _fullSizeCoverUri = new Uri(_fullSizeCoverUriStr);
            _videoUri = new Uri(_videoUriStr);
            _coverUri = new Uri(_coverUriStr);

            Hash = songDetail.Hash;
            _hashHexStr = HashHelper.ToHexString(Convert.FromBase64String(Hash));
            _serverInfo = serverInfo;
            Id = songDetail.Id;
            Timestamp = songDetail.Timestamp;

            for (int i = 0; i < Designers.Length; i++)
            {
                Designers[i] = songDetail.Uploader + "@" + songDetail.Designer;
            }
            _cachePath = Path.Combine(MajEnv.CachePath, $"Net/{_serverInfo.Name}/{_hashHexStr}");
            if (!Directory.Exists(_cachePath))
            {
                Directory.CreateDirectory(_cachePath);
            }
            _preloadCallback = async () => { await UniTask.WhenAll(GetMaidataAsync(), GetCoverAsync(true)); };
        }

        public async UniTask<AudioSampleWrap> GetPreviewAudioTrackAsync(CancellationToken token = default)
        {
            try
            {
                using (await _previewAudioTrackLock.LockAsync(token))
                {
                    token.ThrowIfCancellationRequested();
                    if (_previewAudioTrack is not null)
                        return _previewAudioTrack;
                    var audioManager = MajInstances.AudioManager;
                    var sample = await audioManager.LoadMusicFromUriAsync(_trackUri);

                    _previewAudioTrack = sample;

                    return _previewAudioTrack;
                }
            }
            catch
            {
                throw;
            }
            finally
            {
                await UniTask.Yield();
            }
        }
        public async UniTask<AudioSampleWrap> GetAudioTrackAsync(CancellationToken token = default)
        {
            try
            {
                using (await _audioTrackLock.LockAsync(token))
                {
                    token.ThrowIfCancellationRequested();
                    if (_audioTrack is not null)
                        return _audioTrack;
                    var savePath = Path.Combine(_cachePath, "track.mp3");
                    var cacheFlagPath = Path.Combine(_cachePath, "track.cache");

                    await CheckAndDownloadFile(_trackUri, savePath, token);
                    var sampleWarp = await MajInstances.AudioManager.LoadMusicAsync(savePath, true);
                    if(sampleWarp.IsEmpty)
                    {
                        if(File.Exists(cacheFlagPath))
                        {
                            File.Delete(cacheFlagPath);
                        }
                        await CheckAndDownloadFile(_trackUri, savePath, token);
                    }
                    _audioTrack = sampleWarp;

                    return sampleWarp;
                }
            }
            finally
            {
                await UniTask.Yield();
            }
        }
        public async UniTask Preload(CancellationToken token = default)
        {
            try
            {
                if (!await _preloadLock.TryLockAsync(_preloadCallback, TimeSpan.Zero))
                    return;
            }
            finally
            {
                await UniTask.Yield();
            }
        }
        public async UniTask<string> GetVideoPathAsync(CancellationToken token = default)
        {
            try
            {
                using (await _videoPathLock.LockAsync(token))
                {
                    token.ThrowIfCancellationRequested();
                    var savePath = Path.Combine(_cachePath, "bg.mp4");

                    await CheckAndDownloadFile(_videoUri, savePath, token);

                    return savePath;
                }
            }
            finally
            {
                await UniTask.Yield();
            }
        }
        public async UniTask<Sprite> GetCoverAsync(bool isCompressed, CancellationToken token = default)
        {
            if(isCompressed)
            {
                return await GetCompressedCoverAsync(token);
            }
            else
            {
                return await GetFullSizeCoverAsync(token);
            }
        }
        public async UniTask<SimaiFile> GetMaidataAsync(bool ignoreCache = false, CancellationToken token = default)
        {
            if (!ignoreCache && _maidata is not null)
                return _maidata;
            try
            {
                using (await _maidataLock.LockAsync(token))
                {
                    token.ThrowIfCancellationRequested();
                    var savePath = Path.Combine(_cachePath, "maidata.txt");

                    await CheckAndDownloadFile(_maidataUri, savePath, token);

                    _maidata = await SimaiParser.Shared.ParseAsync(savePath);
                    return _maidata;
                }
            }
            finally
            {
                await UniTask.Yield();
            }
        }
        async UniTask<Sprite> GetCompressedCoverAsync(CancellationToken token = default)
        {
            try
            {
                using (await _coverLock.LockAsync(token))
                {
                    token.ThrowIfCancellationRequested();
                    if (_cover is not null)
                        return _cover;
                    var savePath = Path.Combine(_cachePath, "bg.jpg");

                    await CheckAndDownloadFile(_coverUri, savePath, token);
                    token.ThrowIfCancellationRequested();
                    _cover = await SpriteLoader.LoadAsync(savePath, token);

                    return _cover;
                }
            }
            finally
            {
                await UniTask.Yield();
            }   
        }
        async UniTask<Sprite> GetFullSizeCoverAsync(CancellationToken token = default)
        {
            try
            {
                using (await _fullSizeCoverLock.LockAsync(token))
                {
                    token.ThrowIfCancellationRequested();
                    if (_fullSizeCover is not null)
                        return _fullSizeCover;
                    var savePath = Path.Combine(_cachePath, "bg_fullSize.jpg");

                    await CheckAndDownloadFile(_fullSizeCoverUri, savePath, token);
                    token.ThrowIfCancellationRequested();
                    _fullSizeCover = await SpriteLoader.LoadAsync(savePath, token);

                    return _fullSizeCover;
                }
            }
            finally
            {
                await UniTask.Yield();
            }
        }
        async Task CheckAndDownloadFile(Uri uri, string savePath, CancellationToken token = default)
        {
            await Task.Run(async () =>
            {
                var bufferSize = MajEnv.HTTP_BUFFER_SIZE;
                using var bufferOwner = MemoryPool<byte>.Shared.Rent(bufferSize);
                var fileInfo = new FileInfo(savePath);
                var httpClient = MajEnv.SharedHttpClient;
                var buffer = bufferOwner.Memory;
                var cacheFlagPath = Path.Combine(fileInfo.Directory.FullName, $"{fileInfo.Name}.cache");

                for (int i = 0; i <= MajEnv.HTTP_REQUEST_MAX_RETRY; i++)
                {
                    try
                    {
                        if (File.Exists(cacheFlagPath))
                        {
                            return;
                        }
                        using var rsp = await httpClient.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, token);
                        token.ThrowIfCancellationRequested();
                        MajDebug.Log($"Received http response header from: {uri}");
                        
                        using var fileStream = File.Create(savePath);
                        using var httpStream = await rsp.Content.ReadAsStreamAsync();
                        int read = 0;
                        do
                        {
                            token.ThrowIfCancellationRequested();
                            read = await httpStream.ReadAsync(buffer, token);
                            token.ThrowIfCancellationRequested();
                            await fileStream.WriteAsync(buffer.Slice(0, read), token);
                        }
                        while (read > 0);
                        using (File.Create(cacheFlagPath))
                        break;
                    }
                    catch(OperationCanceledException)
                    {
                        MajDebug.LogWarning($"Request for resource \"{uri}\" was canceled");
                        throw;
                    }
                    catch(Exception e)
                    {
                        if (i == MajEnv.HTTP_REQUEST_MAX_RETRY)
                        {
                            MajDebug.LogError($"Failed to request resource: {uri}\n{e}");
                            throw;
                        }
                    }
                }
            });
        }
    }
}
