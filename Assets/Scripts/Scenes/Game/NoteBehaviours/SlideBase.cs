using Cysharp.Threading.Tasks;
using MajdataPlay.Buffers;
using MajdataPlay.Collections;
using MajdataPlay.Editor;
using MajdataPlay.Extensions;
using MajdataPlay.IO;
using MajdataPlay.Numerics;
using MajdataPlay.Scenes.Game.Notes.Controllers;
using MajdataPlay.Scenes.Game.Notes.Slide;
using MajdataPlay.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;
using UnityEngine.Serialization;

#nullable enable
namespace MajdataPlay.Scenes.Game.Notes.Behaviours
{
    internal abstract class SlideBase : NoteLongDrop
    {
        const int SLIDE_BAR_MODE_SET_ALPHA = 1;
        const int SLIDE_BAR_MODE_SET_MATERIAL = 2;
        const int SLIDE_BAR_MODE_SET_ALPHA_AND_MATERIAL = 3;
        public IConnectableSlide? Parent => ConnectInfo.Parent;
        public ConnSlideInfo ConnectInfo
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set;
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
                var judgeQueues = JudgeQueues.AsSpan();
                for (var i = 0; i < judgeQueues.Length; i++)
                {
                    var queue = judgeQueues[i];
                    reamaining[i] = queue.Length;
                }
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
        public int EndPos
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _endPos;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                if (value.InRange(1, 8))
                {
                    _endPos = value;
                }
                else
                {
                    throw new ArgumentOutOfRangeException("End position must be between 1 and 8");
                }
            }
        }
        public int Multiple
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _multiple;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _multiple = value;
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
        protected readonly Memory<SlideArea>[] JudgeQueues = new Memory<SlideArea>[3]
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
        [FormerlySerializedAs("_slideStarPrefab")]
        protected GameObject SlideStarPrefab;
        /// <summary>
        /// Arrows
        /// </summary>
        [ReadOnlyField]
        [SerializeField]
        protected readonly RentedList<GameObject> SlideBars = new();
        /// <summary>
        /// Arrow Renderers
        /// </summary>
        [ReadOnlyField]
        [SerializeField]
        protected readonly RentedList<SpriteRenderer> SlideBarRenderers = new();
        protected readonly RentedList<Transform> SlideBarTransforms = new();
        /// <summary>
        /// Slide star
        /// </summary>
        protected readonly Memory<GameObject?> Stars = Memory<GameObject?>.Empty;
        protected readonly Memory<Transform> StarTransforms = Memory<Transform>.Empty;

        protected SlideOK? SlideOK;

        protected float LastWaitTimeSec;

        [ReadOnlyField, SerializeField]
        protected float FadeInTiming = 0f;
        [ReadOnlyField, SerializeField]
        protected float FadeInCutoffTiming = 0f;
        [ReadOnlyField, SerializeField]
        protected float FadeInCompletedTiming = 0f;
        [ReadOnlyField, SerializeField]
        protected float FadeInDurationTimeSec = 0.2f;
        [ReadOnlyField, SerializeField]
        protected float FadeInMaxAlpha = 0.5f; // 淡入时最大不透明度


        protected float DJAutoplayProgress = 0;

        protected int LastHiddenSlideBarIndex = 0;
        // Flags
        protected bool IsCheckable = false;
        protected bool IsSoundPlayed = false;
        protected uint SlideBarFadeInFlag = 0;

        [ReadOnlyField, SerializeField]
        int _multiple = 1;

        GameObject?[] _rentedArrayForStars;
        Transform[] _rentedArrayForStarTransforms;
        protected SlideBase()
        {
            _rentedArrayForStars = Pool<GameObject?>.RentArray(3, true);
            _rentedArrayForStarTransforms = Pool<Transform>.RentArray(3, true);

            Stars = _rentedArrayForStars.AsMemory(0, 3);
            StarTransforms = _rentedArrayForStarTransforms.AsMemory(0, 3);
        }
        ~SlideBase()
        {
            Dispose();
        }
        public abstract void Init();
        [Il2CppSetOption(Option.NullChecks, false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected sealed override void Judge(float currentSec)
        {
            if (!ConnectInfo.IsGroupPartEnd && ConnectInfo.IsConnSlide)
            {
                return;
            }
            else if (_isJudged)
            {
                return;
            }
            var stayTimeMSec = LastWaitTimeSec * 1000; // 停留时间

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

            //MajDebug.Log($"Slide diff : {MathF.Round(diffMSec, 2)} ms");
            ConvertJudgeGrade(ref result);
            _judgeResult = result;
            _isJudged = true;

            var remainingStartTime = ThisFrameSec - ConnectInfo.StartTiming;
            if (remainingStartTime < 0)
            {
                LastWaitTimeSec = MathF.Abs(remainingStartTime) / 2;
            }
            else if (diffMSec >= SLIDE_JUDGE_GOOD_AREA_MSEC && !isFast)
            {
                LastWaitTimeSec = 0.05f;
            }
        }
        [Il2CppSetOption(Option.NullChecks, false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void JudgeClassic(float currentSec)
        {
            if (!ConnectInfo.IsGroupPartEnd && ConnectInfo.IsConnSlide)
            {
                return;
            }
            else if (_isJudged)
            {
                return;
            }
            var diffSec = currentSec - JudgeTiming;
            var isFast = diffSec < 0;
            _judgeDiff = diffSec * 1000;
            var diffMSec = MathF.Abs(diffSec) * 1000;
            var judge = JudgeGrade.Miss;
            if (isFast)
            {
                judge = diffMSec switch
                {
                    <= SLIDE_JUDGE_CLASSIC_FAST_SEG_1ST_PERFECT_MSEC => JudgeGrade.Perfect,
                    <= SLIDE_JUDGE_CLASSIC_FAST_SEG_2ND_PERFECT_MSEC => JudgeGrade.FastPerfect2nd,
                    <= SLIDE_JUDGE_CLASSIC_FAST_SEG_3RD_PERFECT_MSEC => JudgeGrade.FastPerfect3rd,
                    <= SLIDE_JUDGE_CLASSIC_FAST_SEG_1ST_GREAT_MSEC => JudgeGrade.FastGreat,
                    <= SLIDE_JUDGE_CLASSIC_FAST_SEG_2ND_GREAT_MSEC => JudgeGrade.FastGreat2nd,
                    <= SLIDE_JUDGE_CLASSIC_FAST_SEG_3RD_GREAT_MSEC => JudgeGrade.FastGreat3rd,
                    _ => JudgeGrade.FastGood
                };
            }
            else
            {
                judge = diffMSec switch
                {
                    <= SLIDE_JUDGE_CLASSIC_LATE_SEG_1ST_PERFECT_MSEC => JudgeGrade.Perfect,
                    <= SLIDE_JUDGE_CLASSIC_LATE_SEG_2ND_PERFECT_MSEC => JudgeGrade.LatePerfect2nd,
                    <= SLIDE_JUDGE_CLASSIC_LATE_SEG_3RD_PERFECT_MSEC => JudgeGrade.LatePerfect3rd,
                    <= SLIDE_JUDGE_CLASSIC_LATE_SEG_1ST_GREAT_MSEC => JudgeGrade.LateGreat,
                    <= SLIDE_JUDGE_CLASSIC_LATE_SEG_2ND_GREAT_MSEC => JudgeGrade.LateGreat2nd,
                    <= SLIDE_JUDGE_CLASSIC_LATE_SEG_3RD_GREAT_MSEC => JudgeGrade.LateGreat3rd,
                    _ => JudgeGrade.LateGood
                };
            }

            //MajDebug.Log($"Slide diff : {MathF.Round(diffMSec, 2)} ms");
            _judgeResult = judge;
            _isJudged = true;

            var remainingStartTime = ThisFrameSec - ConnectInfo.StartTiming;
            if (remainingStartTime < 0)
            {
                LastWaitTimeSec = MathF.Abs(remainingStartTime) / 2;
            }
        }
        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void HideBar(int endIndex)
        {
            endIndex = endIndex - 1;       
            endIndex = Math.Min(endIndex, SlideBars.Count - 1);
            if (endIndex < LastHiddenSlideBarIndex)
            {
                return;
            }
            ref var slideBarRef = ref MemoryMarshal.GetReference(SlideBars.AsSpan());
            for (; LastHiddenSlideBarIndex <= endIndex; LastHiddenSlideBarIndex++)
            {
                ref var slideBar = ref Unsafe.Add(ref slideBarRef, LastHiddenSlideBarIndex);
                //_slideBarRenderers[i].forceRenderingOff = true;
                slideBar.layer = MajEnv.HIDDEN_LAYER;
            }
        }
        [Il2CppSetOption(Option.NullChecks, false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [MemberNotNullWhen(true, "SlideOK")]
        protected bool PlaySlideOK(in NoteJudgeResult result)
        {
            if (SlideOK is null)
            {
                return false;
            }

            bool canPlay;
            canPlay = NoteEffectManager.CheckJudgeDisplaySetting(MajInstances.Settings.Display.SlideJudgeType, result);

            return canPlay;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void HideAllBar() => HideBar(int.MaxValue);
        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void SetSlideBarAlpha(float alpha)
        {
            SetSlideBarRender(alpha, null, SLIDE_BAR_MODE_SET_ALPHA);
        }
        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void SetSlideBarMaterial(Material material)
        {
            SetSlideBarRender(0, material, SLIDE_BAR_MODE_SET_MATERIAL);
        }
        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void SetSlideBarAlphaAndMaterial(float alpha, Material material)
        {
            SetSlideBarRender(alpha, material, SLIDE_BAR_MODE_SET_ALPHA_AND_MATERIAL);
        }
        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void SetSlideBarRender(float alpha, Material? material, int setMode)
        {
            ref var slideBarRef = ref MemoryMarshal.GetReference(SlideBars.AsSpan());
            ref var rendererRef = ref MemoryMarshal.GetReference(SlideBarRenderers.AsSpan());
            var count = SlideBars.Count;
            var newColor = new Color(1f, 1f, 1f, alpha);
            for (var i = 0; i < count; i++)
            {
                ref var sr = ref Unsafe.Add(ref rendererRef, i);
                ref var obj = ref Unsafe.Add(ref slideBarRef, i);
                switch(setMode)
                {
                    case SLIDE_BAR_MODE_SET_ALPHA:
                        if (alpha <= 0f)
                        {
                            obj.layer = MajEnv.HIDDEN_LAYER;
                        }
                        else
                        {
                            obj.layer = MajEnv.DEFAULT_LAYER;
                            sr.color = newColor;
                        }
                        break;
                    case SLIDE_BAR_MODE_SET_MATERIAL:
                        sr.sharedMaterial = material;
                        break;
                    case SLIDE_BAR_MODE_SET_ALPHA_AND_MATERIAL:
                        if (alpha <= 0f)
                        {
                            obj.layer = MajEnv.HIDDEN_LAYER;
                        }
                        else
                        {
                            obj.layer = MajEnv.DEFAULT_LAYER;
                            sr.color = newColor;
                        }
                        sr.sharedMaterial = material;
                        break;
                }
            }
        }
        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public sealed override void SetActive(bool state)
        {
            base.SetActive(state);
            ref var slideBarRef = ref MemoryMarshal.GetReference(SlideBars.AsSpan());
            var count = SlideBars.Count;
            var layer = MajEnv.HIDDEN_LAYER;
            if (state)
            {
                layer = MajEnv.DEFAULT_LAYER;
            }
            for (var i = 0; i < count; i++)
            {
                ref var slideBar = ref Unsafe.Add(ref slideBarRef, i);
                slideBar.layer = layer;
            }
            SetStarActive(state);
        }
        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void SetStarActive(bool state)
        {
            var stars = Stars.Span;
            switch (state)
            {
                case true:
                    for (var i = 0; i < stars.Length; i++)
                    {
                        var star = stars[i];
                        if (star is null)
                        {
                            continue;
                        }
                        star.layer = MajEnv.DEFAULT_LAYER;
                    }
                    break;
                case false:
                    for (var i = 0; i < stars.Length; i++)
                    {
                        var star = stars[i];
                        if (star is null)
                        {
                            continue;
                        }
                        star.layer = MajEnv.HIDDEN_LAYER;
                    }
                    break;
            }
        }
        [Il2CppSetOption(Option.NullChecks, false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected sealed override void PlaySFX()
        {
            if (!IsSoundPlayed)
            {
                _audioEffMana.PlaySlideSound(IsBreak);
                IsSoundPlayed = true;
            }
        }
        [Il2CppSetOption(Option.NullChecks, false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected sealed override void PlayJudgeSFX(in NoteJudgeResult judgeResult)
        {
            if (judgeResult.IsBreak && judgeResult.Grade == JudgeGrade.Perfect)
            {
                _audioEffMana.PlayBreakSlideEndSound();
            }
        }
        protected virtual void TooLateJudge()
        {
            if (QueueRemaining == 1)
            {
                _judgeResult = JudgeGrade.LateGood;
            }
            else
            {
                _judgeResult = JudgeGrade.Miss;
            }
            ConvertJudgeGrade(ref _judgeResult);
            _isJudged = true;
        }
        protected virtual void End()
        {
            if (Parent is not null && !Parent.IsEnded)
            {
                Parent.End();
            }

            SetActive(false);
        }
        /// <summary>
        /// Connection Slide
        /// <para>强制完成该Slide</para>
        /// </summary>
        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ForceFinish()
        {
            if (!ConnectInfo.IsConnSlide || ConnectInfo.IsGroupPartEnd)
            { 
                return; 
            }
            HideAllBar();
            var emptyQueue = Memory<SlideArea>.Empty;
            for (var i = 0; i < 3; i++)
            { 
                JudgeQueues[i] = emptyQueue; 
            }
        }
        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void DestroyStars()
        {
            if (Stars.IsEmpty)
            {
                return;
            }
            SetStarActive(false);
            foreach (ref var star in Stars.Span)
            {
                star = null;
            }
            //GameObjectHelper.Destroy(ref _stars);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void SlideBarFadeIn()
        {
            if (SlideBarFadeInFlag == 1 || IsSlideNoTrack || !IsInited || IsEnded)
            {
                return;
            }
            var thisFrameSec = ThisFrameSec;
            if(thisFrameSec < FadeInTiming)
            {
                return;
            }

            if (thisFrameSec > FadeInCompletedTiming)
            {
                SlideBarFadeInFlag = 1;
                var breakMaterial = BreakMaterial;
                if (IsBreak && breakMaterial is not null)
                {
                    SetSlideBarRender(1f, breakMaterial, SLIDE_BAR_MODE_SET_ALPHA_AND_MATERIAL);
                }
                else
                {
                    SetSlideBarRender(1f, null, SLIDE_BAR_MODE_SET_ALPHA);
                }
            }
            else if (thisFrameSec > FadeInCutoffTiming)
            {
                SetSlideBarRender(FadeInMaxAlpha, null, SLIDE_BAR_MODE_SET_ALPHA);
            }
            else
            {
                var diff = FadeInCutoffTiming - thisFrameSec;
                var alpha = 1 - diff / FadeInDurationTimeSec;

                alpha *= FadeInMaxAlpha;
                SetSlideBarRender(alpha, null, SLIDE_BAR_MODE_SET_ALPHA);
            }
        }
        protected virtual void OnDestroy()
        {
            Dispose();
        }
        protected void JudgeGradeCorrection(ref JudgeGrade result)
        {
            switch (result)
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
        void Dispose()
        {
            if(_rentedArrayForStars.Length != 0)
            {
                Pool<GameObject?>.ReturnArray(_rentedArrayForStars, true);
                _rentedArrayForStars = Array.Empty<GameObject?>();
            }
            if(_rentedArrayForStarTransforms.Length != 0)
            {
                Pool<Transform>.ReturnArray(_rentedArrayForStarTransforms, true);
                _rentedArrayForStarTransforms = Array.Empty<Transform>();
            }
            SlideBars.Dispose();
            SlideBarTransforms.Dispose();
            SlideBarRenderers.Dispose();
        }
        [ReadOnlyField, SerializeField]
        float _startTiming;
        [ReadOnlyField, SerializeField]
        bool _isJustR = false;
        [ReadOnlyField, SerializeField]
        int _endPos = 1;
        [SerializeField]
        string _slideType = string.Empty;
        [ReadOnlyField, SerializeField]
        float _slideLength = 0f;
    }
}
