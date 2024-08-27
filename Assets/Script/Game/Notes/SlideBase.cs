using MajdataPlay.Interfaces;
using MajdataPlay.IO;
using MajdataPlay.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MajdataPlay.Game.Notes
{
    public abstract class SlideBase : NoteLongDrop, IFlasher
    {
        public ConnSlideInfo ConnectInfo { get; set; } = new()
        {
            IsGroupPart = false,
            Parent = null
        };
        public bool isFinished { get => judgeQueue.Length == 0; }
        public bool isPendingFinish { get => judgeQueue.Length == 1; }
        public bool CanShine { get; protected set; } = false;

        protected JudgeArea[] judgeQueue = { }; // 判定队列

        protected GameObject[] slideBars = { };


        /// <summary>
        /// a timing of slide start
        /// </summary>
        public float timeStart;
        public int sortIndex;
        public bool isJustR;
        public float fadeInTime;
        public float fullFadeInTime;
        public int endPosition;
        public string slideType;

        protected Animator fadeInAnimator;
        protected GameObject slideOK;
        protected bool isSoundPlayed = false;
        protected float lastWaitTime;
        protected bool canCheck = false;
        protected bool isChecking = false;
        protected float judgeTiming; // 正解帧
        protected bool isInitialized = false; //防止重复初始化
        protected bool isDestroying = false; // 防止重复销毁

        public abstract void Check(object sender, InputEventArgs arg);
        protected void Judge()
        {
            if (!ConnectInfo.IsGroupPartEnd && ConnectInfo.IsConnSlide)
                return;
            else if (isJudged)
                return;
            //var stayTime = time + LastFor - judgeTiming; // 停留时间
            var stayTime = lastWaitTime; // 停留时间

            // By Minepig
            var diff = judgeTiming - gpManager.AudioTime;
            var isFast = diff > 0;

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

            if (GetJudgeTiming() < 0)
                lastWaitTime = MathF.Abs(GetJudgeTiming()) / 2;
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
        protected void SetSlideBarAlpha(float alpha)
        {
            foreach (var gm in slideBars)
                gm.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, alpha);
        }
    }
}
