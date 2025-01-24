using Cysharp.Threading.Tasks;
using MajdataPlay.IO;
using MajSimai;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        UniTask Preload();
        UniTask<string> GetVideoPathAsync();
        UniTask<Sprite> GetCoverAsync(bool isCompressed);
        UniTask<AudioSampleWrap> GetAudioTrackAsync();
        UniTask<AudioSampleWrap> GetPreviewAudioTrackAsync();
        UniTask<SimaiFile> GetMaidataAsync();
    }
}
