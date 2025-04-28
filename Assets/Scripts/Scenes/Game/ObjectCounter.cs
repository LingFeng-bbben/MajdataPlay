using MajdataPlay.Extensions;
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
using System.Threading.Tasks;
using MajdataPlay.Game.Notes.Behaviours;
#nullable enable
namespace MajdataPlay.Game
{
    public class ObjectCounter : MonoBehaviour
    {
        #region UIconsts

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

        const string DX_ACC_RATE_STRING = "{0:F4}%";
        const string CLASSIC_ACC_RATE_STRING = "{0:F2}%";
        const string COMBO_OR_DXSCORE_STRING = "{0}";
        const string DIFF_STRING = "{0:F2}";
        const string DXSCORE_RANK_HEADER_STRING = "✧{0}";
        const string DXSCORE_RANK_BODY_STRING = "+{0}";
        const string LATE_STRING = "LATE";
        const string FAST_STRING = "FAST";
        const string JUDGE_RESULT_STRING = "{0}\n{1}\n{2}\n{3}\n{4}\n\n{5}\n{6}";

        #endregion

        #region count&scores
        public bool AllFinished
        {
            get
            {
                return TapFinishedCount == TapSum &&
                       HoldFinishedCount == HoldSum &&
                       SlideFinishedCount == SlideSum &&
                       TouchFinishedCount == TouchSum &&
                       BreakFinishedCount == BreakSum;
            }
        }
        public int TapFinishedCount { get; private set; }
        public int HoldFinishedCount { get; private set; }
        public int SlideFinishedCount { get; private set; }
        public int TouchFinishedCount { get; private set; }
        public int BreakFinishedCount { get; private set; }
        public int NoteFinishedCount
        {
            get
            {
                return TapFinishedCount + 
                       HoldFinishedCount + 
                       SlideFinishedCount + 
                       TouchFinishedCount + 
                       BreakFinishedCount;
            }
        }

        public int TapSum { get; private set; }
        public int HoldSum { get; private set; }
        public int SlideSum { get; private set; }
        public int TouchSum { get; private set; }
        public int BreakSum { get; private set; }
        public int NoteSum { get; private set; }

        public long TotalNoteScore => TotalNoteBaseScore + TotalNoteExtraScore;
        public long TotalNoteBaseScore { get; private set; }
        public long TotalNoteExtraScore { get; private set; }

        public long CurrentNoteScore => CurrentNoteBaseScore + CurrentNoteExtraScore;
        public long CurrentNoteScoreClassic => CurrentNoteBaseScore + CurrentNoteExtraScoreClassic;
        public long CurrentNoteBaseScore { get; private set; }
        public long CurrentNoteExtraScore { get; private set; }
        public long CurrentNoteExtraScoreClassic { get; private set; }

        public long TotalLostNoteScore => LostNoteBaseScore + LostNoteExtraScore;
        public long TotalLostNoteScoreClassic => LostNoteBaseScore + LostNoteExtraScoreClassic;
        public long LostNoteBaseScore { get; private set; }
        public long LostNoteExtraScore { get; private set; }
        public long LostNoteExtraScoreClassic { get; private set; }

        readonly double[] _accRate = new double[5]
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

        long _fastCount = 0;
        long _lateCount = 0;

        float _lastJudgeDiff = 0; // Note judge diff
        float _diffTimer = 3;

        public long _totalDXScore = 0;
        long _lostDXScore = 0;

        long _combo = 0; // Combo
        long _pCombo = 0; // Perfect Combo
        long _cPCombo = 0; // Critical Perfect

