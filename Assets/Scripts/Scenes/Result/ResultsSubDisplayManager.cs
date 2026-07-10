using MajdataPlay.Collections;
using MajdataPlay.Scenes.Game;
using MajdataPlay.Scenes.Game.Notes;
using TMPro;
using UnityEngine;

namespace MajdataPlay
{
    public class ResultsSubDisplayManager : MonoBehaviour
    {
        public TextMeshProUGUI Note_Critical;
        public TextMeshProUGUI Hold_Critical;
        public TextMeshProUGUI Slide_Critical;
        public TextMeshProUGUI Touch_Critical;
        public TextMeshProUGUI Break_Critical;
        public TextMeshProUGUI Note_Perfect;
        public TextMeshProUGUI Hold_Perfect;
        public TextMeshProUGUI Slide_Perfect;
        public TextMeshProUGUI Touch_Perfect;
        public TextMeshProUGUI Break_Perfect;
        public TextMeshProUGUI Note_Great;
        public TextMeshProUGUI Hold_Great;
        public TextMeshProUGUI Slide_Great;
        public TextMeshProUGUI Touch_Great;
        public TextMeshProUGUI Break_Great;
        public TextMeshProUGUI Note_Good;
        public TextMeshProUGUI Hold_Good;
        public TextMeshProUGUI Slide_Good;
        public TextMeshProUGUI Touch_Good;
        public TextMeshProUGUI Break_Good;
        public TextMeshProUGUI Note_Miss;
        public TextMeshProUGUI Hold_Miss;
        public TextMeshProUGUI Slide_Miss;
        public TextMeshProUGUI Touch_Miss;
        public TextMeshProUGUI Break_Miss;


        public void Update(JudgeDetail details)
        {
            var HoldJudgeInfo = JudgeDetail.UnpackJudgeRecord(details[ScoreNoteType.Hold]);
            var TouchJudgeInfo = JudgeDetail.UnpackJudgeRecord(details[ScoreNoteType.Touch]);
            var BreakJudgeInfo = JudgeDetail.UnpackJudgeRecord(details[ScoreNoteType.Break]);
            var SlideJudgeInfo = JudgeDetail.UnpackJudgeRecord(details[ScoreNoteType.Slide]);
            var NoteJudgeInfo = JudgeDetail.UnpackJudgeRecord(details[ScoreNoteType.Tap]);
            var breakJudgeRecord = details[ScoreNoteType.Break];
            var break2550Count = breakJudgeRecord[JudgeGrade.FastPerfect2nd] + breakJudgeRecord[JudgeGrade.LatePerfect2nd];
            var break2500Count = breakJudgeRecord[JudgeGrade.FastPerfect3rd] + breakJudgeRecord[JudgeGrade.LatePerfect3rd];
            string breakPerfectCountText;
            if ((break2550Count + break2500Count) == 0)
            {
                breakPerfectCountText = "0";
            }
            else
            {
                breakPerfectCountText = $"{break2550Count}+{break2500Count}";
            }
            Hold_Great.text = HoldJudgeInfo.Great.ToString();
            Hold_Miss.text = HoldJudgeInfo.Miss.ToString();
            Hold_Critical.text = HoldJudgeInfo.CriticalPerfect.ToString();
            Hold_Perfect.text = HoldJudgeInfo.Perfect.ToString();
            Hold_Good.text = HoldJudgeInfo.Good.ToString();
            Touch_Great.text = TouchJudgeInfo.Great.ToString();
            Touch_Miss.text = TouchJudgeInfo.Miss.ToString();
            Touch_Critical.text = TouchJudgeInfo.CriticalPerfect.ToString();
            Touch_Perfect.text = TouchJudgeInfo.Perfect.ToString();
            Touch_Good.text = TouchJudgeInfo.Good.ToString();
            Slide_Great.text = SlideJudgeInfo.Great.ToString();
            Slide_Miss.text = SlideJudgeInfo.Miss.ToString();
            Slide_Critical.text = SlideJudgeInfo.CriticalPerfect.ToString();
            Slide_Perfect.text = SlideJudgeInfo.Perfect.ToString();
            Slide_Good.text = SlideJudgeInfo.Good.ToString();
            Note_Great.text = NoteJudgeInfo.Great.ToString();
            Note_Miss.text = NoteJudgeInfo.Miss.ToString();
            Note_Critical.text = NoteJudgeInfo.CriticalPerfect.ToString();
            Note_Perfect.text = NoteJudgeInfo.Perfect.ToString();
            Note_Good.text = NoteJudgeInfo.Good.ToString();
            Break_Great.text = BreakJudgeInfo.Great.ToString();
            Break_Miss.text = BreakJudgeInfo.Miss.ToString();
            Break_Critical.text = BreakJudgeInfo.CriticalPerfect.ToString();
            Break_Perfect.text = breakPerfectCountText;
            Break_Good.text = BreakJudgeInfo.Good.ToString();


        }
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
