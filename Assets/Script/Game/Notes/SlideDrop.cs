using MajdataPlay.Extensions;
using MajdataPlay.Game.Controllers;
using MajdataPlay.IO;
using MajdataPlay.Types;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#nullable enable
namespace MajdataPlay.Game.Notes
{
    public class SlideDrop : SlideBase
    {
        public bool isMirror;
        public bool isSpecialFlip; // fixes known star problem
        
        private readonly List<Vector3> slidePositions = new();
        private readonly List<Quaternion> slideRotations = new();

        SpriteRenderer starRenderer;
        SlideTable table;
        
        /// <summary>
        /// Slide初始化
        /// </summary>
        public override void Initialize()
        {
            if (isInitialized)
                return;
            base.Start();
            isInitialized = true;
            var slideTable = SlideTables.FindTableByName(slideType);
            if (slideTable is null)
                throw new MissingComponentException($"Slide table of \"{slideType}\" is not found");
            table = slideTable;
            slideOK = transform.GetChild(transform.childCount - 1).gameObject; //slideok is the last one        
            starRenderer = stars[0].GetComponent<SpriteRenderer>();
            slideBars = new GameObject[transform.childCount - 1];
            for (var i = 0; i < transform.childCount - 1; i++)
                slideBars[i] = transform.GetChild(i).gameObject;


            if (isMirror)
            {
                table.Mirror();
                transform.localScale = new Vector3(-1f, 1f, 1f);
                transform.rotation = Quaternion.Euler(0f, 0f, -45f * startPosition);
                slideOK.transform.localScale = new Vector3(-1f, 1f, 1f);
            }
            else
            {
                transform.rotation = Quaternion.Euler(0f, 0f, -45f * (startPosition - 1));
            }

            var diff = Math.Abs(1 - startPosition);
            if(diff != 0)
                table.Diff(diff);

            LoadPath();
            LoadSkin();

            // 计算Slide淡入时机
            // 在8.0速时应当提前300ms显示Slide
            fadeInTiming = -3.926913f / speed;
            fadeInTiming += gameSetting.Game.SlideFadeInOffset;
            fadeInTiming += startTiming;
            // Slide完全淡入时机
            // 正常情况下应为负值；速度过高将忽略淡入
            fullFadeInTiming = fadeInTiming + 0.2f;
            //var interval = fullFadeInTiming - fadeInTiming;
            //fadeInAnimator = GetComponent<Animator>();
            Destroy(GetComponent<Animator>());
            //淡入时机与正解帧间隔小于200ms时，加快淡入动画的播放速度
            //fadeInAnimator.speed = 0.2f / interval;
            //fadeInAnimator.SetTrigger("slide");
            SetSlideBarAlpha(0f);
            judgeQueues[0] = table.JudgeQueue;

            if (ConnectInfo.IsConnSlide && ConnectInfo.IsGroupPartEnd)
                judgeQueues[0].LastOrDefault().SetIsLast();
            else if (ConnectInfo.IsConnSlide)
                judgeQueues[0].LastOrDefault().SetNonLast();


        }
        public float GetSlideLength()
        {
            float len = 0;
            for (int i = 0; i < slidePositions.Count - 2; i++)
            {
                var a = slidePositions[i];
                var b = slidePositions[i + 1];
                len += (b - a).magnitude;
            }
            return len;
        }
        protected override void Start()
        {
            Initialize();
            if (ConnectInfo.IsConnSlide)
            {
                LastFor = ConnectInfo.TotalLength / ConnectInfo.TotalSlideLen * GetSlideLength();
                if (!ConnectInfo.IsGroupPartHead)
                {
                    var parent = ConnectInfo.Parent!.GetComponent<SlideDrop>();
                    timing = parent.timing + parent.LastFor;
                }
            }

            if(ConnectInfo.IsGroupPartEnd || !ConnectInfo.IsConnSlide)
            {
                var percent = table.Const;
                judgeTiming = timing + LastFor * (1 - percent);
                lastWaitTime = LastFor *  percent;
            }

            judgeAreas = table.JudgeQueue.SelectMany(x => x.GetSensorTypes())
                                         .GroupBy(x => x)
                                         .Select(x => x.Key)
                                         .ToArray();

            foreach (var sensor in judgeAreas)
                ioManager.BindSensor(Check, sensor);
            FadeIn().Forget();
        }
        void FixedUpdate()
        {
            /// time      是Slide启动的时间点
            /// timeStart 是Slide完全显示但未启动
            /// LastFor   是Slide的时值
            var timing = gpManager.AudioTime - base.timing;
            var startTiming = gpManager.AudioTime - base.startTiming;
            var tooLateTiming = base.timing + LastFor + 0.6 + MathF.Min(gameSetting.Judge.JudgeOffset , 0);
            var isTooLate = gpManager.AudioTime - tooLateTiming >= 0;

            if (!canCheck)
            {
                if (ConnectInfo.IsGroupPart)
                {
                    if (ConnectInfo.IsGroupPartHead && startTiming >= -0.05f)
                        canCheck = true;
                    else if (!ConnectInfo.IsGroupPartHead)
                        canCheck = ConnectInfo.ParentFinished || ConnectInfo.ParentPendingFinish;
                }
                else if (startTiming >= -0.05f)
                    canCheck = true;
            }

            var canJudge = ConnectInfo.IsGroupPartEnd || !ConnectInfo.IsConnSlide;

            if(canJudge)
            {
                if(!isJudged)
                {
                    if (IsFinished)
                    {
                        HideAllBar();
                        if(IsClassic)
                            Judge_Classic();
                        else
                            Judge();
                        return;
                    }
                    else if(isTooLate)
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
        }
        void Update()
        {
            // ConnSlide
            if (stars.IsEmpty() || stars[0] == null)
            {
                if (IsFinished)
                    DestroySelf();
                return;
            }

            stars[0].SetActive(true);
            var timing = CurrentSec - base.timing;
            if (timing <= 0f)
            {
                CanShine = true;
                float alpha;
                if (ConnectInfo.IsConnSlide && !ConnectInfo.IsGroupPartHead)
                    alpha = 0;
                else
                {
                    // 只有当它是一个起点Slide（而非Slide Group中的子部分）的时候，才会有开始的星星渐入动画
                    alpha = 1f - -timing / (base.timing - startTiming);
                    alpha = alpha > 1f ? 1f : alpha;
                    alpha = alpha < 0f ? 0f : alpha;
                }

                starRenderer.color = new Color(1, 1, 1, alpha);
                stars[0].transform.localScale = new Vector3(alpha + 0.5f, alpha + 0.5f, alpha + 0.5f);
                stars[0].transform.position = slidePositions[0];
                ApplyStarRotation(slideRotations[0]);
            }
            else
                UpdateStar();
            Check();
        }        
        /// <summary>
        /// 判定队列检查
        /// </summary>
        void Check()
        {
            if (IsFinished || !canCheck)
                return;
            else if (isChecking)
                return;
            var queue = judgeQueues[0];
            isChecking = true;
            
            
            var first = queue.First();
            JudgeArea? second = null;

            if (queue.Length >= 2)
                second = queue[1];
            var fType = first.GetSensorTypes();
            foreach (var t in fType)
            {
                var sensor = ioManager.GetSensor(t);
                first.Judge(t, sensor.Status);

            }

            if (first.IsFinished && !isSoundPlayed && (ConnectInfo.IsGroupPartHead || !ConnectInfo.IsConnSlide))
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
                    HideBar(first.SlideIndex);
                    judgeQueues[0] = queue.Skip(2).ToArray();
                    isChecking = false;
                    SetParentFinish();
                    return;
                }
                else if (second.On)
                {
                    HideBar(first.SlideIndex);
                    judgeQueues[0] = queue.Skip(1).ToArray();
                    isChecking = false;
                    SetParentFinish();
                    return;
                }
            }

            if (first.IsFinished)
            {
                HideBar(first.SlideIndex);
                judgeQueues[0] = queue.Skip(1).ToArray();
                isChecking = false;
                SetParentFinish();
                return;
            }
            isChecking = false;
        }
        void SetParentFinish()
        {
            if (ConnectInfo.Parent != null && judgeQueues[0].Length < table.JudgeQueue.Length)
            {
                if (!ConnectInfo.ParentFinished)
                    ConnectInfo.Parent.GetComponent<SlideDrop>().ForceFinish();
            }
        }
        void OnDestroy()
        {
            if (isDestroying)
                return;
            if (ConnectInfo.Parent != null)
                Destroy(ConnectInfo.Parent);
            if (!stars.IsEmpty() && stars[0] != null)
                Destroy(stars[0]);
            foreach (var sensor in judgeAreas)
                ioManager.UnbindSensor(Check, sensor);
            State = NoteStatus.Destroyed;
            if (ConnectInfo.IsGroupPartEnd || !ConnectInfo.IsConnSlide)
            {
                var result = new JudgeResult()
                {
                    Result = judgeResult,
                    Diff = judgeDiff,
                    IsBreak = isBreak
                };
                // 只有组内最后一个Slide完成 才会显示判定条并增加总数
                objectCounter.ReportResult(this, result);
                
                if (isBreak && judgeResult == JudgeType.Perfect) 
                { 
                    var anim = slideOK.GetComponent<Animator>();
                    var audioEffMana = GameObject.Find("NoteAudioManager").GetComponent<NoteAudioManager>();
                    anim.runtimeAnimatorController = SkinManager.Instance.JustBreak;
                    audioEffMana.PlayBreakSlideEndSound();
                }
                slideOK.GetComponent<LoadJustSprite>().SetResult(judgeResult);
                PlaySlideOK(result);
            }
            else
            {
                // 如果不是组内最后一个 那么也要将判定条删掉
                Destroy(slideOK);
            }
            isDestroying = true;
        }
        /// <summary>
        /// 更新引导Star状态
        /// <para>包括位置，角度</para>
        /// </summary>
        void UpdateStar()
        {
            starRenderer.color = Color.white;
            stars[0].transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);

