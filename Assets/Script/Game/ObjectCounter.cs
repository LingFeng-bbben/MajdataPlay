using MajdataPlay.Extensions;
using MajdataPlay.Game.Notes;
using MajdataPlay.Types;
using MajSimaiDecode;
using System;
using System.Collections.Generic;
using System.Linq;
using MajdataPlay.Utils;
using UnityEngine;
using UnityEngine.UI;
using MajdataPlay.Collections;

namespace MajdataPlay.Game
{
    public class ObjectCounter : MonoBehaviour
    {
        public Color AchievementDudColor; // = new Color32(63, 127, 176, 255);
        public Color AchievementBronzeColor; // = new Color32(127, 48, 32, 255);
        public Color AchievementSilverColor; // = new Color32(160, 160, 160, 255);
        public Color AchievementGoldColor; // = new Color32(224, 191, 127, 255);

        public Color CPComboColor;
        public Color PComboColor;
        public Color ComboColor;
        public Color DXScoreColor;

        public Color EarlyDiffColor;
        public Color LateDiffColor;

        public bool AllFinished => tapCount == tapSum &&
            holdCount == holdSum &&
            slideCount == slideSum &&
            touchCount == touchSum &&
            breakCount == breakSum;

        public int tapCount;
        public int holdCount;
        public int slideCount;
        public int touchCount;
        public int breakCount;

        public int tapSum;
        public int holdSum;
        public int slideSum;
        public int touchSum;
        public int breakSum;
        private Text rate;

        Text bgInfoHeader;
        Text bgInfoText;

        private Text table;
        private Text judgeResultCount;

        public double[] accRate = new double[5]
        {
            0.00,    // classic acc (+)
            100.00,  // classic acc (-)
            101.0000,// acc 101(-)
            100.0000,// acc 100(-)
            0.0000,  // acc (+)
        };

        long cPerfectCount = 0;
        long perfectCount = 0;
        long greatCount = 0;
        long goodCount = 0;
        long missCount = 0;

        long fast = 0;
        long late = 0;

        float diff = 0; // Note judge diff
        float diffTimer = 3;

        public long totalDXScore = 0;
        long lostDXScore = 0;

        long combo = 0; // Combo
        long pCombo = 0; // Perfect Combo
        long cPCombo = 0; // Critical Perfect
        Dictionary<JudgeType, int> judgedTapCount;
        Dictionary<JudgeType, int> judgedHoldCount;
        Dictionary<JudgeType, int> judgedTouchCount;
        Dictionary<JudgeType, int> judgedTouchHoldCount;
        Dictionary<JudgeType, int> judgedSlideCount;
        Dictionary<JudgeType, int> judgedBreakCount;
        Dictionary<JudgeType, int> totalJudgedCount;

