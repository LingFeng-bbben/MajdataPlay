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
using MajdataPlay.View;
#nullable enable
namespace MajdataPlay.Game.Notes
{
    internal abstract class NoteDrop : MajComponent, IStatefulNote
    {
        public int StartPos 
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _startPos;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _timing;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _timing = value; 
        }
        public int SortOrder 
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _sortOrder;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _sortOrder = value; 
        }
        public float Speed 
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _speed;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _speed = value; 
        }
        public bool IsEach 
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _isEach;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _isEach = value; 
        }
        public bool IsBreak 
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _isBreak;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _isBreak = value; 
        }
        public bool IsEX 
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _isEX;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _isEX = value; 
        }
        public bool IsInitialized
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => State >= NoteStatus.Initialized;
        }
        public bool IsEnded
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => State == NoteStatus.End;
        }
        public bool IsClassic
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _gameSetting.Judge.Mode == JudgeMode.Classic;
        }
        public NoteStatus State 
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _state;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            protected set => _state = value; 
        }
        public float JudgeTiming
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _judgeTiming + _gameSetting.Judge.JudgeOffset;
        }
        public float ThisFrameSec
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _noteController.ThisFrameSec;
        }

        protected INoteController NoteController
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _noteController;
        }
        protected bool IsAutoplay
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _isAutoplay;
        }
        protected AutoplayMode AutoplayMode
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _autoplayMode;
        }
        protected JudgeGrade AutoplayGrade
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _autoplayGrade;
        }
        protected Material BreakMaterial
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _breakMaterial;
        }
        protected Material DefaultMaterial
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _defaultMaterial;
        }
        protected Material HoldShineMaterial
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _holdShineMaterial;
        }

        protected bool _isJudged = false;
        /// <summary>
        /// The answer frame
        /// </summary>
        protected float _judgeTiming;
        protected float _judgeDiff = -1;
        protected Range<float> _judgableRange = new(float.MinValue, float.MinValue + 1, ContainsType.Closed);
        protected JudgeGrade _judgeResult = JudgeGrade.Miss;

        protected SensorArea _sensorPos;

        readonly protected ObjectCounter _objectCounter = MajInstanceHelper<ObjectCounter>.Instance!;
        readonly protected NoteManager _noteManager = MajInstanceHelper<NoteManager>.Instance!;
        readonly protected NoteEffectManager _effectManager = MajInstanceHelper<NoteEffectManager>.Instance!;
        readonly protected NoteAudioManager _audioEffMana = MajInstanceHelper<NoteAudioManager>.Instance!;
        readonly protected InputManager _ioManager = MajInstances.InputManager;
        readonly protected GameSetting _gameSetting = MajInstances.Setting;
        protected static readonly Random _randomizer = new();

        protected const float TAP_JUDGE_SEG_1ST_PERFECT_MSEC = 16.666666f;
        protected const float TAP_JUDGE_SEG_2ND_PERFECT_MSEC = 2 * TAP_JUDGE_SEG_1ST_PERFECT_MSEC;
        protected const float TAP_JUDGE_SEG_3RD_PERFECT_MSEC = 3 * TAP_JUDGE_SEG_1ST_PERFECT_MSEC;
        protected const float TAP_JUDGE_SEG_1ST_GREAT_MSEC = 4 * TAP_JUDGE_SEG_1ST_PERFECT_MSEC;
        protected const float TAP_JUDGE_SEG_2ND_GREAT_MSEC = 5 * TAP_JUDGE_SEG_1ST_PERFECT_MSEC;
        protected const float TAP_JUDGE_SEG_3RD_GREAT_MSEC = 6 * TAP_JUDGE_SEG_1ST_PERFECT_MSEC;
        protected const float TAP_JUDGE_GOOD_AREA_MSEC = 9 * TAP_JUDGE_SEG_1ST_PERFECT_MSEC;

        protected const float HOLD_CLASSIC_END_JUDGE_SEG_1ST_PERFECT_MSEC = 66.666666f;
        protected const float HOLD_CLASSIC_END_JUDGE_SEG_2ND_PERFECT_MSEC = 133.333333f;
        protected const float HOLD_CLASSIC_END_JUDGE_SEG_3RD_PERFECT_MSEC = 200f;

        protected const float TOUCH_JUDGE_SEG_1ST_PERFECT_MSEC = 150f;
        protected const float TOUCH_JUDGE_SEG_2ND_PERFECT_MSEC = 175f;
        protected const float TOUCH_JUDGE_SEG_3RD_PERFECT_MSEC = 200f;
        protected const float TOUCH_JUDGE_SEG_1ST_GREAT_MSEC = 216.666666f;
        protected const float TOUCH_JUDGE_SEG_2ND_GREAT_MSEC = 233.333333f;
        protected const float TOUCH_JUDGE_SEG_3RD_GREAT_MSEC = 250f;
        protected const float TOUCH_JUDGE_GOOD_AREA_MSEC = 316.666666f;

        protected const float SLIDE_JUDGE_MAXIMUM_ALLOWED_EXT_LENGTH_MSEC = 366.666666f;
        protected const float SLIDE_JUDGE_SEG_BASE_3RD_PERFECT_MSEC = 233.333333f;
        protected const float SLIDE_JUDGE_SEG_1ST_GREAT_MSEC = 350f;
        protected const float SLIDE_JUDGE_SEG_2ND_GREAT_MSEC = 416.666666f;
        protected const float SLIDE_JUDGE_SEG_3RD_GREAT_MSEC = 483.333333f;

        protected const float SLIDE_JUDGE_CLASSIC_SEG_1ST_PERFECT_MSEC = 50f;  // 3f
        protected const float SLIDE_JUDGE_CLASSIC_SEG_2ND_PERFECT_MSEC = 100f; // 6f
        protected const float SLIDE_JUDGE_CLASSIC_SEG_3RD_PERFECT_MSEC = 150f; // 9f
        protected const float SLIDE_JUDGE_CLASSIC_SEG_1ST_GREAT_MSEC = 250f;   // 15f
        protected const float SLIDE_JUDGE_CLASSIC_SEG_2ND_GREAT_MSEC = 350f;   // 21f
        protected const float SLIDE_JUDGE_CLASSIC_SEG_3RD_GREAT_MSEC = 450f;   // 27f
        protected const float SLIDE_JUDGE_GOOD_AREA_MSEC = 600f;               // 36f

        protected const float HOLD_HEAD_IGNORE_LENGTH_SEC = 0.1f;
        protected const float HOLD_TAIL_IGNORE_LENGTH_SEC = 0.2f;
        protected const float TOUCHHOLD_HEAD_IGNORE_LENGTH_SEC = 0.25f;
        protected const float TOUCHHOLD_TAIL_IGNORE_LENGTH_SEC = 0.2f;
        protected const float DELUXE_HOLD_RELEASE_IGNORE_TIME_SEC = 0.03333333f;
        protected const float CLASSIC_HOLD_ALLOW_OVER_LENGTH_SEC = 0.333333f;
        protected override void Awake()
        {
            base.Awake();

            _noteController = MajInstanceHelper<INoteController>.Instance!;

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
            if (_isJudged)
                return;

            var diffSec = currentSec - JudgeTiming;
            var isFast = diffSec < 0;
            _judgeDiff = diffSec * 1000;
            var diffMSec = MathF.Abs(diffSec * 1000);
            var result = diffMSec switch
            {
                <= TAP_JUDGE_SEG_1ST_PERFECT_MSEC => JudgeGrade.Perfect,
                <= TAP_JUDGE_SEG_2ND_PERFECT_MSEC => isFast ? JudgeGrade.FastPerfect2nd : JudgeGrade.LatePerfect2nd,
                <= TAP_JUDGE_SEG_3RD_PERFECT_MSEC => isFast ? JudgeGrade.FastPerfect3rd : JudgeGrade.LatePerfect3rd,
                <= TAP_JUDGE_SEG_1ST_GREAT_MSEC => isFast ? JudgeGrade.FastGreat : JudgeGrade.LateGreat,
                <= TAP_JUDGE_SEG_2ND_GREAT_MSEC => isFast ? JudgeGrade.FastGreat2nd : JudgeGrade.LateGreat2nd,
                <= TAP_JUDGE_SEG_3RD_GREAT_MSEC => isFast ? JudgeGrade.FastGreat3rd : JudgeGrade.LateGreat3rd,
                <= TAP_JUDGE_GOOD_AREA_MSEC => isFast ? JudgeGrade.FastGood : JudgeGrade.LateGood,
                _ => isFast ? JudgeGrade.TooFast : JudgeGrade.Miss
            };

            if (result is JudgeGrade.TooFast)
                return;
            else if (result != JudgeGrade.Miss && IsEX)
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
                var autoplayGrade = NoteController.AutoplayGrade;
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
        protected virtual float GetTimeSpanToArriveTiming() => ThisFrameSec - Timing;
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
            var judgeStyle = NoteController.JudgeStyle;
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
                case JudgeGrade.LateGreat2nd:
                case JudgeGrade.LateGreat3rd:
                case JudgeGrade.FastGreat:
                case JudgeGrade.FastGreat2nd:
                case JudgeGrade.FastGreat3rd:
                    if (isFast)
                        judgeType = JudgeGrade.FastGood;
                    else
                        judgeType = JudgeGrade.LateGood;
                    break;
                case JudgeGrade.LatePerfect3rd:
                case JudgeGrade.FastPerfect3rd:
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
                case JudgeGrade.LatePerfect2nd:
                case JudgeGrade.FastPerfect2nd:
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
                case JudgeGrade.LatePerfect3rd:
                case JudgeGrade.FastPerfect3rd:
                    if (isFast)
                        judgeType = JudgeGrade.FastGood;
                    else
                        judgeType = JudgeGrade.LateGood;
                    break;
                case JudgeGrade.LatePerfect2nd:
                case JudgeGrade.FastPerfect2nd:
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
        [ReadOnlyField, SerializeField]
        int _startPos = 1;
        [ReadOnlyField, SerializeField]
        float _timing;
        [ReadOnlyField, SerializeField]
        float _speed = 7;
        [ReadOnlyField, SerializeField]
        int _sortOrder;
        [ReadOnlyField, SerializeField]
        bool _isEach = false;
        [ReadOnlyField, SerializeField]
        bool _isBreak = false;
        [ReadOnlyField, SerializeField]
        bool _isEX = false;
        [ReadOnlyField, SerializeField]
        bool _isAutoplay = false;
        [ReadOnlyField, SerializeField]
        JudgeGrade _autoplayGrade = JudgeGrade.Perfect;
        [ReadOnlyField, SerializeField]
        AutoplayMode _autoplayMode = AutoplayMode.Disable;
        [ReadOnlyField, SerializeField]
        NoteStatus _state = NoteStatus.Start;

        Material _breakMaterial;
        Material _defaultMaterial;
        Material _holdShineMaterial;

        INoteController _noteController;
    }
}