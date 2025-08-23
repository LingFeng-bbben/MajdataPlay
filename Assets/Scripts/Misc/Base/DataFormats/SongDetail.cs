using Cysharp.Threading.Tasks;
using MajdataPlay.IO;
using MajdataPlay.Net;
using MajdataPlay.Utils;
using MajSimai;
using NeoSmart.AsyncLock;
using System;
using System.IO;
using System.Linq;
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
        public string Hash { get; init; } = string.Empty;
        public DateTime Timestamp { get; init; }
        public ChartStorageLocation Location => ChartStorageLocation.Local;

        readonly SimaiMetadata _simaiMetadata;
        readonly string _maidataPath = string.Empty;
        readonly string _trackPath = string.Empty;
        readonly string _videoPath = string.Empty;
        readonly string _coverPath = string.Empty;

        static readonly Action _emptyCallback = () => { };
        bool _isPreloaded = false;
        bool _isDisposed = false;

        AudioSampleWrap? _audioTrack = null;
        AudioSampleWrap? _previewAudioTrack = null;
        Sprite? _cover = null;
        SimaiFile? _maidata = null;

        readonly AsyncLock _previewAudioTrackLock = new();
        readonly AsyncLock _audioTrackLock = new();
        readonly AsyncLock _coverLock = new();
        readonly AsyncLock _maidataLock = new();
        readonly AsyncLock _preloadLock = new();

        ~SongDetail()
        {
            Dispose();
        }
        public SongDetail(string chartFolder, SimaiMetadata metadata)
        {
            var files = new DirectoryInfo(chartFolder).GetFiles();

            _maidataPath = Path.Combine(chartFolder, "maidata.txt");
            _trackPath = files.FirstOrDefault(o => o.Name is "track.mp3" or "track.ogg").FullName;
            _videoPath = files.FirstOrDefault(o => o.Name is "bg.mp4" or "pv.mp4" or "mv.mp4")?.FullName ?? string.Empty;
            _coverPath = files.FirstOrDefault(o => o.Name is "bg.png" or "bg.jpg")?.FullName ?? string.Empty;
            _maidata = null;

            if (string.IsNullOrEmpty(_coverPath))
            {
                _cover = MajEnv.EmptySongCover;
            }
            _simaiMetadata = metadata;
            Title = metadata.Title;
            Artist = metadata.Artist;
            Hash = metadata.Hash;
            Timestamp = files.FirstOrDefault(x => x.Name is "maidata.txt")?.LastWriteTime ?? DateTime.UnixEpoch;
        }
        public static async Task<SongDetail> ParseAsync(string chartFolder)
        {
            var maidataPath = Path.Combine(chartFolder, "maidata.txt");
            var metadata = await SimaiParser.ParseMetadataAsync(File.OpenRead(maidataPath));

            return new SongDetail(chartFolder, metadata);
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
        public UniTask<string> GetVideoPathAsync(INetProgress? progress = null, CancellationToken token = default)
        {
            ThrowIfDisposed();
            return UniTask.FromResult(_videoPath);
        }
        public async UniTask<Sprite> GetCoverAsync(bool isCompressed, INetProgress? progress = null, CancellationToken token = default)
        {
            ThrowIfDisposed();
            if (_cover is not null)
            {
                return _cover;
            }
            await UniTask.SwitchToThreadPool();
            using (await _coverLock.LockAsync(token))
            {
                token.ThrowIfCancellationRequested();
                if (_cover is not null)
                {
                    return _cover;
                }

                _cover = await SpriteLoader.LoadAsync(_coverPath, token);
                return _cover;
            }
        }
        public async UniTask<AudioSampleWrap> GetAudioTrackAsync(INetProgress? progress = null, CancellationToken token = default)
        {
            ThrowIfDisposed();
            if (_audioTrack is not null)
            {
                return _audioTrack;
            }
            await UniTask.SwitchToThreadPool();
            using (await _audioTrackLock.LockAsync(token))
            {
                token.ThrowIfCancellationRequested();
                if (_audioTrack is not null)
                {
                    return _audioTrack;
                }

                _audioTrack = await MajInstances.AudioManager.LoadMusicAsync(_trackPath, true);
                return _audioTrack;
            }
        }
        public async UniTask<AudioSampleWrap> GetPreviewAudioTrackAsync(INetProgress? progress = null, CancellationToken token = default)
        {
            ThrowIfDisposed();
            if (_previewAudioTrack is not null)
            {
                return _previewAudioTrack;
            }
            await UniTask.SwitchToThreadPool();
            using (await _previewAudioTrackLock.LockAsync(token))
            {
                token.ThrowIfCancellationRequested();
                if (_previewAudioTrack is not null)
                {
                    return _previewAudioTrack;
                }

                _previewAudioTrack = await MajInstances.AudioManager.LoadMusicAsync(_trackPath, false);
                return _previewAudioTrack;
            }
        }
        public async UniTask<SimaiFile> GetMaidataAsync(bool ignoreCache = false, INetProgress? progress = null, CancellationToken token = default)
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

                _maidata = await SimaiParser.ParseAsync(File.OpenRead(_maidataPath));
                return _maidata;
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
            GameObject.DestroyImmediate(_cover, true);
            _audioTrack = null;
            _previewAudioTrack = null;
            _cover = null;
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