        OutlineLoader _outline;
        GamePlayManager _gpManager;
        XxlbAnimationController _xxlbController;
        void Awake()
        {
            MajInstanceHelper<ObjectCounter>.Instance = this;
        }
        void OnDestroy()
        {
            MajInstanceHelper<ObjectCounter>.Free();
        }
        private void Start()
        {
            _outline = MajInstanceHelper<OutlineLoader>.Instance!;
            _xxlbController = MajInstanceHelper<XxlbAnimationController>.Instance!;
            _gpManager = MajInstanceHelper<GamePlayManager>.Instance!;
            judgeResultCount = GameObject.Find("JudgeResultCount").GetComponent<Text>();
            table = GameObject.Find("ObjectCount").GetComponent<Text>();
            rate = GameObject.Find("ObjectRate").GetComponent<Text>();

            bgInfoText = GameObject.Find("ComboText").GetComponent<Text>();
            bgInfoHeader = GameObject.Find("ComboTextHeader").GetComponent<Text>();

            judgedTapCount = new()
            {
                {JudgeType.TooFast, 0 },
                {JudgeType.FastGood, 0 },
                {JudgeType.FastGreat2, 0 },
                {JudgeType.FastGreat1, 0 },
                {JudgeType.FastGreat, 0 },
                {JudgeType.FastPerfect2, 0 },
                {JudgeType.FastPerfect1, 0 },
                {JudgeType.Perfect, 0 },
                {JudgeType.LatePerfect1, 0 },
                {JudgeType.LatePerfect2, 0 },
                {JudgeType.LateGreat, 0 },
                {JudgeType.LateGreat1, 0 },
                {JudgeType.LateGreat2, 0 },
                {JudgeType.LateGood, 0 },
                {JudgeType.Miss, 0 },
            };
            judgedHoldCount = new()
            {
                {JudgeType.TooFast, 0 },
                {JudgeType.FastGood, 0 },
                {JudgeType.FastGreat2, 0 },
                {JudgeType.FastGreat1, 0 },
                {JudgeType.FastGreat, 0 },
                {JudgeType.FastPerfect2, 0 },
                {JudgeType.FastPerfect1, 0 },
                {JudgeType.Perfect, 0 },
                {JudgeType.LatePerfect1, 0 },
                {JudgeType.LatePerfect2, 0 },
                {JudgeType.LateGreat, 0 },
                {JudgeType.LateGreat1, 0 },
                {JudgeType.LateGreat2, 0 },
                {JudgeType.LateGood, 0 },
                {JudgeType.Miss, 0 },
            };
            judgedTouchCount = new()
            {
                {JudgeType.TooFast, 0 },
                {JudgeType.FastGood, 0 },
                {JudgeType.FastGreat2, 0 },
                {JudgeType.FastGreat1, 0 },
                {JudgeType.FastGreat, 0 },
                {JudgeType.FastPerfect2, 0 },
                {JudgeType.FastPerfect1, 0 },
                {JudgeType.Perfect, 0 },
                {JudgeType.LatePerfect1, 0 },
                {JudgeType.LatePerfect2, 0 },
                {JudgeType.LateGreat, 0 },
                {JudgeType.LateGreat1, 0 },
                {JudgeType.LateGreat2, 0 },
                {JudgeType.LateGood, 0 },
                {JudgeType.Miss, 0 },
            };
            judgedTouchHoldCount = new()
            {
                {JudgeType.TooFast, 0 },
                {JudgeType.FastGood, 0 },
                {JudgeType.FastGreat2, 0 },
                {JudgeType.FastGreat1, 0 },
                {JudgeType.FastGreat, 0 },
                {JudgeType.FastPerfect2, 0 },
                {JudgeType.FastPerfect1, 0 },
                {JudgeType.Perfect, 0 },
                {JudgeType.LatePerfect1, 0 },
                {JudgeType.LatePerfect2, 0 },
                {JudgeType.LateGreat, 0 },
                {JudgeType.LateGreat1, 0 },
                {JudgeType.LateGreat2, 0 },
                {JudgeType.LateGood, 0 },
                {JudgeType.Miss, 0 },
            };
            judgedSlideCount = new()
            {
                {JudgeType.TooFast, 0 },
                {JudgeType.FastGood, 0 },
                {JudgeType.FastGreat2, 0 },
                {JudgeType.FastGreat1, 0 },
                {JudgeType.FastGreat, 0 },
                {JudgeType.FastPerfect2, 0 },
                {JudgeType.FastPerfect1, 0 },
                {JudgeType.Perfect, 0 },
                {JudgeType.LatePerfect1, 0 },
                {JudgeType.LatePerfect2, 0 },
                {JudgeType.LateGreat, 0 },
                {JudgeType.LateGreat1, 0 },
                {JudgeType.LateGreat2, 0 },
                {JudgeType.LateGood, 0 },
                {JudgeType.Miss, 0 },
            };
            judgedBreakCount = new()
            {
                {JudgeType.TooFast, 0 },
                {JudgeType.FastGood, 0 },
                {JudgeType.FastGreat2, 0 },
                {JudgeType.FastGreat1, 0 },
                {JudgeType.FastGreat, 0 },
                {JudgeType.FastPerfect2, 0 },
                {JudgeType.FastPerfect1, 0 },
                {JudgeType.Perfect, 0 },
                {JudgeType.LatePerfect1, 0 },
                {JudgeType.LatePerfect2, 0 },
                {JudgeType.LateGreat, 0 },
                {JudgeType.LateGreat1, 0 },
                {JudgeType.LateGreat2, 0 },
                {JudgeType.LateGood, 0 },
                {JudgeType.Miss, 0 },
            };
            totalJudgedCount = new()
            {
                {JudgeType.TooFast, 0 },
                {JudgeType.FastGood, 0 },
                {JudgeType.FastGreat2, 0 },
                {JudgeType.FastGreat1, 0 },
                {JudgeType.FastGreat, 0 },
                {JudgeType.FastPerfect2, 0 },
                {JudgeType.FastPerfect1, 0 },
                {JudgeType.Perfect, 0 },
                {JudgeType.LatePerfect1, 0 },
                {JudgeType.LatePerfect2, 0 },
                {JudgeType.LateGreat, 0 },
                {JudgeType.LateGreat1, 0 },
                {JudgeType.LateGreat2, 0 },
                {JudgeType.LateGood, 0 },
                {JudgeType.Miss, 0 },
            };

            SetBgInfoActive(true);
            switch (MajInstances.Setting.Game.BGInfo)
            {
                case BGInfoType.CPCombo:
                    bgInfoHeader.color = CPComboColor;
                    bgInfoText.color = CPComboColor;
                    bgInfoHeader.text = "CPCombo";
                    //bgInfoHeader.alignment = TextAnchor.MiddleCenter;
                    break;
                case BGInfoType.PCombo:
                    bgInfoHeader.color = PComboColor;
                    bgInfoText.color = PComboColor;
                    bgInfoHeader.text = "PCombo";
                    //bgInfoHeader.alignment = TextAnchor.MiddleCenter;
                    break;
                case BGInfoType.Combo:
                    bgInfoHeader.color = ComboColor;
                    bgInfoText.color = ComboColor;
                    bgInfoHeader.text = "Combo";
                    //bgInfoHeader.alignment = TextAnchor.MiddleCenter;
                    break;
                case BGInfoType.Achievement_101:
                case BGInfoType.Achievement_100:
                case BGInfoType.Achievement:
                case BGInfoType.AchievementClassical:
                case BGInfoType.AchievementClassical_100:
                    bgInfoHeader.text = "Achievement";
                    bgInfoHeader.color = AchievementGoldColor;
                    //bgInfoText.alignment = TextAnchor.MiddleRight;
                    break;
                case BGInfoType.DXScore:
                case BGInfoType.DXScoreRank:
                    bgInfoHeader.text = "でらっくす SCORE";
                    bgInfoHeader.color = DXScoreColor;
                    bgInfoText.color = DXScoreColor;
                    //bgInfoText.alignment = TextAnchor.MiddleCenter;
                    break;
                case BGInfoType.S_Board:
                    bgInfoHeader.text = "S  BORDER";
                    bgInfoHeader.color = AchievementSilverColor;
                    bgInfoText.color = AchievementSilverColor;
                    bgInfoText.text = "4.0000%";
                    //bgInfoText.alignment = TextAnchor.MiddleRight;
                    break;
                case BGInfoType.SS_Board:
                    bgInfoHeader.text = "SS  BORDER";
                    bgInfoHeader.color = AchievementGoldColor;
                    bgInfoText.color = AchievementGoldColor;
                    bgInfoText.text = "2.0000%";
                    //bgInfoText.alignment = TextAnchor.MiddleRight;
                    break;
                case BGInfoType.SSS_Board:
                    bgInfoHeader.text = "SSS  BORDER";
                    bgInfoHeader.color = AchievementGoldColor;
                    bgInfoText.color = AchievementGoldColor;
                    bgInfoText.text = "1.0000%";
                    //bgInfoText.alignment = TextAnchor.MiddleRight;
                    break;
                case BGInfoType.MyBest:
                    bgInfoHeader.text = "MyBestScore BORDER";
                    bgInfoHeader.color = AchievementGoldColor;
                    bgInfoText.color = AchievementGoldColor;
                    bgInfoText.text = "101.0000%";
                    break;
                case BGInfoType.Diff:
                    bgInfoHeader.color = ComboColor;
                    bgInfoText.color = ComboColor;
                    bgInfoHeader.text = "";
                    bgInfoText.text = "";
                    break;
                case BGInfoType.None:
                    SetBgInfoActive(false);
                    break;
                default:
                    return;
            }
            if(_gpManager.IsAutoplay)
                bgInfoHeader.text = "AUTOPLAY";
        }