            var process = MathF.Min((LastFor - GetRemainingTimeWithoutOffset()) / LastFor, 1);
            var indexProcess = (slidePositions.Count - 1) * process;
            var index = (int)indexProcess;
            var pos = indexProcess - index;

            if (process == 1)
            {
                stars[0].transform.position = slidePositions.LastOrDefault();
                ApplyStarRotation(slideRotations.LastOrDefault());
                if (ConnectInfo.IsConnSlide && !ConnectInfo.IsGroupPartEnd)
                    DestroySelf(true);
            }
            else
            {
                var a = slidePositions[index + 1];
                var b = slidePositions[index];
                var ba = a - b;
                var newPos = ba * pos + b;

                stars[0].transform.position = newPos;
                if (index < slideRotations.Count - 1)
                {
                    var _a = slideRotations[index + 1].eulerAngles.z;
                    var _b = slideRotations[index].eulerAngles.z;
                    var dAngle = Mathf.DeltaAngle(_b, _a) * pos;
                    dAngle = Mathf.Abs(dAngle);
                    var newRotation = Quaternion.Euler(0f, 0f,
                                    Mathf.MoveTowardsAngle(_b, _a, dAngle));
                    ApplyStarRotation(newRotation);
                }
            }
        }
        void ApplyStarRotation(Quaternion newRotation)
        {
            var halfFlip = newRotation.eulerAngles;
            halfFlip.z += 180f;
            if (isSpecialFlip)
                stars[0].transform.rotation = Quaternion.Euler(halfFlip);
            else
                stars[0].transform.rotation = newRotation;
        }
        void LoadPath()
        {
            slidePositions.Add(GetPositionFromDistance(4.8f));
            foreach (var bars in slideBars)
            {
                slidePositions.Add(bars.transform.position);

                slideRotations.Add(Quaternion.Euler(bars.transform.rotation.normalized.eulerAngles + new Vector3(0f, 0f, 18f)));
            }
            var endPos = GetPositionFromDistance(4.8f, endPosition);
            slidePositions.Add(endPos);
            slideRotations.Add(slideRotations.LastOrDefault());
        }
        protected override void LoadSkin()
        {
            var bars = slideBars;
            var skin = SkinManager.Instance.GetSlideSkin();

            var barSprite = skin.Normal;
            var starSprite = skin.Star.Normal;
            Material? breakMaterial = null;

            if(isEach)
            {
                barSprite = skin.Each;
                starSprite = skin.Star.Each;
            }
            if(isBreak)
            {
                barSprite = skin.Break;
                starSprite = skin.Star.Break;
                breakMaterial = skin.BreakMaterial;
            }

            foreach(var bar in bars)
            {
                var barRenderer = bar.GetComponent<SpriteRenderer>();
                
                barRenderer.color = new Color(1f, 1f, 1f, 0f);
                barRenderer.sortingOrder = sortIndex--;
                barRenderer.sortingLayerName = "Slides";

                barRenderer.sprite = barSprite;
                

                if(breakMaterial != null)
                {
                    barRenderer.material = breakMaterial;
                    var controller = bar.AddComponent<BreakShineController>();
                    controller.Parent = this;
                }
            }

            var starRenderer = stars[0].GetComponent<SpriteRenderer>();
            starRenderer.sprite = starSprite;
            if (breakMaterial != null)
            {
                starRenderer.material = breakMaterial;
                var controller = stars[0].AddComponent<BreakShineController>();
                controller.Parent = this;
            }

            if (isJustR)
            {
                if (slideOK.GetComponent<LoadJustSprite>().SetR() == 1 && isMirror)
                {
                    slideOK.transform.Rotate(new Vector3(0f, 0f, 180f));
                    var angel = slideOK.transform.rotation.eulerAngles.z * Mathf.Deg2Rad;
                    slideOK.transform.position += new Vector3(Mathf.Sin(angel) * 0.27f, Mathf.Cos(angel) * -0.27f);
                }
            }
            else
            {
                if (slideOK.GetComponent<LoadJustSprite>().SetL() == 1 && !isMirror)
                {
                    slideOK.transform.Rotate(new Vector3(0f, 0f, 180f));
                    var angel = slideOK.transform.rotation.eulerAngles.z * Mathf.Deg2Rad;
                    slideOK.transform.position += new Vector3(Mathf.Sin(angel) * 0.27f, Mathf.Cos(angel) * -0.27f);
                }
            }

            slideOK.SetActive(false);
            slideOK.transform.SetParent(transform.parent);
        }
        protected override void Check(object sender, InputEventArgs arg) => Check();
    }
}