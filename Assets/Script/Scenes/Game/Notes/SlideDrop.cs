using MajdataPlay.Collections;
using MajdataPlay.Extensions;
using MajdataPlay.Game.Controllers;
using MajdataPlay.Game.Types;
using MajdataPlay.Game.Utils;
using MajdataPlay.IO;
using MajdataPlay.Types;
using MajdataPlay.Attributes;
using MajdataPlay.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.VisualScripting;
using System.Diagnostics;
#nullable enable
namespace MajdataPlay.Game.Notes
{
    public sealed class SlideDrop : SlideBase,IConnectableSlide, IEndableNote
    {
        public bool IsMirror 
        { 
            get => _isMirror; 
            set => _isMirror = value; 
        }
        public bool IsSpecialFlip 
        { 
            get => _isSpecialFlip; 
            set => _isSpecialFlip = value; 
        } // fixes known star problem

        List<Vector3> _slidePositions = new();
        List<Quaternion> _slideRotations = new();

        SpriteRenderer _starRenderer;
        SlideTable _table;

        protected override void Awake()
        {
            base.Awake();
            var star = Instantiate(_slideStarPrefab, _noteManager.transform.GetChild(3));
            var slideTable = SlideTables.FindTableByName(_slideType);
            
            if (slideTable is null)
                throw new MissingComponentException($"Slide table of \"{_slideType}\" is not found");

            _table = slideTable;
            _judgeQueues[0] = _table.JudgeQueue;
            _endPos = _slideType switch
            {
                "line3" => 3,
                "line4" => 4,
                "line5" => 5,
                "line6" => 6,
                "line7" => 7,
                "circle1" => 2,
                "circle2" => 3,
                "circle3" => 4,
                "circle4" => 5,
                "circle5" => 6,
                "circle6" => 7,
                "circle7" => 8,
                "circle8" => 1,
                "v1" => 1,
                "v2" => 2,
                "v3" => 3,
                "v4" => 4,
                "v6" => 6,
                "v7" => 7,
                "v8" => 8,
                "ppqq1" => 1,
                "ppqq2" => 2,
                "ppqq3" => 3,
                "ppqq4" => 4,
                "ppqq5" => 5,
                "ppqq6" => 6,
                "ppqq7" => 7,
                "ppqq8" => 8,
                "pq1" => 1,
                "pq2" => 2,
                "pq3" => 3,
                "pq4" => 4,
                "pq5" => 5,
                "pq6" => 6,
                "pq7" => 7,
                "pq8" => 8,
                "s" => 5,
                "L2" => 2,
                "L3" => 3,
                "L4" => 4,
                "L5" => 5,
                _ => 1
            };

            star.SetActive(true);
            _stars[0] = star;
            _starTransforms[0] = star.transform;
            _starRenderer = star.GetComponent<SpriteRenderer>();

            _slideOK = transform.GetChild(transform.childCount - 1).gameObject; //slideok is the last one
            _slideOKAnim = _slideOK.GetComponent<Animator>();
            _slideOKController = _slideOK.GetComponent<LoadJustSprite>();
            _slideOK.SetActive(false);
            
            _slideBars = new GameObject[transform.childCount - 1];
            _slideBarTransforms = new Transform[transform.childCount - 1];
            _slideBarRenderers = new SpriteRenderer[transform.childCount - 1];

            for (var i = 0; i < Transform.childCount - 1; i++)
            {
                _slideBars[i] = Transform.GetChild(i).gameObject;
                _slideBarRenderers[i] = _slideBars[i].GetComponent<SpriteRenderer>();
                _slideBarTransforms[i] = _slideBars[i].transform;
            }
            LoadSlidePath();
            SetActive(false);
            SetStarActive(false);
            SetSlideBarAlpha(0f);
            for (int i = 0; i < _slidePositions.Count - 2; i++)
            {
                var a = _slidePositions[i];
                var b = _slidePositions[i + 1];
                _slideLength += (b - a).magnitude;
            }

            _starTransforms[0].position = _slidePositions[0];
            _starTransforms[0].transform.localScale = new Vector3(0f, 0f, 1f);
        }
        /// <summary>
        /// Slide初始化
        /// </summary>
        public override void Initialize()
        {
            if (IsInitialized)
                return;

            if (_isMirror)
            {
                _table.Mirror();
                Transform.localScale = new Vector3(-1f, 1f, 1f);
                Transform.rotation = Quaternion.Euler(0f, 0f, -45f * StartPos);
                _slideOK.transform.localScale = new Vector3(-1f, 1f, 1f);
            }
            else
            {
                Transform.rotation = Quaternion.Euler(0f, 0f, -45f * (StartPos - 1));
            }

            var diff = Math.Abs(1 - StartPos);
            if(diff != 0)
            {
                _table.Diff(diff);
            }

            LoadSlidePath();
            LoadSkin();
            _slideOK.transform.SetParent(transform.parent);
            // 计算Slide淡入时机
            // 在8.0速时应当提前300ms显示Slide
            _fadeInTiming = -3.926913f / Speed;
            _fadeInTiming += _gameSetting.Game.SlideFadeInOffset;
            _fadeInTiming += _startTiming;
            // Slide完全淡入时机
            // 正常情况下应为负值；速度过高将忽略淡入
            _fullFadeInTiming = _fadeInTiming + 0.2f;
            //var interval = fullFadeInTiming - fadeInTiming;
            //fadeInAnimator = GetComponent<Animator>();
            //Destroy(GetComponent<Animator>());
            //淡入时机与正解帧间隔小于200ms时，加快淡入动画的播放速度
            //fadeInAnimator.speed = 0.2f / interval;
            //fadeInAnimator.SetTrigger("slide");
            
            _starTransforms[0].position = _slidePositions[0];
            _starTransforms[0].transform.localScale = new Vector3(0f, 0f, 1f);
            _judgeQueues[0] = _table.JudgeQueue;

            InitializeSlideGroup();

            State = NoteStatus.Initialized;
        }
        void InitializeSlideGroup()
        {
            var judgeQueue = _judgeQueues[0].Span;

            if (ConnectInfo.IsConnSlide && ConnectInfo.IsGroupPartEnd)
            {
                judgeQueue[judgeQueue.Length - 1].SetIsLast();
            }
            else if (ConnectInfo.IsConnSlide)
            {
                judgeQueue[judgeQueue.Length - 1].SetNonLast();
            }

            if (ConnectInfo.IsConnSlide)
            {
                Length = ConnectInfo.TotalLength / ConnectInfo.TotalSlideLen * _slideLength;
                if (!ConnectInfo.IsGroupPartHead)
                {
                    if (Parent is null)
                        throw new NullReferenceException();
                    var parent = Parent.GameObject.GetComponent<SlideDrop>();
                    Timing = parent.Timing + parent.Length;
                }
                UpdateJudgeQueue();
            }

            if (ConnectInfo.IsGroupPartEnd || !ConnectInfo.IsConnSlide)
            {
                var percent = _table.Const;
                _judgeTiming = Timing + Length * (1 - percent);
                _lastWaitTime = Length * percent;
            }
        }
        void UpdateJudgeQueue()
        {
            var judgeQueue = _judgeQueues[0].Span;
            if (ConnectInfo.TotalJudgeQueueLen < 4)
            {
                if (ConnectInfo.IsGroupPartHead)
                {
                    judgeQueue[0].IsSkippable = true;
                    judgeQueue[1].IsSkippable = false;
                }
                else if (ConnectInfo.IsGroupPartEnd)
                {
                    judgeQueue[0].IsSkippable = false;
                    judgeQueue[1].IsSkippable = true;
                }
            }
            else
            {
                foreach (var judgeArea in judgeQueue)
                    judgeArea.IsSkippable = true;
            }
        }
        void Start()
        {
            FadeIn().Forget();
        }
        public override void ComponentFixedUpdate()
        {
            
        }
        public override void ComponentUpdate()
        {
            SlideCheck();
            CheckSensor();
            // ConnSlide
            //var star = _stars[0];
            var starTransform = _starTransforms[0];
            
            switch(State)
            {
                case NoteStatus.Initialized:
                    SetStarActive(false);
                    if (ThisFrameSec - StartTiming > 0)
                    {
                        if (!(ConnectInfo.IsConnSlide && !ConnectInfo.IsGroupPartHead))
                        {
                            SetStarActive(true);
                        }

                        _starRenderer.color = new Color(1, 1, 1, 0);
                        starTransform.localScale = new Vector3(0, 0, 1);
                        starTransform.position = _slidePositions[0];
                        ApplyStarRotation(_slideRotations[0]);
                        State = NoteStatus.Scaling;
                        goto case NoteStatus.Scaling;
                    }
                    break;
                case NoteStatus.Scaling:
                    var timing = ThisFrameSec - Timing;
                    if (timing > 0f)
                    {
                        _starRenderer.color = new Color(1, 1, 1, 1);
                        starTransform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
                        SetStarActive(true);

                        State = NoteStatus.Running;
                        goto case NoteStatus.Running;
                    }
                    if (ConnectInfo.IsConnSlide && !ConnectInfo.IsGroupPartHead)
                    {
                        return;
                    }
                    // 只有当它是一个起点Slide（而非Slide Group中的子部分）的时候，才会有开始的星星渐入动画
                    var alpha = (1f - -timing / (_timing - _startTiming)).Clamp(0, 1);

                    _starRenderer.color = new Color(1, 1, 1, alpha);
                    starTransform.localScale = new Vector3(alpha + 0.5f, alpha + 0.5f, alpha + 0.5f);

                    break;
                case NoteStatus.Running:
                    if(GetRemainingTimeWithoutOffset() == 0)
                    {
                        starTransform.position = _slidePositions[_slidePositions.Count - 1];
                        ApplyStarRotation(_slideRotations[_slideRotations.Count - 1]);
                        if (ConnectInfo.IsConnSlide && !ConnectInfo.IsGroupPartEnd)
                        {
                            DestroyStars();
                        }
                        State = NoteStatus.Arrived;
                        goto case NoteStatus.Arrived;
                    }
                    var process = (Length - GetRemainingTimeWithoutOffset() / Length).Clamp(0, 1);
                    var indexProcess = (_slidePositions.Count - 1) * process;
                    var index = (int)indexProcess;
                    var pos = indexProcess - index;

                    var a = _slidePositions[index + 1];
                    var b = _slidePositions[index];
                    var ba = a - b;
                    var newPos = ba * pos + b;

                    starTransform.position = newPos;
                    if (index < _slideRotations.Count - 1)
                    {
                        var _a = _slideRotations[index + 1].eulerAngles.z;
                        var _b = _slideRotations[index].eulerAngles.z;
                        var dAngle = Mathf.DeltaAngle(_b, _a) * pos;
                        dAngle = Mathf.Abs(dAngle);
                        var newRotation = Quaternion.Euler(0f, 0f,
                                        Mathf.MoveTowardsAngle(_b, _a, dAngle));
                        ApplyStarRotation(newRotation);
                    }
                    Autoplay();
                    break;
                case NoteStatus.Arrived:
                    break;
            }
        }        
        /// <summary>
        /// 判定队列检查
        /// </summary>
        void CheckSensor()
        {
            if (IsAutoplay || !_isCheckable)
                return;
            else if (IsEnded || !IsInitialized)
                return;
            else if (IsFinished)
                return;
            else if (_isChecking)
                return;

            _isChecking = true;
            try
            {
                ref var queueMemory = ref _judgeQueues[0];
                var queue = queueMemory.Span;
                var first = queue[0];
                var fAreas = first.IncludedAreas;
                var canPlaySFX = ConnectInfo.IsGroupPartHead || !ConnectInfo.IsConnSlide;
                SlideArea? second = null;

                if (queue.Length >= 2)
                    second = queue[1];

                foreach (var area in fAreas)
                {
                    var sensorState = _noteManager.CheckSensorStateInThisFrame(area, SensorStatus.On) ? SensorStatus.On : SensorStatus.Off;
                    first.Check(area, sensorState);
                }

                if (canPlaySFX && first.On)
                    PlaySFX();

                if (second is not null && (first.IsSkippable || first.On))
                {
                    var sAreas = second.IncludedAreas;
                    foreach (var area in sAreas)
                    {
                        var sensorState = _noteManager.CheckSensorStateInThisFrame(area, SensorStatus.On) ? SensorStatus.On : SensorStatus.Off;
                        second.Check(area, sensorState);
                    }

                    if (second.IsFinished)
                    {
                        HideBar(first.SlideIndex);
                        queueMemory = queueMemory.Slice(2);
                        SetParentFinish();
                        return;
                    }
                    else if (second.On)
                    {
                        HideBar(first.SlideIndex);
                        queueMemory = queueMemory.Slice(1);
                        SetParentFinish();
                        return;
                    }
                }

                if (first.IsFinished)
                {
                    HideBar(first.SlideIndex);
                    queueMemory = queueMemory.Slice(1);
                    SetParentFinish();
                    return;
                }
            }
            finally
            {
                _isChecking = false;
            }
        }
        void SlideCheck()
        {
            /// time      是Slide启动的时间点
            /// timeStart 是Slide完全显示但未启动
            /// LastFor   是Slide的时值
            //var timing = _gpManager.AudioTime - _timing;
            var startTiming = _gpManager.AudioTime - _startTiming;
            var tooLateTiming = _timing + _length + 0.6 + MathF.Min(_gameSetting.Judge.JudgeOffset, 0);
            var isTooLate = _gpManager.AudioTime - tooLateTiming >= 0;

            if (!_isCheckable)
            {
                if (ConnectInfo.IsGroupPart)
                {
                    if (ConnectInfo.IsGroupPartHead && startTiming >= -0.05f)
                        _isCheckable = true;
                    else if (!ConnectInfo.IsGroupPartHead)
                        _isCheckable = ConnectInfo.ParentFinished || ConnectInfo.ParentPendingFinish;
                }
                else if (startTiming >= -0.05f)
                    _isCheckable = true;
            }

            var isJudgable = ConnectInfo.IsGroupPartEnd || !ConnectInfo.IsConnSlide;

            if (isJudgable)
            {
                if (!_isJudged)
                {
                    if (IsFinished)
                    {
                        HideAllBar();
                        if (IsClassic)
                            Judge_Classic(ThisFrameSec);
                        else
                            Judge(ThisFrameSec);
                        return;
                    }
                    else if (isTooLate)
                        TooLateJudge();
                }
                else
                {
                    if (_lastWaitTime < 0)
                        End();
                    else
                        _lastWaitTime -= Time.deltaTime;
                }
            }
        }
        void SetParentFinish()
        {
            if (Parent is not null)
            {
                if(_judgeQueues[0].Length < _table.JudgeQueue.Length && !ConnectInfo.ParentFinished)
                    Parent.ForceFinish();
            }
        }
        protected override void TooLateJudge()
        {
            if (_isJudged)
            {
                End();
                return;
            }
            base.TooLateJudge();
            End();
        }
        public override void End(bool forceEnd = false)
        {
            if (IsEnded)
                return;
            State = NoteStatus.End;
            //foreach (var sensor in ArrayHelper.ToEnumerable(_judgeAreas))
            //    _ioManager.UnbindSensor(_noteChecker, sensor);
            base.End();
            if (forceEnd)
            {
                Destroy(_slideOK);
                Destroy(gameObject);
                return;
            }
            

            if (ConnectInfo.IsGroupPartEnd || !ConnectInfo.IsConnSlide)
            {
                ConvertJudgeGrade(ref _judgeResult);
                JudgeResultCorrection(ref _judgeResult);
                var result = new JudgeResult()
                {
                    Grade = _judgeResult,
                    Diff = _judgeDiff,
                    IsEX = IsEX,
                    IsBreak = IsBreak
                };
                // 只有组内最后一个Slide完成 才会显示判定条并增加总数
                _objectCounter.ReportResult(this, result);
                if(PlaySlideOK(result))
                {
                    if (IsClassic)
                    {
                        _slideOKAnim.SetTrigger("classic");
                    }
                    else if (IsBreak && _judgeResult == JudgeGrade.Perfect)
                    {
                        _slideOKAnim.runtimeAnimatorController = MajInstances.SkinManager.JustBreak;
                    }
                    _slideOKController.SetResult(_judgeResult);
                }
                
                PlayJudgeSFX(result);
                //PlaySlideOK(result);
            }
            //else
            //    Destroy(_slideOK);
            // Destroy(gameObject);
            //SetActive(false);
        }
        protected override void Autoplay()
        {
            if (!IsAutoplay)
                return;
            var process = MathF.Min((Length - GetRemainingTimeWithoutOffset()) / Length, 1);
            var queueMemory = _judgeQueues[0];
            var queue = queueMemory.Span;
            var canPlaySFX = ConnectInfo.IsGroupPartHead || !ConnectInfo.IsConnSlide;
            if (queueMemory.IsEmpty)
                return;
            else if (process >= 1)
            {
                HideAllBar();
                var autoplayParam = _gpManager.AutoplayParam;
                if (autoplayParam.InRange(0, 14))
                    _judgeResult = (JudgeGrade)autoplayParam;
                else
                    _judgeResult = (JudgeGrade)_randomizer.Next(0, 15);
                _isJudged = true;
                _lastWaitTime = 0;
                _judgeDiff = _judgeResult switch
                {
                    < JudgeGrade.Perfect => 1,
                    > JudgeGrade.Perfect => -1,
                    _ => 0
                };
                return;
            }
            else if (process > 0 && canPlaySFX)
            {
                PlaySFX();
            }
            var areaIndex = (int)(process * queueMemory.Length) - 1;
            if (areaIndex < 0)
                return;
            var barIndex = queue[areaIndex].SlideIndex;
            HideBar(barIndex);
        }
        void ApplyStarRotation(Quaternion newRotation)
        {
            var star = _stars[0];
            var starTransform = _starTransforms[0];
            if (star is null)
                return;
            var halfFlip = newRotation.eulerAngles;

            halfFlip.z += 180f;
            if (_isSpecialFlip)
                starTransform.rotation = Quaternion.Euler(halfFlip);
            else
                starTransform.rotation = newRotation;
            //starTransform.rotation = newRotation;
        }
        void LoadSlidePath()
        {
            _slidePositions = new();
            _slideRotations = new();

            _slidePositions.Add(GetPositionFromDistance(4.8f));
            for (int i = 0; i < _slideBars.Length; i++)
            {
                var bar = _slideBars[i];
                _slidePositions.Add(bar.transform.position);

                _slideRotations.Add(Quaternion.Euler(bar.transform.rotation.normalized.eulerAngles + new Vector3(0f, 0f, 18f)));
                if(i == _slideBars.Length - 1)
                {
                    var a = _slideBars[i - 1].transform.rotation.normalized.eulerAngles;
                    var b = bar.transform.rotation.normalized.eulerAngles;
                    var diff = a - b;
                    var newEulerAugle = b - diff;
                    _slideRotations.Add(Quaternion.Euler(newEulerAugle + new Vector3(0f, 0f, 18f)));
                }
            }
            var endPos = GetPositionFromDistance(4.8f, _endPos);
            _slidePositions.Add(endPos);
        }
        protected override void LoadSkin()
        {
            var bars = _slideBars;
            var skin = MajInstances.SkinManager.GetSlideSkin();
            var star = _stars[0]!;
            var barSprite = skin.Normal;
            var starSprite = skin.Star.Normal;
            Material? breakMaterial = null;

            if(IsEach)
            {
                barSprite = skin.Each;
                starSprite = skin.Star.Each;
            }
            if(IsBreak)
            {
                barSprite = skin.Break;
                starSprite = skin.Star.Break;
                breakMaterial = BreakMaterial;
            }

            foreach(var bar in bars)
            {
                var barRenderer = bar.GetComponent<SpriteRenderer>();
                
                barRenderer.color = new Color(1f, 1f, 1f, 0f);
                barRenderer.sortingOrder = _sortOrder--;
                barRenderer.sortingLayerName = "Slides";

                barRenderer.sprite = barSprite;
                

                if(breakMaterial is not null)
                {
                    barRenderer.sharedMaterial = breakMaterial;
                    //var controller = bar.AddComponent<BreakShineController>();
                    //controller.Parent = this;
                }
            }

            var starRenderer = star.GetComponent<SpriteRenderer>();
            starRenderer.sprite = starSprite;
            if (breakMaterial is not null)
            {
                starRenderer.sharedMaterial = breakMaterial;
            }

            if (_isJustR)
            {
                if (_slideOKController.SetR() == 1 && _isMirror)
                {
                    _slideOK.transform.Rotate(new Vector3(0f, 0f, 180f));
                    var angel = _slideOK.transform.rotation.eulerAngles.z * Mathf.Deg2Rad;
                    _slideOK.transform.position += new Vector3(Mathf.Sin(angel) * 0.27f, Mathf.Cos(angel) * -0.27f);
                }
            }
            else
            {
                if (_slideOKController.SetL() == 1 && !_isMirror)
                {
                    _slideOK.transform.Rotate(new Vector3(0f, 0f, 180f));
                    var angel = _slideOK.transform.rotation.eulerAngles.z * Mathf.Deg2Rad;
                    _slideOK.transform.position += new Vector3(Mathf.Sin(angel) * 0.27f, Mathf.Cos(angel) * -0.27f);
                }
            }
        }

        [ReadOnlyField]
        [SerializeField]
        bool _isMirror = false;
        [ReadOnlyField]
        [SerializeField]
        bool _isSpecialFlip = false;
    }
}