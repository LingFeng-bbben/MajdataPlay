using System;
using System.Runtime.CompilerServices;
using MajdataPlay.Editor;
using MajdataPlay.Extensions;
using MajdataPlay.Numerics;
using UnityEngine;
#nullable enable
namespace MajdataPlay.Scenes.Game.Notes.Behaviours
{
    internal abstract class NoteLongDrop : NoteDrop
    {
        public float Length
        {
            get => _length;
            set => _length = value;
        }

        [ReadOnlyField]
        [SerializeField]
        protected float _playerReleaseTimeSec = 0;
        [ReadOnlyField]
        [SerializeField]
        protected float _length = 1f;

        protected readonly static Range<float> DEFAULT_HOLD_BODY_CHECK_RANGE = new Range<float>(float.MinValue, float.MinValue, ContainsType.Closed);
        protected readonly static Range<float> CLASSIC_HOLD_BODY_CHECK_RANGE = new Range<float>(float.MinValue, float.MaxValue, ContainsType.Closed);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual float GetRemainingTime() => MathF.Max(Length - GetTimeSpanToJudgeTiming(), 0);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual float GetRemainingTimeWithoutOffset() => MathF.Max(Length - GetTimeSpanToArriveTiming(), 0);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual float GetFakeRemainingTimeWithoutOffset() => MathF.Max(Length - GetFakeTimeSpanToArriveTiming(), 0);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected JudgeGrade HoldEndJudge(in JudgeGrade headGrade, in float ingoreTimeSec)
        {
            if (!_isJudged)
                return headGrade;

            var offset = (int)_judgeResult > 7 ? 0 : _judgeDiff;
            var realityHT = (Length - ingoreTimeSec - offset / 1000f).Clamp(0, Length - 0.3f);
            var percent = ((realityHT - _playerReleaseTimeSec) / realityHT).Clamp(0, 1);

            if (realityHT <= 0)
            {
                return headGrade;
            }
            //See also: https://www.bilibili.com/opus/694985211225571337/
            var pressGrade = percent switch
            {
                >= 1f => 0,   // >= 100%
                >= 0.67f => 1,// [0.67, 1)
                >= 0.33f => 2,// [0.33, 0.67)
                >= 0.05f => 3,// [0.05, 0.33)
                _ => 4,       // [0, 0.05)
            };

            switch (pressGrade)
            {
                case 0:// >= 100%
                    {
                        switch (headGrade)
                        {
                            case JudgeGrade.LatePerfect3rd:
                            case JudgeGrade.LatePerfect2nd:
                            case JudgeGrade.Perfect:
                            case JudgeGrade.FastPerfect2nd:
                            case JudgeGrade.FastPerfect3rd:
                                return headGrade;
                            case JudgeGrade.LateGood:
                            case JudgeGrade.LateGreat3rd:
                            case JudgeGrade.LateGreat2nd:
                            case JudgeGrade.LateGreat:
                                return JudgeGrade.LateGreat;
                            case JudgeGrade.FastGreat:
                            case JudgeGrade.FastGreat2nd:
                            case JudgeGrade.FastGreat3rd:
                            case JudgeGrade.FastGood:
                                return JudgeGrade.FastGreat;
                            case JudgeGrade.Miss:
                                return JudgeGrade.LateGood;
                            case JudgeGrade.TooFast:
                                return JudgeGrade.FastGood;
                        }
                    }
                    break;
                case 1:// [0.67, 1)
                    {
                        switch (headGrade)
                        {
                            case JudgeGrade.Perfect:
                                if (_judgeDiff >= 0)
                                {
                                    return JudgeGrade.LatePerfect2nd;
                                }
                                else
                                {
                                    return JudgeGrade.FastPerfect2nd;
                                }
                            case JudgeGrade.LatePerfect3rd:
                            case JudgeGrade.LatePerfect2nd:
                            case JudgeGrade.FastPerfect2nd:
                            case JudgeGrade.FastPerfect3rd:
                                return headGrade;
                            case JudgeGrade.LateGood:
                            case JudgeGrade.LateGreat3rd:
                            case JudgeGrade.LateGreat2nd:
                            case JudgeGrade.LateGreat:
                                return JudgeGrade.LateGreat;
                            case JudgeGrade.FastGreat:
                            case JudgeGrade.FastGreat2nd:
                            case JudgeGrade.FastGreat3rd:
                            case JudgeGrade.FastGood:
                                return JudgeGrade.FastGreat;
                            case JudgeGrade.Miss:
                                return JudgeGrade.LateGood;
                            case JudgeGrade.TooFast:
                                return JudgeGrade.FastGood;
                        }
                    }
                    break;
                case 2:// [0.33, 0.67)
                    {
                        switch (headGrade)
                        {
                            case JudgeGrade.Perfect:
                                if (_judgeDiff >= 0)
                                {
                                    return JudgeGrade.LateGreat2nd;
                                }
                                else
                                {
                                    return JudgeGrade.FastGreat2nd;
                                }
                            case JudgeGrade.LateGood:
                            case JudgeGrade.LateGreat3rd:
                            case JudgeGrade.LateGreat2nd:
                            case JudgeGrade.LateGreat:
                            case JudgeGrade.LatePerfect3rd:
                            case JudgeGrade.LatePerfect2nd:
                                return JudgeGrade.LateGreat;
                            case JudgeGrade.FastPerfect2nd:
                            case JudgeGrade.FastPerfect3rd:
                            case JudgeGrade.FastGreat:
                            case JudgeGrade.FastGreat2nd:
                            case JudgeGrade.FastGreat3rd:
                            case JudgeGrade.FastGood:
                                return JudgeGrade.FastGreat;
                            case JudgeGrade.Miss:
                                return JudgeGrade.LateGood;
                            case JudgeGrade.TooFast:
                                return JudgeGrade.FastGood;
                        }
                    }
                    break;
                case 3:// [0.05, 0.33)
                    {
                        switch (headGrade)
                        {
                            case JudgeGrade.Perfect:
                                if (_judgeDiff >= 0)
                                {
                                    return JudgeGrade.LateGood;
                                }
                                else
                                {
                                    return JudgeGrade.FastGood;
                                }
                            case JudgeGrade.Miss:
                            case JudgeGrade.LateGood:
                            case JudgeGrade.LateGreat3rd:
                            case JudgeGrade.LateGreat2nd:
                            case JudgeGrade.LateGreat:
                            case JudgeGrade.LatePerfect3rd:
                            case JudgeGrade.LatePerfect2nd:
                                return JudgeGrade.LateGood;
                            case JudgeGrade.FastPerfect2nd:
                            case JudgeGrade.FastPerfect3rd:
                            case JudgeGrade.FastGreat:
                            case JudgeGrade.FastGreat2nd:
                            case JudgeGrade.FastGreat3rd:
                            case JudgeGrade.FastGood:
                            case JudgeGrade.TooFast:
                                return JudgeGrade.FastGood;
                        }
                    }
                    break;
                case 4:// [0, 0.05)
                    {
                        switch (headGrade)
                        {
                            case JudgeGrade.Perfect:
                                if (_judgeDiff >= 0)
                                {
                                    return JudgeGrade.LateGood;
                                }
                                else
                                {
                                    return JudgeGrade.FastGood;
                                }
                            case JudgeGrade.LateGood:
                            case JudgeGrade.LateGreat3rd:
                            case JudgeGrade.LateGreat2nd:
                            case JudgeGrade.LateGreat:
                            case JudgeGrade.LatePerfect3rd:
                            case JudgeGrade.LatePerfect2nd:
                                return JudgeGrade.LateGood;
                            case JudgeGrade.FastPerfect2nd:
                            case JudgeGrade.FastPerfect3rd:
                            case JudgeGrade.FastGreat:
                            case JudgeGrade.FastGreat2nd:
                            case JudgeGrade.FastGreat3rd:
                            case JudgeGrade.FastGood:
                                return JudgeGrade.FastGood;
                            case JudgeGrade.Miss:
                            case JudgeGrade.TooFast:
                                return headGrade;
                        }
                    }
                    break;
            }

            throw new ArgumentOutOfRangeException();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected JudgeGrade HoldClassicEndJudge(in JudgeGrade headGrade,float offset)
        {
            if (!_isJudged)
                return headGrade;
            else if (headGrade.IsMissOrTooFast())
                return headGrade;

            var releaseTiming = ThisFrameSec - USERSETTING_JUDGE_OFFSET_SEC - offset;
            var diffSec = Timing + Length - releaseTiming;
            var isFast = diffSec > 0;
            var diffMSec = MathF.Abs(diffSec) * 1000;

            var endGrade = diffMSec switch
            {
                <= HOLD_CLASSIC_END_JUDGE_PERFECT_AREA_MSEC => JudgeGrade.Perfect,
                _ => isFast ? JudgeGrade.FastGood : JudgeGrade.LateGood
            };

            var num = Math.Abs(7 - (int)headGrade);
            var endNum = Math.Abs(7 - (int)endGrade);
            if (endNum > num) // 取最差判定
                return endGrade;
            else
                return headGrade;
        }
    }
}