using MajdataPlay.Extensions;
using MajdataPlay.IO;
using MajdataPlay.Types;
using MajdataPlay.Attributes;
using MajdataPlay.Utils;
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;
using Random = System.Random;
using MajdataPlay.Game.Types;
#nullable enable
namespace MajdataPlay.Game.Notes
{
    internal abstract class NoteDrop : MajComponent, IStatefulNote
    {
        public int StartPos 
        { 
            get => _startPos; 
            set
            {
                if (value.InRange(1, 8))
                    _startPos = value;
                else
                    throw new ArgumentOutOfRangeException("Start position must be between 1 and 8");
            } 
        }
        public float Timing 
        { 
            get => _timing; 
            set => _timing = value; 
        }
        public int SortOrder 
        { 
            get => _sortOrder; 
            set => _sortOrder = value; 
        }
        public float Speed 
        { 
            get => _speed; 
            set => _speed = value; 
        }
        public bool IsEach 
        { 
            get => _isEach; 
            set => _isEach = value; 
        }
        public bool IsBreak 
        { 
            get => _isBreak; 
            set => _isBreak = value; 
        }
        public bool IsEX 
        { 
            get => _isEX;
            set => _isEX = value; 
        }


        public bool IsInitialized => State >= NoteStatus.Initialized;
        public bool IsEnded => State == NoteStatus.End;
        public bool IsClassic => _gameSetting.Judge.Mode == JudgeMode.Classic;
        public NoteStatus State 
        { 
            get => _state; 
            protected set => _state = value; 
        }
        public float JudgeTiming => _judgeTiming + _gameSetting.Judge.JudgeOffset;
        public float ThisFrameSec => _noteController.ThisFrameSec;
        public float ThisFixedUpdateSec => _noteController.ThisFixedUpdateSec;

        protected INoteController NoteController => _noteController;
        protected bool IsAutoplay => _isAutoplay;
        protected AutoplayMode AutoplayMode => _autoplayMode;
        protected JudgeGrade AutoplayGrade => _autoplayGrade;
        protected Material BreakMaterial => _breakMaterial;
        protected Material DefaultMaterial => _defaultMaterial;
        protected Material HoldShineMaterial => _holdShineMaterial;

        [ReadOnlyField]
        [SerializeField]
        protected NoteStatus _state = NoteStatus.Start;
        protected GamePlayManager _gpManager;
        protected InputManager _ioManager = MajInstances.InputManager;
        protected bool _isJudged = false;
        /// <summary>
        /// The answer frame
        /// </summary>
        protected float _judgeTiming;
        protected float _judgeDiff = -1;
        protected Range<float> _judgableRange = new(float.MinValue, float.MinValue + 1, ContainsType.Closed);
        protected JudgeGrade _judgeResult = JudgeGrade.Miss;

        protected SensorType _sensorPos;
        protected ObjectCounter _objectCounter;
        protected NoteManager _noteManager;
        protected NoteEffectManager _effectManager;
        protected NoteAudioManager _audioEffMana;
        protected GameSetting _gameSetting = MajInstances.Setting;
        protected EventHandler<InputEventArgs> _noteChecker;
        protected static readonly Random _randomizer = new();

        Material _breakMaterial;
        Material _defaultMaterial;
        Material _holdShineMaterial;


