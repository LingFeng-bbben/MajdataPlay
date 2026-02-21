using MajdataPlay.Buffers;
using MajdataPlay.Collections;
using MajdataPlay.Extensions;
using MajdataPlay.IO;
using MajdataPlay.Numerics;
using MajdataPlay.Scenes.Game.Notes.Slide;
using MajdataPlay.Scenes.Game.Notes.Slide.Utils;
using MajdataPlay.Scenes.Game.Utils;
using MajdataPlay.Settings;
using MajdataPlay.Utils;
using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

#nullable enable
namespace MajdataPlay.Scenes.Game.Notes.Behaviours
{
    using Unsafe = System.Runtime.CompilerServices.Unsafe;
    internal sealed class WifiDrop : SlideBase, IMajComponent
    {

        readonly Vector3[] _starEndPositions = new Vector3[3];

        readonly SpriteRenderer[] _starRenderers = new SpriteRenderer[3];

        Vector3[] _starStartPositions = new Vector3[3];

        WifiTable _wifiTable = SlideTables.GetWifiTable(1);

        protected override void Awake()
        {
            base.Awake();
            EndPos = 5;
            var stars = _stars.Span;
            var starTransforms = _starTransforms.Span;

            var slideParent = _noteManager.transform.GetChild(3);
            var centerStar = Instantiate(_slideStarPrefab, slideParent);
            var leftStar = Instantiate(_slideStarPrefab, slideParent);
            var rightStar = Instantiate(_slideStarPrefab, slideParent);

            var sensorPos = (SensorArea)(EndPos - 1);
            var rIndex = sensorPos.Diff(-1).GetIndex();
            var lIndex = sensorPos.Diff(1).GetIndex();

            rightStar.SetActive(true);
            centerStar.SetActive(true);
            leftStar.SetActive(true);
            stars[0] = rightStar;
            stars[1] = centerStar;
            stars[2] = leftStar;
            _starRenderers[0] = stars[0]!.GetComponent<SpriteRenderer>();
            _starRenderers[1] = stars[1]!.GetComponent<SpriteRenderer>();
            _starRenderers[2] = stars[2]!.GetComponent<SpriteRenderer>();
            _judgeQueues[0] = _wifiTable.Left;
            _judgeQueues[1] = _wifiTable.Center;
            _judgeQueues[2] = _wifiTable.Right;
            _starEndPositions[0] = NoteHelper.GetTapPosition(rIndex, 4.8f);// R
            _starEndPositions[1] = NoteHelper.GetTapPosition(EndPos, 4.8f);// Center
            _starEndPositions[2] = NoteHelper.GetTapPosition(lIndex, 4.8f); // L

            if (IsClassic)
            {
                _starStartPositions[0] = NoteHelper.GetTapPosition(StartPos + 0.11f, 4.55f);
                _starStartPositions[1] = NoteHelper.GetTapPosition(StartPos, 4.8f);
                _starStartPositions[2] = NoteHelper.GetTapPosition(StartPos - 0.13f, 4.55f);

                _starRenderers[0].sortingOrder = -1;
                _starRenderers[2].sortingOrder = -1;
            }
            else
            {
                _starStartPositions[0] = NoteHelper.GetTapPosition(StartPos, 4.8f);
                _starStartPositions[1] = NoteHelper.GetTapPosition(StartPos, 4.8f);
                _starStartPositions[2] = NoteHelper.GetTapPosition(StartPos, 4.8f);
            }

            var slideOK = transform.GetChild(transform.childCount - 1).gameObject; //slideok is the last one
            slideOK.SetActive(true);
            _slideOK = slideOK.GetComponent<SlideOK>();
            _slideOK.IsClassic = IsClassic;
            _slideOK.Shape = NoteHelper.GetSlideOKShapeFromSlideType("wifi");

            //Transform.rotation = Quaternion.Euler(0f, 0f, -45f * (StartPos - 1));

            for (var i = 0; i < Transform.childCount - 1; i++)
            {
                _slideBars.Add(Transform.GetChild(i).gameObject);
                _slideBarTransforms.Add(_slideBars[i].transform);
                _slideBarRenderers.Add(_slideBars[i].GetComponent<SpriteRenderer>());
            }

            SetActive(false);
            SetStarActive(false);
            SetSlideBarAlpha(0f);

            for (var i = 0; i < _stars.Length; i++)
            {
                var star = stars[i];
                if (star is null)
                {
                    continue;
                }
                starTransforms[i] = star.transform;
                star.transform.position = _starStartPositions[i];
                star.transform.localScale = new Vector3(0f, 0f, 1f);
            }
            SlideLength = 20;
        }
        public override void Initialize()
        {
            if (State >= NoteStatus.Initialized)
            {
                return;
            }
            _wifiTable.Dispose();
            _wifiTable = SlideTables.GetWifiTable(StartPos);
            var wifiConst = _wifiTable.Const;

            _judgeQueues[0] = _wifiTable.Left;
            _judgeQueues[1] = _wifiTable.Center;
            _judgeQueues[2] = _wifiTable.Right;

            _judgeTiming = StartTiming + Length * (1 - wifiConst);
            _lastWaitTimeSec = Length * wifiConst;

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
            //Destroy(GetComponent<Animator>());
            _maxFadeInAlpha = 1f;
            //淡入时机与正解帧间隔小于200ms时，加快淡入动画的播放速度
            //fadeInAnimator.speed = 0.2f / interval;
            //fadeInAnimator.SetTrigger("wifi");
            var sensorPos = (SensorArea)(EndPos - 1);
            var rIndex = sensorPos.Diff(-1).GetIndex();
            var lIndex = sensorPos.Diff(1).GetIndex();
            _starEndPositions[0] = NoteHelper.GetTapPosition(rIndex, 4.8f);// R
            _starEndPositions[1] = NoteHelper.GetTapPosition(EndPos, 4.8f);// Center
            _starEndPositions[2] = NoteHelper.GetTapPosition(lIndex, 4.8f); // L

            if (IsClassic)
            {
                _starStartPositions[0] = NoteHelper.GetTapPosition(StartPos + 0.11f, 4.55f);
                _starStartPositions[1] = NoteHelper.GetTapPosition(StartPos, 4.8f);
                _starStartPositions[2] = NoteHelper.GetTapPosition(StartPos - 0.13f, 4.55f);
            }
            else
            {
                _starStartPositions[0] = NoteHelper.GetTapPosition(StartPos, 4.8f);
                _starStartPositions[1] = NoteHelper.GetTapPosition(StartPos, 4.8f);
                _starStartPositions[2] = NoteHelper.GetTapPosition(StartPos, 4.8f);
            }

            Transform.rotation = Quaternion.Euler(0f, 0f, -45f * (StartPos - 1));

            LoadSkin();
            _slideOK!.transform.SetParent(transform.parent);
            var stars = _stars.Span;
            var starTransforms = _starTransforms.Span;
            for (var i = 0; i < stars.Length; i++)
            {
                var star = stars[i];
                if (star is null)
                {
                    continue;
                }
                starTransforms[i] = star.transform;
                star.transform.position = _starStartPositions[i];
                star.transform.localScale = new Vector3(0f, 0f, 1f);
            }

            State = NoteStatus.Initialized;
        }
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

