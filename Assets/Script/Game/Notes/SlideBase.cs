using Cysharp.Threading.Tasks;
using MajdataPlay.Extensions;
using MajdataPlay.Interfaces;
using MajdataPlay.IO;
using MajdataPlay.Types;
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
                foreach (var (i, queue) in judgeQueues.WithIndex())
                    reamaining[i] = queue.Length;
                return reamaining.Max();
            }
        }

        protected JudgeArea[][] judgeQueues = new JudgeArea[3][]
        { 
            Array.Empty<JudgeArea>(), 
            Array.Empty<JudgeArea>(), 
            Array.Empty<JudgeArea>()
        }; // 判定队列

        protected GameObject[] slideBars = { }; // Arrows


        /// <summary>
        /// a timing of slide start
        /// </summary>
        public float startTiming;
        public int sortIndex;
        public bool isJustR;
        public float fadeInTiming;
        public float fullFadeInTiming;
        public int endPosition;
        public string slideType;

        /// <summary>
        /// 引导Star
        /// </summary>
        public GameObject[] stars = new GameObject[3];

        protected Animator fadeInAnimator;
        protected GameObject slideOK;
        protected bool isSoundPlayed = false;
        protected float lastWaitTime;
        protected bool canCheck = false;
        protected bool isChecking = false;
        
        protected float maxFadeInAlpha = 0.5f; // 淡入时最大不透明度
        /// <summary>
        /// 存储Slide Queue中会经过的区域
        /// <para>用于绑定或解绑Event</para>
        /// </summary>
        protected IEnumerable<SensorType> judgeAreas;
        public abstract void Initialize();
        protected override void Judge(float currentSec)
        {
            if (!ConnectInfo.IsGroupPartEnd && ConnectInfo.IsConnSlide)
                return;
            else if (isJudged)
                return;
            //var stayTime = time + LastFor - judgeTiming; // 停留时间
            var stayTime = lastWaitTime; // 停留时间

            // By Minepig
            var diff = currentSec - JudgeTiming;
            judgeDiff = diff * 1000;
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
            judgeResult = judge ?? JudgeType.Miss;
            isJudged = true;

            var remainingStartTime = gpManager.AudioTime - ConnectInfo.StartTiming;
            if (remainingStartTime < 0)
                lastWaitTime = MathF.Abs(remainingStartTime) / 2;
            else if (diff >= 0.6166679 && !isFast)
                lastWaitTime = 0;
        }
        protected void Judge_Classic(float currentSec)
        {
            if (!ConnectInfo.IsGroupPartEnd && ConnectInfo.IsConnSlide)
                return;
            else if (isJudged)
                return;

            var diff = currentSec - JudgeTiming;
            judgeDiff = diff * 1000;
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
            judgeResult = judge ?? JudgeType.Miss;
            isJudged = true;

            var remainingStartTime = gpManager.AudioTime - ConnectInfo.StartTiming;
            if (remainingStartTime < 0)
                lastWaitTime = MathF.Abs(remainingStartTime) / 2;
            else if (diff >= 0.6166679 && !isFast)
                lastWaitTime = 0;
        }
        protected void HideBar(int endIndex)
        {
            endIndex = endIndex - 1;
            endIndex = Math.Min(endIndex, slideBars.Length - 1);
            for (int i = 0; i <= endIndex; i++)
                slideBars[i].SetActive(false);
        }
        protected void PlaySlideOK(in JudgeResult result)
        {
            if (slideOK == null)
                return;
            
            var canPlay = NoteEffectManager.CheckJudgeDisplaySetting(GameManager.Instance.Setting.Display.SlideJudgeType, result);

            if (canPlay)
                slideOK.SetActive(true);
            else
                Destroy(slideOK);
        }
        protected void HideAllBar() => HideBar(int.MaxValue);
        protected void SetSlideBarAlpha(float alpha)
        {
            foreach (var gm in slideBars)
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
            if (isJudged)
            {
                DestroySelf();
                return;
            }

            if (QueueRemaining == 1)
                judgeResult = JudgeType.LateGood;
            else
                judgeResult = JudgeType.Miss;
            isJudged = true;
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

                foreach (GameObject obj in slideBars)
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
                judgeQueues[i] = emptyQueue;
        }
        void DestroyStars()
        {
            if (stars.IsEmpty())
                return;
            foreach (var star in stars)
            {
                if (star != null)
                    Destroy(star);
            }
            stars = Array.Empty<GameObject>();
        }
        protected async UniTaskVoid FadeIn()
        {
            fadeInTiming = Math.Max(fadeInTiming,CurrentSec);
            var num = startTiming - 0.05f;
            float interval = (num - fadeInTiming).Clamp(0, 0.2f);
            float fullFadeInTiming = fadeInTiming + interval;//淡入到maxFadeInAlpha的时间点

            while (CurrentSec < fullFadeInTiming) 
            {
                var diff = (fullFadeInTiming - CurrentSec).Clamp(0, interval);
                float alpha = 0;

                if(interval != 0)
                    alpha = 1 - (diff / interval);
                alpha *= maxFadeInAlpha;
                SetSlideBarAlpha(alpha);
                await UniTask.Yield();
            }
            SetSlideBarAlpha(maxFadeInAlpha);
            while (CurrentSec < num)
                await UniTask.Yield();
            SetSlideBarAlpha(1f);
        }
    }
}
