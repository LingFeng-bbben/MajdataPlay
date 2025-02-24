using Cysharp.Text;
using MajdataPlay.Extensions;
using MajdataPlay.Game.Types;
using MajdataPlay.Interfaces;
using MajdataPlay.Types;
using MajdataPlay.Utils;
using System;
using UnityEngine;
using UnityEngine.UI;
#nullable enable
namespace MajdataPlay.Game
{
    public class TimeDisplayer : MonoBehaviour
    {
        public Text timeText;
        public Text rTimeText;
        public Slider progress;

        TimeSpan _audioTimeOffset = TimeSpan.Zero;
        GameInfo _gameInfo;
        GamePlayManager _gpManager;

        const string NEGATIVE_TIME_STRING = "-{0}:{1:00}.{2:000}";
        const string TIME_STRING = "{0}:{1:00}.{2:000}";

        

        void Awake()
        {
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
            _gpManager = Majdata<GamePlayManager>.Instance!;
        }

        void Update()
        {
            // Lock AudioTime variable for real
            var audioLen = TimeSpan.FromSeconds(_gpManager.AudioLength);
            var current = TimeSpan.FromSeconds(_gpManager.AudioTime) - _audioTimeOffset;
            var remaining = audioLen - current;
            var timeStr = current.TotalSeconds < 0 ? NEGATIVE_TIME_STRING : TIME_STRING;

            timeText.text = ZString.Format(timeStr,current.Minutes,current.Seconds,current.Milliseconds);
            rTimeText.text = ZString.Format(TIME_STRING, remaining.Minutes, remaining.Seconds, remaining.Milliseconds);
            progress.value = ((float)(current.TotalMilliseconds / audioLen.TotalMilliseconds)).Clamp(0, 1);
        }

        string TimeToString(float time)
        {
            var timenowInt = (int)time;
            var minute = timenowInt / 60;
            var second = timenowInt - 60 * minute;
            double mili = (time - timenowInt) * 10000;

            // Make timing display "cleaner" on negative timing.
            if (time < 0)
            {
                minute = Math.Abs(minute);
                second = Math.Abs(second);
                mili = Math.Abs(mili);

                //_timeText.text = string.Format("-{0}:{1:00}.{2:000}", minute, second, mili / 10);
                return $"-{minute}:{second:00}.{mili / 10:000}";
            }
            else
            {
                return $"{minute}:{second:00}.{mili / 10:000}";
            }
        }
    }
}