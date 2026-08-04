using MajdataPlay.Settings;
using MajdataPlay.Scenes.Game.Notes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MajdataPlay.Scenes.Game
{
    public readonly struct GameModInfo
    {
        public float PlaybackSpeed { get; init; }
        public AutoplayModeOption AutoPlay { get; init; }
        public JudgeStyleOption JudgeStyle { get; init; }
        public bool SubdivideSlideJudgeGrade { get; init; }
        public bool AllBreak { get; init; }
        public bool AllEx { get; init; }
        public bool AllTouch { get; init; }
        public bool SlideNoHead { get; init; }
        public bool SlideNoTrack { get; init; }
        public bool ButtonRingForTouch { get; init; }
        public string NoteMask { get; init; }

        public GameModInfo(ModOptions options)
        {
            PlaybackSpeed = options.PlaybackSpeed;
            AutoPlay = options.AutoPlay;
            JudgeStyle = options.JudgeStyle;
            SubdivideSlideJudgeGrade = options.SubdivideSlideJudgeGrade;
            AllBreak = options.AllBreak;
            AllEx = options.AllEx;
            AllTouch = options.AllTouch;
            SlideNoHead = options.SlideNoHead;
            SlideNoTrack = options.SlideNoTrack;
#if !(UNITY_ANDROID || UNITY_IOS)
            ButtonRingForTouch = options.ButtonRingForTouch;
#endif
            NoteMask = options.NoteMask;
        }
        public GameModInfo(GameModInfo options)
        {
            PlaybackSpeed = options.PlaybackSpeed;
            AutoPlay = options.AutoPlay;
            JudgeStyle = options.JudgeStyle;
            SubdivideSlideJudgeGrade = options.SubdivideSlideJudgeGrade;
            AllBreak = options.AllBreak;
            AllEx = options.AllEx;
            AllTouch = options.AllTouch;
            SlideNoHead = options.SlideNoHead;
            SlideNoTrack = options.SlideNoTrack;
            ButtonRingForTouch = options.ButtonRingForTouch;
            NoteMask = options.NoteMask;
        }

        public static implicit operator GameModInfo(ModOptions options)
        {
            return new(options);
        }
    }
}
