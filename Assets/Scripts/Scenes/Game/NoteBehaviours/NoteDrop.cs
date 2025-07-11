using MajdataPlay.IO;
using MajdataPlay.Utils;
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using MajdataPlay.Editor;
using UnityEngine;
using Random = System.Random;
using MajdataPlay.View;
using MajdataPlay.Game.Notes.Controllers;
using MajdataPlay.Numerics;
#nullable enable
namespace MajdataPlay.Game.Notes.Behaviours
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
            get => USERSETTING_SLIDE_JUDGE_MODE == JudgeMode.Classic;
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
            get => _judgeTiming + USERSETTING_JUDGE_OFFSET;
        }
        public float ThisFrameSec
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _noteController.ThisFrameSec;
        }

        protected bool IsUseButtonRingForTouch
        {
            get; private set;
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
        protected GameModInfo ModInfo
        {
            get;
            private set;
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

        readonly protected ObjectCounter _objectCounter = Majdata<ObjectCounter>.Instance!;
        readonly protected NoteManager _noteManager = Majdata<NoteManager>.Instance!;
        readonly protected NoteEffectManager _effectManager = Majdata<NoteEffectManager>.Instance!;
        readonly protected NoteAudioManager _audioEffMana = Majdata<NoteAudioManager>.Instance!;
        readonly protected GameSetting _gameSetting = MajInstances.Settings;
        protected static readonly Random _randomizer = new();

        protected readonly float USERSETTING_JUDGE_OFFSET = (MajInstances.Settings?.Judge.JudgeOffset ?? 0)  + (MajInstances.Settings?.Debug.DisplayOffset ?? 0);
        protected readonly float USERSETTING_TOUCHPANEL_OFFSET = MajInstances.Settings?.Judge.TouchPanelOffset ?? 0;
        protected readonly float USERSETTING_TAP_SCALE = MajInstances.Settings?.Display.TapScale ?? 1;
        protected readonly float USERSETTING_HOLD_SCALE = MajInstances.Settings?.Display.HoldScale ?? 1;
        protected readonly float USERSETTING_TOUCH_SCALE = MajInstances.Settings?.Display.TouchScale ?? 1;
        protected readonly float USERSETTING_SLIDE_SCALE = MajInstances.Settings?.Display.SlideScale ?? 1;
        protected readonly bool USERSETTING_DISPLAY_HOLD_HEAD_JUDGE_RESULT = MajInstances.Settings?.Display.DisplayHoldHeadJudgeResult ?? false;
        protected readonly JudgeMode USERSETTING_SLIDE_JUDGE_MODE = MajInstances.Settings?.Judge.Mode ?? JudgeMode.Modern;
        protected readonly DJAutoPolicy USERSETTING_DJAUTO_POLICY = MajInstances.Settings?.Debug.DJAutoPolicy ?? DJAutoPolicy.Strict;

        protected const float FRAME_LENGTH_SEC = 1f / 60;
        protected const float FRAME_LENGTH_MSEC = FRAME_LENGTH_SEC * 1000;

        protected const float TAP_JUDGE_SEG_1ST_PERFECT_MSEC = 1 * FRAME_LENGTH_MSEC;
        protected const float TAP_JUDGE_SEG_2ND_PERFECT_MSEC = 2 * FRAME_LENGTH_MSEC;
        protected const float TAP_JUDGE_SEG_3RD_PERFECT_MSEC = 3 * FRAME_LENGTH_MSEC;
        protected const float TAP_JUDGE_SEG_1ST_GREAT_MSEC = 4 * FRAME_LENGTH_MSEC;
        protected const float TAP_JUDGE_SEG_2ND_GREAT_MSEC = 5 * FRAME_LENGTH_MSEC;
        protected const float TAP_JUDGE_SEG_3RD_GREAT_MSEC = 6 * FRAME_LENGTH_MSEC;
        protected const float TAP_JUDGE_GOOD_AREA_MSEC = 9 * FRAME_LENGTH_MSEC;

        protected const float HOLD_CLASSIC_END_JUDGE_PERFECT_AREA_MSEC = 12 * FRAME_LENGTH_MSEC;

        protected const float TOUCH_JUDGE_SEG_1ST_PERFECT_MSEC = 9 * FRAME_LENGTH_MSEC;
        protected const float TOUCH_JUDGE_SEG_2ND_PERFECT_MSEC = 10.5f * FRAME_LENGTH_MSEC;
        protected const float TOUCH_JUDGE_SEG_3RD_PERFECT_MSEC = 12 * FRAME_LENGTH_MSEC;
        protected const float TOUCH_JUDGE_SEG_1ST_GREAT_MSEC = 13 * FRAME_LENGTH_MSEC;
        protected const float TOUCH_JUDGE_SEG_2ND_GREAT_MSEC = 14 * FRAME_LENGTH_MSEC;
        protected const float TOUCH_JUDGE_SEG_3RD_GREAT_MSEC = 15 * FRAME_LENGTH_MSEC;
        protected const float TOUCH_JUDGE_GOOD_AREA_MSEC = 18 * FRAME_LENGTH_MSEC;

        protected const float SLIDE_JUDGE_MAXIMUM_ALLOWED_EXT_LENGTH_MSEC = 22 * FRAME_LENGTH_MSEC;
        protected const float SLIDE_JUDGE_SEG_BASE_3RD_PERFECT_MSEC = 14 * FRAME_LENGTH_MSEC;
        protected const float SLIDE_JUDGE_SEG_1ST_GREAT_MSEC = 21 * FRAME_LENGTH_MSEC;
        protected const float SLIDE_JUDGE_SEG_2ND_GREAT_MSEC = 25 * FRAME_LENGTH_MSEC;
        protected const float SLIDE_JUDGE_SEG_3RD_GREAT_MSEC = 29 * FRAME_LENGTH_MSEC;

        protected const float SLIDE_JUDGE_CLASSIC_SEG_BASE_3RD_PERFECT_MSEC = 9 * FRAME_LENGTH_MSEC; // 9f
        protected const float SLIDE_JUDGE_CLASSIC_SEG_1ST_GREAT_MSEC = 15 * FRAME_LENGTH_MSEC;  // 15f
        protected const float SLIDE_JUDGE_CLASSIC_SEG_2ND_GREAT_MSEC = 21 * FRAME_LENGTH_MSEC;  // 21f
        protected const float SLIDE_JUDGE_CLASSIC_SEG_3RD_GREAT_MSEC = 27 * FRAME_LENGTH_MSEC;  // 27f
        protected const float SLIDE_JUDGE_GOOD_AREA_MSEC = 36 * FRAME_LENGTH_MSEC;              // 36f

        protected const float HOLD_HEAD_IGNORE_LENGTH_SEC = 6 * FRAME_LENGTH_SEC;
        protected const float HOLD_TAIL_IGNORE_LENGTH_SEC = 12 * FRAME_LENGTH_SEC;
        protected const float TOUCHHOLD_HEAD_IGNORE_LENGTH_SEC = 15 * FRAME_LENGTH_SEC;
        protected const float TOUCHHOLD_TAIL_IGNORE_LENGTH_SEC = 12 * FRAME_LENGTH_SEC;
        protected const float DELUXE_HOLD_RELEASE_IGNORE_TIME_SEC = 2 * FRAME_LENGTH_SEC;
        protected const float CLASSIC_HOLD_ALLOW_OVER_LENGTH_SEC = 20 * FRAME_LENGTH_SEC;
        protected override void Awake()
        {
            base.Awake();

            _noteController = Majdata<INoteController>.Instance!;
            ModInfo = _noteController.ModInfo;
            _breakMaterial = _noteController.BreakMaterial;
            _defaultMaterial = _noteController.DefaultMaterial;
            _holdShineMaterial = _noteController.HoldShineMaterial;
            _isAutoplay = _noteController.IsAutoplay;
            _autoplayMode = ModInfo.AutoPlay;
            _autoplayGrade = _noteController.AutoplayGrade;
            
            IsUseButtonRingForTouch = ModInfo.ButtonRingForTouch;
        }
        void OnDestroy()
        {
            Active = false;
        }
        protected abstract void LoadSkin();
        protected abstract void PlaySFX();
        protected abstract void PlayJudgeSFX(in NoteJudgeResult judgeResult);
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
                _ => isFast ? JudgeGrade.FastGood : JudgeGrade.LateGood
            };

            if (IsEX)
            {
                result = JudgeGrade.Perfect;
            }

            ConvertJudgeGrade(ref result);
            _judgeResult = result;
            _isJudged = true;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void Autoplay()
        {
            if (_isJudged || !IsAutoplay)
                return;
            if (GetTimeSpanToArriveTiming() >= -0.016667f)
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
        protected void ConvertJudgeGrade(ref JudgeGrade grade)
        {
            var judgeStyle = ModInfo.JudgeStyle;
            switch (judgeStyle)
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
            switch (judgeType)
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