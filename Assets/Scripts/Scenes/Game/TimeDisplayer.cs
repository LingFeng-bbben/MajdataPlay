using Cysharp.Text;
using MajdataPlay.Buffers;
using MajdataPlay.Extensions;
using MajdataPlay.Numerics;
using MajdataPlay.Utils;
using System;
using TMPro;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;
using UnityEngine.UI;
#nullable enable
namespace MajdataPlay.Scenes.Game
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    public class TimeDisplayer : MonoBehaviour
    {
        const string NEGATIVE_TIME_STRING = "-{0}:{1:00}:{2:0000}";
        const string TIME_STRING = "{0}:{1:00}:{2:0000}";

        public TextMeshProUGUI timeText;
        public TextMeshProUGUI rTimeText;
        public Slider progress;

        TimeSpan _lastUpdateTime = TimeSpan.Zero;
        TimeSpan _audioTimeOffset = TimeSpan.Zero;
        GameInfo _gameInfo;
        INoteController _noteController;

        Utf16ValueStringBuilder _sb = ZString.CreateStringBuilder();

        readonly static Utf16PreparedFormat<int, int, int> NEGATIVE_TIME_FORMAT = ZString.PrepareUtf16<int, int, int>(NEGATIVE_TIME_STRING);
        readonly static Utf16PreparedFormat<int, int, int> TIME_FORMAT = ZString.PrepareUtf16<int, int, int>(TIME_STRING);

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
        void OnDestroy()
        {
            _sb.Dispose();
        }

        internal void OnPreUpdate()
        {
            // Lock AudioTime variable for real
            var audioLen = TimeSpan.FromSeconds(_noteController.AudioLength);
            var current = TimeSpan.FromSeconds(_noteController.ThisFrameSec) - _audioTimeOffset;
            var remaining = audioLen - current;
            var timeFormat = current.TotalSeconds < 0 ? NEGATIVE_TIME_FORMAT : TIME_FORMAT;

            if(_lastUpdateTime.TotalSeconds != current.TotalSeconds)
            {
                _sb.Clear();

                timeFormat.FormatTo(ref _sb, current.Minutes, current.Seconds,current.Milliseconds);
                var a = _sb.AsArraySegment();
                timeText.SetCharArray(a.Array, a.Offset, a.Count);

                _sb.Clear();

                TIME_FORMAT.FormatTo(ref _sb, remaining.Minutes, remaining.Seconds, remaining.Milliseconds);
                var b = _sb.AsArraySegment();
                rTimeText.SetCharArray(b.Array, b.Offset, b.Count);

                _lastUpdateTime = current;
            }

            progress.value = ((float)(current.TotalMilliseconds / audioLen.TotalMilliseconds)).Clamp(0, 1);
        }
    }
}