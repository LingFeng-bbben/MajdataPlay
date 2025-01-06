using Cysharp.Threading.Tasks;
using MajdataPlay.Collections;
using MajdataPlay.Extensions;
using MajdataPlay.Game.Controllers;
using MajdataPlay.Game.Types;
using MajdataPlay.Interfaces;
using MajdataPlay.IO;
using MajdataPlay.Types;
using MajdataPlay.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using MajdataPlay.Attributes;
using UnityEngine;
#nullable enable
namespace MajdataPlay.Game.Notes
{
    public abstract class SlideBase : NoteLongDrop
    {
        public IConnectableSlide? Parent => ConnectInfo.Parent;
        public ConnSlideInfo ConnectInfo { get; set; } = new()
        {
            IsGroupPart = false,
            Parent = null
        };
        /// <summary>
        /// 如果判定队列已经完成，返回True，反之False
        /// </summary>
        public bool IsFinished { get => QueueRemaining == 0; }
        /// <summary>
        /// 如果判定队列剩余1个未完成判定区，返回True
        /// </summary>
        public bool IsPendingFinish { get => QueueRemaining == 1; }
        /// <summary>
        /// 返回判定队列中未完成判定区的数量
        /// </summary>
        public int QueueRemaining 
        { 
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
            get => _startTiming;
            set => _startTiming = value;
        }
        public bool IsJustR
        {
            get => _isJustR;
            set => _isJustR = value;
        }
        public float FadeInTiming
        {
            get => _fadeInTiming;
            set => _fadeInTiming = value;
        }
        public float FullFadeInTiming
        {
            get => _fullFadeInTiming;
            set => _fullFadeInTiming = value;
        }
        public int EndPos
        {
            get => _endPos;
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
            get => _slideType;
            set => _slideType = value;
        }
        protected JudgeArea[][] _judgeQueues = new JudgeArea[3][]
        { 
            Array.Empty<JudgeArea>(), 
            Array.Empty<JudgeArea>(), 
            Array.Empty<JudgeArea>()
        }; // 判定队列
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

        protected Transform[] _starTransforms = { };
        protected Transform[] _slideBarTransforms = { };
        /// <summary>
        /// Slide star
        /// </summary>
        public GameObject?[] _stars = new GameObject[3];

        protected GameObject _slideOK;
        protected Animator _slideOKAnim;
        protected LoadJustSprite _slideOKController;

        protected float _lastWaitTime;
        protected bool _canCheck = false;
        protected float _maxFadeInAlpha = 0.5f; // 淡入时最大不透明度

        // Flags
        protected bool _isSoundPlayed = false;
        protected bool _isChecking = false;
        protected bool _isStarActive = false;
        protected bool _isArrived = false;