        List<float> _noteJudgeDiffList = new();
        Dictionary<JudgeGrade, int> _judgedTapCount = new()
        {
            {JudgeGrade.TooFast, 0 },
            {JudgeGrade.FastGood, 0 },
            {JudgeGrade.FastGreat3rd, 0 },
            {JudgeGrade.FastGreat2nd, 0 },
            {JudgeGrade.FastGreat, 0 },
            {JudgeGrade.FastPerfect3rd, 0 },
            {JudgeGrade.FastPerfect2nd, 0 },
            {JudgeGrade.Perfect, 0 },
            {JudgeGrade.LatePerfect2nd, 0 },
            {JudgeGrade.LatePerfect3rd, 0 },
            {JudgeGrade.LateGreat, 0 },
            {JudgeGrade.LateGreat2nd, 0 },
            {JudgeGrade.LateGreat3rd, 0 },
            {JudgeGrade.LateGood, 0 },
            {JudgeGrade.Miss, 0 },
        };
        Dictionary<JudgeGrade, int> _judgedHoldCount = new()
        {
            {JudgeGrade.TooFast, 0 },
            {JudgeGrade.FastGood, 0 },
            {JudgeGrade.FastGreat3rd, 0 },
            {JudgeGrade.FastGreat2nd, 0 },
            {JudgeGrade.FastGreat, 0 },
            {JudgeGrade.FastPerfect3rd, 0 },
            {JudgeGrade.FastPerfect2nd, 0 },
            {JudgeGrade.Perfect, 0 },
            {JudgeGrade.LatePerfect2nd, 0 },
            {JudgeGrade.LatePerfect3rd, 0 },
            {JudgeGrade.LateGreat, 0 },
            {JudgeGrade.LateGreat2nd, 0 },
            {JudgeGrade.LateGreat3rd, 0 },
            {JudgeGrade.LateGood, 0 },
            {JudgeGrade.Miss, 0 },
        };
        Dictionary<JudgeGrade, int> _judgedTouchCount = new()
        {
            {JudgeGrade.TooFast, 0 },
            {JudgeGrade.FastGood, 0 },
            {JudgeGrade.FastGreat3rd, 0 },
            {JudgeGrade.FastGreat2nd, 0 },
            {JudgeGrade.FastGreat, 0 },
            {JudgeGrade.FastPerfect3rd, 0 },
            {JudgeGrade.FastPerfect2nd, 0 },
            {JudgeGrade.Perfect, 0 },
            {JudgeGrade.LatePerfect2nd, 0 },
            {JudgeGrade.LatePerfect3rd, 0 },
            {JudgeGrade.LateGreat, 0 },
            {JudgeGrade.LateGreat2nd, 0 },
            {JudgeGrade.LateGreat3rd, 0 },
            {JudgeGrade.LateGood, 0 },
            {JudgeGrade.Miss, 0 },
        };
        Dictionary<JudgeGrade, int> _judgedTouchHoldCount = new()
        {
            {JudgeGrade.TooFast, 0 },
            {JudgeGrade.FastGood, 0 },
            {JudgeGrade.FastGreat3rd, 0 },
            {JudgeGrade.FastGreat2nd, 0 },
            {JudgeGrade.FastGreat, 0 },
            {JudgeGrade.FastPerfect3rd, 0 },
            {JudgeGrade.FastPerfect2nd, 0 },
            {JudgeGrade.Perfect, 0 },
            {JudgeGrade.LatePerfect2nd, 0 },
            {JudgeGrade.LatePerfect3rd, 0 },
            {JudgeGrade.LateGreat, 0 },
            {JudgeGrade.LateGreat2nd, 0 },
            {JudgeGrade.LateGreat3rd, 0 },
            {JudgeGrade.LateGood, 0 },
            {JudgeGrade.Miss, 0 },
        };
        Dictionary<JudgeGrade, int> _judgedSlideCount = new()
        {
            {JudgeGrade.TooFast, 0 },
            {JudgeGrade.FastGood, 0 },
            {JudgeGrade.FastGreat3rd, 0 },
            {JudgeGrade.FastGreat2nd, 0 },
            {JudgeGrade.FastGreat, 0 },
            {JudgeGrade.FastPerfect3rd, 0 },
            {JudgeGrade.FastPerfect2nd, 0 },
            {JudgeGrade.Perfect, 0 },
            {JudgeGrade.LatePerfect2nd, 0 },
            {JudgeGrade.LatePerfect3rd, 0 },
            {JudgeGrade.LateGreat, 0 },
            {JudgeGrade.LateGreat2nd, 0 },
            {JudgeGrade.LateGreat3rd, 0 },
            {JudgeGrade.LateGood, 0 },
            {JudgeGrade.Miss, 0 },
        };
        Dictionary<JudgeGrade, int> _judgedBreakCount = new()
        {
            {JudgeGrade.TooFast, 0 },
            {JudgeGrade.FastGood, 0 },
            {JudgeGrade.FastGreat3rd, 0 },
            {JudgeGrade.FastGreat2nd, 0 },
            {JudgeGrade.FastGreat, 0 },
            {JudgeGrade.FastPerfect3rd, 0 },
            {JudgeGrade.FastPerfect2nd, 0 },
            {JudgeGrade.Perfect, 0 },
            {JudgeGrade.LatePerfect2nd, 0 },
            {JudgeGrade.LatePerfect3rd, 0 },
            {JudgeGrade.LateGreat, 0 },
            {JudgeGrade.LateGreat2nd, 0 },
            {JudgeGrade.LateGreat3rd, 0 },
            {JudgeGrade.LateGood, 0 },
            {JudgeGrade.Miss, 0 },
        };
        Dictionary<JudgeGrade, int> _totalJudgedCount = new()
        {
            {JudgeGrade.TooFast, 0 },
            {JudgeGrade.FastGood, 0 },
            {JudgeGrade.FastGreat3rd, 0 },
            {JudgeGrade.FastGreat2nd, 0 },
            {JudgeGrade.FastGreat, 0 },
            {JudgeGrade.FastPerfect3rd, 0 },
            {JudgeGrade.FastPerfect2nd, 0 },
            {JudgeGrade.Perfect, 0 },
            {JudgeGrade.LatePerfect2nd, 0 },
            {JudgeGrade.LatePerfect3rd, 0 },
            {JudgeGrade.LateGreat, 0 },
            {JudgeGrade.LateGreat2nd, 0 },
            {JudgeGrade.LateGreat3rd, 0 },
            {JudgeGrade.LateGood, 0 },
            {JudgeGrade.Miss, 0 },
        };

