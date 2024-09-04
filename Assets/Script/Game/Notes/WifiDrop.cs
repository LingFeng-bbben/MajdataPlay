using MajdataPlay.Extensions;
using MajdataPlay.Game.Controllers;
using MajdataPlay.Interfaces;
using MajdataPlay.IO;
using MajdataPlay.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#nullable enable
namespace MajdataPlay.Game.Notes
{
    public class WifiDrop : SlideBase
    {

        List<int> areaStep = new List<int>();


        readonly Vector3[] SlidePositionEnd = new Vector3[3];

        readonly SpriteRenderer[] starRenderers = new SpriteRenderer[3];

        private Vector3 SlidePositionStart;

        public override void Initialize()
        {
            if (isInitialized)
                return;
            base.Start();
            isInitialized = true;

            judgeQueues = SlideTables.FindWifiTable(startPosition);
            areaStep = NoteLoader.SLIDE_AREA_STEP_MAP["wifi"];

            // 计算Slide淡入时机
            // 在8.0速时应当提前300ms显示Slide
            fadeInTiming = -3.926913f / speed;
            fadeInTiming += gameSetting.Game.SlideFadeInOffset;
            fadeInTiming += timeStart;
            // Slide完全淡入时机
            // 正常情况下应为负值；速度过高将忽略淡入
            fullFadeInTiming = fadeInTiming + 0.2f;
            //var interval = fullFadeInTiming - fadeInTiming;
            fadeInAnimator = GetComponent<Animator>();
            //淡入时机与正解帧间隔小于200ms时，加快淡入动画的播放速度
            //fadeInAnimator.speed = 0.2f / interval;
            fadeInAnimator.SetTrigger("wifi");

            SlidePositionEnd[0] = effectManager.transform.GetChild(0).GetChild(endPosition - 2 < 0 ? 7 : endPosition - 2).position;// R
            SlidePositionEnd[1] = effectManager.transform.GetChild(0).GetChild(endPosition - 1).position;// Center
            SlidePositionEnd[2] = effectManager.transform.GetChild(0).GetChild(endPosition >= 8 ? 0 : endPosition).position; // L
            SlidePositionStart = GetPositionFromDistance(4.8f);

            slideOK = transform.GetChild(transform.childCount - 1).gameObject; //slideok is the last one

            transform.rotation = Quaternion.Euler(0f, 0f, -45f * (startPosition - 1));
            slideBars = new GameObject[transform.childCount - 1];

            for (var i = 0; i < transform.childCount - 1; i++)
                slideBars[i] = transform.GetChild(i).gameObject;

            LoadSkin();
        }
        protected override void Start()
        {
            
            Initialize();

            var wifiConst = 0.162870f;
            judgeTiming = (time + LastFor) * (1 - wifiConst);
            lastWaitTime = LastFor * wifiConst;

            judgeAreas = judgeQueues.SelectMany(x => x.SelectMany(y => y.GetSensorTypes()))
                                    .GroupBy(x => x)
                                    .Select(x => x.Key);

            foreach (var sensor in judgeAreas)
                ioManager.BindSensor(Check, sensor);
        }
        private void FixedUpdate()
        {
            /// time      是Slide启动的时间点
            /// timeStart 是Slide完全显示但未启动
            /// LastFor   是Slide的时值
            var timing = gpManager.AudioTime - time;
            var startTiming = gpManager.AudioTime - timeStart;
            var tooLateTiming = time + LastFor + 0.6 + MathF.Min(gameSetting.Judge.JudgeOffset, 0);
            var isTooLate = gpManager.AudioTime - tooLateTiming >= 0;

            if (startTiming >= -0.05f)
                canCheck = true;

            if(!isJudged)
            {
                if (IsFinished)
                {
                    HideAllBar();
                    Judge();
                }
                else if (isTooLate)
                    TooLateJudge();
            }
            else
            {
                if (lastWaitTime < 0)
                    DestroySelf();
                else
                    lastWaitTime -= Time.fixedDeltaTime;
            }
        }
        protected override void Check(object sender, InputEventArgs arg) => CheckAll();
        void CheckAll()
        {
            if (IsFinished || !canCheck)
                return;
            else if (isChecking)
                return;
            isChecking = true;
            for (int i = 0; i < 3; i++)
                Check(ref judgeQueues[i]);
            isChecking = false;
        }
        void Check(ref JudgeArea[] judgeQueue)
        {
            if (judgeQueue.IsEmpty())
                return;

            var first = judgeQueue.First();
            JudgeArea? second = null;

            if (judgeQueue.Length >= 2)
                second = judgeQueue[1];
            var fType = first.GetSensorTypes();
            foreach (var t in fType)
            {
                var sensor = ioManager.GetSensor(t);
                first.Judge(t, sensor.Status);
            }

            if (first.IsFinished && !isSoundPlayed)
            {
                var audioEffMana = GameObject.Find("NoteAudioManager").GetComponent<NoteAudioManager>();
                audioEffMana.PlaySlideSound(isBreak);
                isSoundPlayed = true;
            }

            if (second is not null && (first.CanSkip || first.On))
            {
                var sType = second.GetSensorTypes();
                foreach (var t in sType)
                {
                    var sensor = ioManager.GetSensor(t);
                    second.Judge(t, sensor.Status);
                }

                if (second.IsFinished)
                {
                    judgeQueue = judgeQueue.Skip(2).ToArray();
                    return;
                }
                else if (second.On)
                {
                    judgeQueue = judgeQueue.Skip(1).ToArray();
                    return;
                }
            }

            if (first.IsFinished)
            {
                judgeQueue = judgeQueue.Skip(1).ToArray();
                return;
            }
            if (!IsFinished)
            {
                var index = areaStep[4 - QueueRemaining];
                HideBar(index);
            }

        }
        void Update()
        {
            // Wifi Slide淡入期间，不透明度从0到1耗时200ms
            var currentSec = gpManager.AudioTime;
            var startiming = currentSec - timeStart;

            if (fadeInTiming > timeStart)
            {
                if (currentSec > fadeInTiming)
                    SetSlideBarAlpha(1f);
            }
            else if (currentSec > timeStart)
                SetSlideBarAlpha(1f);
            else if (currentSec > fadeInTiming)
            {
                if (startiming >= -0.05f)
                {
                    fadeInAnimator.enabled = false;
                    SetSlideBarAlpha(1f);
                }
                else
                    fadeInAnimator.enabled = true;
                return;
            }
            fadeInAnimator.enabled = false;

            foreach (var star in stars)
                star.SetActive(true);

            var timing = gpManager.AudioTime - time;
            if (timing <= 0f)
            {
                CanShine = true;
                float alpha;
                alpha = 1f - -timing / (time - timeStart);
                alpha = alpha > 1f ? 1f : alpha;
                alpha = alpha < 0f ? 0f : alpha;

                for (var i = 0; i < stars.Length; i++)
                {
                    starRenderers[i].color = new Color(1, 1, 1, alpha);
                    stars[i].transform.localScale = new Vector3(alpha + 0.5f, alpha + 0.5f, alpha + 0.5f);
                    stars[i].transform.position = SlidePositionStart;
                }
            }
            else
                UpdateStar();
            CheckAll();
        }
        void UpdateStar()
        {
            var timing = gpManager.AudioTime - time;
            var process = (LastFor - timing) / LastFor;
            process = 1f - process;

            if (process >= 1)
            {
                for (var i = 0; i < stars.Length; i++)
                {
                    starRenderers[i].color = Color.white;
                    stars[i].transform.position = SlidePositionEnd[i];
                    stars[i].transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
                }
                if (IsFinished && isJudged)
                    DestroySelf();
            }
            else
            {
                for (var i = 0; i < stars.Length; i++)
                {
                    starRenderers[i].color = Color.white;
                    stars[i].transform.position =
                        (SlidePositionEnd[i] - SlidePositionStart) * process + SlidePositionStart; //TODO add some runhua
                    stars[i].transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
                }
            }
        }
        void OnDestroy()
        {
            if (isDestroying)
                return;
            foreach (var sensor in judgeAreas)
                ioManager.UnbindSensor(Check, sensor);

            objectCounter.ReportResult(this, judgeResult, isBreak);
            if (isBreak && judgeResult == JudgeType.Perfect)
            {
                var anim = slideOK.GetComponent<Animator>();
                var audioEffMana = GameObject.Find("NoteAudioManager").GetComponent<NoteAudioManager>();
                anim.runtimeAnimatorController = SkinManager.Instance.JustBreak;
                audioEffMana.PlayBreakSlideEndSound();
            }
            slideOK.GetComponent<LoadJustSprite>().SetResult(judgeResult);
            slideOK.SetActive(true);

            isDestroying = true;
        }
        protected override void LoadSkin()
        {
            var bars = slideBars;
            var skin = SkinManager.Instance.GetWifiSkin();

            var barSprites = skin.Normal;
            var starSprite = skin.Star.Normal;
            Material? breakMaterial = null;

            if (isEach)
            {
                barSprites = skin.Each;
                starSprite = skin.Star.Each;
            }
            if (isBreak)
            {
                barSprites = skin.Break;
                starSprite = skin.Star.Break;
                breakMaterial = skin.BreakMaterial;
            }
            foreach(var (i,bar) in bars.WithIndex())
            {
                var barRenderer = bar.GetComponent<SpriteRenderer>();

                barRenderer.color = new Color(1f, 1f, 1f, 0f);
                barRenderer.sortingOrder = sortIndex--;
                barRenderer.sortingLayerName = "Slides";

                barRenderer.sprite = barSprites[i];
                if (breakMaterial != null)
                {
                    barRenderer.material = breakMaterial;
                    var controller = bar.AddComponent<BreakShineController>();
                    controller.parent = this;
                }
            }
            foreach(var (i, star) in stars.WithIndex())
            {
                var starRenderer = star.GetComponent<SpriteRenderer>();
                starRenderers[i] = starRenderer;
                starRenderer.sprite = starSprite;
                if (breakMaterial != null)
                {
                    starRenderer.material = breakMaterial;
                    var controller = star.AddComponent<BreakShineController>();
                    controller.parent = this;
                }
                star.transform.rotation = Quaternion.Euler(0, 0, -22.5f * (8 + i + 2 * (startPosition - 1)));
                star.SetActive(false);
            }

            if (isJustR)
                slideOK.GetComponent<LoadJustSprite>().setR();
            else
            {
                slideOK.GetComponent<LoadJustSprite>().setL();
                slideOK.transform.Rotate(new Vector3(0f, 0f, 180f));
            }
            slideOK.SetActive(false);
            slideOK.transform.SetParent(transform.parent);
        }
    }
}