        /// <summary>
        /// 存储Slide Queue中会经过的区域
        /// <para>用于绑定或解绑Event</para>
        /// </summary>
        protected SensorType[] _judgeAreas = Array.Empty<SensorType>();
        public abstract void Initialize();
        protected override void Judge(float currentSec)
        {
            if (!ConnectInfo.IsGroupPartEnd && ConnectInfo.IsConnSlide)
                return;
            else if (_isJudged)
                return;
            var stayTime = _lastWaitTime; // 停留时间

            // By Minepig
            var diff = currentSec - JudgeTiming;
            var isFast = diff < 0;
            _judgeDiff = diff * 1000;
            // input latency simulation
            //var ext = MathF.Max(0.05f, MathF.Min(stayTime / 4, 0.36666667f));
            var ext = MathF.Min(stayTime / 4, 0.36666667f);

            const float JUDGE_GREAT_AREA = 0.4833333f;
            const float JUDGE_SEG_GREAT1 = 0.35f;
            const float JUDGE_SEG_GREAT2 = 0.4166667f;

            var JUDGE_PERFECT_AREA = 0.2333333f + ext;
            var JUDGE_SEG_PERFECT1 = JUDGE_PERFECT_AREA * 0.333333f;
            var JUDGE_SEG_PERFECT2 = JUDGE_PERFECT_AREA * 0.666666f;
            diff = MathF.Abs(diff);

            var result = diff switch
            {
                _ when diff <= JUDGE_SEG_PERFECT1 => JudgeGrade.Perfect,
                _ when diff <= JUDGE_SEG_PERFECT2 => isFast ? JudgeGrade.FastPerfect1 : JudgeGrade.LatePerfect1,
                _ when diff <= JUDGE_PERFECT_AREA => isFast ? JudgeGrade.FastPerfect2 : JudgeGrade.LatePerfect2,
                <= JUDGE_SEG_GREAT1 => isFast ? JudgeGrade.FastGreat : JudgeGrade.LateGreat,
                <= JUDGE_SEG_GREAT2 => isFast ? JudgeGrade.FastGreat1 : JudgeGrade.LateGreat1,
                <= JUDGE_GREAT_AREA => isFast ? JudgeGrade.FastGreat2 : JudgeGrade.LateGreat2,
                _ => isFast ? JudgeGrade.FastGood : JudgeGrade.LateGood
            };

            print($"Slide diff : {MathF.Round(diff * 1000, 2)} ms");
            ConvertJudgeResult(ref result);
            _judgeResult = result;
            _isJudged = true;

            var remainingStartTime = _gpManager.AudioTime - ConnectInfo.StartTiming;
            if (remainingStartTime < 0)
                _lastWaitTime = MathF.Abs(remainingStartTime) / 2;
            else if (diff >= 0.6166679 && !isFast)
                _lastWaitTime = 0;
        }
        protected void Judge_Classic(float currentSec)
        {
            if (!ConnectInfo.IsGroupPartEnd && ConnectInfo.IsConnSlide)
                return;
            else if (_isJudged)
                return;
            const float JUDGE_GREAT_AREA = 0.3916672f;
            const float JUDGE_PERFECT_AREA = 0.15f;

            const float JUDGE_SEG_PERFECT1 = 0.05f;
            const float JUDGE_SEG_PERFECT2 = 0.1f;
            const float JUDGE_SEG_GREAT1 = 0.2305557f;
            const float JUDGE_SEG_GREAT2 = 0.3111114f;

            var diff = currentSec - JudgeTiming;
            var isFast = diff < 0;
            _judgeDiff = diff * 1000;
            diff = MathF.Abs(diff);

            var judge = diff switch
            {
                <= JUDGE_SEG_PERFECT1 => JudgeGrade.Perfect,
                <= JUDGE_SEG_PERFECT2 => isFast ? JudgeGrade.FastPerfect1 : JudgeGrade.LatePerfect1,
                <= JUDGE_PERFECT_AREA => isFast ? JudgeGrade.FastPerfect2 : JudgeGrade.LatePerfect2,
                <= JUDGE_SEG_GREAT1 => isFast ? JudgeGrade.FastGreat : JudgeGrade.LateGreat,
                <= JUDGE_SEG_GREAT2 => isFast ? JudgeGrade.FastGreat1 : JudgeGrade.LateGreat1,
                <= JUDGE_GREAT_AREA => isFast ? JudgeGrade.FastGreat2 : JudgeGrade.LateGreat2,
                _ => isFast ? JudgeGrade.FastGood : JudgeGrade.LateGood
            };

            print($"Slide diff : {MathF.Round(diff * 1000, 2)} ms");
            _judgeResult = judge;
            _isJudged = true;

            var remainingStartTime = _gpManager.AudioTime - ConnectInfo.StartTiming;
            if (remainingStartTime < 0)
                _lastWaitTime = MathF.Abs(remainingStartTime) / 2;
            else if (diff >= 0.6166679 && !isFast)
                _lastWaitTime = 0;
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
            if (_slideOK == null)
                return false;
            
            bool canPlay;
            if(result.IsBreak)
                canPlay = NoteEffectManager.CheckJudgeDisplaySetting(MajInstances.Setting.Display.BreakJudgeType, result);
            else
                canPlay = NoteEffectManager.CheckJudgeDisplaySetting(MajInstances.Setting.Display.SlideJudgeType, result);

            if (canPlay)
            {
                _slideOK.SetActive(true);
            }
            //else
            //{
            //    //Destroy(_slideOK);
            //}
            return canPlay;
        }
        protected void HideAllBar() => HideBar(int.MaxValue);
        protected void SetSlideBarAlpha(float alpha)
        {
            foreach (var sr in _slideBarRenderers.AsSpan())
            {
                if (IsDestroyed)
                    return;
                if (alpha <= 0f)
                {
                    sr.forceRenderingOff = true;
                }
                else {
                    sr.forceRenderingOff = false;
                    sr.color = new Color(1f, 1f, 1f, alpha);
                }
            }
        }
        public override void SetActive(bool state)
        {
            base.SetActive(state);
            if (state)
            {
                if (State >= NoteStatus.PreInitialized && State <= NoteStatus.Initialized)
                {
                    foreach (var sensor in ArrayHelper.ToEnumerable(_judgeAreas))
                        _ioManager.BindSensor(_noteChecker, sensor);
                    State = NoteStatus.Running;
                }
                foreach (var slideBar in _slideBars.AsSpan())
                    slideBar.layer = 0;
            }
            else
            {
                
                foreach (var slideBar in _slideBars.AsSpan())
                    slideBar.layer = 3;
            }
            SetStarActive(state);
            Active = state;
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
                        star.layer = 0;
                    }
                    break;
                case false:
                    foreach (var star in _stars.AsSpan())
                    {
                        if (star is null)
                            continue;
                        star.layer = 3;
                    }
                    break;
            }
        }
        protected override void PlaySFX()
        {
            if(!_isSoundPlayed)
            {
                _audioEffMana.PlaySlideSound(IsBreak);
                _isSoundPlayed = true;
            }
        }
        protected override void PlayJudgeSFX(in JudgeResult judgeResult)
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
            ConvertJudgeResult(ref _judgeResult);
            _isJudged = true;
        }
        /// <summary>
        /// 销毁当前Slide
        /// <para>当 <paramref name="onlyStar"/> 为true时，仅销毁引导Star</para>
        /// </summary>
        /// <param name="onlyStar"></param>
        public virtual void End(bool forceEnd = false)
        {
            if (Parent is not null && !Parent.IsDestroyed)
                Parent.End(true);
            //foreach (var obj in _slideBars.AsSpan())
            //    obj.SetActive(false);
            //DestroyStars();
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
            var emptyQueue = Array.Empty<JudgeArea>();
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
        protected async UniTaskVoid FadeIn()
        {
            _fadeInTiming = Math.Max(_fadeInTiming,CurrentSec);
            var num = _startTiming - 0.05f;
            float interval = (num - _fadeInTiming).Clamp(0, 0.2f);
            float fullFadeInTiming = _fadeInTiming + interval;//淡入到maxFadeInAlpha的时间点

            while (!Active)
                await UniTask.Yield();
            while (CurrentSec < fullFadeInTiming) 
            {
                var diff = (fullFadeInTiming - CurrentSec).Clamp(0, interval);
                float alpha = 0;

                if(interval != 0)
                    alpha = 1 - (diff / interval);
                alpha *= _maxFadeInAlpha;
                SetSlideBarAlpha(alpha);
                await UniTask.Yield();
            }
            SetSlideBarAlpha(_maxFadeInAlpha);
            while (CurrentSec < num)
                await UniTask.Yield();
            SetSlideBarAlpha(1f);
        }
        protected void JudgeResultCorrection(ref JudgeGrade result)
        {
            switch(result)
            {
                case JudgeGrade.LatePerfect2:
                case JudgeGrade.LatePerfect1:
                case JudgeGrade.FastPerfect1:
                case JudgeGrade.FastPerfect2:
                    result = JudgeGrade.Perfect;
                    break;
            }
        }
        [ReadOnlyField]
        [SerializeField]
        protected float _startTiming;
        [ReadOnlyField]
        [SerializeField]
        protected bool _isJustR = false;
        [ReadOnlyField]
        [SerializeField]
        protected float _fadeInTiming = 0;
        [ReadOnlyField]
        [SerializeField]
        protected float _fullFadeInTiming = 0.2f;
        [ReadOnlyField]
        [SerializeField]
        protected int _endPos = 1;
        [ReadOnlyField]
        [SerializeField]
        protected string _slideType = string.Empty;
    }
}