        #endregion

        #region UIrefs
        Text _bgInfoHeader;
        Text _bgInfoText;
        Text _judgeResultCount;
        TextMeshProUGUI _rate;

        [SerializeField]
        GameObject _topInfoJudgeParent;
        [SerializeField]
        Text _topInfoPerfect;
        [SerializeField]
        Text _topInfoGreat;
        [SerializeField]
        Text _topInfoGood;
        [SerializeField]
        Text _topInfoMiss;

        [SerializeField]
        GameObject _topInfoTimingParent;
        [SerializeField]
        Text _topInfoFast;
        [SerializeField]
        Text _topInfoLate;

        #endregion

        XxlbDanceRequest _xxlbDanceRequest = new();
        bool _isOutlinePlayRequested = false;

        GameInfo _gameInfo = Majdata<GameInfo>.Instance!;
        OutlineLoader _outline;
        GamePlayManager _gpManager;
        XxlbAnimationController _xxlbController;


        void Awake()
        {
            Majdata<ObjectCounter>.Instance = this;
            _judgeResultCount = GameObject.Find("JudgeResultCount").GetComponent<Text>();
            _rate = GameObject.Find("ObjectRate").GetComponent<TextMeshProUGUI>();

            _bgInfoText = GameObject.Find("ComboText").GetComponent<Text>();
            _bgInfoHeader = GameObject.Find("ComboTextHeader").GetComponent<Text>();

            switch (MajInstances.Settings.Game.TopInfo)
            {
                case TopInfoDisplayType.Judge:
                    _topInfoJudgeParent.SetActive(true);
                    _topInfoTimingParent.SetActive(false);
                    break;
                case TopInfoDisplayType.Timing:
                    _topInfoJudgeParent.SetActive(false);
                    _topInfoTimingParent.SetActive(true);
                    break;
                case TopInfoDisplayType.None:
                default:
                    _topInfoJudgeParent.SetActive(false);
                    _topInfoTimingParent.SetActive(false);
                    break;
            }

            SetBgInfoActive(true);
            switch (MajInstances.Settings.Game.BGInfo)
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
                case BGInfoType.S_Border:
                    _bgInfoHeader.text = "S  BORDER";
                    _bgInfoHeader.color = AchievementSilverColor;
                    _bgInfoText.color = AchievementSilverColor;
                    _bgInfoText.text = "4.0000%";
                    //bgInfoText.alignment = TextAnchor.MiddleRight;
                    break;
                case BGInfoType.SS_Border:
                    _bgInfoHeader.text = "SS  BORDER";
                    _bgInfoHeader.color = AchievementGoldColor;
                    _bgInfoText.color = AchievementGoldColor;
                    _bgInfoText.text = "2.0000%";
                    //bgInfoText.alignment = TextAnchor.MiddleRight;
                    break;
                case BGInfoType.SSS_Border:
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
            Majdata<ObjectCounter>.Free();
        }
        void Start()
        {
            _outline = Majdata<OutlineLoader>.Instance!;
            _xxlbController = Majdata<XxlbAnimationController>.Instance!;
            _gpManager = Majdata<GamePlayManager>.Instance!;

            if (MajEnv.Mode == RunningMode.View) return;

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

        internal void OnLateUpdate()
        {
            UpdateAccRate();
            UpdateOutput();
            if(_xxlbDanceRequest.IsRequested)
            {
                _xxlbController.Dance(_xxlbDanceRequest.Grade);
                _xxlbDanceRequest = new();
            }
            if(_isOutlinePlayRequested)
            {
                _outline.Play();
                _isOutlinePlayRequested = false;
            }
        }
        internal void Clear()
        {
            _judgedTapCount = new()
            {
                {JudgeGrade.TooFast, 0 },
                {JudgeGrade.FastGood, 0 },
                {JudgeGrade.FastGreat3rd, 0 },
                {JudgeGrade.FastGreat2nd, 0 },
                {JudgeGrade.FastGreat, 0 },
                {JudgeGrade.FastPerfect3rd, 0 },
                {JudgeGrade.FastPerfect2nd, 0 },
                {JudgeGrade.Perfect, 0 },
                {JudgeGrade.LatePerfect2nd, 0 },
                {JudgeGrade.LatePerfect3rd, 0 },
                {JudgeGrade.LateGreat, 0 },
                {JudgeGrade.LateGreat2nd, 0 },
                {JudgeGrade.LateGreat3rd, 0 },
                {JudgeGrade.LateGood, 0 },
                {JudgeGrade.Miss, 0 },
            };
            _judgedHoldCount = new()
            {
                {JudgeGrade.TooFast, 0 },
                {JudgeGrade.FastGood, 0 },
                {JudgeGrade.FastGreat3rd, 0 },
                {JudgeGrade.FastGreat2nd, 0 },
                {JudgeGrade.FastGreat, 0 },
                {JudgeGrade.FastPerfect3rd, 0 },
                {JudgeGrade.FastPerfect2nd, 0 },
                {JudgeGrade.Perfect, 0 },
                {JudgeGrade.LatePerfect2nd, 0 },
                {JudgeGrade.LatePerfect3rd, 0 },
                {JudgeGrade.LateGreat, 0 },
                {JudgeGrade.LateGreat2nd, 0 },
                {JudgeGrade.LateGreat3rd, 0 },
                {JudgeGrade.LateGood, 0 },
                {JudgeGrade.Miss, 0 },
            };
            _judgedTouchCount = new()
            {
                {JudgeGrade.TooFast, 0 },
                {JudgeGrade.FastGood, 0 },
                {JudgeGrade.FastGreat3rd, 0 },
                {JudgeGrade.FastGreat2nd, 0 },
                {JudgeGrade.FastGreat, 0 },
                {JudgeGrade.FastPerfect3rd, 0 },
                {JudgeGrade.FastPerfect2nd, 0 },
                {JudgeGrade.Perfect, 0 },
                {JudgeGrade.LatePerfect2nd, 0 },
                {JudgeGrade.LatePerfect3rd, 0 },
                {JudgeGrade.LateGreat, 0 },
                {JudgeGrade.LateGreat2nd, 0 },
                {JudgeGrade.LateGreat3rd, 0 },
                {JudgeGrade.LateGood, 0 },
                {JudgeGrade.Miss, 0 },
            };
            _judgedTouchHoldCount = new()
            {
                {JudgeGrade.TooFast, 0 },
                {JudgeGrade.FastGood, 0 },
                {JudgeGrade.FastGreat3rd, 0 },
                {JudgeGrade.FastGreat2nd, 0 },
                {JudgeGrade.FastGreat, 0 },
                {JudgeGrade.FastPerfect3rd, 0 },
                {JudgeGrade.FastPerfect2nd, 0 },
                {JudgeGrade.Perfect, 0 },
                {JudgeGrade.LatePerfect2nd, 0 },
                {JudgeGrade.LatePerfect3rd, 0 },
                {JudgeGrade.LateGreat, 0 },
                {JudgeGrade.LateGreat2nd, 0 },
                {JudgeGrade.LateGreat3rd, 0 },
                {JudgeGrade.LateGood, 0 },
                {JudgeGrade.Miss, 0 },
            };
            _judgedSlideCount = new()
            {
                {JudgeGrade.TooFast, 0 },
                {JudgeGrade.FastGood, 0 },
                {JudgeGrade.FastGreat3rd, 0 },
                {JudgeGrade.FastGreat2nd, 0 },
                {JudgeGrade.FastGreat, 0 },
                {JudgeGrade.FastPerfect3rd, 0 },
                {JudgeGrade.FastPerfect2nd, 0 },
                {JudgeGrade.Perfect, 0 },
                {JudgeGrade.LatePerfect2nd, 0 },
                {JudgeGrade.LatePerfect3rd, 0 },
                {JudgeGrade.LateGreat, 0 },
                {JudgeGrade.LateGreat2nd, 0 },
                {JudgeGrade.LateGreat3rd, 0 },
                {JudgeGrade.LateGood, 0 },
                {JudgeGrade.Miss, 0 },
            };
            _judgedBreakCount = new()
            {
                {JudgeGrade.TooFast, 0 },
                {JudgeGrade.FastGood, 0 },
                {JudgeGrade.FastGreat3rd, 0 },
                {JudgeGrade.FastGreat2nd, 0 },
                {JudgeGrade.FastGreat, 0 },
                {JudgeGrade.FastPerfect3rd, 0 },
                {JudgeGrade.FastPerfect2nd, 0 },
                {JudgeGrade.Perfect, 0 },
                {JudgeGrade.LatePerfect2nd, 0 },
                {JudgeGrade.LatePerfect3rd, 0 },
                {JudgeGrade.LateGreat, 0 },
                {JudgeGrade.LateGreat2nd, 0 },
                {JudgeGrade.LateGreat3rd, 0 },
                {JudgeGrade.LateGood, 0 },
                {JudgeGrade.Miss, 0 },
            };
            _totalJudgedCount = new()
            {
                {JudgeGrade.TooFast, 0 },
                {JudgeGrade.FastGood, 0 },
                {JudgeGrade.FastGreat3rd, 0 },
                {JudgeGrade.FastGreat2nd, 0 },
                {JudgeGrade.FastGreat, 0 },
                {JudgeGrade.FastPerfect3rd, 0 },
                {JudgeGrade.FastPerfect2nd, 0 },
                {JudgeGrade.Perfect, 0 },
                {JudgeGrade.LatePerfect2nd, 0 },
                {JudgeGrade.LatePerfect3rd, 0 },
                {JudgeGrade.LateGreat, 0 },
                {JudgeGrade.LateGreat2nd, 0 },
                {JudgeGrade.LateGreat3rd, 0 },
                {JudgeGrade.LateGood, 0 },
                {JudgeGrade.Miss, 0 },
            };
            Span<double> newAccRate = stackalloc double[5]
            {
                0.00,    // classic acc (+)
                100.00,  // classic acc (-)
                101.0000,// acc 101(-)
                100.0000,// acc 100(-)
                0.0000,  // acc (+)
            };
            newAccRate.CopyTo(_accRate);
            TapFinishedCount = 0;
            HoldFinishedCount = 0;
            SlideFinishedCount = 0;
            TouchFinishedCount = 0;
            BreakFinishedCount = 0;

            TapSum = 0;
            HoldSum = 0;
            SlideSum = 0;
            TouchSum = 0;
            BreakSum = 0;


            _cPerfectCount = 0;
            _perfectCount = 0;
            _greatCount = 0;
            _goodCount = 0;
            _missCount = 0;

            _fastCount = 0;
            _lateCount = 0;

            _lastJudgeDiff = 0; // Note judge diff
            _diffTimer = 3;

            _totalDXScore = 0;
            _lostDXScore = 0;

            _combo = 0; // Combo
            _pCombo = 0; // Perfect Combo
            _cPCombo = 0; // Critical Perfect
        }
        internal GameResult GetPlayRecord(ISongDetail song, ChartLevel level)
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
                SongDetail = song,
                Level = level,
                JudgeRecord = judgeRecord,
                Fast = _fastCount,
                Late = _lateCount,
                DXScore = _totalDXScore + _lostDXScore,
                TotalDXScore = _totalDXScore,
                ComboState = cState,
                NoteJudgeDiffs = _noteJudgeDiffList.ToArray()
            };
        }