        // Update is called once per frame
        private void Update()
        {
            UpdateState();
            UpdateOutput();
        }
        internal GameResult GetPlayRecord(SongDetail song, ChartLevel level)
        {
            //var fast = totalJudgedCount.Where(x => x.Key > JudgeType.Perfect && x.Key != JudgeType.Miss)
            //                           .Select(x => x.Value)
            //                           .Sum();
            //var late = totalJudgedCount.Where(x => x.Key < JudgeType.Perfect && x.Key != JudgeType.Miss)
            //                           .Select(x => x.Value)
            //                           .Sum();
            var holdRecord = judgedHoldCount.ToDictionary(
                kv => kv.Key,
                kv => judgedHoldCount[kv.Key] + judgedTouchHoldCount[kv.Key]
            );
            var record = new Dictionary<ScoreNoteType, JudgeInfo>()
            {
                { ScoreNoteType.Tap, new (judgedTapCount) },
                { ScoreNoteType.Hold, new (holdRecord)},
                { ScoreNoteType.Slide,new (judgedSlideCount)},
                { ScoreNoteType.Break, new (judgedBreakCount)},
                { ScoreNoteType.Touch, new (judgedTouchCount) }
            };
            var judgeRecord = new JudgeDetail(record);

            var unpackedinfo = JudgeDetail.UnpackJudgeRecord(judgeRecord.TotalJudgeInfo);
            var breakunpackedinfo = JudgeDetail.UnpackJudgeRecord(judgeRecord[ScoreNoteType.Break]);
            var cState = ComboState.None;
            if (unpackedinfo.IsAllPerfect && breakunpackedinfo.IsTheoretical)
            {
                cState = ComboState.APPlus;
            }
            else if (unpackedinfo.IsAllPerfect)
            {
                cState = ComboState.AP;
            }
            else if (unpackedinfo.IsFullComboPlus)
            {
                cState = ComboState.FCPlus;
            }
            else if (unpackedinfo.IsFullCombo)
            {
                cState = ComboState.FC;
            }

            return new GameResult()
            {
                Acc = new()
                {
                    DX = accRate[3],
                    Classic = accRate[1]
                },
                SongInfo = song,
                ChartLevel = level,
                JudgeRecord = judgeRecord,
                Fast = fast,
                Late = late,
                DXScore = totalDXScore + lostDXScore,
                TotalDXScore = totalDXScore,
                ComboState = cState
            };
        }

