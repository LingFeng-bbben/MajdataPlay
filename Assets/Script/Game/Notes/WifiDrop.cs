using MajdataPlay.Extensions;
using MajdataPlay.Game.Controllers;
using MajdataPlay.Interfaces;
using MajdataPlay.IO;
using MajdataPlay.Types;
using MajdataPlay.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#nullable enable
namespace MajdataPlay.Game.Notes
{
    public sealed class WifiDrop : SlideBase
    {


        readonly Vector3[] SlidePositionEnd = new Vector3[3];

        readonly SpriteRenderer[] starRenderers = new SpriteRenderer[3];

        Vector3[] SlidePositionStart = new Vector3[3];

        public override void Initialize()
        {
            if (IsInitialized)
                return;
            base.Start();
            State = NoteStatus.Initialized;
            ConnectInfo.StartTiming = Timing;
            judgeQueues = SlideTables.GetWifiTable(StartPos);

            // 计算Slide淡入时机
            // 在8.0速时应当提前300ms显示Slide
            fadeInTiming = -3.926913f / Speed;
            fadeInTiming += _gameSetting.Game.SlideFadeInOffset;
            fadeInTiming += startTiming;
            // Slide完全淡入时机
            // 正常情况下应为负值；速度过高将忽略淡入
            fullFadeInTiming = fadeInTiming + 0.2f;
            //var interval = fullFadeInTiming - fadeInTiming;
            Destroy(GetComponent<Animator>());
            maxFadeInAlpha = 1f;
            //淡入时机与正解帧间隔小于200ms时，加快淡入动画的播放速度
            //fadeInAnimator.speed = 0.2f / interval;
            //fadeInAnimator.SetTrigger("wifi");
            var sensorPos = (SensorType)(endPosition - 1);
            var rIndex = sensorPos.Diff(-1).GetIndex();
            var lIndex = sensorPos.Diff(1).GetIndex();
            SlidePositionEnd[0] = GetPositionFromDistance(4.8f, rIndex);// R
            SlidePositionEnd[1] = GetPositionFromDistance(4.8f, endPosition);// Center
            SlidePositionEnd[2] = GetPositionFromDistance(4.8f, lIndex); // L

            if(IsClassic)
            {
                SlidePositionStart[0] = GetPositionFromDistance(4.55f,StartPos + 0.11f);
                SlidePositionStart[1] = GetPositionFromDistance(4.8f);
                SlidePositionStart[2] = GetPositionFromDistance(4.55f, StartPos - 0.13f);
            }
            else
            {
                SlidePositionStart[0] = GetPositionFromDistance(4.8f);
                SlidePositionStart[1] = GetPositionFromDistance(4.8f);
                SlidePositionStart[2] = GetPositionFromDistance(4.8f);
            }

            slideOK = transform.GetChild(transform.childCount - 1).gameObject; //slideok is the last one

            transform.rotation = Quaternion.Euler(0f, 0f, -45f * (StartPos - 1));
            slideBars = new GameObject[transform.childCount - 1];

            for (var i = 0; i < transform.childCount - 1; i++)
                slideBars[i] = transform.GetChild(i).gameObject;

            LoadSkin();
        }
        protected override void Start()
        {
            
            Initialize();

            var wifiConst = 0.162870f;
            _judgeTiming = Timing + (Length * (1 - wifiConst));
            lastWaitTime = Length * wifiConst;

            judgeAreas = judgeQueues.SelectMany(x => x.SelectMany(y => y.GetSensorTypes()))
                                    .GroupBy(x => x)
                                    .Select(x => x.Key);

            foreach (var sensor in judgeAreas)
                _ioManager.BindSensor(Check, sensor);
            FadeIn().Forget();
        }
        private void FixedUpdate()
        {
            /// time      是Slide启动的时间点
            /// timeStart 是Slide完全显示但未启动
            /// LastFor   是Slide的时值
            var timing = _gpManager.AudioTime - base.Timing;
            var startTiming = _gpManager.AudioTime - base.startTiming;
            var tooLateTiming = base.Timing + Length + 0.6 + MathF.Min(_gameSetting.Judge.JudgeOffset, 0);
            var isTooLate = _gpManager.AudioTime - tooLateTiming >= 0;

            if (startTiming >= -0.05f)
                canCheck = true;

            if(!_isJudged)
            {
                if (IsFinished)
                {
                    HideAllBar();
                    if (IsClassic)
                        Judge_Classic(_gpManager.ThisFrameSec);
                    else
                        Judge(_gpManager.ThisFrameSec);
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
                var sensor = _ioManager.GetSensor(t);
                first.Judge(t, sensor.Status);
            }

            if (!isSoundPlayed && first.On)
            {
                _audioEffMana.PlaySlideSound(IsBreak);
                isSoundPlayed = true;
            }

            if (second is not null && (first.CanSkip || first.On))
            {
                var sType = second.GetSensorTypes();
                foreach (var t in sType)
                {
                    var sensor = _ioManager.GetSensor(t);
                    second.Judge(t, sensor.Status);
                }

                if (second.IsFinished)
                {
                    judgeQueue = judgeQueue.Skip(2).ToArray();
                    HideBar(GetIndex());
                    return;
                }
                else if (second.On)
                {
                    judgeQueue = judgeQueue.Skip(1).ToArray();
                    HideBar(GetIndex());
                    return;
                }
            }

            if (first.IsFinished)
            {
                judgeQueue = judgeQueue.Skip(1).ToArray();
                HideBar(GetIndex());
                return;
            }

        }
        int GetIndex()
        {
            if(judgeQueues.IsEmpty())
                return int.MaxValue;
            else if (judgeQueues[1].IsEmpty())
            {
                if (judgeQueues[0].Length <= 1 && judgeQueues[2].Length <= 1)
                    return 9;
            }
            var nums = new int[3];
            foreach(var (i,queue) in judgeQueues.WithIndex())
                nums[i] = queue.Length;
            var max = nums.Max();
            var index = nums.FindIndex(x => x == max);

            return judgeQueues[index].First().SlideIndex;
        }
        void Update()
        {
            foreach (var star in stars)
                star.SetActive(true);

            var timing = CurrentSec - base.Timing;
            if (timing <= 0f)
            {
                CanShine = true;
                float alpha;
                alpha = 1f - -timing / (base.Timing - startTiming);
                alpha = alpha > 1f ? 1f : alpha;
                alpha = alpha < 0f ? 0f : alpha;

                for (var i = 0; i < stars.Length; i++)
                {
                    starRenderers[i].color = new Color(1, 1, 1, alpha);
                    stars[i].transform.localScale = new Vector3(alpha + 0.5f, alpha + 0.5f, alpha + 0.5f);
                    stars[i].transform.position = SlidePositionStart[i];
                }
            }
            else
                UpdateStar();
            CheckAll();
        }
        void UpdateStar()
        {
            var timing = _gpManager.AudioTime - base.Timing;
            var process = (Length - timing) / Length;
            process = 1f - process;

            if (process >= 1)
            {
                for (var i = 0; i < stars.Length; i++)
                {
                    starRenderers[i].color = Color.white;
                    stars[i].transform.position = SlidePositionEnd[i];
                    stars[i].transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
                }
            }
            else
            {
                for (var i = 0; i < stars.Length; i++)
                {
                    starRenderers[i].color = Color.white;
                    stars[i].transform.position =
                        (SlidePositionEnd[i] - SlidePositionStart[i]) * process + SlidePositionStart[i]; //TODO add some runhua
                    stars[i].transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
                }
            }
        }
        void OnDestroy()
        {
            if (IsDestroyed)
                return;
            foreach (var sensor in judgeAreas)
                _ioManager.UnbindSensor(Check, sensor);
            State = NoteStatus.Destroyed;
            var result = new JudgeResult()
            {
                Result = _judgeResult,
                Diff = _judgeDiff,
                IsEX = IsEX,
                IsBreak = IsBreak
            };

            _objectCounter.ReportResult(this, result);
            if (IsBreak && _judgeResult == JudgeType.Perfect)
            {
                var anim = slideOK.GetComponent<Animator>();
                anim.runtimeAnimatorController = MajInstances.SkinManager.JustBreak;
                _audioEffMana.PlayBreakSlideEndSound();
            }
            slideOK.GetComponent<LoadJustSprite>().SetResult(_judgeResult);
            PlaySlideOK(result);
        }
        protected override void LoadSkin()
        {
            var bars = slideBars;
            var skin = MajInstances.SkinManager.GetWifiSkin();

            var barSprites = skin.Normal;
            var starSprite = skin.Star.Normal;
            Material? breakMaterial = null;

            if (IsEach)
            {
                barSprites = skin.Each;
                starSprite = skin.Star.Each;
            }
            if (IsBreak)
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
                    controller.Parent = this;
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
                    controller.Parent = this;
                }
                star.transform.rotation = Quaternion.Euler(0, 0, -22.5f * (8 + i + 2 * (StartPos - 1)));
                star.SetActive(false);
            }
            if(IsClassic)
            {
                starRenderers[0].sortingOrder = -1;
                starRenderers[2].sortingOrder = -1;
            }

            if (isJustR)
                slideOK.GetComponent<LoadJustSprite>().SetR();
            else
            {
                slideOK.GetComponent<LoadJustSprite>().SetL();
                slideOK.transform.Rotate(new Vector3(0f, 0f, 180f));
            }
            slideOK.SetActive(false);
            slideOK.transform.SetParent(transform.parent);
        }
    }
}