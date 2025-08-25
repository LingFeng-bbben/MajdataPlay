using Cysharp.Text;
using MajdataPlay.Extensions;
using MajdataPlay.Numerics;
using MajdataPlay.Utils;
using System;
using UnityEngine;
using UnityEngine.UI;
#nullable enable
namespace MajdataPlay.Scenes.Game
{
    public class TimeDisplayer : MonoBehaviour
    {
        public Text timeText;
        public Text rTimeText;
        public Slider progress;

        TimeSpan _lastUpdateTime = TimeSpan.Zero;
        TimeSpan _audioTimeOffset = TimeSpan.Zero;
        GameInfo _gameInfo;
        INoteController _noteController;

        const string NEGATIVE_TIME_STRING = "-{0}:{1:00}:{2:0000}";
        const string TIME_STRING = "{0}:{1:00}:{2:0000}";

        void Awake()
        {
            Majdata<TimeDisplayer>.Instance = this;
            _gameInfo = Majdata<GameInfo>.Instance!;

            //if(_gameInfo.Mode == GameMode.Practice)
            //{
            //    if (_gameInfo.TimeRange is Range<double> timeRange)
            //    {
            //        var startAt = timeRange.Start;
            //        startAt = Math.Max(startAt, 0);

            //        _audioTimeOffset = TimeSpan.FromSeconds(startAt);
            //    }
            //}
        }
        void Start()
        {
            _noteController = Majdata<INoteController>.Instance!;
            _lastUpdateTime = TimeSpan.FromSeconds(_noteController.ThisFrameSec) - _audioTimeOffset;
        }

        internal void OnPreUpdate()
        {
            // Lock AudioTime variable for real
            var audioLen = TimeSpan.FromSeconds(_noteController.AudioLength);
            var current = TimeSpan.FromSeconds(_noteController.ThisFrameSec) - _audioTimeOffset;
            var remaining = audioLen - current;
            var timeStr = current.TotalSeconds < 0 ? NEGATIVE_TIME_STRING : TIME_STRING;

            if(_lastUpdateTime.TotalSeconds != current.TotalSeconds)
            {
                timeText.text = ZString.Format(timeStr, current.Minutes, current.Seconds,current.Milliseconds);
                rTimeText.text = ZString.Format(TIME_STRING, remaining.Minutes, remaining.Seconds, remaining.Milliseconds);
                _lastUpdateTime = current;
            }

            progress.value = ((float)(current.TotalMilliseconds / audioLen.TotalMilliseconds)).Clamp(0, 1);
        }
    }
}