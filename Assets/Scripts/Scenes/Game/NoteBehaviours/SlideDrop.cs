using MajdataPlay.Extensions;
using MajdataPlay.Scenes.Game.Utils;
using MajdataPlay.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;
using MajdataPlay.Editor;
using MajdataPlay.Scenes.Game.Notes.Slide;
using MajdataPlay.Scenes.Game.Notes.Slide.Utils;
using MajdataPlay.IO;
using MajdataPlay.Numerics;
using MajdataPlay.Buffers;
using MajdataPlay.Settings;
using System.Runtime.CompilerServices;
using Unity.IL2CPP.CompilerServices;

#nullable enable
namespace MajdataPlay.Scenes.Game.Notes.Behaviours
{
    using Unsafe = System.Runtime.CompilerServices.Unsafe;
    internal sealed class SlideDrop : SlideBase, IConnectableSlide, IMajComponent
    {
        public bool IsMirror
        {
            get => _isMirror;
            set => _isMirror = value;
        }

        public Quaternion FinalStarAngle { get; private set; } = default;

        RentedList<Vector3> _starPositions = new();
        RentedList<Quaternion> _starRotations = new();

        SpriteRenderer _starRenderer;
        SlideTable _table;
        float _djAutoplayRatio = 1;

        int _parentForceFinishFlag = 0;

//#if UNITY_EDITOR
//        Transform _judgeFramePoint;
//        [SerializeField]
//        float _runtimeSlideConst;
//#endif
        protected override void Awake()
        {
            base.Awake();
            var star = Instantiate(_slideStarPrefab, _noteManager.transform.GetChild(3));
            var slideTable = SlideTables.FindTableByName(SlideType);

            if (slideTable is null)
            {
                throw new MissingComponentException($"Slide table of \"{SlideType}\" is not found");
            }

            _table = slideTable;
            _judgeQueues[0] = _table.JudgeQueue;
            EndPos = SlideType switch
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
            var stars = _stars.Span;
            var starTransforms = _starTransforms.Span;

            star.SetActive(true);
            stars[0] = star;
            starTransforms[0] = star.transform;
            _starRenderer = star.GetComponent<SpriteRenderer>();

            var slideOK = transform.GetChild(transform.childCount - 1).gameObject; //slideok is the last one
            slideOK.SetActive(true);
            _slideOK = slideOK.GetComponent<SlideOK>();
            _slideOK.IsClassic = IsClassic;
            _slideOK.Shape = NoteHelper.GetSlideOKShapeFromSlideType(SlideType);


            for (var i = 0; i < Transform.childCount - 1; i++)
            {
                _slideBars.Add(Transform.GetChild(i).gameObject);
                _slideBarRenderers.Add(_slideBars[i].GetComponent<SpriteRenderer>());
                _slideBarTransforms.Add(_slideBars[i].transform);
                _slideBarTransforms[i].localScale *= USERSETTING_SLIDE_SCALE;
            }
            LoadSlidePath();
            SetActive(false);
            SetStarActive(false);
            SetSlideBarAlpha(0f);
            SlideLength = _slideBars.Count + 1;

            starTransforms[0].position = _starPositions[0];
            starTransforms[0].transform.localScale = new Vector3(0f, 0f, 1f);
        }
        /// <summary>
        /// Slide初始化
        /// </summary>
        public override void Initialize()
        {
            if (IsInitialized)
            {
                return;
            }

            if (_isMirror)
            {
                _table.Mirror();
                Transform.localScale = new Vector3(-1f, 1f, 1f);
                Transform.rotation = Quaternion.Euler(0f, 0f, -45f * StartPos);
                _slideOK!.transform.localScale = new Vector3(-1f, 1f, 1f);
            }
            else
            {
                Transform.rotation = Quaternion.Euler(0f, 0f, -45f * (StartPos - 1));
            }

            var diff = Math.Abs(1 - StartPos);
            if (diff != 0)
            {
                _table.Diff(diff);
            }

            LoadSlidePath();
            LoadSkin();
            _slideOK!.transform.SetParent(Transform.parent);
            // 计算Slide淡入时机
            // 在8.0速时应当提前300ms显示Slide
            FadeInTiming = -3.926913f / Speed;
            var fadeInOffset = 0f;
            if (_settings.Debug.OffsetUnit == OffsetUnitOption.Second)
            {
                fadeInOffset = _settings.Game.SlideFadeInOffset;
            }
            else
            {
                fadeInOffset = _settings.Game.SlideFadeInOffset * MajEnv.FRAME_LENGTH_SEC;
            }
            FadeInTiming += fadeInOffset;
            FadeInTiming += Timing;
            // Slide完全淡入时机
            // 正常情况下应为负值；速度过高将忽略淡入
            FullFadeInTiming = FadeInTiming + 0.2f;
            //var interval = fullFadeInTiming - fadeInTiming;
            //fadeInAnimator = GetComponent<Animator>();
            //Destroy(GetComponent<Animator>());
            //淡入时机与正解帧间隔小于200ms时，加快淡入动画的播放速度
            //fadeInAnimator.speed = 0.2f / interval;
            //fadeInAnimator.SetTrigger("slide");
            var starTransforms = _starTransforms.Span;
            starTransforms[0].position = _starPositions[0];
            starTransforms[0].transform.localScale = new Vector3(0f, 0f, 1f);
            _judgeQueues[0] = _table.JudgeQueue;

            InitializeSlideGroup();

            if (ConnectInfo.IsConnSlide && !ConnectInfo.IsGroupPartEnd)
            {
                Destroy(_slideOK);
                _slideOK = null;
            }

            State = NoteStatus.Initialized;
            _djAutoplayRatio = SlideLength / 14;
//#if UNITY_EDITOR
//            var obj = Instantiate(_slideBars[0]);
//            Destroy(obj.GetComponent<SpriteRenderer>());
//            _judgeFramePoint = obj.transform;
//            _runtimeSlideConst = IsClassic ? _table.ClassicConst : _table.Const;
//            var indexProcess = (_starPositions.Count - 1) * (1 - _runtimeSlideConst);
//            var index = (int)indexProcess;
//            var pos = indexProcess - index;

//            var a = _starPositions[index + 1];
//            var b = _starPositions[index];
//            var ba = a - b;
//            var newPos = ba * pos + b;

//            _judgeFramePoint.position = newPos;
//#endif
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
                //Length = ConnectInfo.TotalLength / ConnectInfo.TotalSlideLen * SlideLength;
                if (!ConnectInfo.IsGroupPartHead)
                {
                    if (Parent is null)
                    {
                        throw new NullReferenceException();
                    }
                    var parent = Parent.GameObject.GetComponent<SlideDrop>();
                    StartTiming = parent.StartTiming + parent.Length;
                }
                UpdateJudgeQueue();
            }

