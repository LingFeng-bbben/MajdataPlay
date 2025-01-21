using Cysharp.Text;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Triggers;
using MajdataPlay.Extensions;
using MajdataPlay.Game;
using MajdataPlay.Game.Types;
using MajdataPlay.IO;
using MajdataPlay.References;
using MajdataPlay.Types;
using MajdataPlay.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
#nullable enable
public class PracticeManager : MonoBehaviour
{
    public TextMeshProUGUI startTimeText;
    public TextMeshProUGUI endTimeText;
    public ChartAnalyzer chartAnalyzer;
    public RectTransform selectionBox;
    public Text timeText;
    public Text rTimeText;
    public Slider progress;

    const string TIME_STRING = "{0}:{1:00}.{2:000}";

    private float startTime = 0;
    private float endTime = 0;
    private float totalTime = 0;
    private AudioSampleWrap audioTrack;

    private CancellationTokenSource cts;
    // Start is called before the first frame update
    private void Start()
    {
        cts = new CancellationTokenSource();
        Load().Forget();
    }
    async UniTaskVoid Load()
    {
        var gameinfo = MajInstanceHelper<GameInfo>.Instance;
        var songinfo = gameinfo.Charts.FirstOrDefault();
        var level = gameinfo.Levels.FirstOrDefault();
        if (songinfo.IsOnline)
        {
            songinfo = await songinfo.DumpToLocal(cts.Token);
        }
        audioTrack = (await MajInstances.AudioManager.LoadMusicAsync(songinfo.TrackPath ?? string.Empty, true))!;
        //audioTrack.Speed = MajInstances.GameManager.Setting.Mod.PlaybackSpeed;
        totalTime = (float)audioTrack.Length.TotalSeconds;
        await UniTask.SwitchToMainThread();
        await chartAnalyzer.AnalyzeSongDetail(songinfo, level);
        if (gameinfo.TimeRange is not null)
        {
            startTime = (float)gameinfo.TimeRange.Value.Start;
            endTime = (float)gameinfo.TimeRange.Value.End;
            
        }
        else { endTime = totalTime; }
        audioTrack.Play();
        audioTrack.CurrentSec = startTime;
        MajInstances.InputManager.BindAnyArea(OnAreaDown);
        MajInstances.SceneSwitcher.FadeOut();
    }

    bool isPressed = false;
    private void OnAreaDown(object sender, InputEventArgs e)
    {
        if (e.IsUp)
        {
            isPressed = false;
            return;
        }
        if (e.IsButton)
        {
            switch (e.Type)
            {
                case SensorType.A4:
                    var gameinfo = MajInstanceHelper<GameInfo>.Instance;
                    gameinfo.TimeRange = new Range<double>(startTime, endTime);
                    MajInstances.SceneSwitcher.SwitchScene("Game", false);
                    break;
                case SensorType.A5:
                    MajInstances.SceneSwitcher.SwitchScene("List");
                    break;
            }
            return;
        }
        else
        {
            switch (e.Type)
            {
                case SensorType.E6:
                    startTime = Mathf.Clamp(startTime - 0.2f, 0, totalTime);
                    audioTrack.CurrentSec = startTime;
                    isPressed = true;
                    ChangeValue(new Ref<float>(ref startTime), -3f).Forget();
                    break;
                case SensorType.B5:
                    startTime = Mathf.Clamp(startTime + 0.2f, 0, totalTime);
                    audioTrack.CurrentSec = startTime;
                    isPressed = true;
                    ChangeValue(new Ref<float>(ref startTime), 3f).Forget();
                    break;
                case SensorType.B4:
                    endTime = Mathf.Clamp(endTime - 0.2f, 0, totalTime);
                    audioTrack.CurrentSec = endTime-1f;
                    isPressed = true;
                    ChangeValue(new Ref<float>(ref endTime), -3f).Forget();
                    break;
                case SensorType.E4:
                    endTime = Mathf.Clamp(endTime + 0.2f, 0, totalTime);
                    audioTrack.CurrentSec = endTime - 1f;
                    isPressed = true;
                    ChangeValue(new Ref<float>(ref endTime), 3f).Forget();
                    break;
            }
        }
    }

    async UniTask ChangeValue(Ref<float> value, float delta)
    {
        var time = 0f;
        while (isPressed)
        {
            await UniTask.Yield();
            time += Time.deltaTime;
            if (time > 0.4)
            {
                value.Target = Mathf.Clamp(value.Target + delta*Time.deltaTime, 0, totalTime);
                audioTrack.CurrentSec = value.Target;
            }
            if (time > 1)
            {
                value.Target = Mathf.Clamp(value.Target + delta * Time.deltaTime * 2, 0, totalTime);
                audioTrack.CurrentSec = value.Target;
            }
            if (time > 5)
            {
                value.Target = Mathf.Clamp(value.Target + delta * Time.deltaTime * 8, 0, totalTime);
                audioTrack.CurrentSec = value.Target;
            }

        }
    }

    // Update is called once per frame
    void Update()
    {
        if(audioTrack is null) { return; }
        var start = TimeSpan.FromSeconds(startTime);
        startTimeText.text = ZString.Format(TIME_STRING, start.Minutes, start.Seconds, start.Milliseconds);
        var end = TimeSpan.FromSeconds(endTime);
        endTimeText.text = ZString.Format(TIME_STRING, end.Minutes, end.Seconds, end.Milliseconds);
        var anarect = chartAnalyzer.GetComponent<RectTransform>().rect;
        var x = startTime / totalTime * anarect.width;
        var width = (endTime - startTime) / totalTime * anarect.width;
        selectionBox.sizeDelta =  new Vector2((float)width, anarect.height);
        selectionBox.anchoredPosition = new Vector2((float)x, 0);

        var audioLen = audioTrack.Length;
        var current = TimeSpan.FromSeconds(audioTrack.CurrentSec);
        var remaining = audioLen - current;
        timeText.text = ZString.Format(TIME_STRING, current.Minutes, current.Seconds, current.Milliseconds);
        rTimeText.text = ZString.Format(TIME_STRING, remaining.Minutes, remaining.Seconds, remaining.Milliseconds);
        progress.value = ((float)(current.TotalMilliseconds / audioLen.TotalMilliseconds)).Clamp(0, 1);


        if (audioTrack.CurrentSec >= endTime+1)
        {
            audioTrack.CurrentSec = startTime;
        }
    }

    private void OnDestroy()
    {
        cts?.Cancel();
        audioTrack?.Dispose();
    }
}
