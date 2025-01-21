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

    private CancellationTokenSource cts = new CancellationTokenSource();

    Ref<float>? _valueRef = null;
    Ref<float> _startTimeRef = default;
    Ref<float> _endTimeRef = default;


    bool _isPressed = false;
    float _pressTime = 0;
    float _releaseTime = 0;
    float _minValue = 0;
    float _maxValue = 0;
    float _step = 0.2f;
    float _direction = 1;

    GameInfo _gameInfo;

    private void Start()
    {
        _gameInfo = MajInstanceHelper<GameInfo>.Instance!;
        _startTimeRef = new Ref<float>(ref startTime);
        _endTimeRef = new Ref<float>(ref endTime);
        Load().Forget();
    }
    async UniTaskVoid Load()
    {
        var songinfo = _gameInfo.Charts.FirstOrDefault();
        var level = _gameInfo.Levels.FirstOrDefault();
        if (songinfo.IsOnline)
        {
            songinfo = await songinfo.DumpToLocal(cts.Token);
        }
        audioTrack = (await MajInstances.AudioManager.LoadMusicAsync(songinfo.TrackPath ?? string.Empty, true))!;
        //audioTrack.Speed = MajInstances.GameManager.Setting.Mod.PlaybackSpeed;
        totalTime = (float)audioTrack.Length.TotalSeconds;
        await UniTask.SwitchToMainThread();
        await chartAnalyzer.AnalyzeSongDetail(songinfo, level);
        if (_gameInfo.TimeRange is not null)
        {
            startTime = (float)_gameInfo.TimeRange.Value.Start;
            endTime = (float)_gameInfo.TimeRange.Value.End;
            
        }
        else 
        { 
            endTime = totalTime; 
        }
        audioTrack.Play();
        audioTrack.CurrentSec = startTime;
        MajInstances.InputManager.BindAnyArea(OnAreaDown);
        MajInstances.SceneSwitcher.FadeOut();
    }

    
    private void OnAreaDown(object sender, InputEventArgs e)
    {
        if (e.IsUp)
        {
            _valueRef = null;
            _isPressed = false;
            return;
        }
        if (e.IsButton)
        {
            switch (e.Type)
            {
                case SensorType.A4:
                    _gameInfo.TimeRange = new Range<double>(startTime, endTime);
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
                    
                    _isPressed = true;
                    _valueRef = _startTimeRef;
                    _direction = -1;
                    _maxValue = endTime;
                    _minValue = 0;
                    break;
                case SensorType.B5:
                    startTime = Mathf.Clamp(startTime + 0.2f, 0, totalTime);
                    
                    _isPressed = true;
                    _valueRef = _startTimeRef;
                    _direction = 1;
                    _maxValue = endTime;
                    _minValue = 0;
                    break;
                case SensorType.B4:
                    endTime = Mathf.Clamp(endTime - 0.2f, 0, totalTime);

                    _isPressed = true;
                    _valueRef = _endTimeRef;
                    _direction = -1;
                    _maxValue = totalTime;
                    _minValue = startTime;
                    break;
                case SensorType.E4:
                    endTime = Mathf.Clamp(endTime + 0.2f, 0, totalTime);

                    _isPressed = true;
                    _valueRef = _endTimeRef;
                    _direction = 1;
                    _maxValue = totalTime;
                    _minValue = startTime;
                    break;
            }
        }
    }

    void Update()
    {
        UpdateSBTextMeshProUGUI();

        if(_isPressed)
        {
            _releaseTime = 0;
            if (_valueRef is not Ref<float> valueRef)
                return;
            if(_pressTime >= 0.5f)
            {
                audioTrack.Pause();
            }

            var deltaTime = Time.deltaTime;
            var ratio = _pressTime switch
            {
                > 7 => 64,
                > 5 => 32,
                > 3 => 16,
                > 1 => 8,
                > 0.5f => 2,
                _ => 0
            };
            var oldValue = valueRef.Target;
            var newValue = (oldValue + (_step * deltaTime * ratio * _direction)).Clamp(_minValue, _maxValue);
            valueRef.Target = newValue;
            _pressTime += deltaTime;
            MajDebug.Log($"Ratio: {ratio}x");
        }
        else
        {
            _pressTime = 0;
            if(_releaseTime < 0.5f)
            {
                _releaseTime += Time.deltaTime;
                return;
            }
            else if(!audioTrack.IsPlaying)
            {
                //var currentSec = audioTrack.CurrentSec;

                //if (currentSec > endTime)
                //{
                //    audioTrack.CurrentSec = startTime;
                //}
                //else if (currentSec < startTime)
                //{
                //    audioTrack.CurrentSec = startTime;
                //}
                audioTrack.CurrentSec = startTime;
                audioTrack.Play();
            }
            else
            {
                var currentSec = audioTrack.CurrentSec;

                if(currentSec > endTime)
                {
                    audioTrack.CurrentSec = startTime;
                }
            }
        }
    }
    void UpdateSBTextMeshProUGUI()
    {
        if (audioTrack is null)
        {
            return;
        }
        var start = TimeSpan.FromSeconds(startTime);
        var end = TimeSpan.FromSeconds(endTime);
        var anarect = chartAnalyzer.GetComponent<RectTransform>().rect;
        var x = startTime / totalTime * anarect.width;
        var width = (endTime - startTime) / totalTime * anarect.width;

        startTimeText.text = ZString.Format(TIME_STRING, start.Minutes, start.Seconds, start.Milliseconds);
        endTimeText.text = ZString.Format(TIME_STRING, end.Minutes, end.Seconds, end.Milliseconds);
        selectionBox.sizeDelta = new Vector2((float)width, anarect.height);
        selectionBox.anchoredPosition = new Vector2((float)x, 0);

        var audioLen = audioTrack.Length;
        var current = TimeSpan.FromSeconds(audioTrack.CurrentSec);
        var remaining = audioLen - current;
        timeText.text = ZString.Format(TIME_STRING, current.Minutes, current.Seconds, current.Milliseconds);
        rTimeText.text = ZString.Format(TIME_STRING, remaining.Minutes, remaining.Seconds, remaining.Milliseconds);
        progress.value = ((float)(current.TotalMilliseconds / audioLen.TotalMilliseconds)).Clamp(0, 1);
    }
    private void OnDestroy()
    {
        cts?.Cancel();
        audioTrack?.Dispose();
    }
}
