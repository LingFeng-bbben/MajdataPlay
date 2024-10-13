using Cysharp.Threading.Tasks;
using MajdataPlay.Extensions;
using MajdataPlay.Interfaces;
using MajdataPlay.IO;
using MajdataPlay.Types;
using MajdataPlay.Types.Attribute;
using MajdataPlay.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                int[] reamaining = new int[3];
                foreach (var (i, queue) in _judgeQueues.WithIndex())
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
        [ReadOnlyField]
        [SerializeField]
        protected GameObject[] _slideBars = { }; // Arrows


        

        /// <summary>
        /// 引导Star
        /// </summary>
        public GameObject[] _stars = new GameObject[3];

        protected GameObject _slideOK;
        protected bool _isSoundPlayed = false;
        protected float _lastWaitTime;
        protected bool _canCheck = false;
        protected bool _isChecking = false;
        
        protected float _maxFadeInAlpha = 0.5f; // 淡入时最大不透明度
        /// <summary>
        /// 存储Slide Queue中会经过的区域
        /// <para>用于绑定或解绑Event</para>
        /// </summary>
        protected IEnumerable<SensorType> _judgeAreas;
        public abstract void Initialize();
        protected override void Judge(float currentSec)
        {
            if (!ConnectInfo.IsGroupPartEnd && ConnectInfo.IsConnSlide)
                return;
            else if (_isJudged)
                return;
            //var stayTime = time + LastFor - judgeTiming; // 停留时间
            var stayTime = _lastWaitTime; // 停留时间

            // By Minepig
            var diff = currentSec - JudgeTiming;
            _judgeDiff = diff * 1000;
            var isFast = diff < 0;

            // input latency simulation
            //var ext = MathF.Max(0.05f, MathF.Min(stayTime / 4, 0.36666667f));
            var ext = MathF.Min(stayTime / 4, 0.36666667f);

            var perfect = 0.2333333f + ext;

            diff = MathF.Abs(diff);
            JudgeType? judge = null;

            if (diff <= perfect)// 其实最小0.2833333f, 17帧
                judge = JudgeType.Perfect;
            else
            {
                judge = diff switch
                {
                    <= 0.35f => isFast ? JudgeType.FastGreat : JudgeType.LateGreat,
                    <= 0.4166667f => isFast ? JudgeType.FastGreat1 : JudgeType.LateGreat1,
                    <= 0.4833333f => isFast ? JudgeType.FastGreat2 : JudgeType.LateGreat2,
                    _ => isFast ? JudgeType.FastGood : JudgeType.LateGood
                };
            }

            print($"Slide diff : {MathF.Round(diff * 1000, 2)} ms");
            _judgeResult = judge ?? JudgeType.Miss;
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

            var diff = currentSec - JudgeTiming;
            _judgeDiff = diff * 1000;
            var isFast = diff < 0;

            var perfect = 0.15f;

            diff = MathF.Abs(diff);
            JudgeType? judge = null;

            if (diff <= perfect)
                judge = JudgeType.Perfect;
            else
            {
                judge = diff switch
                {
                    <= 0.2305557f => isFast ? JudgeType.FastGreat : JudgeType.LateGreat,
                    <= 0.3111114f => isFast ? JudgeType.FastGreat1 : JudgeType.LateGreat1,
                    <= 0.3916672f => isFast ? JudgeType.FastGreat2 : JudgeType.LateGreat2,
                    _ => isFast ? JudgeType.FastGood : JudgeType.LateGood
                };
            }

            print($"Slide diff : {MathF.Round(diff * 1000, 2)} ms");
            _judgeResult = judge ?? JudgeType.Miss;
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
                _slideBars[i].SetActive(false);
        }
        protected void PlaySlideOK(in JudgeResult result)
        {
            if (_slideOK == null)
                return;
            
            bool canPlay;
            if(result.IsBreak)
                canPlay = NoteEffectManager.CheckJudgeDisplaySetting(MajInstances.Setting.Display.BreakJudgeType, result);
            else
                canPlay = NoteEffectManager.CheckJudgeDisplaySetting(MajInstances.Setting.Display.SlideJudgeType, result);

            if (canPlay)
                _slideOK.SetActive(true);
            else
                Destroy(_slideOK);
        }
        protected void HideAllBar() => HideBar(int.MaxValue);
        protected void SetSlideBarAlpha(float alpha)
        {
            foreach (var gm in _slideBars)
            {
                var sr = gm.GetComponent<SpriteRenderer>();
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
        protected void TooLateJudge()
        {
            if (_isJudged)
            {
                DestroySelf();
                return;
            }

            if (QueueRemaining == 1)
                _judgeResult = JudgeType.LateGood;
            else
                _judgeResult = JudgeType.Miss;
            _isJudged = true;
            DestroySelf();
        }
        /// <summary>
        /// 销毁当前Slide
        /// <para>当 <paramref name="onlyStar"/> 为true时，仅销毁引导Star</para>
        /// </summary>
        /// <param name="onlyStar"></param>
        protected void DestroySelf(bool onlyStar = false)
        {

            if (onlyStar)
                DestroyStars();
            else
            {
                if (Parent is not null && !Parent.IsDestroyed)
                    Destroy(Parent.GameObject);

                foreach (GameObject obj in _slideBars)
                    obj.SetActive(false);

                DestroyStars();
                Destroy(gameObject);
            }
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
        void DestroyStars()
        {
            if (_stars.IsEmpty())
                return;
            foreach (var star in _stars)
            {
                if (star != null)
                    Destroy(star);
            }
            _stars = Array.Empty<GameObject>();
        }
        protected async UniTaskVoid FadeIn()
        {
            FadeInTiming = Math.Max(FadeInTiming,CurrentSec);
            var num = StartTiming - 0.05f;
            float interval = (num - FadeInTiming).Clamp(0, 0.2f);
            float fullFadeInTiming = FadeInTiming + interval;//淡入到maxFadeInAlpha的时间点

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
