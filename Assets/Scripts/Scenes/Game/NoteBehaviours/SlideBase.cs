using Cysharp.Threading.Tasks;
using MajdataPlay.Collections;
using MajdataPlay.Extensions;
using MajdataPlay.Game.Controllers;
using MajdataPlay.IO;
using MajdataPlay.Types;
using MajdataPlay.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Runtime.CompilerServices;
using MajdataPlay.Editor;
using MajdataPlay.Game.Notes.Slide;

#nullable enable
namespace MajdataPlay.Game.Notes
{
    internal abstract class SlideBase : NoteLongDrop
    {
        public IConnectableSlide? Parent => ConnectInfo.Parent;
        public ConnSlideInfo ConnectInfo 
        { 
            [MethodImpl(MethodImplOptions.AggressiveInlining)] get; 
            [MethodImpl(MethodImplOptions.AggressiveInlining)] set; 
        } = new()
        {
            IsGroupPart = false,
            Parent = null
        };
        /// <summary>
        /// If all judgment areas have been completed, return True, otherwise False
        /// </summary>
        public bool IsFinished 
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => QueueRemaining == 0; 
        }
        /// <summary>
        /// Returns the number of unfinished judgment areas in the judgment queue
        /// </summary>
        public bool IsPendingFinish 
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => QueueRemaining == 1; 
        }
        /// <summary>
        /// 返回判定队列中未完成判定区的数量
        /// </summary>
        public int QueueRemaining 
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                Span<int> reamaining = stackalloc int[3];
                var judgeQueues = _judgeQueues.AsSpan();
                foreach (var (i, queue) in judgeQueues.WithIndex())
                    reamaining[i] = queue.Length;
                return reamaining.Max();
            }
        }
        /// <summary>
        /// a timing of slide start
        /// </summary>
        public float StartTiming
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _startTiming;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _startTiming = value;
        }
        public float ArriveTiming
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _startTiming + Length;
        }
        public bool IsJustR
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _isJustR;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _isJustR = value;
        }
        public float FadeInTiming
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _fadeInTiming;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _fadeInTiming = value;
        }
        public float FullFadeInTiming
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _fullFadeInTiming;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _fullFadeInTiming = value;
        }
        public int EndPos
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _endPos;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                if (value.InRange(1, 8))
                    _endPos = value;
                else
                    throw new ArgumentOutOfRangeException("End position must be between 1 and 8");
            }
        }
        public string SlideType
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _slideType;
        }
        public float SlideLength
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _slideLength;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            protected set => _slideLength = value;
        }
        public bool IsSlideNoHead 
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set; 
        } = false;
        public bool IsSlideNoTrack 
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set; 
        } = false;
        protected readonly Memory<SlideArea>[] _judgeQueues = new Memory<SlideArea>[3]
        { 
            Memory<SlideArea>.Empty,
            Memory<SlideArea>.Empty,
            Memory<SlideArea>.Empty
        }; // 判定队列

        /// <summary>
        /// Slide star prefab
        /// <para>Readonly</para>
        /// </summary>
        [SerializeField]
        protected GameObject _slideStarPrefab;
        /// <summary>
        /// Arrows
        /// </summary>
        [ReadOnlyField]
        [SerializeField]
        protected GameObject[] _slideBars = { };
        /// <summary>
        /// Arrow Renderers
        /// </summary>
        [ReadOnlyField]
        [SerializeField]
        protected SpriteRenderer[] _slideBarRenderers = { };

        protected readonly Transform[] _starTransforms = new Transform[3];
        protected Transform[] _slideBarTransforms = { };
        /// <summary>
        /// Slide star
        /// </summary>
        protected GameObject?[] _stars = new GameObject[3];

        protected SlideOK? _slideOK;

        protected float _lastWaitTimeSec;
        
        protected float _maxFadeInAlpha = 0.5f; // 淡入时最大不透明度

        // Flags
        protected bool _isCheckable = false;
        protected bool _isSoundPlayed = false;
        protected bool _isChecking = false;

        public abstract void Initialize();
        protected sealed override void Judge(float currentSec)
        {
            if (!ConnectInfo.IsGroupPartEnd && ConnectInfo.IsConnSlide)
                return;
            else if (_isJudged)
                return;
            var stayTimeMSec = _lastWaitTimeSec * 1000; // 停留时间

            // By Minepig
            var diffSec = currentSec - JudgeTiming;
            var isFast = diffSec < 0;
            var diffMSec = MathF.Abs(diffSec) * 1000;
            _judgeDiff = diffSec * 1000;
            // input latency simulation
            //var ext = MathF.Max(0.05f, MathF.Min(stayTime / 4, 0.36666667f));
            var ext = MathF.Min(stayTimeMSec / 4, SLIDE_JUDGE_MAXIMUM_ALLOWED_EXT_LENGTH_MSEC);
            var JUDGE_SEG_3RD_PERFECT_MSEC = SLIDE_JUDGE_SEG_BASE_3RD_PERFECT_MSEC + ext;
            var JUDGE_SEG_1ST_PERFECT_MSEC = JUDGE_SEG_3RD_PERFECT_MSEC * 0.333333f;
            var JUDGE_SEG_2ND_PERFECT_MSEC = JUDGE_SEG_3RD_PERFECT_MSEC * 0.666666f;

            var result = diffMSec switch
            {
                _ when diffMSec <= JUDGE_SEG_1ST_PERFECT_MSEC => JudgeGrade.Perfect,
                _ when diffMSec <= JUDGE_SEG_2ND_PERFECT_MSEC => isFast ? JudgeGrade.FastPerfect2nd : JudgeGrade.LatePerfect2nd,
                _ when diffMSec <= JUDGE_SEG_3RD_PERFECT_MSEC => isFast ? JudgeGrade.FastPerfect3rd : JudgeGrade.LatePerfect3rd,
                <= SLIDE_JUDGE_SEG_1ST_GREAT_MSEC => isFast ? JudgeGrade.FastGreat : JudgeGrade.LateGreat,
                <= SLIDE_JUDGE_SEG_2ND_GREAT_MSEC => isFast ? JudgeGrade.FastGreat2nd : JudgeGrade.LateGreat2nd,
                <= SLIDE_JUDGE_SEG_3RD_GREAT_MSEC => isFast ? JudgeGrade.FastGreat3rd : JudgeGrade.LateGreat3rd,
                _ => isFast ? JudgeGrade.FastGood : JudgeGrade.LateGood
            };

            MajDebug.Log($"Slide diff : {MathF.Round(diffMSec, 2)} ms");
            ConvertJudgeGrade(ref result);
            _judgeResult = result;
            _isJudged = true;

            var remainingStartTime = ThisFrameSec - ConnectInfo.StartTiming;
            if (remainingStartTime < 0)
                _lastWaitTimeSec = MathF.Abs(remainingStartTime) / 2;
            else if (diffMSec >= SLIDE_JUDGE_GOOD_AREA_MSEC && !isFast)
                _lastWaitTimeSec = 0.05f;
        }
        protected void Judge_Classic(float currentSec)
        {
            if (!ConnectInfo.IsGroupPartEnd && ConnectInfo.IsConnSlide)
                return;
            else if (_isJudged)
                return;

            var diffSec = currentSec - JudgeTiming;
            var isFast = diffSec < 0;
            _judgeDiff = diffSec * 1000;
            var diffMSec = MathF.Abs(diffSec) * 1000;

            var judge = diffMSec switch
            {
                <= SLIDE_JUDGE_CLASSIC_SEG_1ST_PERFECT_MSEC => JudgeGrade.Perfect,
                <= SLIDE_JUDGE_CLASSIC_SEG_2ND_PERFECT_MSEC => isFast ? JudgeGrade.FastPerfect2nd : JudgeGrade.LatePerfect2nd,
                <= SLIDE_JUDGE_CLASSIC_SEG_3RD_PERFECT_MSEC => isFast ? JudgeGrade.FastPerfect3rd : JudgeGrade.LatePerfect3rd,
                <= SLIDE_JUDGE_CLASSIC_SEG_1ST_GREAT_MSEC => isFast ? JudgeGrade.FastGreat : JudgeGrade.LateGreat,
                <= SLIDE_JUDGE_CLASSIC_SEG_2ND_GREAT_MSEC => isFast ? JudgeGrade.FastGreat2nd : JudgeGrade.LateGreat2nd,
                <= SLIDE_JUDGE_CLASSIC_SEG_3RD_GREAT_MSEC => isFast ? JudgeGrade.FastGreat3rd : JudgeGrade.LateGreat3rd,
                _ => isFast ? JudgeGrade.FastGood : JudgeGrade.LateGood
            };

            MajDebug.Log($"Slide diff : {MathF.Round(diffMSec, 2)} ms");
            _judgeResult = judge;
            _isJudged = true;

            var remainingStartTime = ThisFrameSec - ConnectInfo.StartTiming;
            if (remainingStartTime < 0)
                _lastWaitTimeSec = MathF.Abs(remainingStartTime) / 2;
            else if (diffSec >= SLIDE_JUDGE_GOOD_AREA_MSEC && !isFast)
                _lastWaitTimeSec = 0.05f;
        }
        protected void HideBar(int endIndex)
        {
            endIndex = endIndex - 1;
            endIndex = Math.Min(endIndex, _slideBars.Length - 1);
            for (int i = 0; i <= endIndex; i++)
            {
                //_slideBarRenderers[i].forceRenderingOff = true;
                _slideBars[i].layer = 3;
            }
        }
        protected bool PlaySlideOK(in JudgeResult result)
        {
            if (_slideOK is null)
                return false;
            
            bool canPlay;
            canPlay = NoteEffectManager.CheckJudgeDisplaySetting(MajInstances.Setting.Display.SlideJudgeType, result);

            return canPlay;
        }
        protected void HideAllBar() => HideBar(int.MaxValue);
        protected void SetSlideBarAlpha(float alpha)
        {
            foreach (var sr in _slideBarRenderers.AsSpan())
            {
                if (IsEnded)
                    return;
                if (alpha <= 0f)
                {
                    sr.forceRenderingOff = true;
                }
                else 
                {
                    sr.forceRenderingOff = false;
                    sr.color = new Color(1f, 1f, 1f, alpha);
                }
            }
        }
        public sealed override void SetActive(bool state)
        {
            base.SetActive(state);
            if (state)
            {
                foreach (var slideBar in _slideBars.AsSpan())
                    slideBar.layer = MajEnv.DEFAULT_LAYER;
            }
            else
            {
                
                foreach (var slideBar in _slideBars.AsSpan())
                    slideBar.layer = MajEnv.HIDDEN_LAYER;
            }
            SetStarActive(state);
        }
        protected void SetStarActive(bool state)
        {
            switch(state)
            {
                case true:
                    foreach (var star in _stars.AsSpan())
                    {
                        if (star is null)
                            continue;
                        star.layer = MajEnv.DEFAULT_LAYER;
                    }
                    break;
                case false:
                    foreach (var star in _stars.AsSpan())
                    {
                        if (star is null)
                            continue;
                        star.layer = MajEnv.HIDDEN_LAYER;
                    }
                    break;
            }
        }
        protected sealed override void PlaySFX()
        {
            if(!_isSoundPlayed)
            {
                _audioEffMana.PlaySlideSound(IsBreak);
                _isSoundPlayed = true;
            }
        }
        protected sealed override void PlayJudgeSFX(in JudgeResult judgeResult)
        {
            if(judgeResult.IsBreak && !judgeResult.IsMissOrTooFast)
                _audioEffMana.PlayBreakSlideEndSound();
        }
        protected virtual void TooLateJudge()
        {
            if (QueueRemaining == 1)
                _judgeResult = JudgeGrade.LateGood;
            else
                _judgeResult = JudgeGrade.Miss;
            ConvertJudgeGrade(ref _judgeResult);
            _isJudged = true;
        }
        public virtual void End()
        {
            if (Parent is not null && !Parent.IsEnded)
                Parent.End();

            SetActive(false);
        }
        /// <summary>
        /// Connection Slide
        /// <para>强制完成该Slide</para>
        /// </summary>
        public void ForceFinish()
        {
            if (!ConnectInfo.IsConnSlide || ConnectInfo.IsGroupPartEnd)
                return;
            HideAllBar();
            var emptyQueue = Memory<SlideArea>.Empty;
            for (int i = 0; i < 2; i++)
                _judgeQueues[i] = emptyQueue;
        }
        protected void DestroyStars()
        {
            if (_stars.IsEmpty())
                return;
            SetStarActive(false);
            foreach (ref var star in _stars.AsSpan())
                star = null;
            //GameObjectHelper.Destroy(ref _stars);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void SlideBarFadeIn()
        {
            if (IsEnded || IsSlideNoTrack)
                return;

            var num = Timing - 0.05f;
            float interval = (num - _fadeInTiming).Clamp(0, 0.2f);
            float fullFadeInTiming = _fadeInTiming + interval;//淡入到maxFadeInAlpha的时间点

            if(ThisFrameSec > num)
            {
                SetSlideBarAlpha(1f);
                return;
            }
            else if(ThisFrameSec > fullFadeInTiming)
            {
                SetSlideBarAlpha(_maxFadeInAlpha);
                return;
            }
            else if(ThisFrameSec < fullFadeInTiming)
            {
                var diff = (fullFadeInTiming - ThisFrameSec).Clamp(0, interval);
                float alpha = 0;

                if (interval != 0)
                    alpha = 1 - (diff / interval);
                alpha *= _maxFadeInAlpha;
                SetSlideBarAlpha(alpha);
            }
        }
        protected void JudgeResultCorrection(ref JudgeGrade result)
        {
            switch(result)
            {
                case JudgeGrade.LatePerfect3rd:
                case JudgeGrade.LatePerfect2nd:
                case JudgeGrade.FastPerfect2nd:
                case JudgeGrade.FastPerfect3rd:
                    result = JudgeGrade.Perfect;
                    break;
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected sealed override float GetRemainingTime() => GetRemainingTimeWithoutOffset();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected sealed override float GetRemainingTimeWithoutOffset() => MathF.Max(ArriveTiming - ThisFrameSec, 0);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected sealed override float GetTimeSpanToArriveTiming() => ThisFrameSec - ArriveTiming;
        [ReadOnlyField, SerializeField]
        float _startTiming;
        [ReadOnlyField, SerializeField]
        bool _isJustR = false;
        [ReadOnlyField, SerializeField]
        float _fadeInTiming = 0;
        [ReadOnlyField, SerializeField]
        float _fullFadeInTiming = 0.2f;
        [ReadOnlyField, SerializeField]
        int _endPos = 1;
        [SerializeField]
        string _slideType = string.Empty;
        [ReadOnlyField, SerializeField]
        float _slideLength = 0f;
    }
}
