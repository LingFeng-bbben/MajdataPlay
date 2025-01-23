using MajdataPlay.Extensions;
using MajdataPlay.Game.Notes;
using MajdataPlay.Types;
using MajSimai;
using System;
using System.Collections.Generic;
using System.Linq;
using MajdataPlay.Utils;
using UnityEngine;
using UnityEngine.UI;
using MajdataPlay.Collections;
using TMPro;
using Cysharp.Text;
using MajdataPlay.Game.Types;

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
        private TextMeshProUGUI _rate;

        Text _bgInfoHeader;
        Text _bgInfoText;

        private Text _table;
        private Text _judgeResultCount;

        public double[] _accRate = new double[5]
        {
            0.00,    // classic acc (+)
            100.00,  // classic acc (-)
            101.0000,// acc 101(-)
            100.0000,// acc 100(-)
            0.0000,  // acc (+)
        };

        long _cPerfectCount = 0;
        long _perfectCount = 0;
        long _greatCount = 0;
        long _goodCount = 0;
        long _missCount = 0;

        long _fast = 0;
        long _late = 0;

        float _diff = 0; // Note judge diff
        float _diffTimer = 3;

        public long _totalDXScore = 0;
        long _lostDXScore = 0;

        long _combo = 0; // Combo
        long _pCombo = 0; // Perfect Combo
        long _cPCombo = 0; // Critical Perfect
        Dictionary<JudgeGrade, int> _judgedTapCount = new()
        { 
            {JudgeGrade.TooFast, 0 },
            {JudgeGrade.FastGood, 0 },
            {JudgeGrade.FastGreat2, 0 },
            {JudgeGrade.FastGreat1, 0 },
            {JudgeGrade.FastGreat, 0 },
            {JudgeGrade.FastPerfect2, 0 },
            {JudgeGrade.FastPerfect1, 0 },
            {JudgeGrade.Perfect, 0 },
            {JudgeGrade.LatePerfect1, 0 },
            {JudgeGrade.LatePerfect2, 0 },
            {JudgeGrade.LateGreat, 0 },
            {JudgeGrade.LateGreat1, 0 },
            {JudgeGrade.LateGreat2, 0 },
            {JudgeGrade.LateGood, 0 },
            {JudgeGrade.Miss, 0 },
        };
        Dictionary<JudgeGrade, int> _judgedHoldCount = new()
        {
            {JudgeGrade.TooFast, 0 },
            {JudgeGrade.FastGood, 0 },
            {JudgeGrade.FastGreat2, 0 },
            {JudgeGrade.FastGreat1, 0 },
            {JudgeGrade.FastGreat, 0 },
            {JudgeGrade.FastPerfect2, 0 },
            {JudgeGrade.FastPerfect1, 0 },
            {JudgeGrade.Perfect, 0 },
            {JudgeGrade.LatePerfect1, 0 },
            {JudgeGrade.LatePerfect2, 0 },
            {JudgeGrade.LateGreat, 0 },
            {JudgeGrade.LateGreat1, 0 },
            {JudgeGrade.LateGreat2, 0 },
            {JudgeGrade.LateGood, 0 },
            {JudgeGrade.Miss, 0 },
        };
        Dictionary<JudgeGrade, int> _judgedTouchCount = new()
        {
            {JudgeGrade.TooFast, 0 },
            {JudgeGrade.FastGood, 0 },
            {JudgeGrade.FastGreat2, 0 },
            {JudgeGrade.FastGreat1, 0 },
            {JudgeGrade.FastGreat, 0 },
            {JudgeGrade.FastPerfect2, 0 },
            {JudgeGrade.FastPerfect1, 0 },
            {JudgeGrade.Perfect, 0 },
            {JudgeGrade.LatePerfect1, 0 },
            {JudgeGrade.LatePerfect2, 0 },
            {JudgeGrade.LateGreat, 0 },
            {JudgeGrade.LateGreat1, 0 },
            {JudgeGrade.LateGreat2, 0 },
            {JudgeGrade.LateGood, 0 },
            {JudgeGrade.Miss, 0 },
        };
        Dictionary<JudgeGrade, int> _judgedTouchHoldCount = new()
        {
            {JudgeGrade.TooFast, 0 },
            {JudgeGrade.FastGood, 0 },
            {JudgeGrade.FastGreat2, 0 },
            {JudgeGrade.FastGreat1, 0 },
            {JudgeGrade.FastGreat, 0 },
            {JudgeGrade.FastPerfect2, 0 },
            {JudgeGrade.FastPerfect1, 0 },
            {JudgeGrade.Perfect, 0 },
            {JudgeGrade.LatePerfect1, 0 },
            {JudgeGrade.LatePerfect2, 0 },
            {JudgeGrade.LateGreat, 0 },
            {JudgeGrade.LateGreat1, 0 },
            {JudgeGrade.LateGreat2, 0 },
            {JudgeGrade.LateGood, 0 },
            {JudgeGrade.Miss, 0 },
        };
        Dictionary<JudgeGrade, int> _judgedSlideCount = new()
        {
            {JudgeGrade.TooFast, 0 },
            {JudgeGrade.FastGood, 0 },
            {JudgeGrade.FastGreat2, 0 },
            {JudgeGrade.FastGreat1, 0 },
            {JudgeGrade.FastGreat, 0 },
            {JudgeGrade.FastPerfect2, 0 },
            {JudgeGrade.FastPerfect1, 0 },
            {JudgeGrade.Perfect, 0 },
            {JudgeGrade.LatePerfect1, 0 },
            {JudgeGrade.LatePerfect2, 0 },
            {JudgeGrade.LateGreat, 0 },
            {JudgeGrade.LateGreat1, 0 },
            {JudgeGrade.LateGreat2, 0 },
            {JudgeGrade.LateGood, 0 },
            {JudgeGrade.Miss, 0 },
        };
        Dictionary<JudgeGrade, int> _judgedBreakCount = new()
        {
            {JudgeGrade.TooFast, 0 },
            {JudgeGrade.FastGood, 0 },
            {JudgeGrade.FastGreat2, 0 },
            {JudgeGrade.FastGreat1, 0 },
            {JudgeGrade.FastGreat, 0 },
            {JudgeGrade.FastPerfect2, 0 },
            {JudgeGrade.FastPerfect1, 0 },
            {JudgeGrade.Perfect, 0 },
            {JudgeGrade.LatePerfect1, 0 },
            {JudgeGrade.LatePerfect2, 0 },
            {JudgeGrade.LateGreat, 0 },
            {JudgeGrade.LateGreat1, 0 },
            {JudgeGrade.LateGreat2, 0 },
            {JudgeGrade.LateGood, 0 },
            {JudgeGrade.Miss, 0 },
        };
        Dictionary<JudgeGrade, int> _totalJudgedCount = new()
        {
            {JudgeGrade.TooFast, 0 },
            {JudgeGrade.FastGood, 0 },
            {JudgeGrade.FastGreat2, 0 },
            {JudgeGrade.FastGreat1, 0 },
            {JudgeGrade.FastGreat, 0 },
            {JudgeGrade.FastPerfect2, 0 },
            {JudgeGrade.FastPerfect1, 0 },
            {JudgeGrade.Perfect, 0 },
            {JudgeGrade.LatePerfect1, 0 },
            {JudgeGrade.LatePerfect2, 0 },
            {JudgeGrade.LateGreat, 0 },
            {JudgeGrade.LateGreat1, 0 },
            {JudgeGrade.LateGreat2, 0 },
            {JudgeGrade.LateGood, 0 },
            {JudgeGrade.Miss, 0 },
        };

        GameInfo _gameInfo = MajInstanceHelper<GameInfo>.Instance!;
        OutlineLoader _outline;
        GamePlayManager _gpManager;
        XxlbAnimationController _xxlbController;

        const string DX_ACC_RATE_STRING = "{0:F4}%";
        const string CLASSIC_ACC_RATE_STRING = "{0:F2}%";
        const string COMBO_OR_DXSCORE_STRING = "{0}";
        const string DIFF_STRING = "{0:F2}";
        const string DXSCORE_RANK_HEADER_STRING = "✧{0}";
        const string DXSCORE_RANK_BODY_STRING = "+{0}";
        const string LATE_STRING = "LATE";
        const string FAST_STRING = "FAST";
        const string JUDGE_RESULT_STRING = "{0}\n{1}\n{2}\n{3}\n{4}\n\n{5}\n{6}";
        void Awake()
        {
            MajInstanceHelper<ObjectCounter>.Instance = this;
            _judgeResultCount = GameObject.Find("JudgeResultCount").GetComponent<Text>();
            _table = GameObject.Find("ObjectCount").GetComponent<Text>();
            _rate = GameObject.Find("ObjectRate").GetComponent<TextMeshProUGUI>();

            _bgInfoText = GameObject.Find("ComboText").GetComponent<Text>();
            _bgInfoHeader = GameObject.Find("ComboTextHeader").GetComponent<Text>();
            
            SetBgInfoActive(true);
            switch (MajInstances.Setting.Game.BGInfo)
            {
                case BGInfoType.CPCombo:
                    _bgInfoHeader.color = CPComboColor;
                    _bgInfoText.color = CPComboColor;
                    _bgInfoHeader.text = "CPCombo";
                    //bgInfoHeader.alignment = TextAnchor.MiddleCenter;
                    break;
                case BGInfoType.PCombo:
                    _bgInfoHeader.color = PComboColor;
                    _bgInfoText.color = PComboColor;
                    _bgInfoHeader.text = "PCombo";
                    //bgInfoHeader.alignment = TextAnchor.MiddleCenter;
                    break;
                case BGInfoType.Combo:
                    _bgInfoHeader.color = ComboColor;
                    _bgInfoText.color = ComboColor;
                    _bgInfoHeader.text = "Combo";
                    //bgInfoHeader.alignment = TextAnchor.MiddleCenter;
                    break;
                case BGInfoType.Achievement_101:
                case BGInfoType.Achievement_100:
                case BGInfoType.Achievement:
                case BGInfoType.AchievementClassical:
                case BGInfoType.AchievementClassical_100:
                    _bgInfoHeader.text = "Achievement";
                    _bgInfoHeader.color = AchievementGoldColor;
                    //bgInfoText.alignment = TextAnchor.MiddleRight;
                    break;
                case BGInfoType.DXScore:
                case BGInfoType.DXScoreRank:
                    _bgInfoHeader.text = "でらっくす SCORE";
                    _bgInfoHeader.color = DXScoreColor;
                    _bgInfoText.color = DXScoreColor;
                    //bgInfoText.alignment = TextAnchor.MiddleCenter;
                    break;
                case BGInfoType.S_Board:
                    _bgInfoHeader.text = "S  BORDER";
                    _bgInfoHeader.color = AchievementSilverColor;
                    _bgInfoText.color = AchievementSilverColor;
                    _bgInfoText.text = "4.0000%";
                    //bgInfoText.alignment = TextAnchor.MiddleRight;
                    break;
                case BGInfoType.SS_Board:
                    _bgInfoHeader.text = "SS  BORDER";
                    _bgInfoHeader.color = AchievementGoldColor;
                    _bgInfoText.color = AchievementGoldColor;
                    _bgInfoText.text = "2.0000%";
                    //bgInfoText.alignment = TextAnchor.MiddleRight;
                    break;
                case BGInfoType.SSS_Board:
                    _bgInfoHeader.text = "SSS  BORDER";
                    _bgInfoHeader.color = AchievementGoldColor;
                    _bgInfoText.color = AchievementGoldColor;
                    _bgInfoText.text = "1.0000%";
                    //bgInfoText.alignment = TextAnchor.MiddleRight;
                    break;
                case BGInfoType.MyBest:
                    _bgInfoHeader.text = "MyBestScore BORDER";
                    _bgInfoHeader.color = AchievementGoldColor;
                    _bgInfoText.color = AchievementGoldColor;
                    _bgInfoText.text = "101.0000%";
                    break;
                case BGInfoType.Diff:
                    _bgInfoHeader.color = ComboColor;
                    _bgInfoText.color = ComboColor;
                    _bgInfoHeader.text = "";
                    _bgInfoText.text = "";
                    break;
                case BGInfoType.None:
                    SetBgInfoActive(false);
                    break;
                default:
                    return;
            }
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
            

            if (_gameInfo.IsDanMode)
            {
                SetBgInfoActive(true);
                _bgInfoHeader.text = "LIFE";
                _bgInfoHeader.color = ComboColor;
                _bgInfoText.text = _gameInfo.CurrentHP.ToString();
                _bgInfoText.color = ComboColor;
            }
            else if(_gpManager.IsAutoplay)
            {
                _bgInfoHeader.text = "AUTOPLAY";
            }
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
            var holdRecord = _judgedHoldCount.ToDictionary(
                kv => kv.Key,
                kv => _judgedHoldCount[kv.Key] + _judgedTouchHoldCount[kv.Key]
            );
            var record = new Dictionary<ScoreNoteType, JudgeInfo>()
            {
                { ScoreNoteType.Tap, new (_judgedTapCount) },
                { ScoreNoteType.Hold, new (holdRecord)},
                { ScoreNoteType.Slide,new (_judgedSlideCount)},
                { ScoreNoteType.Break, new (_judgedBreakCount)},
                { ScoreNoteType.Touch, new (_judgedTouchCount) }
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
                    DX = _accRate[4],
                    Classic = _accRate[0]
                },
                SongInfo = song,
                Level = level,
                JudgeRecord = judgeRecord,
                Fast = _fast,
                Late = _late,
                DXScore = _totalDXScore + _lostDXScore,
                TotalDXScore = _totalDXScore,
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
            Dictionary<JudgeGrade, int> collection = null;
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
                        collection = _judgedTapCount;
                        baseScore = 500;
                        break;
                    case SimaiNoteType.Slide:
                        collection = _judgedSlideCount;
                        baseScore = 1500;
                        break;
                    case SimaiNoteType.TouchHold:
                        collection = _judgedTouchHoldCount;
                        baseScore = 1000;
                        break;
                    case SimaiNoteType.Hold:
                        collection = _judgedHoldCount;
                        baseScore = 1000;
                        break;
                    case SimaiNoteType.Touch:
                        collection = _judgedTouchCount;
                        baseScore = 500;
                        break;
                }

                foreach (var judgeResult in collection)
                {
                    var count = judgeResult.Value;
                    switch (judgeResult.Key)
                    {
                        case JudgeGrade.LatePerfect2:
                        case JudgeGrade.LatePerfect1:
                        case JudgeGrade.Perfect:
                        case JudgeGrade.FastPerfect1:
                        case JudgeGrade.FastPerfect2:
                            score += baseScore * 1 * count;
                            break;
                        case JudgeGrade.LateGreat2:
                        case JudgeGrade.LateGreat1:
                        case JudgeGrade.LateGreat:
                        case JudgeGrade.FastGreat:
                        case JudgeGrade.FastGreat1:
                        case JudgeGrade.FastGreat2:
                            score += (long)(baseScore * 0.8) * count;
                            lostScore += (long)(baseScore * 0.2) * count;
                            break;
                        case JudgeGrade.LateGood:
                        case JudgeGrade.FastGood:
                            score += (long)(baseScore * 0.5) * count;
                            lostScore += (long)(baseScore * 0.5) * count;
                            break;
                        case JudgeGrade.TooFast:
                        case JudgeGrade.Miss:
                            lostScore += baseScore * count;
                            break;
                    }
                }
            }
            foreach (var judgeResult in _judgedBreakCount)
            {
                var count = judgeResult.Value;
                switch (judgeResult.Key)
                {
                    case JudgeGrade.Perfect:
                        score += 2500 * count;
                        extraScore += 100 * count;
                        extraScoreClassic += 100 * count;
                        break;
                    case JudgeGrade.LatePerfect1:
                    case JudgeGrade.FastPerfect1:
                        score += 2500 * count;
                        extraScore += 75 * count;
                        extraScoreClassic += 50 * count;
                        lostExtraScore += 25 * count;
                        lostExtraScoreClassic += 50 * count;
                        break;
                    case JudgeGrade.LatePerfect2:
                    case JudgeGrade.FastPerfect2:
                        score += 2500 * count;
                        extraScore += 50 * count;
                        extraScoreClassic += 0 * count;
                        lostExtraScore += 50 * count;
                        lostExtraScoreClassic += 100 * count;
                        break;
                    case JudgeGrade.LateGreat:
                    case JudgeGrade.FastGreat:
                        score += 2000 * count;
                        extraScore += 40 * count;
                        extraScoreClassic += 0 * count;
                        lostScore += 500 * count;
                        lostExtraScore += 60 * count;
                        lostExtraScoreClassic += 100 * count;
                        break;
                    case JudgeGrade.LateGreat1:
                    case JudgeGrade.FastGreat1:
                        score += 1500 * count;
                        extraScore += 40 * count;
                        extraScoreClassic += 0 * count;
                        lostScore += 1000 * count;
                        lostExtraScore += 60 * count;
                        lostExtraScoreClassic += 100 * count;
                        break;
                    case JudgeGrade.LateGreat2:
                    case JudgeGrade.FastGreat2:
                        score += 1250 * count;
                        extraScore += 40 * count;
                        extraScoreClassic += 0 * count;
                        lostScore += 1250 * count;
                        lostExtraScore += 60 * count;
                        lostExtraScoreClassic += 100 * count;
                        break;
                    case JudgeGrade.LateGood:
                    case JudgeGrade.FastGood:
                        score += 1000 * count;
                        extraScore += 30 * count;
                        extraScoreClassic += 0 * count;
                        lostScore += 1500 * count;
                        lostExtraScore += 70 * count;
                        lostExtraScoreClassic += 100 * count;
                        break;
                    case JudgeGrade.TooFast:
                    case JudgeGrade.Miss:
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
            var currentNoteScore = GetNoteScoreSum();
            var totalScore = (tapSum + touchSum) * 500 + holdSum * 1000 + slideSum * 1500 + breakSum * 2500;
            var totalExtraScore = Math.Max(breakSum * 100, 1);

            _accRate[0] = (currentNoteScore.TotalScore + currentNoteScore.TotalExtraScoreClassic) / (double)totalScore * 100;
            _accRate[1] = (totalScore + currentNoteScore.TotalExtraScoreClassic - currentNoteScore.LostScore) / (double)totalScore * 100;
            _accRate[2] = (totalScore - currentNoteScore.LostScore) / (double)totalScore * 100 + (totalExtraScore - currentNoteScore.LostExtraScore) / (double)totalExtraScore;
            _accRate[3] = (totalScore - currentNoteScore.LostScore) / (double)totalScore * 100 + currentNoteScore.TotalExtraScore / (double)totalExtraScore;
            _accRate[4] = currentNoteScore.TotalScore / (double)totalScore * 100 + currentNoteScore.TotalExtraScore / (double)totalExtraScore;
        }
        internal void ReportResult<T>(T note, in JudgeResult judgeResult) where T: NoteDrop
        {
            var noteType = GetNoteType(note);
            var result = judgeResult.Grade;
            var isBreak = judgeResult.IsBreak;
            var isSlide = false;

            switch (note)
            {
                case TapDrop:
                    if (isBreak)
                    {
                        _judgedBreakCount[result]++;
                        breakCount++;
                    }
                    else
                    {
                        _judgedTapCount[result]++;
                        tapCount++;
                    }
                    break;
                case WifiDrop:
                case SlideDrop:
                    if (isBreak)
                    {
                        _judgedBreakCount[result]++;
                        breakCount++;
                    }
                    else
                    {
                        _judgedSlideCount[result]++;
                        slideCount++;
                    }
                    isSlide = true;
                    break;
                case HoldDrop:
                    if (isBreak)
                    {
                        _judgedBreakCount[result]++;
                        breakCount++;
                    }
                    else
                    {
                        _judgedHoldCount[result]++;
                        holdCount++;
                    }
                    break;
                case TouchDrop:
                    if (isBreak)
                    {
                        _judgedBreakCount[result]++;
                        breakCount++;
                    }
                    else
                    {
                        _judgedTouchCount[result]++;
                        touchCount++;
                    }
                    break;
                case TouchHoldDrop:
                    if (isBreak)
                    {
                        _judgedBreakCount[result]++;
                        breakCount++;
                    }
                    else
                    {
                        _judgedTouchHoldCount[result]++;
                        holdCount++;
                    }
                    break;
            }
            _totalJudgedCount[result]++;

            if (!isSlide && !judgeResult.IsMissOrTooFast)
            {
                _diff = judgeResult.Diff;
                _diffTimer = 3;
            }

            if (!judgeResult.IsMissOrTooFast)
            {
                _combo++;
                switch(note)
                {
                    case TapDrop:
                    case HoldDrop:
                        _outline.Play();
                        break;
                }
            }

            switch (result)
            {
                case JudgeGrade.TooFast:
                case JudgeGrade.Miss:
                    _missCount++;
                    _combo = 0;
                    _cPCombo = 0;
                    _pCombo = 0;
                    _lostDXScore -= 3;
                    break;
                case JudgeGrade.Perfect:
                    _cPerfectCount++;
                    _cPCombo++;
                    _pCombo++;
                    break;
                case JudgeGrade.LatePerfect2:
                case JudgeGrade.LatePerfect1:
                case JudgeGrade.FastPerfect1:
                case JudgeGrade.FastPerfect2:
                    _cPCombo = 0;
                    _pCombo++;
                    _perfectCount++;
                    _lostDXScore -= 1;
                    break;
                case JudgeGrade.LateGreat2:
                case JudgeGrade.LateGreat1:
                case JudgeGrade.LateGreat:
                case JudgeGrade.FastGreat:
                case JudgeGrade.FastGreat1:
                case JudgeGrade.FastGreat2:
                    _cPCombo = 0;
                    _pCombo = 0;
                    _greatCount++;
                    _lostDXScore -= 2;
                    break;
                case JudgeGrade.LateGood:
                case JudgeGrade.FastGood:
                    _cPCombo = 0;
                    _pCombo = 0;
                    _goodCount++;
                    _lostDXScore -= 3;
                    break;
            }

            if (_gameInfo.IsDanMode)
            {
                _gameInfo.OnNoteJudged(judgeResult.Grade);
                if (_gameInfo.CurrentHP == 0 && _gameInfo.IsForceGameover)
                {
                    _gpManager.GameOver();
                }
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
            var gameSetting = judgeResult.IsBreak ? MajInstances.Setting.Display.BreakFastLateType : MajInstances.Setting.Display.FastLateType;
            var resultValue = (int)judgeResult.Grade;
            var absValue = Math.Abs(7 - resultValue);

            switch (gameSetting)
            {
                case JudgeDisplayType.All:
                    if (judgeResult.Diff == 0 || judgeResult.IsMissOrTooFast)
                        break;
                    else if (judgeResult.IsFast)
                        _fast++;
                    else
                        _late++;
                    break;
                case JudgeDisplayType.BelowCP:
                    if (judgeResult.IsMissOrTooFast || judgeResult.Grade == JudgeGrade.Perfect)
                        break;
                    else if (judgeResult.IsFast)
                        _fast++;
                    else
                        _late++;
                    break;
                //默认只统计Great、Good的Fast/Late
                case JudgeDisplayType.BelowP:
                case JudgeDisplayType.BelowGR:
                case JudgeDisplayType.Disable:
                    if (judgeResult.IsMissOrTooFast || absValue <= 2)
                        break;
                    else if (judgeResult.IsFast)
                        _fast++;
                    else
                        _late++;
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
                _bgInfoText.text = ZString.Format(COMBO_OR_DXSCORE_STRING, combo);
            }
        }
        /// <summary>
        /// 更新BgInfo
        /// </summary>
        void UpdateMainOutput()
        {
            var bgInfo = MajInstances.Setting.Game.BGInfo;
            if (_gameInfo.IsDanMode)
            {
                _bgInfoText.text = _gameInfo.CurrentHP.ToString();
                _bgInfoText.color = ComboColor;
                SetBgInfoActive(true);
                return;
            }
            switch (bgInfo)
            {
                case BGInfoType.CPCombo:
                    UpdateCombo(_cPCombo);
                    break;
                case BGInfoType.PCombo:
                    UpdateCombo(_pCombo);
                    break;
                case BGInfoType.Combo:
                    UpdateCombo(_combo);
                    break;
                case BGInfoType.Achievement_101:
                    _bgInfoText.text = ZString.Format(DX_ACC_RATE_STRING, _accRate[2]);
                    UpdateAchievementColor(_accRate[2], _bgInfoText);
                    break;
                case BGInfoType.Achievement_100:
                    _bgInfoText.text = ZString.Format(DX_ACC_RATE_STRING, _accRate[3]);
                    UpdateAchievementColor(_accRate[3], _bgInfoText);
                    break;
                case BGInfoType.Achievement:
                    _bgInfoText.text = ZString.Format(DX_ACC_RATE_STRING, _accRate[4]);
                    UpdateAchievementColor(_accRate[4], _bgInfoText);
                    break;
                case BGInfoType.AchievementClassical:
                    _bgInfoText.text = ZString.Format(CLASSIC_ACC_RATE_STRING, _accRate[0]);
                    UpdateAchievementColor(_accRate[0], _bgInfoText);
                    break;
                case BGInfoType.AchievementClassical_100:
                    _bgInfoText.text = ZString.Format(CLASSIC_ACC_RATE_STRING, _accRate[1]);
                    UpdateAchievementColor(_accRate[1], _bgInfoText);
                    break;
                case BGInfoType.DXScore:
                    _bgInfoText.text = ZString.Format(COMBO_OR_DXSCORE_STRING, _lostDXScore);
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
                    _bgInfoText.text = ZString.Format(DIFF_STRING, _diff);
                    var oldColor = _bgInfoText.color;
                    if (_diff < 0)
                    {
                        oldColor = EarlyDiffColor;
                        _bgInfoHeader.text = FAST_STRING;
                    }
                    else
                    {
                        oldColor = LateDiffColor;
                        _bgInfoHeader.text = LATE_STRING;
                    }
                    var newColor = new Color()
                    {
                        r = oldColor.r,
                        g = oldColor.g,
                        b = oldColor.b,
                        a = _diffTimer.Clamp(0, 1)
                    };
                    _bgInfoHeader.color = newColor;
                    _bgInfoText.color = newColor;
                    _diffTimer -= Time.deltaTime * 3;
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
                    rate = _accRate[2] - 97;
                    break;
                case BGInfoType.SS_Board:
                    rate = _accRate[2] - 99;
                    break;
                case BGInfoType.SSS_Board:
                    rate = _accRate[2] - 100;
                    break;
                case BGInfoType.MyBest:
                    rate = _accRate[2] - _gpManager.HistoryScore.Acc.DX;
                    break;
                default:
                    return;
            }
            if (rate >= 0)
                _bgInfoText.text = ZString.Format(DX_ACC_RATE_STRING, rate);
            else
                SetBgInfoActive(false);
        }
        void SetBgInfoActive(bool state)
        {
            switch(state)
            {
                case true:
                    _bgInfoText.gameObject.layer = 0;
                    _bgInfoHeader.gameObject.layer = 0;
                    break;
                case false:
                    _bgInfoText.gameObject.layer = 3;
                    _bgInfoHeader.gameObject.layer = 3;
                    break;
            }
        }
        void UpdateDXScoreRank()
        {
            var remainingDXScore = _totalDXScore + _lostDXScore;
            var dxRank = new DXScoreRank(remainingDXScore, _totalDXScore);
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
            _bgInfoHeader.text = ZString.Format(DXSCORE_RANK_HEADER_STRING, dxRank.Rank);
            _bgInfoText.text = ZString.Format(DXSCORE_RANK_BODY_STRING, num);
            switch (dxRank.Rank)
            {
                case 5:
                case 4:
                case 3:
                    _bgInfoHeader.color = AchievementGoldColor;
                    _bgInfoText.color = AchievementGoldColor;
                    break;
                case 2:
                case 1:
                    _bgInfoHeader.color = DXScoreColor;
                    _bgInfoText.color = DXScoreColor;
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
            //_judgeResultCount.text = $"{_cPerfectCount}\n{_perfectCount}\n{_greatCount}\n{_goodCount}\n{_missCount}\n\n{_fast}\n{_late}";
            _judgeResultCount.text = ZString.Format(JUDGE_RESULT_STRING, 
                                                    _cPerfectCount, 
                                                    _perfectCount, 
                                                    _greatCount, 
                                                    _goodCount, 
                                                    _missCount, 
                                                    _fast, 
                                                    _late);
        }
        /// <summary>
        /// 更新SubDisplay左侧的Note详情
        /// </summary>
        void UpdateSideOutput()
        {
//            var comboN = tapCount + holdCount + slideCount + touchCount + breakCount;

//            table.text = $@"TAP: {tapCount} / {tapSum}
//HOD: {holdCount} / {holdSum}
//SLD: {slideCount} / {slideSum}
//TOH: {touchCount} / {touchSum}
//BRK: {breakCount} / {breakSum}
//ALL: {comboN} / {tapSum + holdSum + slideSum + touchSum + breakSum}";


            var isClassic = MajInstances.GameManager.Setting.Judge.Mode == JudgeMode.Classic;
            var formatStr = isClassic ? CLASSIC_ACC_RATE_STRING : DX_ACC_RATE_STRING;
            var value = isClassic ? _accRate[0] : _accRate[4];
            _rate.SetTextFormat(formatStr, value);
        }
        /// <summary>
        /// 计算最终达成率
        /// </summary>
        public float CalculateFinalResult()
        {
            CalAccRate();
            return (float)_accRate[2];
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