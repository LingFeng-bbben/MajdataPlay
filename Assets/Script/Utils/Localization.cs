using MajdataPlay.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#nullable enable
namespace MajdataPlay.Utils
{
    public static class Localization
    {
        public static Language Current { get; set; } = Language.Default;
        public static Language[] Available { get; private set; } = Array.Empty<Language>();

        public static string GetLocalizedText(MajText textType)
        {
            var table = Current.MappingTable;
            if (!table.ContainsKey(textType))
                return textType.ToString();
            else
                return table[textType];
        }
        public static string GetLocalizedText(string origin)
        {
            return origin switch
            {
                "Game" => GetLocalizedText(MajText.GAME),
                "Judge" => GetLocalizedText(MajText.JUDGE),
                "Display" => GetLocalizedText(MajText.DISPLAY),
                "Audio" => GetLocalizedText(MajText.AUDIO),
                "TapSpeed" => GetLocalizedText(MajText.TAP_SPEED),
                "TouchSpeed" => GetLocalizedText(MajText.TOUCH_SPEED),
                "SlideFadeInOffset" => GetLocalizedText(MajText.SLIDE_FADEIN_OFFSET),
                "BackgroundDim" => GetLocalizedText(MajText.BACKGROUND_DIM),
                "StarRotation" => GetLocalizedText(MajText.STAR_ROTATION),
                "BGInfo" => GetLocalizedText(MajText.BGINFO),
                "AudioOffset" => GetLocalizedText(MajText.AUDIO_OFFSET),
                "JudgeOffset" => GetLocalizedText(MajText.JUDGE_OFFSET),
                "Mode" => GetLocalizedText(MajText.MODE),
                "Skin" => GetLocalizedText(MajText.NOTE_SKIN),
                "DisplayCriticalPerfect" => GetLocalizedText(MajText.DISPLAY_CRITICAL_PERFECT),
                "FastLateType" => GetLocalizedText(MajText.FAST_LATE_TYPE),
                "NoteJudgeType" => GetLocalizedText(MajText.NOTE_JUDGE_TYPE),
                "TouchJudgeType" => GetLocalizedText(MajText.TOUCH_JUDGE_TYPE),
                "SlideJudgeType" => GetLocalizedText(MajText.SLIDE_JUDGE_TYPE),
                "OuterJudgeDistance" => GetLocalizedText(MajText.OUTER_JUDGE_DISTANCE),
                "InnerJudgeDistance" => GetLocalizedText(MajText.INNER_JUDGE_DISTANCE),
                "Volume" => GetLocalizedText(MajText.VOLUME),
                "Answer" => GetLocalizedText(MajText.ANSWER_VOL),
                "BGM" => GetLocalizedText(MajText.BGM_VOL),
                "Tap" => GetLocalizedText(MajText.TAP_VOL),
                "Slide" => GetLocalizedText(MajText.SLIDE_VOL),
                "Break" => GetLocalizedText(MajText.BREAK_VOL),
                "Touch" => GetLocalizedText(MajText.TOUCH_VOL),
                "Voice" => GetLocalizedText(MajText.VOICE_VOL),
                _ => origin
            };
        }
        public static void GetLocalizedText(MajText textType,out string origin)
        {
            var table = Current.MappingTable;
            if (!table.ContainsKey(textType))
                origin = textType.ToString();
            else
                origin = table[textType];
        }
    }
}