            for (var i = 0; i < 3; i++)
            {
                SensorCheckInternal(ref _judgeQueues[i]);
            }
        }
        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void SensorCheckInternal(ref Memory<SlideArea> queueMemory)
        {
            if (queueMemory.IsEmpty)
            {
                return;
            }

            for (; !queueMemory.IsEmpty;)
            {
                var queue = queueMemory.Span;
                ref var first = ref queue[0];
                ref SlideArea second = ref Unsafe.NullRef<SlideArea>();
                var fAreas = first.IncludedAreas;

                if (queueMemory.Length >= 2)
                {
                    second = ref queue[1];
                }

                for (var i = 0; i < fAreas.Length; i++)
                {
                    var area = fAreas[i];
                    var sensorState = _noteManager.GetSensorStatusInThisFrame(area);
                    first.Check(area, sensorState);
                }

                if (first.On)
                {
                    PlaySFX();
                }

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
                        queueMemory = queueMemory.Slice(2);
                        HideBar(GetIndex());
                        continue;
                    }
                    else if (second.On)
                    {
                        queueMemory = queueMemory.Slice(1);
                        HideBar(GetIndex());
                        continue;
                    }
                }

                if (first.IsFinished)
                {
                    queueMemory = queueMemory.Slice(1);
                    HideBar(GetIndex());
                }
                return;
            }
        }
        void SlideCheck()
        {
            var thisFrameSec = ThisFrameSec;
            var startTiming = thisFrameSec - Timing;
            var tooLateTiming = StartTiming + Length + SLIDE_JUDGE_GOOD_AREA_MSEC / 1000 + MathF.Min(USERSETTING_JUDGE_OFFSET_SEC, 0);
            var isTooLate = thisFrameSec - tooLateTiming > 0;

            if (startTiming >= -0.05f)
            {
                _isCheckable = true;
            }

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
        int GetIndex()
        {
            if (_judgeQueues.IsEmpty())
            {
                return int.MaxValue;
            }
            else if (IsClassic)
            {
                var isRemainingOne = _judgeQueues.All(x => x.Length <= 1);
                if (isRemainingOne)
                {
                    return 8;
                }
            }
            else if (_judgeQueues[1].IsEmpty)
            {
                if (_judgeQueues[0].Length <= 1 && _judgeQueues[2].Length <= 1)
                {
                    return 9;
                }
            }
            var nums = new int[3];
            foreach (var (i, queue) in _judgeQueues.WithIndex())
                nums[i] = queue.Length;
            var max = nums.Max();
            var index = nums.FindIndex(x => x == max);

            return _judgeQueues[index].Span[0].ArrowProgressWhenFinished;
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
            Autoplay();
            SensorCheck();
            var stars = _stars.Span;
            var starTransforms = _starTransforms.Span;
            switch (State)
            {
                case NoteStatus.Initialized:
                    SetStarActive(false);
                    if (ThisFrameSec - Timing > 0)
                    {
                        SetStarActive(true);
                        for (var i = 0; i < stars.Length; i++)
                        {
                            var starTransform = starTransforms[i];

                            starTransform.position = _starStartPositions[i];
                        }
                        State = NoteStatus.Scaling;
                        goto case NoteStatus.Scaling;
                    }
                    break;
                case NoteStatus.Scaling:
                    var timing = ThisFrameSec - StartTiming;
                    if (timing > 0f)
                    {
                        for (var i = 0; i < stars.Length; i++)
                        {
                            var starTransform = starTransforms[i];

                            _starRenderers[i].color = new Color(1, 1, 1, 1);
                            if (!IsSlideNoHead)
                            {
                                starTransform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
                            }
                        }
                        State = NoteStatus.Running;
                        goto case NoteStatus.Running;
                    }
                    else if (IsSlideNoHead)
                    {
                        return;
                    }
                    var alpha = (1f - -timing / (StartTiming - Timing)).Clamp(0, 1);

                    for (var i = 0; i < stars.Length; i++)
                    {
                        var starTransform = starTransforms[i];

                        _starRenderers[i].color = new Color(1, 1, 1, alpha);
                        starTransform.localScale = new Vector3(alpha + 0.5f, alpha + 0.5f, alpha + 0.5f);
                    }
                    break;
                case NoteStatus.Running:
                    if (GetRemainingTimeWithoutOffset() == 0)
                    {
                        for (var i = 0; i < stars.Length; i++)
                        {
                            var starTransform = starTransforms[i];
                            starTransform.position = _starEndPositions[i];
                        }
                        State = NoteStatus.Arrived;
                        goto case NoteStatus.Arrived;
                    }
                    var process = ((Length - GetRemainingTimeWithoutOffset()) / Length).Clamp(0, 1);

                    for (var i = 0; i < stars.Length; i++)
                    {
                        var starTransform = starTransforms[i];
                        var a = _starEndPositions[i];
                        var b = _starStartPositions[i];
                        var ba = a - b;
                        var newPos = ba * process + b;

                        starTransform.position = newPos; //TODO add some runhua
                    }
                    break;
                case NoteStatus.Arrived:
                    break;
            }
        }
        protected override void Autoplay()
        {
            if (!IsAutoplay)
            {
                return;
            }
            switch(State)
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
                    if (queueMemory.IsEmpty)
                    {
                        return;
                    }
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
                    else if (process > 0)
                        PlaySFX();
                    var areaIndex = (int)(process * queueMemory.Length) - 1;
                    if (areaIndex < 0)
                    {
                        return;
                    }
                    var barIndex = queue[areaIndex].ArrowProgressWhenFinished;
                    HideBar(barIndex);
                    break;
                case AutoplayModeOption.DJAuto_ButtonRing_First:
                case AutoplayModeOption.DJAuto_TouchPanel_First:
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
            var startPos = NoteHelper.GetTapPosition(StartPos, 4.8f);
            var step = (currentProgress - _djAutoplayProgress) / 4;
            for (; ; _djAutoplayProgress += step)
            {
                for (var j = 0; j < 3; j++)
                {
                    float rad;
                    if(j == 1)
                    {
                        rad = 0.8f;
                    }
                    else
                    {
                        rad = 0.15f;
                    }
                    var pos = (_starEndPositions[j] - startPos) * _djAutoplayProgress.Clamp(0, 1) + startPos;
                    pos.z = -10;
                    for (int i = 0; i < 9; i++)
                    {
                        
                        var circular = new Vector3(rad * Mathf.Sin(45f * i), rad * Mathf.Cos(45f * i));
                        if (i == 8)
                        {
                            circular = Vector3.zero;
                        }
                        var ray = new Ray(pos + circular, Vector3.forward);
                        var ishit = Physics.Raycast(ray, out var hitInfom);
                        if (ishit)
                        {
                            var id = hitInfom.colliderInstanceID;
                            var area = InputManager.GetSensorAreaFromInstanceID(id);
                            _noteManager.SimulateSensorPress(area);
                        }
                    }
                }
                if(_djAutoplayProgress >= currentProgress)
                {
                    break;
                }
            }
            _djAutoplayProgress = _djAutoplayProgress.Clamp(0, currentProgress);
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
        protected override void End()
        {
            if (IsEnded)
            {
                return;
            }
            State = NoteStatus.End;
            base.End();
            ConvertJudgeGrade(ref _judgeResult);
            if (!ModInfo.SubdivideSlideJudgeGrade)
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

            _objectCounter.ReportResult(this, result, Multiple);
            if (PlaySlideOK(result))
            {
                _slideOK!.PlayResult(result);
            }
            PlayJudgeSFX(result);
        }
        protected override void LoadSkin()
        {
            var barRenderers = _slideBarRenderers;
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
                breakMaterial = BreakMaterial;
            }
            foreach (var (i, renderer) in barRenderers.WithIndex())
            {
                renderer.color = new Color(1f, 1f, 1f, 0f);
                renderer.sortingOrder = SortOrder--;
                renderer.sortingLayerName = "Slides";

                renderer.sprite = barSprites[i];
            }
            foreach (var (i, star) in _stars.Span.WithIndex())
            {
                var starRenderer = _starRenderers[i];
                starRenderer.sprite = starSprite;
                if (breakMaterial is not null)
                {
                    starRenderer.sharedMaterial = breakMaterial;
                }
                star!.transform.rotation = Quaternion.Euler(0, 0, -22.5f * (8 + i + 2 * (StartPos - 1)));
            }


            if (IsJustR)
            {
                _slideOK!.SetR();
            }
            else
            {
                _slideOK!.SetL();
                _slideOK!.transform.Rotate(new Vector3(0f, 0f, 180f));
            }
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();
            _wifiTable.Dispose();
        }
    }
}