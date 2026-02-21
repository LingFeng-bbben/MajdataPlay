using Cysharp.Threading.Tasks;
using MajdataPlay.IO;
using MajdataPlay.Net;
using MajdataPlay.Utils;
using MajSimai;
using NeoSmart.AsyncLock;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
#nullable enable
namespace MajdataPlay
{
    internal class SongDetail : ISongDetail, IDisposable
    {
        public string Title { get; init; } = string.Empty;
        public string Artist { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public ReadOnlySpan<string> Designers
        {
            get
            {
                return _simaiMetadata.Designers;
            }
        }
        public ReadOnlySpan<string> Levels
        {
            get
            {
                return _simaiMetadata.Levels;
            }
        }
        public string Hash 
        { 
            get => _simaiMetadata.Hash; 
        }
        public DateTime Timestamp { get; init; }
        public ChartStorageLocation Location => ChartStorageLocation.Local;

        readonly string _maidataPath = string.Empty;
        readonly string _trackPath = string.Empty;
        readonly string _videoPath = string.Empty;
        readonly string _coverPath = string.Empty;

        static readonly Action _emptyCallback = () => { };
        bool _isPreloaded = false;
        bool _isDisposed = false;

        WeakReference<AudioSampleWrap> _audioTrackRef = new(null!);
        WeakReference<AudioSampleWrap> _previewAudioTrackRef = new(null!);
        WeakReference<Sprite> _coverRef = new(null!);
        SimaiFile? _maidata = null;
        SimaiMetadata _simaiMetadata;

        readonly bool _isEmptyCover = false;
        readonly AsyncLock _previewAudioTrackLock = new();
        readonly AsyncLock _audioTrackLock = new();
        readonly AsyncLock _coverLock = new();
        readonly AsyncLock _maidataLock = new();
        readonly AsyncLock _preloadLock = new();

        ~SongDetail()
        {
            Dispose(false);
        }
        public SongDetail(string chartFolder, SimaiMetadata metadata)
        {
            var files = new DirectoryInfo(chartFolder).GetFiles();

            _maidataPath = Path.Combine(chartFolder, "maidata.txt");
            _trackPath = files.FirstOrDefault(o => o.Name.ToLower() is "track.opus" or "track.mp3" or "track.ogg" or "track.aac").FullName;
            _videoPath = files.FirstOrDefault(o => o.Name.ToLower() is "bg.mp4" or "pv.mp4" or "mv.mp4")?.FullName ?? string.Empty;
            _coverPath = files.FirstOrDefault(o => o.Name.ToLower() is "bg.png" or "bg.jpg")?.FullName ?? string.Empty;
            _maidata = null;

            if (string.IsNullOrEmpty(_coverPath))
            {
                _isEmptyCover = true;
            }
            _simaiMetadata = metadata;
            Title = metadata.Title;
            Artist = metadata.Artist;
            Timestamp = files.FirstOrDefault(x => x.Name is "maidata.txt")?.LastWriteTime ?? DateTime.UnixEpoch;
        }
        public static async Task<SongDetail> ParseAsync(string chartFolder)
        {
            var maidataPath = Path.Combine(chartFolder, "maidata.txt");
            var metadata = await SimaiParser.ParseMetadataAsync(File.OpenRead(maidataPath));

            return new SongDetail(chartFolder, metadata);
        }
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
            await Task.WhenAll(GetMaidataAsync(token: token).AsTask(), GetCoverAsync(true, token: token).AsTask());
            _isPreloaded = true;
        }
        public ValueTask<string> GetVideoPathAsync(INetProgress? progress = null, CancellationToken token = default)
        {
            ThrowIfDisposed();
            return UniTask.FromResult(_videoPath);
        }
        public async ValueTask<Sprite> GetCoverAsync(bool isCompressed, INetProgress? progress = null, CancellationToken token = default)
        {
            ThrowIfDisposed();
            if(_isEmptyCover)
            {
                return MajEnv.EmptySongCover;
            }
            if (_coverRef.TryGetTarget(out var cover) && await cover.IsNativeAliveAsync())
            {
                return cover;
            }
            await using (UniTask.ReturnToCurrentSynchronizationContext())
            {
                await UniTask.SwitchToThreadPool();
                using (await _coverLock.LockAsync(token))
                {
                    token.ThrowIfCancellationRequested();
                    if (_coverRef.TryGetTarget(out cover) && await cover.IsNativeAliveAsync())
                    {
                        return cover;
                    }
                    progress?.Report(1);
                    cover = await SpriteLoader.LoadAsync(_coverPath, token);
                    _coverRef.SetTarget(cover);
                    return cover;
                }
            }
        }
        public async ValueTask<AudioSampleWrap> GetAudioTrackAsync(INetProgress? progress = null, CancellationToken token = default)
        {
            ThrowIfDisposed();
            if (_audioTrackRef.TryGetTarget(out var audioTrack))
            {
                return audioTrack;
            }
            await using (UniTask.ReturnToCurrentSynchronizationContext())
            {
                await UniTask.SwitchToThreadPool();
                using (await _audioTrackLock.LockAsync(token))
                {
                    token.ThrowIfCancellationRequested();
                    if (_audioTrackRef.TryGetTarget(out audioTrack))
                    {
                        return audioTrack;
                    }
                    progress?.Report(1);
                    audioTrack = await MajInstances.AudioManager.LoadMusicAsync(_trackPath, true, true);
                    _audioTrackRef.SetTarget(audioTrack);
                    return audioTrack;
                }
            }
                
        }
        public async ValueTask<AudioSampleWrap> GetPreviewAudioTrackAsync(INetProgress? progress = null, CancellationToken token = default)
        {
            ThrowIfDisposed();
            if (_previewAudioTrackRef.TryGetTarget(out var audioTrack))
            {
                return audioTrack;
            }
            await UniTask.SwitchToThreadPool();
            using (await _previewAudioTrackLock.LockAsync(token))
            {
                token.ThrowIfCancellationRequested();
                if (_previewAudioTrackRef.TryGetTarget(out audioTrack))
                {
                    return audioTrack;
                }
                progress?.Report(1);
                audioTrack = await MajInstances.AudioManager.LoadMusicAsync(_trackPath, true, false);
                _previewAudioTrackRef.SetTarget(audioTrack);
                return audioTrack;
            }
        }
        public async ValueTask<SimaiFile> GetMaidataAsync(bool ignoreCache = false, INetProgress? progress = null, CancellationToken token = default)
        {
            ThrowIfDisposed();
            if (!ignoreCache && _maidata is not null)
            {
                return _maidata;
            }
            await UniTask.SwitchToThreadPool();
            using (await _maidataLock.LockAsync(token))
            {
                token.ThrowIfCancellationRequested();
                if (!ignoreCache && _maidata is not null)
                {
                    return _maidata;
                }
                using var fileStream = File.OpenRead(_maidataPath);
                progress?.Report(1);
                var metadata = await SimaiParser.ParseMetadataAsync(fileStream);
                if (metadata.Hash == _simaiMetadata.Hash)
                {
                    _maidata ??= await SimaiParser.ParseAsync(metadata);
                    return _maidata;
                }
                else
                {
                    _maidata = await SimaiParser.ParseAsync(metadata);
                    _simaiMetadata = metadata;
                    return _maidata;
                }
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
            });
            if(disposing)
            {
                _audioTrackRef.SetTarget(null!);
                _previewAudioTrackRef.SetTarget(null!);
                _coverRef.SetTarget(null!);
            }
            _maidata = null;
        }
        public ValueTask DisposeAsync()
        {
            return DisposeAsync(true);
        }
        async ValueTask DisposeAsync(bool disposing)
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
            }
            if(disposing)
            {
                _audioTrackRef.SetTarget(null!);
                _previewAudioTrackRef.SetTarget(null!);
                _coverRef.SetTarget(null!);
            }
            _maidata = null;
        }
        void ThrowIfDisposed()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(SongDetail));
            }
        }
    }
}