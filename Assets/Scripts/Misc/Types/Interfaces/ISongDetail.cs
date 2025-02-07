using Cysharp.Threading.Tasks;
using MajdataPlay.IO;
using MajSimai;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
#nullable enable
namespace MajdataPlay.Types
{
    public interface ISongDetail
    {
        string Title { get; }
        string Artist { get; }
        string Description { get; }
        string[] Designers { get; }
        string[] Levels { get; }
        ChartStorageLocation Location { get; }
        DateTime Timestamp { get; }
        string Hash { get; }
        bool IsOnline => Location == ChartStorageLocation.Online;

        UniTask PreloadAsync(CancellationToken token = default);
        UniTask<string> GetVideoPathAsync(CancellationToken token = default);
        UniTask<Sprite> GetCoverAsync(bool isCompressed, CancellationToken token = default);
        UniTask<AudioSampleWrap> GetAudioTrackAsync(CancellationToken token = default);
        UniTask<AudioSampleWrap> GetPreviewAudioTrackAsync(CancellationToken token = default);
        UniTask<SimaiFile> GetMaidataAsync(bool ignoreCache = false, CancellationToken token = default);
    }
}