        private void UpdateOutput()
        {
            UpdateMainOutput();
            UpdateJudgeResult();
            UpdateTopAcc();
        }
        void UpdateAccRate()
        {
            // classic acc (+)
            // classic acc (-)
            // acc 101(-)
            // acc 100(-)
            // acc (+)
            //var currentNoteScore = GetNoteScoreSum();
            //var totalScore = (TapSum + TouchSum) * 500 + HoldSum * 1000 + SlideSum * 1500 + BreakSum * 2500;
            //var totalExtraScore = Math.Max(BreakSum * 100, 1);
            Span<double> newAccRate = stackalloc double[5];

            newAccRate[0] = CurrentNoteScoreClassic / (double)TotalNoteScore;
            newAccRate[1] = (TotalNoteScore - LostNoteBaseScore + CurrentNoteExtraScoreClassic) / (double)TotalNoteScore;
            newAccRate[2] = ((TotalNoteBaseScore - LostNoteBaseScore) / (double)TotalNoteBaseScore) + ((TotalNoteExtraScore - LostNoteExtraScore) / ((double)(TotalNoteExtraScore is 0 ? 1 : TotalNoteExtraScore) * 100));
            newAccRate[3] = ((TotalNoteBaseScore - LostNoteBaseScore) / (double)TotalNoteBaseScore) + ((CurrentNoteExtraScore) / ((double)(TotalNoteExtraScore is 0 ? 1 : TotalNoteExtraScore) * 100));
            newAccRate[4] = ((CurrentNoteBaseScore) / (double)TotalNoteBaseScore) + ((CurrentNoteExtraScore) / ((double)(TotalNoteExtraScore is 0 ? 1 : TotalNoteExtraScore) * 100));

            _accRate[0] = newAccRate[0] * 100;
            _accRate[1] = newAccRate[1] * 100;
            _accRate[2] = newAccRate[2] * 100;
            _accRate[3] = newAccRate[3] * 100;
            _accRate[4] = newAccRate[4] * 100;
        }
        internal async ValueTask CountNoteSumAsync(SimaiChart chart)
        {
            await Task.Run(() =>
            {
                foreach (var timing in chart.NoteTimings)
                {
                    foreach (var note in timing.Notes)
                    {
                        if (!note.IsBreak)
                        {
                            switch(note.Type)
                            {
                                case SimaiNoteType.Tap:
                                    TapSum++;
                                    break;
                                case SimaiNoteType.Hold:
                                case SimaiNoteType.TouchHold:
                                    HoldSum++;
                                    break;
                                case SimaiNoteType.Slide:
                                    if (!note.IsSlideNoHead)
                                        TapSum++;
                                    if (note.IsSlideBreak)
                                        BreakSum++;
                                    else
                                        SlideSum++;
                                    break;
                                case SimaiNoteType.Touch:
                                    TouchSum++;
                                    break;
                            }
                        }
                        else
                        {
                            if (note.Type == SimaiNoteType.Slide)
                            {
                                if (!note.IsSlideNoHead) 
                                    BreakSum++;
                                if (note.IsSlideBreak)
                                    BreakSum++;
                                else
                                    SlideSum++;
                            }
                            else
                            {
                                BreakSum++;
                            }
                        }
                    }
                }
                NoteSum = TapSum + HoldSum + TouchSum + BreakSum + SlideSum;
                TotalNoteBaseScore = (TapSum + TouchSum) * 500 + HoldSum * 1000 + SlideSum * 1500 + BreakSum * 2500;
                TotalNoteExtraScore = BreakSum * 100;
                _totalDXScore = NoteSum * 3;
                _noteJudgeDiffList = new(NoteSum);
            });
        }
        internal void ReportResult<T>(T note, in JudgeResult judgeResult) where T: NoteDrop
        {
            var grade = judgeResult.Grade;
            var isBreak = judgeResult.IsBreak;
            var isSlide = note is SlideDrop or WifiDrop;

            if (!isSlide && !judgeResult.IsMissOrTooFast)
            {
                _lastJudgeDiff = judgeResult.Diff;
                _diffTimer = 3;
            }

            if (!judgeResult.IsMissOrTooFast)
            {
                _combo++;
                switch(note)
                {
                    case TapDrop:
                    case HoldDrop:
                        _isOutlinePlayRequested = true;
                        _noteJudgeDiffList.Add(judgeResult.Diff);
                        break;
                }
            }

            if (MajEnv.Mode == RunningMode.Play && _gameInfo.IsDanMode) 
            {
                _gameInfo.OnNoteJudged(judgeResult.Grade);
                if (_gameInfo.CurrentHP == 0 && _gameInfo.IsForceGameover)
                {
                    _gpManager.GameOver();
                }
            }

            _xxlbDanceRequest = new()
            {
                IsRequested = true,
                Grade = grade,
            };
            
            UpdateComboCount(grade);
            UpdateJudgeCount(note, grade, isBreak);
            UpdateNoteScoreCount(note, judgeResult);
            UpdateFastLateCount(judgeResult);
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
            var bgInfo = MajInstances.Settings.Game.BGInfo;
            if (MajEnv.Mode != RunningMode.View &&_gameInfo.IsDanMode)
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
                case BGInfoType.S_Border:
                case BGInfoType.SS_Border:
                case BGInfoType.SSS_Border:
                case BGInfoType.MyBest:
                    UpdateRankBoard(bgInfo);
                    break;
                case BGInfoType.Diff:
                    _bgInfoText.text = ZString.Format(DIFF_STRING, _lastJudgeDiff);
                    var oldColor = _bgInfoText.color;
                    if (_lastJudgeDiff < 0)
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
                case BGInfoType.S_Border:
                    rate = _accRate[2] - 97;
                    break;
                case BGInfoType.SS_Border:
                    rate = _accRate[2] - 99;
                    break;
                case BGInfoType.SSS_Border:
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
            _judgeResultCount.text = ZString.Format(JUDGE_RESULT_STRING, 
                                                    _cPerfectCount, 
                                                    _perfectCount, 
                                                    _greatCount, 
                                                    _goodCount, 
                                                    _missCount, 
                                                    _fastCount, 
                                                    _lateCount);
            switch (MajInstances.Settings.Game.TopInfo)
            {
                case TopInfoDisplayType.Judge:
                    var p = _cPerfectCount + _perfectCount;
                    _topInfoPerfect.text = ZString.Concat(p);
                    _topInfoGreat.text = ZString.Concat(_greatCount);
                    _topInfoGood.text = ZString.Concat(_goodCount);
                    _topInfoMiss.text = ZString.Concat(_missCount);
                    break;
                case TopInfoDisplayType.Timing:
                    _topInfoFast.text = ZString.Concat(_fastCount);
                    _topInfoLate.text = ZString.Concat(_lateCount);
                    break;
                case TopInfoDisplayType.None:
                default:
                    break;
            }
        }
        /// <summary>
        /// 更新顶部的总达成率
        /// </summary>
        void UpdateTopAcc()
        {
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
            UpdateAccRate();
            return (float)_accRate[2];
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

        #region Counter update
        void UpdateJudgeCount<T>(T note, JudgeGrade grade, bool isBreak) where T : NoteDrop
        {
            switch (note)
            {
                case TapDrop:
                    if (isBreak)
                    {
                        _judgedBreakCount[grade]++;
                        BreakFinishedCount++;
                    }
                    else
                    {
                        _judgedTapCount[grade]++;
                        TapFinishedCount++;
                    }
                    break;
                case WifiDrop:
                case SlideDrop:
                    if (isBreak)
                    {
                        _judgedBreakCount[grade]++;
                        BreakFinishedCount++;
                    }
                    else
                    {
                        _judgedSlideCount[grade]++;
                        SlideFinishedCount++;
                    }
                    break;
                case HoldDrop:
                    if (isBreak)
                    {
                        _judgedBreakCount[grade]++;
                        BreakFinishedCount++;
                    }
                    else
                    {
                        _judgedHoldCount[grade]++;
                        HoldFinishedCount++;
                    }
                    break;
                case TouchDrop:
                    if (isBreak)
                    {
                        _judgedBreakCount[grade]++;
                        BreakFinishedCount++;
                    }
                    else
                    {
                        _judgedTouchCount[grade]++;
                        TouchFinishedCount++;
                    }
                    break;
                case TouchHoldDrop:
                    if (isBreak)
                    {
                        _judgedBreakCount[grade]++;
                        BreakFinishedCount++;
                    }
                    else
                    {
                        _judgedTouchHoldCount[grade]++;
                        HoldFinishedCount++;
                    }
                    break;
            }
            _totalJudgedCount[grade]++;
        }
        void UpdateComboCount(JudgeGrade grade)
        {
            switch (grade)
            {
                case JudgeGrade.Perfect:
                    _cPerfectCount++;
                    _cPCombo++;
                    _pCombo++;
                    break;
                case JudgeGrade.LatePerfect2nd:
                case JudgeGrade.FastPerfect2nd:
                case JudgeGrade.LatePerfect3rd:
                case JudgeGrade.FastPerfect3rd:
                    _cPCombo = 0;
                    _pCombo++;
                    _perfectCount++;
                    _lostDXScore -= 1;
                    break;
                case JudgeGrade.LateGreat3rd:
                case JudgeGrade.LateGreat2nd:
                case JudgeGrade.LateGreat:
                case JudgeGrade.FastGreat:
                case JudgeGrade.FastGreat2nd:
                case JudgeGrade.FastGreat3rd:
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
                case JudgeGrade.TooFast:
                case JudgeGrade.Miss:
                    _missCount++;
                    _combo = 0;
                    _cPCombo = 0;
                    _pCombo = 0;
                    _lostDXScore -= 3;
                    break;
            }
        }
        void UpdateNoteScoreCount<T>(T note, in JudgeResult judgeResult) where T : NoteDrop
        {
            var baseScore = 500;

            switch (note)
            {
                case TapDrop:
                case TouchDrop:
                    baseScore = 500;
                    break;
                case HoldDrop:
                case TouchHoldDrop:
                    baseScore = 1000;
                    break;
                case SlideDrop:
                case WifiDrop:
                    baseScore = 1500;
                    break;
            }
            if (!judgeResult.IsBreak)
            {
                switch (judgeResult.Grade)
                {
                    case JudgeGrade.Miss:
                    case JudgeGrade.TooFast:
                        //CurrentNoteBaseScore += baseScore * 0;
                        LostNoteBaseScore += baseScore;
                        break;
                    case JudgeGrade.LateGood:
                    case JudgeGrade.FastGood:
                        CurrentNoteBaseScore += (long)(baseScore * 0.5);
                        LostNoteBaseScore += (long)(baseScore * 0.5);
                        break;
                    case JudgeGrade.LateGreat3rd:
                    case JudgeGrade.LateGreat2nd:
                    case JudgeGrade.LateGreat:
                    case JudgeGrade.FastGreat:
                    case JudgeGrade.FastGreat2nd:
                    case JudgeGrade.FastGreat3rd:
                        CurrentNoteBaseScore += (long)(baseScore * 0.8);
                        LostNoteBaseScore += (long)(baseScore * 0.2);
                        break;
                    default:
                        CurrentNoteBaseScore += baseScore;
                        //LostNoteBaseScore += 0;
                        break;
                }
            }
            else
            {

                switch (judgeResult.Grade)
                {
                    case JudgeGrade.Miss:
                    case JudgeGrade.TooFast:
                        LostNoteBaseScore += 2500;
                        LostNoteExtraScore += 100;
                        LostNoteExtraScoreClassic += 100;
                        break;
                    case JudgeGrade.LateGood:
                    case JudgeGrade.FastGood:
                        CurrentNoteBaseScore += 1000;
                        CurrentNoteExtraScore += 30;
                        LostNoteBaseScore += 1500;
                        LostNoteExtraScore += 70;
                        LostNoteExtraScoreClassic += 100;
                        break;
                    case JudgeGrade.LateGreat3rd:
                    case JudgeGrade.FastGreat3rd:
                        CurrentNoteBaseScore += 1250;
                        CurrentNoteExtraScore += 40;
                        LostNoteBaseScore += 1250;
                        LostNoteExtraScore += 60;
                        LostNoteExtraScoreClassic += 100;
                        break;
                    case JudgeGrade.FastGreat2nd:
                    case JudgeGrade.LateGreat2nd:
                        CurrentNoteBaseScore += 1500;
                        CurrentNoteExtraScore += 40;
                        LostNoteBaseScore += 1000;
                        LostNoteExtraScore += 60;
                        LostNoteExtraScoreClassic += 100;
                        break;
                    case JudgeGrade.LateGreat:
                    case JudgeGrade.FastGreat:
                        CurrentNoteBaseScore += 2000;
                        CurrentNoteExtraScore += 40;
                        LostNoteBaseScore += 500;
                        LostNoteExtraScore += 60;
                        LostNoteExtraScoreClassic += 100;
                        break;
                    case JudgeGrade.LatePerfect3rd:
                    case JudgeGrade.FastPerfect3rd:
                        CurrentNoteBaseScore += 2500;
                        CurrentNoteExtraScore += 50;
                        LostNoteExtraScore += 50;
                        LostNoteExtraScoreClassic += 100;
                        break;
                    case JudgeGrade.LatePerfect2nd:
                    case JudgeGrade.FastPerfect2nd:
                        CurrentNoteBaseScore += 2500;
                        CurrentNoteExtraScore += 75;
                        CurrentNoteExtraScoreClassic += 50;
                        LostNoteExtraScore += 25;
                        LostNoteExtraScoreClassic += 50;
                        break;
                    case JudgeGrade.Perfect:
                        CurrentNoteBaseScore += 2500;
                        CurrentNoteExtraScore += 100;
                        CurrentNoteExtraScoreClassic += 100;
                        LostNoteExtraScore += 0;
                        LostNoteExtraScoreClassic += 0;
                        break;
                }
            }
        }
        /// <summary>
        /// Update Fast/Late count
        /// </summary>
        /// <param name="judgeResult"></param>
        void UpdateFastLateCount(in JudgeResult judgeResult)
        {
            var gameSetting = judgeResult.IsBreak ? MajInstances.Settings.Display.BreakFastLateType : MajInstances.Settings.Display.FastLateType;
            var resultValue = (int)judgeResult.Grade;
            var absValue = Math.Abs(7 - resultValue);

            switch (gameSetting)
            {
                case JudgeDisplayType.All:
                    if (judgeResult.Diff == 0 || judgeResult.IsMissOrTooFast)
                        break;
                    else if (judgeResult.IsFast)
                        _fastCount++;
                    else
                        _lateCount++;
                    break;
                case JudgeDisplayType.BelowCP:
                    if (judgeResult.IsMissOrTooFast || judgeResult.Grade == JudgeGrade.Perfect)
                        break;
                    else if (judgeResult.IsFast)
                        _fastCount++;
                    else
                        _lateCount++;
                    break;
                //默认只统计Great、Good的Fast/Late
                case JudgeDisplayType.BelowP:
                case JudgeDisplayType.BelowGR:
                case JudgeDisplayType.Disable:
                    if (judgeResult.IsMissOrTooFast || absValue <= 2)
                        break;
                    else if (judgeResult.IsFast)
                        _fastCount++;
                    else
                        _lateCount++;
                    break;
            }
        }
        #endregion
        readonly struct XxlbDanceRequest
        {
            public bool IsRequested { get; init; }
            public JudgeGrade Grade { get; init; }
        }
    }
}