        bool _isAutoplay = false;
        JudgeGrade _autoplayGrade = JudgeGrade.Perfect;
        AutoplayMode _autoplayMode = AutoplayMode.Disable;
        INoteController _noteController;
        protected override void Awake()
        {
            base.Awake();
            _effectManager = FindObjectOfType<NoteEffectManager>();
            _objectCounter = FindObjectOfType<ObjectCounter>();
            _noteManager = FindObjectOfType<NoteManager>();
            _audioEffMana = FindObjectOfType<NoteAudioManager>();
            _gpManager = FindObjectOfType<GamePlayManager>();
            _noteController = _gpManager;

            _breakMaterial = _noteController.BreakMaterial;
            _defaultMaterial = _noteController.DefaultMaterial;
            _holdShineMaterial = _noteController.HoldShineMaterial;
            _isAutoplay = _noteController.IsAutoplay;
            _autoplayGrade = _noteController.AutoplayGrade;
            _autoplayMode = _noteController.AutoplayMode;
        }
        void OnDestroy()
        {
            Active = false;
        }
        protected abstract void LoadSkin();
        protected abstract void PlaySFX();
        protected abstract void PlayJudgeSFX(in JudgeResult judgeResult);
        protected virtual void Judge(float currentSec)
        {
            const int JUDGE_GOOD_AREA = 150;
            const int JUDGE_GREAT_AREA = 100;
            const int JUDGE_PERFECT_AREA = 50;

            const float JUDGE_SEG_PERFECT1 = 16.66667f;
            const float JUDGE_SEG_PERFECT2 = 33.33334f;
            const float JUDGE_SEG_GREAT1 = 66.66667f;
            const float JUDGE_SEG_GREAT2 = 83.33334f;

            if (_isJudged)
                return;

            //var timing = GetTimeSpanToJudgeTiming();
            var timing = currentSec - JudgeTiming;
            var isFast = timing < 0;
            _judgeDiff = timing * 1000;
            var diff = MathF.Abs(timing * 1000);

            if (diff > JUDGE_GOOD_AREA && isFast)
                return;
            var result = diff switch
            {
                <= JUDGE_SEG_PERFECT1 => JudgeGrade.Perfect,
                <= JUDGE_SEG_PERFECT2 => JudgeGrade.LatePerfect1,
                <= JUDGE_PERFECT_AREA => JudgeGrade.LatePerfect2,
                <= JUDGE_SEG_GREAT1 => JudgeGrade.LateGreat,
                <= JUDGE_SEG_GREAT2 => JudgeGrade.LateGreat1,
                <= JUDGE_GREAT_AREA => JudgeGrade.LateGreat,
                <= JUDGE_GOOD_AREA => JudgeGrade.LateGood,
                _ => JudgeGrade.Miss
            };

            if (result != JudgeGrade.Miss && isFast)
                result = 14 - result;
            if (result != JudgeGrade.Miss && IsEX)
                result = JudgeGrade.Perfect;

            ConvertJudgeGrade(ref result);
            _judgeResult = result;
            _isJudged = true;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void Autoplay()
        {
            if (_isJudged || !IsAutoplay)
                return;
            if (GetTimeSpanToJudgeTiming() >= -0.016667f)
            {
                var autoplayGrade = _gpManager.AutoplayGrade;
                if (((int)autoplayGrade).InRange(0, 14))
                    _judgeResult = autoplayGrade;
                else
                    _judgeResult = (JudgeGrade)_randomizer.Next(0, 15);
                ConvertJudgeGrade(ref _judgeResult);
                _isJudged = true;
                _judgeDiff = _judgeResult switch
                {
                    < JudgeGrade.Perfect => 1,
                    > JudgeGrade.Perfect => -1,
                    _ => 0
                };
            }
        }

        /// <summary>
        /// Gets the time offset from the current moment to the judgment line.
        /// </summary>
        /// <returns>
        /// If the current moment is behind the judgment line, the result is a positive number.
        /// <para>If the current moment is ahead of the judgment line, the result is a negative number.</para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected float GetTimeSpanToArriveTiming() => ThisFrameSec - Timing;
        /// <summary>
        /// Gets the time offset from the current moment to the answer frame.
        /// </summary>
        /// <returns>
        /// If the current moment is behind the answer frame, the result is a positive number.
        /// <para>If the current moment is ahead of the answer frame, the result is a negative number.</para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected float GetTimeSpanToJudgeTiming() => ThisFrameSec - JudgeTiming;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected float GetTimeSpanToJudgeTiming(float baseTiming) => baseTiming - JudgeTiming;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Vector3 GetPositionFromDistance(float distance) => GetPositionFromDistance(distance, StartPos);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 GetPositionFromDistance(float distance, int position)
        {
            return new Vector3(
                distance * Mathf.Cos((position * -2f + 5f) * 0.125f * Mathf.PI),
                distance * Mathf.Sin((position * -2f + 5f) * 0.125f * Mathf.PI));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 GetPositionFromDistance(float distance, float position)
        {
            return new Vector3(
                distance * Mathf.Cos((position * -2f + 5f) * 0.125f * Mathf.PI),
                distance * Mathf.Sin((position * -2f + 5f) * 0.125f * Mathf.PI));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void ConvertJudgeGrade(ref JudgeGrade grade)
        {
            var judgeStyle = _gpManager.JudgeStyle;
            switch(judgeStyle)
            {
                case JudgeStyleType.MAJI:
                    ConvertToMAJI(ref grade); 
                    break;
                case JudgeStyleType.GACHI:
                    ConvertToGACHI(ref grade);
                    break;
                case JudgeStyleType.GORI:
                    ConvertToGORI(ref grade);
                    break;
                case JudgeStyleType.DEFAULT:
                default:
                    return;
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void ConvertToMAJI(ref JudgeGrade judgeType)
        {
            var isFast = (int)judgeType > 7;
            switch(judgeType)
            {
                case JudgeGrade.LateGreat:
                case JudgeGrade.LateGreat1:
                case JudgeGrade.LateGreat2:
                case JudgeGrade.FastGreat:
                case JudgeGrade.FastGreat1:
                case JudgeGrade.FastGreat2:
                    if (isFast)
                        judgeType = JudgeGrade.FastGood;
                    else
                        judgeType = JudgeGrade.LateGood;
                    break;
                case JudgeGrade.LatePerfect2:
                case JudgeGrade.FastPerfect2:
                    if (isFast)
                        judgeType = JudgeGrade.FastGreat;
                    else
                        judgeType = JudgeGrade.LateGreat;
                    break;
                default:
                    if (judgeType > JudgeGrade.Perfect)
                        judgeType = JudgeGrade.TooFast;
                    else if (judgeType < JudgeGrade.Perfect)
                        judgeType = JudgeGrade.Miss;
                    break;
                case JudgeGrade.LatePerfect1:
                case JudgeGrade.FastPerfect1:
                case JudgeGrade.Perfect:
                case JudgeGrade.Miss:
                case JudgeGrade.TooFast:
                    return;
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void ConvertToGACHI(ref JudgeGrade judgeType)
        {
            var isFast = (int)judgeType > 7;
            switch (judgeType)
            {                    
                case JudgeGrade.LatePerfect2:
                case JudgeGrade.FastPerfect2:
                    if (isFast)
                        judgeType = JudgeGrade.FastGood;
                    else
                        judgeType = JudgeGrade.LateGood;
                    break;
                case JudgeGrade.LatePerfect1:
                case JudgeGrade.FastPerfect1:
                    if (isFast)
                        judgeType = JudgeGrade.FastGreat;
                    else
                        judgeType = JudgeGrade.LateGreat;
                    break;
                default:
                    if (judgeType > JudgeGrade.Perfect)
                        judgeType = JudgeGrade.TooFast;
                    else if (judgeType < JudgeGrade.Perfect)
                        judgeType = JudgeGrade.Miss;
                    break;
                case JudgeGrade.Perfect:
                case JudgeGrade.Miss:
                    return;
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void ConvertToGORI(ref JudgeGrade judgeType)
        {
            switch (judgeType)
            { 
                case JudgeGrade.Perfect:
                case JudgeGrade.Miss:
                    return;
                default:
                    if (judgeType > JudgeGrade.Perfect)
                        judgeType = JudgeGrade.TooFast;
                    else if (judgeType < JudgeGrade.Perfect)
                        judgeType = JudgeGrade.Miss;
                    break;
            }
        }
        [ReadOnlyField]
        [SerializeField]
        protected int _startPos = 1;
        [ReadOnlyField]
        [SerializeField]
        protected float _timing;
        [ReadOnlyField]
        [SerializeField]
        protected float _speed = 7;
        [ReadOnlyField]
        [SerializeField]
        protected int _sortOrder;
        [ReadOnlyField]
        [SerializeField]
        protected bool _isEach = false;
        [ReadOnlyField]
        [SerializeField]
        protected bool _isBreak = false;
        [ReadOnlyField]
        [SerializeField]
        protected bool _isEX = false;
    }
}