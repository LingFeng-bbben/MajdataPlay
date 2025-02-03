using Cysharp.Threading.Tasks;
using MajdataPlay.IO;
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
namespace MajdataPlay.Types
{

    public class SongDetail : ISongDetail
    {
        public string Title { get; init; } = string.Empty;
        public string Artist { get; init; } = string.Empty;
        public string[] Designers { get; init; } = new string[7];
        public string Description { get; init; } = string.Empty;
        public string[] Levels { get; init; } = new string[7];
        public string Hash { get; init; } = string.Empty;
        public DateTime Timestamp { get; init; }
        public ChartStorageLocation Location => ChartStorageLocation.Local;

        readonly string _maidataPath = string.Empty;
        readonly string _trackPath = string.Empty;
        readonly string _videoPath = string.Empty;
        readonly string _coverPath = string.Empty;

        AudioSampleWrap? _audioTrack = null;
        AudioSampleWrap? _previewAudioTrack = null;
        Sprite? _cover = null;
        SimaiFile? _maidata = null;

        readonly AsyncLock _previewAudioTrackLock = new();
        readonly AsyncLock _audioTrackLock = new();
        readonly AsyncLock _coverLock = new();
        readonly AsyncLock _maidataLock = new();
        public SongDetail(string chartFolder, SimaiMetadata metadata)
        {
            var files = new DirectoryInfo(chartFolder).GetFiles();

            _maidataPath = Path.Combine(chartFolder, "maidata.txt");
            _trackPath = files.FirstOrDefault(o => o.Name is "track.mp3" or "track.ogg").FullName;
            _videoPath = files.FirstOrDefault(o => o.Name is "bg.mp4" or "pv.mp4" or "mv.mp4")?.FullName ?? string.Empty;
            _coverPath = files.FirstOrDefault(o => o.Name is "bg.png" or "bg.jpg")?.FullName ?? string.Empty;
            _maidata = null;

            if (string.IsNullOrEmpty(_coverPath))
                _cover = MajEnv.EmptySongCover;
            
            Title = metadata.Title;
            Artist = metadata.Artist;
            Designers = metadata.Designers;
            Levels = metadata.Levels;
            Hash = metadata.Hash;
            Timestamp = files.FirstOrDefault(x => x.Name is "maidata.txt")?.LastWriteTime ?? DateTime.UnixEpoch;
        }
        public static async Task<SongDetail> ParseAsync(string chartFolder)
        {
            var maidataPath = Path.Combine(chartFolder, "maidata.txt");
            var metadata = await SimaiParser.Shared.ParseMetadataAsync(maidataPath);

            return new SongDetail(chartFolder, metadata);
        }
        public async UniTask Preload(CancellationToken token = default)
        {
            await UniTask.WhenAll(GetMaidataAsync(token: token), GetCoverAsync(true, token));
        }
        public async UniTask<string> GetVideoPathAsync(CancellationToken token = default)
        {
            await UniTask.Yield();
            return _videoPath;
        }
        public async UniTask<Sprite> GetCoverAsync(bool isCompressed, CancellationToken token = default)
        {
            try
            {
                using (await _coverLock.LockAsync(token))
                {
                    if (_cover is not null)
                        return _cover;

                    _cover = await SpriteLoader.LoadAsync(_coverPath, token);
                    return _cover;
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
                    if (_audioTrack is not null)
                        return _audioTrack;

                    _audioTrack = await MajInstances.AudioManager.LoadMusicAsync(_trackPath, true);
                    return _audioTrack;
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
        public async UniTask<AudioSampleWrap> GetPreviewAudioTrackAsync(CancellationToken token = default)
        {
            try
            {
                using (await _previewAudioTrackLock.LockAsync(token))
                {
                    if (_previewAudioTrack is not null)
                        return _previewAudioTrack;

                    _previewAudioTrack = await MajInstances.AudioManager.LoadMusicAsync(_trackPath, false);
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
        public async UniTask<SimaiFile> GetMaidataAsync(bool ignoreCache = false, CancellationToken token = default)
        {
            try
            {
                using (await _maidataLock.LockAsync(token))
                {
                    if (!ignoreCache && _maidata is not null)
                        return _maidata;

                    _maidata = await SimaiParser.Shared.ParseAsync(_maidataPath);
                    return _maidata;
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
    }
}