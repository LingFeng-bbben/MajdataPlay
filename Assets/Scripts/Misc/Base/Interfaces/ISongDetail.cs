using Cysharp.Threading.Tasks;
using MajdataPlay.IO;
using MajdataPlay.Net;
using MajSimai;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
#nullable enable
namespace MajdataPlay
{
    public interface ISongDetail
    {
        string Title { get; }
        string Artist { get; }
        string Description { get; }
        ReadOnlySpan<string> Designers { get; }
        ReadOnlySpan<string> Levels { get; }
        ChartStorageLocation Location { get; }
        DateTime Timestamp { get; }
        string Hash { get; }
        bool IsOnline => Location == ChartStorageLocation.Online;

        UniTask PreloadAsync(INetProgress? progress = null, CancellationToken token = default);
        UniTask<string> GetVideoPathAsync(INetProgress? progress = null, CancellationToken token = default);
        UniTask<Sprite> GetCoverAsync(bool isCompressed, INetProgress? progress = null, CancellationToken token = default);
        UniTask<AudioSampleWrap> GetAudioTrackAsync(INetProgress? progress = null, CancellationToken token = default);
        UniTask<AudioSampleWrap> GetPreviewAudioTrackAsync(INetProgress? progress = null, CancellationToken token = default);
        UniTask<SimaiFile> GetMaidataAsync(bool ignoreCache = false, INetProgress? progress = null, CancellationToken token = default);
    }
}
