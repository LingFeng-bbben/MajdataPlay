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
    public class SlideDrop : NoteLongDrop, IFlasher
    {
        // Start is called before the first frame update
        public GameObject star_slide;

        public Sprite spriteNormal;
        public Sprite spriteEach;
        public Sprite spriteBreak;
        public RuntimeAnimatorController slideShine;
        public RuntimeAnimatorController judgeBreakShine;

        public bool isMirror;
        public bool isJustR;
        public bool isSpecialFlip; // fixes known star problem
        public bool isBreak;

        public float timeStart;

        public int sortIndex;

        public float fadeInTime;

        public float fullFadeInTime;

        public float slideConst;
        float arriveTime = -1;

        public List<int> areaStep = new List<int>();
        public bool smoothSlideAnime = false;

        public Material breakMaterial;
        public string slideType;

        

        


        Animator fadeInAnimator;


        private readonly List<GameObject> slideBars = new();

        private readonly List<Vector3> slidePositions = new();
        private readonly List<Quaternion> slideRotations = new();
        private GameObject slideOK;

        private SpriteRenderer spriteRenderer_star;

        public int endPosition;



        
        public ConnSlideInfo ConnectInfo { get; set; } = new();
        public bool isFinished { get => judgeQueue.Length == 0; }
        public bool isPendingFinish { get => judgeQueue.Length == 1; }
        public bool CanShine { get; private set; } = false;

        bool canCheck = false;
        bool isChecking = false;
        float judgeTiming; // 正解帧
        bool isInitialized = false; //防止重复初始化
        bool isDestroying = false; // 防止重复销毁
        bool isSoundPlayed = false;
        float lastWaitTime;
        SlideTable table;
        JudgeArea[] judgeQueue = { }; // 判定队列
        IEnumerable<SensorType> judgeAreas;
        /// <summary>
        /// Slide初始化
        /// </summary>
        public void Initialize()
        {
            if (isInitialized)
                return;
            isInitialized = true;
            var slideTable = SlideTables.FindTableByName(slideType);
            if (slideTable is null)
                throw new MissingComponentException($"Slide table of \"{slideType}\" is not found");
            table = slideTable;
            slideOK = transform.GetChild(transform.childCount - 1).gameObject; //slideok is the last one        
            objectCounter = GameObject.Find("ObjectCounter").GetComponent<ObjectCounter>();

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
                table.SetDiff(diff);

            if (isJustR)
            {
                if (slideOK.GetComponent<LoadJustSprite>().setR() == 1 && isMirror)
                {
                    slideOK.transform.Rotate(new Vector3(0f, 0f, 180f));
                    var angel = slideOK.transform.rotation.eulerAngles.z * Mathf.Deg2Rad;
                    slideOK.transform.position += new Vector3(Mathf.Sin(angel) * 0.27f, Mathf.Cos(angel) * -0.27f);
                }
            }
            else
            {
                if (slideOK.GetComponent<LoadJustSprite>().setL() == 1 && !isMirror)
                {
                    slideOK.transform.Rotate(new Vector3(0f, 0f, 180f));
                    var angel = slideOK.transform.rotation.eulerAngles.z * Mathf.Deg2Rad;
                    slideOK.transform.position += new Vector3(Mathf.Sin(angel) * 0.27f, Mathf.Cos(angel) * -0.27f);
                }
            }
            spriteRenderer_star = star_slide.GetComponent<SpriteRenderer>();

            if (isBreak)
            {
                spriteRenderer_star.material = breakMaterial;
                spriteRenderer_star.material.SetFloat("_Brightness", 0.95f);
                var controller = star_slide.AddComponent<BreakShineController>();
                controller.enabled = true;
                controller.parent = this;
            }

            for (var i = 0; i < transform.childCount - 1; i++) 
                slideBars.Add(transform.GetChild(i).gameObject);

            slideOK.SetActive(false);
            slideOK.transform.SetParent(transform.parent);
            slidePositions.Add(getPositionFromDistance(4.8f));
            foreach (var bars in slideBars)
            {
                slidePositions.Add(bars.transform.position);
                slideRotations.Add(Quaternion.Euler(bars.transform.rotation.eulerAngles + new Vector3(0f, 0f, 18f)));
            }
            var endPos = getPositionFromDistance(4.8f, endPosition);
            var x = slidePositions.LastOrDefault() - Vector3.zero;
            var y = endPos - Vector3.zero;
            var angle = Mathf.Acos(Vector3.Dot(x, y) / (x.magnitude * y.magnitude)) * Mathf.Rad2Deg;
            var offset = slideRotations.TakeLast(1).First().eulerAngles - slideRotations.TakeLast(2).First().eulerAngles;
            if (offset.z < 0)
                angle = -angle;

            var q = slideRotations.LastOrDefault() * Quaternion.Euler(0, 0, angle);
            slidePositions.Add(endPos);
            slideRotations.Add(q);
            foreach (var gm in slideBars)
            {
                var sr = gm.GetComponent<SpriteRenderer>();
                sr.color = new Color(1f, 1f, 1f, 0f);
                sr.sortingOrder = sortIndex--;
                sr.sortingLayerName = "Slides";
                if (isBreak)
                {
                    sr.sprite = spriteBreak;
                    sr.material = breakMaterial;
                    sr.material.SetFloat("_Brightness", 0.95f);
                    var controller = gm.AddComponent<BreakShineController>();
                    controller.parent = this;
                    controller.enabled = true;
                    //anim.runtimeAnimatorController = slideShine;
                    //anim.enabled = false;
                    //animators.Add(anim);
                }
                else if (isEach)
                {
                    sr.sprite = spriteEach;
                }
                else
                {
                    sr.sprite = spriteNormal;
                }
            }

            //timeProvider = GameObject.Find("AudioTimeProvider").GetComponent<AudioTimeProvider>();
            // 计算Slide淡入时机
            // 在8.0速时应当提前300ms显示Slide
            fadeInTime = -3.926913f / speed;
            // Slide完全淡入时机
            // 正常情况下应为负值；速度过高将忽略淡入
            fullFadeInTime = Math.Min(fadeInTime + 0.2f, 0);
            var interval = fullFadeInTime - fadeInTime;
            fadeInAnimator = GetComponent<Animator>();
            //淡入时机与正解帧间隔小于200ms时，加快淡入动画的播放速度; interval永不为0
            fadeInAnimator.speed = 0.2f / interval;
            fadeInAnimator.SetTrigger("slide");

            judgeQueue = table.JudgeQueue;

            if (ConnectInfo.IsConnSlide && ConnectInfo.IsGroupPartEnd)
                judgeQueue.LastOrDefault().SetIsLast();
            else if (ConnectInfo.IsConnSlide)
                judgeQueue.LastOrDefault().SetNonLast();


        }
        /// <summary>
        /// Connection Slide
        /// <para>强制完成该Slide</para>
        /// </summary>
        public void ForceFinish()
        {
            if (!ConnectInfo.IsConnSlide || ConnectInfo.IsGroupPartEnd)
                return;
            HideBar(areaStep.LastOrDefault());
            judgeQueue = Array.Empty<JudgeArea>();
        }
        private void Start()
        {
            Initialize();
            if (ConnectInfo.IsConnSlide)
            {
                LastFor = ConnectInfo.TotalLength / ConnectInfo.TotalSlideLen * GetSlideLength();
                if (!ConnectInfo.IsGroupPartHead)
                {
                    var parent = ConnectInfo.Parent!.GetComponent<SlideDrop>();
                    time = parent.time + parent.LastFor;
                }
            }

            if(ConnectInfo.IsGroupPartEnd || !ConnectInfo.IsConnSlide)
            {
                var percent = table.Const;
                judgeTiming = time + LastFor * percent;
                lastWaitTime = LastFor * (1 - percent);
            }

            judgeAreas = table.JudgeQueue.SelectMany(x => x.GetSensorTypes())
                                         .GroupBy(x => x)
                                         .Select(x => x.Key)
                                         .ToArray();

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
            var forceJudgeTiming = time + LastFor + 0.6;

            if(!canCheck)
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
                    if (isFinished)
                    {
                        HideBar(areaStep.LastOrDefault());
                        Judge();
                        return;
                    }
                    else if(gpManager.AudioTime - forceJudgeTiming >= 0)
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
        // Update is called once per frame
        private void Update()
        {
            if (star_slide == null)
            {
                if (isFinished)
                    DestroySelf();
                return;
            }
            // Slide淡入期间，不透明度从0到0.55耗时200ms
            var startiming = gpManager.AudioTime - timeStart;
            if (startiming <= 0f)
            {
                if (startiming >= -0.05f)
                {
                    fadeInAnimator.enabled = false;
                    SetSlideBarAlpha(1f);
                }
                else if (!fadeInAnimator.enabled && startiming >= fadeInTime)
                    fadeInAnimator.enabled = true;
                return;

            }
            fadeInAnimator.enabled = false;
            SetSlideBarAlpha(1f);

            star_slide.SetActive(true);
            var timing = gpManager.AudioTime - time;
            if (timing <= 0f)
            {
                CanShine = true;
                float alpha;
                if (ConnectInfo.IsConnSlide && !ConnectInfo.IsGroupPartHead)
                    alpha = 0;
                else
                {
                    // 只有当它是一个起点Slide（而非Slide Group中的子部分）的时候，才会有开始的星星渐入动画
                    alpha = 1f - -timing / (time - timeStart);
                    alpha = alpha > 1f ? 1f : alpha;
                    alpha = alpha < 0f ? 0f : alpha;
                }

                spriteRenderer_star.color = new Color(1, 1, 1, alpha);
                star_slide.transform.localScale = new Vector3(alpha + 0.5f, alpha + 0.5f, alpha + 0.5f);
                star_slide.transform.position = slidePositions[0];
                ApplyStarRotation(slideRotations[0]);
            }
            else
                UpdateStar();
            Check();
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
        public void Check(object sender, InputEventArgs arg) => Check();
        /// <summary>
        /// 判定队列检查
        /// </summary>
        public void Check()
        {
            if (isFinished || !canCheck)
                return;
            else if (isChecking)
                return;
            isChecking = true;
            if (ConnectInfo.Parent != null && judgeQueue.Length < table.JudgeQueue.Length)
            {
                if (!ConnectInfo.ParentFinished)
                    ConnectInfo.Parent.GetComponent<SlideDrop>().ForceFinish();
            }

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
                    judgeQueue = judgeQueue.Skip(2).ToArray();
                    isChecking = false;
                    return;
                }
                else if (second.On)
                {
                    HideBar(first.SlideIndex);
                    judgeQueue = judgeQueue.Skip(1).ToArray();
                    isChecking = false;
                    return;
                }
            }

            if (first.IsFinished)
            {
                HideBar(first.SlideIndex);
                judgeQueue = judgeQueue.Skip(1).ToArray();
                isChecking = false;
                return;
            }
            isChecking = false;
        }
        void HideBar(int endIndex)
        {
            endIndex = endIndex - 1;
            endIndex = Math.Min(endIndex, slideBars.Count - 1);
            for (int i = 0; i <= endIndex; i++)
                slideBars[i].SetActive(false);
        }
        
        /// <summary>
        /// Slide判定
        /// </summary>
        void Judge()
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
        
        /// <summary>
        /// 强制将Slide判定为TooLate并销毁
        /// </summary>
        void TooLateJudge()
        {
            if(isJudged)
            {
                DestroySelf();
                return;
            }

            if (judgeQueue.Length == 1)
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
        void DestroySelf(bool onlyStar = false)
        {

            if (onlyStar)
                Destroy(star_slide);
            else
            {
                if (ConnectInfo.Parent != null)
                    Destroy(ConnectInfo.Parent);

                foreach (GameObject obj in slideBars)
                    obj.SetActive(false);

                if (star_slide != null)
                    Destroy(star_slide);
                Destroy(gameObject);
            }
        }
        void OnDestroy()
        {
            if (isDestroying)
                return;
            if (ConnectInfo.Parent != null)
                Destroy(ConnectInfo.Parent);
            if (star_slide != null)
                Destroy(star_slide);
            foreach (var sensor in judgeAreas)
                ioManager.UnbindSensor(Check, sensor);
            if (ConnectInfo.IsGroupPartEnd || !ConnectInfo.IsConnSlide)
            {
                // 只有组内最后一个Slide完成 才会显示判定条并增加总数
                objectCounter.ReportResult(this, judgeResult, isBreak);
                
                if (isBreak && judgeResult == JudgeType.Perfect) { 
                    slideOK.GetComponent<Animator>().runtimeAnimatorController = judgeBreakShine;
                    var audioEffMana = GameObject.Find("NoteAudioManager").GetComponent<NoteAudioManager>();
                    audioEffMana.PlayBreakSlideEndSound();
                }
                slideOK.GetComponent<LoadJustSprite>().SetResult(judgeResult);
                slideOK.SetActive(true);
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
            spriteRenderer_star.color = Color.white;
            star_slide.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);

            var process = MathF.Min((LastFor - GetRemainingTime()) / LastFor, 1);
            var indexProcess = (slidePositions.Count - 1) * process;
            var index = (int)indexProcess;
            var pos = indexProcess - index;

            if (process == 1)
            {
                star_slide.transform.position = slidePositions.LastOrDefault();
                ApplyStarRotation(slideRotations.LastOrDefault());
                if (ConnectInfo.IsConnSlide && !ConnectInfo.IsGroupPartEnd)
                    DestroySelf(true);
                else if (isFinished && isJudged)
                    DestroySelf();
            }
            else
            {
                var a = slidePositions[index + 1];
                var b = slidePositions[index];
                var ba = a - b;
                var newPos = ba * pos + b;

                star_slide.transform.position = newPos;
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

        private void SetSlideBarAlpha(float alpha)
        {
            foreach (var gm in slideBars) gm.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, alpha);
        }
        private void ApplyStarRotation(Quaternion newRotation)
        {
            var halfFlip = newRotation.eulerAngles;
            halfFlip.z += 180f;
            if (isSpecialFlip)
                star_slide.transform.rotation = Quaternion.Euler(halfFlip);
            else
                star_slide.transform.rotation = newRotation;
        }
    }
}