using MajdataPlay.Buffers;
using MajdataPlay.Editor;
using MajdataPlay.Extensions;
using MajdataPlay.IO;
using MajdataPlay.Numerics;
using MajdataPlay.Scenes.Game.Notes.Slide;
using MajdataPlay.Scenes.Game.Notes.Slide.Utils;
using MajdataPlay.Scenes.Game.Utils;
using MajdataPlay.Settings;
using MajdataPlay.Utils;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.IL2CPP.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Profiling;

#nullable enable
namespace MajdataPlay.Scenes.Game.Notes.Behaviours
{
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

        static readonly Quaternion s_Z180Rotation = Quaternion.Euler(0f, 0f, 180f);

//#if UNITY_EDITOR
//        Transform _judgeFramePoint;
//        [SerializeField]
//        float _runtimeSlideConst;
//#endif
        protected override void Awake()
        {
            base.Awake();
            var star = Instantiate(SlideStarPrefab, NoteManager.transform.GetChild(3));
            var slideTable = SlideTables.FindTableByName(SlideType);

            if (slideTable is null)
            {
                throw new MissingComponentException($"Slide table of \"{SlideType}\" is not found");
            }

            _table = slideTable;
            JudgeQueues[0] = _table.JudgeQueue;
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
            var stars = Stars.Span;
            var starTransforms = StarTransforms.Span;

            star.SetActive(true);
            stars[0] = star;
            starTransforms[0] = star.transform;
            _starRenderer = star.GetComponent<SpriteRenderer>();

            var slideOK = transform.GetChild(transform.childCount - 1).gameObject; //slideok is the last one
            slideOK.SetActive(true);
            SlideOK = slideOK.GetComponent<SlideOK>();
            SlideOK.IsClassic = IsClassic;
            SlideOK.Shape = NoteHelper.GetSlideOKShapeFromSlideType(SlideType);


            for (var i = 0; i < Transform.childCount - 1; i++)
            {
                SlideBars.Add(Transform.GetChild(i).gameObject);
                SlideBarRenderers.Add(SlideBars[i].GetComponent<SpriteRenderer>());
                SlideBarTransforms.Add(SlideBars[i].transform);
                SlideBarTransforms[i].localScale *= USERSETTING_SLIDE_SCALE;
            }
            LoadSlidePath();
            SetActive(false);
            SetStarActive(false);
            SetSlideBarAlpha(0f);
            SlideLength = SlideBars.Count + 1;

            starTransforms[0].position = _starPositions[0];
            starTransforms[0].transform.localScale = new Vector3(0f, 0f, 1f);
        }
        /// <summary>
        /// Slide初始化
        /// </summary>
        public override void Init()
        {
            if (IsInited)
            {
                return;
            }
            if (IsMine)
            {
                AutoplayMode = AutoplayModeOption.Enable;
                AutoplayGrade = JudgeGrade.Perfect;
            }
            if (_isMirror)
            {
                _table.Mirror();
                Transform.localScale = new Vector3(-1f, 1f, 1f);
                Transform.rotation = Quaternion.Euler(0f, 0f, -45f * StartPos);
                SlideOK!.transform.localScale = new Vector3(-1f, 1f, 1f);
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
            SlideOK!.transform.SetParent(Transform.parent);
            // 计算Slide淡入时机
            // 在8.0速时应当提前300ms显示Slide
            FadeInTiming = -3.926913f / Speed;
            var fadeInOffset = 0f;
            if (Settings.Debug.OffsetUnit == OffsetUnitOption.Second)
            {
                fadeInOffset = Settings.Game.SlideFadeInOffset;
            }
            else
            {
                fadeInOffset = Settings.Game.SlideFadeInOffset * MajEnv.FRAME_LENGTH_SEC;
            }
            FadeInTiming += fadeInOffset;
            FadeInTiming += Timing;
            FadeInCompletedTiming = Timing - 0.05f;
            // Slide完全淡入时机
            // 正常情况下应为负值；速度过高将忽略淡入
            FadeInDurationTimeSec = (FadeInCompletedTiming - FadeInTiming).Clamp(0, 0.2f);
            FadeInCutoffTiming = FadeInTiming + FadeInDurationTimeSec;
            //var interval = fullFadeInTiming - fadeInTiming;
            //fadeInAnimator = GetComponent<Animator>();
            //Destroy(GetComponent<Animator>());
            //淡入时机与正解帧间隔小于200ms时，加快淡入动画的播放速度
            //fadeInAnimator.speed = 0.2f / interval;
            //fadeInAnimator.SetTrigger("slide");
            var starTransforms = StarTransforms.Span;
            starTransforms[0].position = _starPositions[0];
            starTransforms[0].transform.localScale = new Vector3(0f, 0f, 1f);
            JudgeQueues[0] = _table.JudgeQueue;

            InitializeSlideGroup();

            if (ConnectInfo.IsConnSlide && !ConnectInfo.IsGroupPartEnd)
            {
                Destroy(SlideOK);
                SlideOK = null;
            }

            State = NoteStatus.Inited;
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
            var judgeQueue = JudgeQueues[0].Span;

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
            }
            UpdateJudgeQueue();

            if (ConnectInfo.IsGroupPartEnd || !ConnectInfo.IsConnSlide)
            {
                var percent = _table.Const;
                if (IsClassic)
                {
                    percent = _table.ClassicConst;
                }
                JudgeTiming = StartTiming + Length * (1 - percent);
                LastWaitTimeSec = Length * percent;
            }
        }
        void UpdateJudgeQueue()
        {
            var judgeQueue = JudgeQueues[0].Span;
            if (!USERSETTING_SLIDE_SKIPPING)
            {
                foreach (ref var judgeArea in judgeQueue)
                {
                    judgeArea.IsSkippable = false;
                }
            }
            else
            {
                if (ConnectInfo.IsConnSlide)
                {
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
            }     
        }
        [OnPreUpdate]
        void OnPreUpdate()
        {
            using (UnityProfiler.Create("SlideDrop.OnPreUpdate"))
            {
                SlideBarFadeIn();
                SlideCheck();
            }
        }
        [OnUpdate]
        void OnUpdate()
        {
            using (UnityProfiler.Create("SlideDrop.OnUpdate"))
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
                var starTransform = StarTransforms.Span[0];

                Autoplay();
                SensorCheck();

                switch (State)
                {
                    case NoteStatus.Inited:
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
                        if (IsClassic)
                        {
                            var scale = 1 + alpha / 2;
                            starTransform.localScale = new Vector3(scale, scale, scale);
                        }
                        else
                        {
                            starTransform.localScale = new Vector3(alpha + 0.5f, alpha + 0.5f, alpha + 0.5f);
                        }
                        break;
                    case NoteStatus.Running:
                        var remaingTimeWithoutOffset = GetRemainingTimeWithoutOffset();
                        if (remaingTimeWithoutOffset == 0)
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
                        var process = ((Length - remaingTimeWithoutOffset) / Length).Clamp(0, 1);
                        var indexProcess = (_starPositions.Count - 1) * process;
                        var index = (int)indexProcess;
                        var pos = indexProcess - index;

                        var a = _starPositions[index];
                        var b = _starPositions[index + 1];
                        var newPosition = Vector3.LerpUnclamped(a, b, pos);
                        var newRotation = Quaternion.SlerpUnclamped(
                            _starRotations[index],
                            _starRotations[index + 1],
                            pos
                        );
                        starTransform.position = newPosition;
                        ApplyStarRotation(newRotation);
                        break;
                    case NoteStatus.Arrived:
                        break;
                }
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
            if (AutoplayMode == AutoplayModeOption.Enable || !IsCheckable)
            {
                return;
            }
            else if (IsEnded || !IsInited)
            {
                return;
            }
            else if (IsFinished)
            {
                return;
            }
            ref var queueMemory = ref JudgeQueues[0];
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
                    var sensorState = NoteManager.GetSensorStatusInThisFrame(area);
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
                        var sensorState = NoteManager.GetSensorStatusInThisFrame(area);
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

            if (!IsCheckable)
            {
                if (ConnectInfo.IsGroupPart)
                {
                    if (ConnectInfo.IsGroupPartHead && startTiming >= -0.05f)
                    {
                        IsCheckable = true;
                    }
                    else if (!ConnectInfo.IsGroupPartHead)
                    {
                        IsCheckable = ConnectInfo.ParentFinished || ConnectInfo.ParentPendingFinish;
                    }
                }
                else if (startTiming >= -0.05f)
                {
                    IsCheckable = true;
                }
            }

            var isJudgable = ConnectInfo.IsGroupPartEnd || !ConnectInfo.IsConnSlide;

            if (isJudgable)
            {
                if (!IsJudged)
                {
                    if (IsFinished)
                    {
                        HideAllBar();
                        if (IsClassic)
                        {
                            JudgeClassic(thisFrameSec - USERSETTING_TOUCHPANEL_OFFSET_SEC);
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
                    if (LastWaitTimeSec <= 0)
                    {
                        End();
                    }
                    else
                    {
                        LastWaitTimeSec -= MajTimeline.DeltaTime;
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
                            if (JudgeQueues[0].Length < _table.JudgeQueue.Length && !ConnectInfo.ParentFinished)
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
            if (IsJudged)
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
                ConvertJudgeGrade(ref JudgeResult);
                if(!ModInfo.SubdivideSlideJudgeGrade)
                {
                    JudgeGradeCorrection(ref JudgeResult);
                }
                var result = new NoteJudgeResult()
                {
                    Grade = JudgeResult,
                    Diff = JudgeDiff,
                    IsEX = IsEX,
                    IsBreak = IsBreak
                };
                // 只有组内最后一个Slide完成 才会显示判定条并增加总数
                ObjectCounter.ReportResult(this, result, Multiple);
                if (PlaySlideOK(result))
                {
                    SlideOK.PlayResult(result);
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
                    var queueMemory = JudgeQueues[0];
                    var queue = queueMemory.Span;
                    var canPlaySFX = ConnectInfo.IsGroupPartHead || !ConnectInfo.IsConnSlide;
                    if (queueMemory.IsEmpty)
                        return;
                    else if (process >= 1)
                    {
                        HideAllBar();
                        var autoplayGrade = AutoplayGrade;
                        if (((int)autoplayGrade).InRange(0, 14))
                            JudgeResult = autoplayGrade;
                        else
                            JudgeResult = (JudgeGrade)Randomizer.Next(0, 15);
                        IsJudged = true;
                        LastWaitTimeSec = 0;
                        JudgeDiff = JudgeResult switch
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
            const float DJAUTO_SIMULATE_RAD = 0.3f;
            if (IsFinished)
            {
                return;
            }
            var currentProgress = ((Length - GetRemainingTimeWithoutOffset()) / Length).Clamp(0, 1);
            var step = (currentProgress - DJAutoplayProgress) / (8 * _djAutoplayRatio);
            var delta = 0f;
            for(; ; )
            {
                var cubeRay = GetPositionFromProgress(DJAutoplayProgress);
                SlideDJAutoSimulateSensorPress(cubeRay, DJAUTO_SIMULATE_RAD);

                if (delta > 0.2f || 
                   delta + step > 0.2f ||
                   DJAutoplayProgress >= currentProgress)
                {
                    break;
                }
                delta += step;
                DJAutoplayProgress += step;
            }
            DJAutoplayProgress = DJAutoplayProgress.Clamp(0, currentProgress);
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
        void ApplyStarRotation(in Quaternion newRotation)
        {
            var star = Stars.Span[0];
            var starTransform = StarTransforms.Span[0];
            if (star is null)
            {
                return;
            }

            if (_isMirror)
            {
                starTransform.rotation = newRotation * s_Z180Rotation;
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
            _starRotations.Add(Quaternion.Euler(SlideBars[0].transform.rotation.normalized.eulerAngles + new Vector3(0f, 0f, 18f)));
            for (var i = 0; i < SlideBars.Count; i++)
            {
                var bar = SlideBars[i];
                _starPositions.Add(bar.transform.position);

                _starRotations.Add(Quaternion.Euler(SlideBars[i].transform.rotation.normalized.eulerAngles + new Vector3(0f, 0f, 18f)));
                if (i == SlideBars.Count - 1)
                {
                    var a = SlideBars[i - 1].transform.rotation.normalized.eulerAngles;
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
            var barRenderers = SlideBarRenderers;
            var skin = MajInstances.SkinManager.GetSlideSkin();
            var star = Stars.Span[0]!;
            var barSprite = skin.Normal;
            var starSprite = skin.Star.Normal;
            Material? breakMaterial = null;

            if (IsMine)
            {
                barSprite = skin.Mine;
                starSprite = skin.Star.Mine;
                if (IsBreak)
                {
                    barSprite = skin.BreakMine;
                    starSprite = skin.Star.BreakMine;
                    breakMaterial = BreakMaterial;
                }
            }
            else
            {
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
                if (SlideOK!.SetR() == 1 && _isMirror)
                {
                    SlideOK!.transform.Rotate(new Vector3(0f, 0f, 180f));
                    var angel = SlideOK.transform.rotation.eulerAngles.z * Mathf.Deg2Rad;
                    SlideOK.transform.position += new Vector3(Mathf.Sin(angel) * 0.27f, Mathf.Cos(angel) * -0.27f);
                }
            }
            else
            {
                if (SlideOK!.SetL() == 1 && !_isMirror)
                {
                    SlideOK!.transform.Rotate(new Vector3(0f, 0f, 180f));
                    var angel = SlideOK.transform.rotation.eulerAngles.z * Mathf.Deg2Rad;
                    SlideOK.transform.position += new Vector3(Mathf.Sin(angel) * 0.27f, Mathf.Cos(angel) * -0.27f);
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