        private void UpdateOutput()
        {
            UpdateMainOutput();
            UpdateJudgeResult();
            UpdateSideOutput();
        }
        NoteScore GetNoteScoreSum()
        {
            Dictionary<JudgeType, int> collection = null;
            long score = 0;
            long lostScore = 0;
            long extraScore = 0;
            long extraScoreClassic = 0;
            long lostExtraScore = 0;
            long lostExtraScoreClassic = 0;
            int baseScore = 500;
            Span<SimaiNoteType> types = stackalloc SimaiNoteType[] { SimaiNoteType.Tap, SimaiNoteType.Slide, SimaiNoteType.Hold, SimaiNoteType.Touch, SimaiNoteType.TouchHold };

            foreach (var type in types)
            {
                switch (type)
                {
                    case SimaiNoteType.Tap:
                        collection = judgedTapCount;
                        baseScore = 500;
                        break;
                    case SimaiNoteType.Slide:
                        collection = judgedSlideCount;
                        baseScore = 1500;
                        break;
                    case SimaiNoteType.TouchHold:
                        collection = judgedTouchHoldCount;
                        baseScore = 1000;
                        break;
                    case SimaiNoteType.Hold:
                        collection = judgedHoldCount;
                        baseScore = 1000;
                        break;
                    case SimaiNoteType.Touch:
                        collection = judgedTouchCount;
                        baseScore = 500;
                        break;
                }

                foreach (var judgeResult in collection)
                {
                    var count = judgeResult.Value;
                    switch (judgeResult.Key)
                    {
                        case JudgeType.LatePerfect2:
                        case JudgeType.LatePerfect1:
                        case JudgeType.Perfect:
                        case JudgeType.FastPerfect1:
                        case JudgeType.FastPerfect2:
                            score += baseScore * 1 * count;
                            break;
                        case JudgeType.LateGreat2:
                        case JudgeType.LateGreat1:
                        case JudgeType.LateGreat:
                        case JudgeType.FastGreat:
                        case JudgeType.FastGreat1:
                        case JudgeType.FastGreat2:
                            score += (long)(baseScore * 0.8) * count;
                            lostScore += (long)(baseScore * 0.2) * count;
                            break;
                        case JudgeType.LateGood:
                        case JudgeType.FastGood:
                            score += (long)(baseScore * 0.5) * count;
                            lostScore += (long)(baseScore * 0.5) * count;
                            break;
                        case JudgeType.TooFast:
                        case JudgeType.Miss:
                            lostScore += baseScore * count;
                            break;
                    }
                }
            }
            foreach (var judgeResult in judgedBreakCount)
            {
                var count = judgeResult.Value;
                switch (judgeResult.Key)
                {
                    case JudgeType.Perfect:
                        score += 2500 * count;
                        extraScore += 100 * count;
                        extraScoreClassic += 100 * count;
                        break;
                    case JudgeType.LatePerfect1:
                    case JudgeType.FastPerfect1:
                        score += 2500 * count;
                        extraScore += 75 * count;
                        extraScoreClassic += 50 * count;
                        lostExtraScore += 25 * count;
                        lostExtraScoreClassic += 50 * count;
                        break;
                    case JudgeType.LatePerfect2:
                    case JudgeType.FastPerfect2:
                        score += 2500 * count;
                        extraScore += 50 * count;
                        extraScoreClassic += 0 * count;
                        lostExtraScore += 50 * count;
                        lostExtraScoreClassic += 100 * count;
                        break;
                    case JudgeType.LateGreat:
                    case JudgeType.FastGreat:
                        score += 2000 * count;
                        extraScore += 40 * count;
                        extraScoreClassic += 0 * count;
                        lostScore += 500 * count;
                        lostExtraScore += 60 * count;
                        lostExtraScoreClassic += 100 * count;
                        break;
                    case JudgeType.LateGreat1:
                    case JudgeType.FastGreat1:
                        score += 1500 * count;
                        extraScore += 40 * count;
                        extraScoreClassic += 0 * count;
                        lostScore += 1000 * count;
                        lostExtraScore += 60 * count;
                        lostExtraScoreClassic += 100 * count;
                        break;
                    case JudgeType.LateGreat2:
                    case JudgeType.FastGreat2:
                        score += 1250 * count;
                        extraScore += 40 * count;
                        extraScoreClassic += 0 * count;
                        lostScore += 1250 * count;
                        lostExtraScore += 60 * count;
                        lostExtraScoreClassic += 100 * count;
                        break;
                    case JudgeType.LateGood:
                    case JudgeType.FastGood:
                        score += 1000 * count;
                        extraScore += 30 * count;
                        extraScoreClassic += 0 * count;
                        lostScore += 1500 * count;
                        lostExtraScore += 70 * count;
                        lostExtraScoreClassic += 100 * count;
                        break;
                    case JudgeType.TooFast:
                    case JudgeType.Miss:
                        score += 0 * count;
                        extraScore += 0 * count;
                        extraScoreClassic += 0 * count;
                        lostScore += 2500 * count;
                        lostExtraScore += 100 * count;
                        lostExtraScoreClassic += 100 * count;
                        break;
                }
            }
            return new NoteScore()
            {
                TotalScore = score,
                TotalExtraScore = extraScore,
                TotalExtraScoreClassic = extraScoreClassic,
                LostScore = lostScore,
                LostExtraScore = lostExtraScore,
                LostExtraScoreClassic = lostExtraScoreClassic
            };
        }
        void CalAccRate()
        {
            long totalScore = 0;
            long totalExtraScore = 0;

            var currentNoteScore = GetNoteScoreSum();

            totalScore = (tapSum + touchSum) * 500 + holdSum * 1000 + slideSum * 1500 + breakSum * 2500;
            totalExtraScore = Math.Max(breakSum * 100, 1);

            accRate[0] = (currentNoteScore.TotalScore + currentNoteScore.TotalExtraScoreClassic) / (double)totalScore * 100;
            accRate[1] = (totalScore + currentNoteScore.TotalExtraScoreClassic - currentNoteScore.LostScore) / (double)totalScore * 100;
            accRate[2] = (totalScore - currentNoteScore.LostScore) / (double)totalScore * 100 + (totalExtraScore - currentNoteScore.LostExtraScore) / (double)totalExtraScore;
            accRate[3] = (totalScore - currentNoteScore.LostScore) / (double)totalScore * 100 + currentNoteScore.TotalExtraScore / (double)totalExtraScore;
            accRate[4] = currentNoteScore.TotalScore / (double)totalScore * 100 + currentNoteScore.TotalExtraScore / (double)totalExtraScore;
        }
        internal void ReportResult(NoteDrop note, in JudgeResult judgeResult)
        {
            var noteType = GetNoteType(note);
            var result = judgeResult.Result;
            var isBreak = judgeResult.IsBreak;


            switch (noteType)
            {
                case SimaiNoteType.Tap:
                    if (isBreak)
                    {
                        judgedBreakCount[result]++;
                        breakCount++;
                    }
                    else
                    {
                        judgedTapCount[result]++;
                        tapCount++;
                    }
                    break;
                case SimaiNoteType.Slide:
                    if (isBreak)
                    {
                        judgedBreakCount[result]++;
                        breakCount++;
                    }
                    else
                    {
                        judgedSlideCount[result]++;
                        slideCount++;
                    }
                    break;
                case SimaiNoteType.Hold:
                    if (isBreak)
                    {
                        judgedBreakCount[result]++;
                        breakCount++;
                    }
                    else
                    {
                        judgedHoldCount[result]++;
                        holdCount++;
                    }
                    break;
                case SimaiNoteType.Touch:
                    if (isBreak)
                    {
                        judgedBreakCount[result]++;
                        breakCount++;
                    }
                    else
                    {
                        judgedTouchCount[result]++;
                        touchCount++;
                    }
                    break;
                case SimaiNoteType.TouchHold:
                    if (isBreak)
                    {
                        judgedBreakCount[result]++;
                        breakCount++;
                    }
                    else
                    {
                        judgedTouchHoldCount[result]++;
                        holdCount++;
                    }
                    break;
            }
            totalJudgedCount[result]++;

            if (noteType != SimaiNoteType.Slide && !judgeResult.IsMissOrTooFast)
            {
                diff = judgeResult.Diff;
                diffTimer = 3;
            }

            if (!judgeResult.IsMissOrTooFast)
            {
                combo++;
                switch(noteType)
                {
                    case SimaiNoteType.Tap:
                    case SimaiNoteType.Hold:
                        _outline.Play();
                        break;
                }
            }

            switch (result)
            {
                case JudgeType.TooFast:
                case JudgeType.Miss:
                    missCount++;
                    combo = 0;
                    cPCombo = 0;
                    pCombo = 0;
                    lostDXScore -= 3;
                    break;
                case JudgeType.Perfect:
                    cPerfectCount++;
                    cPCombo++;
                    pCombo++;
                    break;
                case JudgeType.LatePerfect2:
                case JudgeType.LatePerfect1:
                case JudgeType.FastPerfect1:
                case JudgeType.FastPerfect2:
                    cPCombo = 0;
                    pCombo++;
                    perfectCount++;
                    lostDXScore -= 1;
                    break;
                case JudgeType.LateGreat2:
                case JudgeType.LateGreat1:
                case JudgeType.LateGreat:
                case JudgeType.FastGreat:
                case JudgeType.FastGreat1:
                case JudgeType.FastGreat2:
                    cPCombo = 0;
                    pCombo = 0;
                    greatCount++;
                    lostDXScore -= 2;
                    break;
                case JudgeType.LateGood:
                case JudgeType.FastGood:
                    cPCombo = 0;
                    pCombo = 0;
                    goodCount++;
                    lostDXScore -= 3;
                    break;
            }

            _xxlbController.Dance(result);
            UpdateFastLate(judgeResult);
            CalAccRate();
        }
        /// <summary>
        /// 更新Fast/Late统计信息
        /// </summary>
        /// <param name="judgeResult"></param>
        void UpdateFastLate(in JudgeResult judgeResult)
        {
            JudgeDisplayType gameSetting;
            if (judgeResult.IsBreak)
                gameSetting = MajInstances.Setting.Display.BreakFastLateType;
            else
                gameSetting = MajInstances.Setting.Display.FastLateType;
            var resultValue = (int)judgeResult.Result;
            var absValue = Math.Abs(7 - resultValue);

            switch (gameSetting)
            {
                case JudgeDisplayType.All:
                    if (judgeResult.Diff == 0 || judgeResult.IsMissOrTooFast)
                        break;
                    else if (judgeResult.IsFast)
                        fast++;
                    else
                        late++;
                    break;
                case JudgeDisplayType.BelowCP:
                    if (judgeResult.IsMissOrTooFast || judgeResult.Result == JudgeType.Perfect)
                        break;
                    else if (judgeResult.IsFast)
                        fast++;
                    else
                        late++;
                    break;
                //默认只统计Great、Good的Fast/Late
                case JudgeDisplayType.BelowP:
                case JudgeDisplayType.BelowGR:
                case JudgeDisplayType.Disable:
                    if (judgeResult.IsMissOrTooFast || absValue <= 2)
                        break;
                    else if (judgeResult.IsFast)
                        fast++;
                    else
                        late++;
                    break;
            }
        }
        /// <summary>
        /// 更新Combo
        /// </summary>
        /// <param name="combo"></param>
        void UpdateCombo(long combo)
        {
            if (combo == 0)
            {
                SetBgInfoActive(false);
            }
            else
            {
                SetBgInfoActive(true);
                bgInfoText.text = $"{combo}";
            }
        }
        /// <summary>
        /// 更新BgInfo
        /// </summary>
        void UpdateMainOutput()
        {
            var bgInfo = MajInstances.Setting.Game.BGInfo;
            switch (bgInfo)
            {
                case BGInfoType.CPCombo:
                    UpdateCombo(cPCombo);
                    break;
                case BGInfoType.PCombo:
                    UpdateCombo(pCombo);
                    break;
                case BGInfoType.Combo:
                    UpdateCombo(combo);
                    break;
                case BGInfoType.Achievement_101:
                    bgInfoText.text = $"{accRate[2]:F4}%";
                    UpdateAchievementColor(accRate[2], bgInfoText);
                    break;
                case BGInfoType.Achievement_100:
                    bgInfoText.text = $"{accRate[3]:F4}%";
                    UpdateAchievementColor(accRate[3], bgInfoText);
                    break;
                case BGInfoType.Achievement:
                    bgInfoText.text = $"{accRate[4]:F4}%";
                    UpdateAchievementColor(accRate[4], bgInfoText);
                    break;
                case BGInfoType.AchievementClassical:
                    bgInfoText.text = $"{accRate[0]:F2}%";
                    UpdateAchievementColor(accRate[0], bgInfoText);
                    break;
                case BGInfoType.AchievementClassical_100:
                    bgInfoText.text = $"{accRate[1]:F2}%";
                    UpdateAchievementColor(accRate[1], bgInfoText);
                    break;
                case BGInfoType.DXScore:
                    bgInfoText.text = $"{lostDXScore}";
                    break;
                case BGInfoType.DXScoreRank:
                    UpdateDXScoreRank();
                    break;
                case BGInfoType.S_Board:
                case BGInfoType.SS_Board:
                case BGInfoType.SSS_Board:
                case BGInfoType.MyBest:
                    UpdateRankBoard(bgInfo);
                    break;
                case BGInfoType.Diff:
                    bgInfoText.text = $"{diff:F2}";
                    var oldColor = bgInfoText.color;
                    if (diff < 0)
                    {
                        oldColor = EarlyDiffColor;
                        bgInfoHeader.text = "FAST";
                    }
                    else
                    {
                        oldColor = LateDiffColor;
                        bgInfoHeader.text = "LATE";
                    }
                    var newColor = new Color()
                    {
                        r = oldColor.r,
                        g = oldColor.g,
                        b = oldColor.b,
                        a = diffTimer.Clamp(0, 1)
                    };
                    bgInfoHeader.color = newColor;
                    bgInfoText.color = newColor;
                    diffTimer -= Time.deltaTime * 3;
                    break;
                default:
                    return;
            }
        }
        void UpdateRankBoard(in BGInfoType bgInfo)
        {
            double rate = -1;
            switch (bgInfo)
            {
                case BGInfoType.S_Board:
                    rate = accRate[2] - 97;
                    break;
                case BGInfoType.SS_Board:
                    rate = accRate[2] - 99;
                    break;
                case BGInfoType.SSS_Board:
                    rate = accRate[2] - 100;
                    break;
                case BGInfoType.MyBest:
                    rate = accRate[2] - _gpManager.HistoryScore.Acc.DX;
                    break;
                default:
                    return;
            }
            if (rate >= 0)
                bgInfoText.text = $"{rate:F4}%";
            else
                SetBgInfoActive(false);
        }
        void SetBgInfoActive(bool state)
        {
            switch(state)
            {
                case true:
                    bgInfoText.gameObject.layer = 0;
                    bgInfoHeader.gameObject.layer = 0;
                    break;
                case false:
                    bgInfoText.gameObject.layer = 3;
                    bgInfoHeader.gameObject.layer = 3;
                    break;
            }
        }
        void UpdateDXScoreRank()
        {
            var remainingDXScore = totalDXScore + lostDXScore;
            var dxRank = new DXScoreRank(remainingDXScore, totalDXScore);
            var num = remainingDXScore - (dxRank.Lower + 1);
            if (dxRank.Rank == 0)
            {
                SetBgInfoActive(false);
                return;
            }
            else
            {
                SetBgInfoActive(true);
            }
            bgInfoHeader.text = $"✧{dxRank.Rank}";
            bgInfoText.text = $"+{num}";
            switch (dxRank.Rank)
            {
                case 5:
                case 4:
                case 3:
                    bgInfoHeader.color = AchievementGoldColor;
                    bgInfoText.color = AchievementGoldColor;
                    break;
                case 2:
                case 1:
                    bgInfoHeader.color = DXScoreColor;
                    bgInfoText.color = DXScoreColor;
                    break;
            }
        }
        /// <summary>
        /// 更新SubDisplay的JudgeResult
        /// </summary>
        void UpdateJudgeResult()
        {
            //var fast = totalJudgedCount.Where(x => x.Key > JudgeType.Perfect && x.Key != JudgeType.Miss)
            //                           .Select(x => x.Value)
            //                           .Sum();
            //var late = totalJudgedCount.Where(x => x.Key < JudgeType.Perfect && x.Key != JudgeType.Miss)
            //                           .Select(x => x.Value)
            //                           .Sum();
            judgeResultCount.text = $"{cPerfectCount}\n{perfectCount}\n{greatCount}\n{goodCount}\n{missCount}\n\n{fast}\n{late}";
        }
        /// <summary>
        /// 更新SubDisplay左侧的Note详情
        /// </summary>
        void UpdateSideOutput()
        {
            var comboN = tapCount + holdCount + slideCount + touchCount + breakCount;

            table.text = $@"TAP: {tapCount} / {tapSum}
HOD: {holdCount} / {holdSum}
SLD: {slideCount} / {slideSum}
TOH: {touchCount} / {touchSum}
BRK: {breakCount} / {breakSum}
ALL: {comboN} / {tapSum + holdSum + slideSum + touchSum + breakSum}";

            rate.text = $@"FiNALE  Rate:
{accRate[0]:F2}   %
DELUXE Rate:
{accRate[4]:F4} %";
        }
        /// <summary>
        /// 计算最终达成率
        /// </summary>
        public float CalculateFinalResult()
        {
            CalAccRate();
            return (float)accRate[2];
        }
        void UpdateState()
        {
            // Only define this when debugging (of this feature) is needed.
            // I don't bother compiling this as Debug.
#if COMBO_CAN_SWAP_NOW
        if (Input.GetKeyDown(KeyCode.Space)) {
            var validModes = Enum.GetValues(textMode.GetType());
            int i = 0;
            foreach(EditorComboIndicator compareMode in validModes) {
                if (compareMode == textMode) {
                    ComboSetActive((EditorComboIndicator)validModes.GetValue((i + 1) % (validModes.Length - 1)));
                    break;
                }
                i += 1;
            }
        }
#endif
        }
        /// <summary>
        /// 根据Achievement更新BgInfoHeader的颜色
        /// </summary>
        /// <param name="achievementRate"></param>
        /// <param name="textElement"></param>
        void UpdateAchievementColor(double achievementRate, Text textElement)
        {
            var newColor = achievementRate switch
            {
                >= 100 => AchievementGoldColor,
                >= 97f => AchievementSilverColor,
                >= 80f => AchievementBronzeColor,
                _ => AchievementDudColor
            };

            if (textElement.color != newColor)
                textElement.color = newColor;
        }
        SimaiNoteType GetNoteType(NoteDrop note) => note switch
        {
            TapDrop => SimaiNoteType.Tap,
            //StarDrop => SimaiNoteType.Tap,
            HoldDrop => SimaiNoteType.Hold,
            SlideDrop => SimaiNoteType.Slide,
            WifiDrop => SimaiNoteType.Slide,
            TouchHoldDrop => SimaiNoteType.TouchHold,
            TouchDrop => SimaiNoteType.Touch,
            _ => throw new InvalidOperationException()
        };
    }
}