            if (ConnectInfo.IsGroupPartEnd || !ConnectInfo.IsConnSlide)
            {
                var percent = _table.Const;
                _judgeTiming = StartTiming + Length * (1 - percent);
                _lastWaitTimeSec = Length * percent;
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
                foreach (ref var judgeArea in judgeQueue)
                {
                    judgeArea.IsSkippable = true;
                }
            }
        }
        [OnPreUpdate]
        void OnPreUpdate()
        {
            SlideBarFadeIn();
            SlideCheck();
        }
        [OnUpdate]
        void OnUpdate()
        {
//#if UNITY_EDITOR
//            {
//                var indexProcess = (_starPositions.Count - 1) * (1 - _runtimeSlideConst);
//                var index = (int)indexProcess;
//                var pos = indexProcess - index;

//                var a = _starPositions[index + 1];
//                var b = _starPositions[index];
//                var ba = a - b;
//                var newPos = ba * pos + b;
//                _judgeFramePoint.position = newPos;
//            }
//#endif
            // ConnSlide
            //var star = _stars[0];
            var starTransform = _starTransforms.Span[0];

            Autoplay();
            SensorCheck();

            switch (State)
            {
                case NoteStatus.Initialized:
                    SetStarActive(false);
                    if (ThisFrameSec - Timing > 0)
                    {
                        if (!(ConnectInfo.IsConnSlide && !ConnectInfo.IsGroupPartHead))
                        {
                            SetStarActive(true);
                        }

                        _starRenderer.color = new Color(1, 1, 1, 0);
                        starTransform.localScale = new Vector3(0, 0, 1);
                        starTransform.position = _starPositions[0];
                        ApplyStarRotation(_starRotations[0]);
                        State = NoteStatus.Scaling;
                        goto case NoteStatus.Scaling;
                    }
                    break;
                case NoteStatus.Scaling:
                    var timing = ThisFrameSec - StartTiming;
                    if (timing > 0f)
                    {
                        _starRenderer.color = new Color(1, 1, 1, 1);
                        if (!IsSlideNoHead)
                        {
                            starTransform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
                        }
                        SetStarActive(true);

                        State = NoteStatus.Running;
                        goto case NoteStatus.Running;
                    }
                    if (ConnectInfo.IsConnSlide && !ConnectInfo.IsGroupPartHead)
                    {
                        return;
                    }
                    else if (IsSlideNoHead)
                    {
                        return;
                    }
                    // 只有当它是一个起点Slide（而非Slide Group中的子部分）的时候，才会有开始的星星渐入动画
                    var alpha = (1f - -timing / (StartTiming - Timing)).Clamp(0, 1);

                    _starRenderer.color = new Color(1, 1, 1, alpha);
                    starTransform.localScale = new Vector3(alpha + 0.5f, alpha + 0.5f, alpha + 0.5f);

                    break;
                case NoteStatus.Running:
                    if (GetRemainingTimeWithoutOffset() == 0)
                    {
                        starTransform.position = _starPositions[_starPositions.Count - 1];
                        ApplyStarRotation(_starRotations[_starRotations.Count - 1]);
                        if (ConnectInfo.IsConnSlide && !ConnectInfo.IsGroupPartEnd)
                        {
                            DestroyStars();
                        }
                        State = NoteStatus.Arrived;
                        goto case NoteStatus.Arrived;
                    }
                    var process = ((Length - GetRemainingTimeWithoutOffset()) / Length).Clamp(0, 1);
                    var indexProcess = (_starPositions.Count - 1) * process;
                    var index = (int)indexProcess;
                    var pos = indexProcess - index;

                    var a = _starPositions[index + 1];
                    var b = _starPositions[index];
                    var ba = a - b;
                    var newPos = ba * pos + b;

                    starTransform.position = newPos;
                    if (index < _starRotations.Count - 1)
                    {
                        var _a = _starRotations[index + 1].eulerAngles.z;
                        var _b = _starRotations[index].eulerAngles.z;
                        var dAngle = Mathf.DeltaAngle(_b, _a) * pos;
                        dAngle = Mathf.Abs(dAngle);
                        var newRotation = Quaternion.Euler(0f, 0f,
                                        Mathf.MoveTowardsAngle(_b, _a, dAngle));
                        ApplyStarRotation(newRotation);
                    }
                    break;
                case NoteStatus.Arrived:
                    break;
            }
        }
        /// <summary>
        /// 判定队列检查
        /// </summary>
        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void SensorCheck()
        {
            if (AutoplayMode == AutoplayModeOption.Enable || !_isCheckable)
            {
                return;
            }
            else if (IsEnded || !IsInitialized)
            {
                return;
            }
            else if (IsFinished)
            {
                return;
            }
            ref var queueMemory = ref _judgeQueues[0];
            var canPlaySFX = ConnectInfo.IsGroupPartHead || !ConnectInfo.IsConnSlide;

            for(; !queueMemory.IsEmpty; )
            {
                var queue = queueMemory.Span;
                ref var first = ref queue[0];
                ref SlideArea second = ref Unsafe.NullRef<SlideArea>();
                var fAreas = first.IncludedAreas;

                if (queue.Length >= 2)
                {
                    second = ref queue[1];
                }

                for (var i = 0; i < fAreas.Length; i++)
                {
                    var area = fAreas[i];
                    var sensorState = _noteManager.GetSensorStatusInThisFrame(area);
                    first.Check(area, sensorState);
                }

                if (canPlaySFX && first.On)
                {
                    PlaySFX();
                }

                // Check the second area

                if (!Unsafe.IsNullRef(ref second) && (first.IsSkippable || first.On))
                {
                    var sAreas = second.IncludedAreas;

                    for (var i = 0; i < sAreas.Length; i++)
                    {
                        var area = sAreas[i];
                        var sensorState = _noteManager.GetSensorStatusInThisFrame(area);
                        second.Check(area, sensorState);
                    }

                    if (second.IsFinished)
                    {
                        HideBar(second.ArrowProgressWhenFinished);
                        queueMemory = queueMemory.Slice(2);
                        SetParentFinish();
                        continue;
                    }
                    else if (second.On)
                    {
                        HideBar(second.ArrowProgressWhenOn);
                        queueMemory = queueMemory.Slice(1);
                        SetParentFinish();
                        continue;
                    }
                }

                // Finally check the first area

                if (first.IsFinished)
                {
                    HideBar(first.ArrowProgressWhenFinished);
                    queueMemory = queueMemory.Slice(1);
                    SetParentFinish();
                }
                else if (first.On)
                {
                    HideBar(first.ArrowProgressWhenOn);
                }
                return;
            }
        }
        void SlideCheck()
        {
            var thisFrameSec = ThisFrameSec;
            var startTiming = thisFrameSec - Timing;
            var tooLateTiming = StartTiming + _length + SLIDE_JUDGE_GOOD_AREA_MSEC / 1000 + MathF.Min(USERSETTING_JUDGE_OFFSET_SEC, 0);
            var isTooLate = thisFrameSec - tooLateTiming > 0;

            if (!_isCheckable)
            {
                if (ConnectInfo.IsGroupPart)
                {
                    if (ConnectInfo.IsGroupPartHead && startTiming >= -0.05f)
                    {
                        _isCheckable = true;
                    }
                    else if (!ConnectInfo.IsGroupPartHead)
                    {
                        _isCheckable = ConnectInfo.ParentFinished || ConnectInfo.ParentPendingFinish;
                    }
                }
                else if (startTiming >= -0.05f)
                {
                    _isCheckable = true;
                }
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
                        {
                            ClassicJudge(thisFrameSec - USERSETTING_TOUCHPANEL_OFFSET_SEC);
                        }
                        else
                        {
                            Judge(thisFrameSec - USERSETTING_TOUCHPANEL_OFFSET_SEC);
                        }
                        return;
                    }
                    else if (isTooLate)
                    {
                        TooLateJudge();
                    }
                }
                else
                {
                    if (_lastWaitTimeSec <= 0)
                    {
                        End();
                    }
                    else
                    {
                        _lastWaitTimeSec -= MajTimeline.DeltaTime;
                    }
                }
            }
        }
        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void SetParentFinish()
        {
            switch(_parentForceFinishFlag)
            {
                case 0:
                    {
                        _parentForceFinishFlag = 1;
                        if (Parent is not null)
                        {
                            if (_judgeQueues[0].Length < _table.JudgeQueue.Length && !ConnectInfo.ParentFinished)
                            {
                                Parent.ForceFinish();
                            }
                        }
                    }
                    break;
                default:
                    return;
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public new void End()
        {
            if (IsEnded)
            {
                return;
            }
            State = NoteStatus.End;
            base.End();


            if (ConnectInfo.IsGroupPartEnd || !ConnectInfo.IsConnSlide)
            {
                ConvertJudgeGrade(ref _judgeResult);
                if(!ModInfo.SubdivideSlideJudgeGrade)
                {
                    JudgeGradeCorrection(ref _judgeResult);
                }
                var result = new NoteJudgeResult()
                {
                    Grade = _judgeResult,
                    Diff = _judgeDiff,
                    IsEX = IsEX,
                    IsBreak = IsBreak
                };
                // 只有组内最后一个Slide完成 才会显示判定条并增加总数
                _objectCounter.ReportResult(this, result, Multiple);
                if (PlaySlideOK(result))
                {
                    _slideOK.PlayResult(result);
                }

                PlayJudgeSFX(result);
            }
        }
        protected override void Autoplay()
        {
            if (!IsAutoplay)
                return;
            switch (State)
            {
                case NoteStatus.Running:
                case NoteStatus.Arrived:
                    break;
                default:
                    return;
            }
            switch(AutoplayMode)
            {
                case AutoplayModeOption.Enable:
                    var process = ((Length - GetRemainingTimeWithoutOffset()) / Length).Clamp(0, 1);
                    var queueMemory = _judgeQueues[0];
                    var queue = queueMemory.Span;
                    var canPlaySFX = ConnectInfo.IsGroupPartHead || !ConnectInfo.IsConnSlide;
                    if (queueMemory.IsEmpty)
                        return;
                    else if (process >= 1)
                    {
                        HideAllBar();
                        var autoplayGrade = AutoplayGrade;
                        if (((int)autoplayGrade).InRange(0, 14))
                            _judgeResult = autoplayGrade;
                        else
                            _judgeResult = (JudgeGrade)_randomizer.Next(0, 15);
                        _isJudged = true;
                        _lastWaitTimeSec = 0;
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
                    var areaIndex = (int)(process * queueMemory.Length);
                    var isLast = areaIndex == queueMemory.Length - 1;
                    var delta = (process * queueMemory.Length) - areaIndex;
                    if (areaIndex < 0)
                        return;
                    int barIndex;
                    if (delta > 0.9)
                    {
                        barIndex = queue[areaIndex].ArrowProgressWhenFinished;
                    }
                    else if (delta > 0.4 && !isLast)
                    {
                        barIndex = queue[areaIndex].ArrowProgressWhenOn;
                    }
                    else
                    {
                        return;
                    }
                    HideBar(barIndex);
                    break;
                case AutoplayModeOption.DJAuto_TouchPanel_First:
                case AutoplayModeOption.DJAuto_ButtonRing_First:
                    DJAutoplay();
                    break;
            }
        }
        void DJAutoplay()
        {
            if (IsFinished)
            {
                return;
            }
            var currentProgress = ((Length - GetRemainingTimeWithoutOffset()) / Length).Clamp(0, 1);
            var step = (currentProgress - _djAutoplayProgress) / (8 * _djAutoplayRatio);
            var delta = 0f;
            for(; ; )
            {
                var pos = GetPositionFromProgress(_djAutoplayProgress);
                pos.z = -10;
                for (int i = 0; i < 9; i++)
                {
                    const float rad = 0.3f;
                    var circular = new Vector3(rad * Mathf.Sin(45f * i), rad * Mathf.Cos(45f * i));
                    if (i == 8) 
                        circular = Vector3.zero;
                    var ray = new Ray(pos + circular, Vector3.forward);
                    var ishit = Physics.Raycast(ray, out var hitInfom);
                    if (ishit)
                    {
                        var id = hitInfom.colliderInstanceID;
                        var area = InputManager.GetSensorAreaFromInstanceID(id);
                        _noteManager.SimulateSensorPress(area);
                    }
                }
                if(delta > 0.2f || 
                   delta + step > 0.2f ||
                   _djAutoplayProgress >= currentProgress)
                {
                    break;
                }
                delta += step;
                _djAutoplayProgress += step;
            }
            _djAutoplayProgress = _djAutoplayProgress.Clamp(0, currentProgress);
        }
        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        Vector3 GetPositionFromProgress(float progress)
        {
            progress = progress.Clamp(0, 1);
            if(progress == 1)
            {
                return _starPositions[_starPositions.Count - 1];
            }
            var indexProcess = (_starPositions.Count - 1) * progress;
            var index = (int)indexProcess;
            var pos = indexProcess - index;

            var a = _starPositions[index + 1];
            var b = _starPositions[index];
            var ba = a - b;
            var newPos = ba * pos + b;

            return newPos;
        }
        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void ApplyStarRotation(Quaternion newRotation)
        {
            var star = _stars.Span[0];
            var starTransform = _starTransforms.Span[0];
            if (star is null)
            {
                return;
            }

            if (_isMirror)
            {
                var halfFlip = newRotation.eulerAngles;
                halfFlip.z += 180f;
                starTransform.rotation = Quaternion.Euler(halfFlip);
            }
            else
            {
                starTransform.rotation = newRotation;
            }
            //starTransform.rotation = newRotation;
        }
        void LoadSlidePath()
        {
            _starPositions.Clear();
            _starRotations.Clear();
            if (StartPos == 0)
            {
                StartPos = 1;
            }
            _starPositions.Add(NoteHelper.GetTapPosition(StartPos, 4.8f));
            for (var i = 0; i < _slideBars.Count; i++)
            {
                var bar = _slideBars[i];
                _starPositions.Add(bar.transform.position);

                _starRotations.Add(Quaternion.Euler(bar.transform.rotation.normalized.eulerAngles + new Vector3(0f, 0f, 18f)));
                if (i == _slideBars.Count - 1)
                {
                    var a = _slideBars[i - 1].transform.rotation.normalized.eulerAngles;
                    var b = bar.transform.rotation.normalized.eulerAngles;
                    var diff = a - b;
                    var newEulerAugle = b - diff;
                    _starRotations.Add(Quaternion.Euler(newEulerAugle + new Vector3(0f, 0f, 18f)));
                }
            }
            var endPos = NoteHelper.GetTapPosition(EndPos, 4.8f);
            _starPositions.Add(endPos);
            FinalStarAngle = _starRotations[_starRotations.Count - 1];
            if (ConnectInfo.IsConnSlide)
            {
                var parent = ConnectInfo.Parent;
                if (parent is not null)
                {
                    _starRotations[0] = parent.FinalStarAngle;
                }
            }
        }
        protected override void LoadSkin()
        {
            var barRenderers = _slideBarRenderers;
            var skin = MajInstances.SkinManager.GetSlideSkin();
            var star = _stars.Span[0]!;
            var barSprite = skin.Normal;
            var starSprite = skin.Star.Normal;
            Material? breakMaterial = null;

            if (IsEach)
            {
                barSprite = skin.Each;
                starSprite = skin.Star.Each;
            }
            if (IsBreak)
            {
                barSprite = skin.Break;
                starSprite = skin.Star.Break;
                breakMaterial = BreakMaterial;
            }

            foreach (var renderer in barRenderers)
            {
                renderer.color = new Color(1f, 1f, 1f, 0f);
                renderer.sortingOrder = SortOrder--;
                renderer.sortingLayerName = "Slides";

                renderer.sprite = barSprite;
            }

            var starRenderer = star.GetComponent<SpriteRenderer>();
            starRenderer.sprite = starSprite;
            if (breakMaterial is not null)
            {
                starRenderer.sharedMaterial = breakMaterial;
            }

            if (IsJustR)
            {
                if (_slideOK!.SetR() == 1 && _isMirror)
                {
                    _slideOK!.transform.Rotate(new Vector3(0f, 0f, 180f));
                    var angel = _slideOK.transform.rotation.eulerAngles.z * Mathf.Deg2Rad;
                    _slideOK.transform.position += new Vector3(Mathf.Sin(angel) * 0.27f, Mathf.Cos(angel) * -0.27f);
                }
            }
            else
            {
                if (_slideOK!.SetL() == 1 && !_isMirror)
                {
                    _slideOK!.transform.Rotate(new Vector3(0f, 0f, 180f));
                    var angel = _slideOK.transform.rotation.eulerAngles.z * Mathf.Deg2Rad;
                    _slideOK.transform.position += new Vector3(Mathf.Sin(angel) * 0.27f, Mathf.Cos(angel) * -0.27f);
                }
            }
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();
            _table?.Dispose();
            _starPositions.Dispose();
            _starRotations.Dispose();
        }
        [ReadOnlyField]
        [SerializeField]
        bool _isMirror = false;
    }
}