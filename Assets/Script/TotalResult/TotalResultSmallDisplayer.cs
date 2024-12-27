using MajdataPlay.Types;
using MajdataPlay.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using MajdataPlay.Utils;
using MajdataPlay.Collections;
using MajdataPlay;

public class TotalResultSmallDisplayer : MonoBehaviour
{
    public TextMeshProUGUI title;
    public TextMeshProUGUI artist;
    public TextMeshProUGUI designer;
    public TextMeshProUGUI level;
    public TextMeshProUGUI accDX;
    public Image coverImg;

    public void DisplayResult(SongDetail song, GameResult result, ChartLevel chartlevel) {
        var isClassic = MajInstances.GameManager.Setting.Judge.Mode == JudgeMode.Classic;
        title.text = song.Title;
        artist.text = song.Artist;
        designer.text = song.Designers[(int)chartlevel];
        level.text = chartlevel.ToString() + " " + song.Levels[(int)chartlevel];
        accDX.text = isClassic ? $"{result.Acc.Classic:F2}%" : $"{result.Acc.DX:F4}%";
        LoadCover(song).Forget();
    }
    async UniTask LoadCover(SongDetail song)
    {
        var task = song.GetSpriteAsync();
        while (!task.IsCompleted)
        {
            await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
        }
        coverImg.sprite = task.Result;
    }
}
