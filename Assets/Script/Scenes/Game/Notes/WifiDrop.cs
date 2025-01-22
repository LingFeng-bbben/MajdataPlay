using MajdataPlay.Collections;
using MajdataPlay.Extensions;
using MajdataPlay.Game.Controllers;
using MajdataPlay.Game.Types;
using MajdataPlay.Game.Utils;
using MajdataPlay.Interfaces;
using MajdataPlay.IO;
using MajdataPlay.Types;
using MajdataPlay.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using Unity.VisualScripting;
using UnityEngine;

#nullable enable
namespace MajdataPlay.Game.Notes
{
    internal sealed class WifiDrop : SlideBase, IMajComponent
    {

        readonly Vector3[] _slideEndPositions = new Vector3[3];

        readonly SpriteRenderer[] _starRenderers = new SpriteRenderer[3];

        Vector3[] _slideStartPositions = new Vector3[3];

        protected override void Awake()
        {
            base.Awake();
            _endPos = 5;
            var slideParent = _noteManager.transform.GetChild(3);
            var centerStar = Instantiate(_slideStarPrefab, slideParent);
            var leftStar = Instantiate(_slideStarPrefab, slideParent);
            var rightStar = Instantiate(_slideStarPrefab, slideParent);
            var wifiTable = SlideTables.GetWifiTable(StartPos);

            var sensorPos = (SensorType)(_endPos - 1);
            var rIndex = sensorPos.Diff(-1).GetIndex();
            var lIndex = sensorPos.Diff(1).GetIndex();

            rightStar.SetActive(true);
            centerStar.SetActive(true);
            leftStar.SetActive(true);
            _stars[0] = rightStar;
            _stars[1] = centerStar;
            _stars[2] = leftStar;
            _starRenderers[0] = _stars[0]!.GetComponent<SpriteRenderer>();
            _starRenderers[1] = _stars[1]!.GetComponent<SpriteRenderer>();
            _starRenderers[2] = _stars[2]!.GetComponent<SpriteRenderer>();
            _judgeQueues[0] = wifiTable[0];
            _judgeQueues[1] = wifiTable[1];
            _judgeQueues[2] = wifiTable[2];
            _slideEndPositions[0] = GetPositionFromDistance(4.8f, rIndex);// R
            _slideEndPositions[1] = GetPositionFromDistance(4.8f, _endPos);// Center
            _slideEndPositions[2] = GetPositionFromDistance(4.8f, lIndex); // L

            if (IsClassic)
            {
                _slideStartPositions[0] = GetPositionFromDistance(4.55f, StartPos + 0.11f);
                _slideStartPositions[1] = GetPositionFromDistance(4.8f);
                _slideStartPositions[2] = GetPositionFromDistance(4.55f, StartPos - 0.13f);
            }
            else
            {
                _slideStartPositions[0] = GetPositionFromDistance(4.8f);
                _slideStartPositions[1] = GetPositionFromDistance(4.8f);
                _slideStartPositions[2] = GetPositionFromDistance(4.8f);
            }

            _slideOK = Transform.GetChild(Transform.childCount - 1).gameObject; //slideok is the last one
            _slideOKAnim = _slideOK.GetComponent<Animator>();
            _slideOKController = _slideOK.GetComponent<LoadJustSprite>();
            _slideOK.SetActive(false);

            //Transform.rotation = Quaternion.Euler(0f, 0f, -45f * (StartPos - 1));
            _slideBars = new GameObject[Transform.childCount - 1];
            _slideBarRenderers = new SpriteRenderer[Transform.childCount - 1];
            _slideBarTransforms = new Transform[Transform.childCount - 1];

            for (var i = 0; i < Transform.childCount - 1; i++)
            {
                _slideBars[i] = Transform.GetChild(i).gameObject;
                _slideBarTransforms[i] = _slideBars[i].transform;
                _slideBarRenderers[i] = _slideBars[i].GetComponent<SpriteRenderer>();
            }

            SetActive(false);
            SetStarActive(false);
            SetSlideBarAlpha(0f);

            for (var i = 0; i < _stars.Length; i++)
            {
                var star = _stars[i];
                if (star is null)
                    continue;
                _starTransforms[i] = star.transform;
                star.transform.position = _slideStartPositions[i];
                star.transform.localScale = new Vector3(0f, 0f, 1f);
            }
            _slideLength = (_slideStartPositions[1] - _slideEndPositions[1]).magnitude;
        }
        public override void Initialize()
        {
            if (State >= NoteStatus.Initialized)
                return;
            var wifiTable = SlideTables.GetWifiTable(StartPos);
            const float wifiConst = 0.162870f;

            _judgeQueues[0] = wifiTable[0];
            _judgeQueues[1] = wifiTable[1];
            _judgeQueues[2] = wifiTable[2];

            _judgeTiming = Timing + (Length * (1 - wifiConst));
            _lastWaitTime = Length * wifiConst;

            // 计算Slide淡入时机
            // 在8.0速时应当提前300ms显示Slide
            _fadeInTiming = -3.926913f / Speed;
            _fadeInTiming += _gameSetting.Game.SlideFadeInOffset;
            _fadeInTiming += _startTiming;
            // Slide完全淡入时机
            // 正常情况下应为负值；速度过高将忽略淡入
            _fullFadeInTiming = _fadeInTiming + 0.2f;
            //var interval = fullFadeInTiming - fadeInTiming;
            //Destroy(GetComponent<Animator>());
            _maxFadeInAlpha = 1f;
            //淡入时机与正解帧间隔小于200ms时，加快淡入动画的播放速度
            //fadeInAnimator.speed = 0.2f / interval;
            //fadeInAnimator.SetTrigger("wifi");
            var sensorPos = (SensorType)(_endPos - 1);
            var rIndex = sensorPos.Diff(-1).GetIndex();
            var lIndex = sensorPos.Diff(1).GetIndex();
            _slideEndPositions[0] = GetPositionFromDistance(4.8f, rIndex);// R
            _slideEndPositions[1] = GetPositionFromDistance(4.8f, _endPos);// Center
            _slideEndPositions[2] = GetPositionFromDistance(4.8f, lIndex); // L

            if(IsClassic)
            {
                _slideStartPositions[0] = GetPositionFromDistance(4.55f,StartPos + 0.11f);
                _slideStartPositions[1] = GetPositionFromDistance(4.8f);
                _slideStartPositions[2] = GetPositionFromDistance(4.55f, StartPos - 0.13f);

                _starRenderers[0].sortingOrder = -1;
                _starRenderers[2].sortingOrder = -1;
            }
            else
            {
                _slideStartPositions[0] = GetPositionFromDistance(4.8f);
                _slideStartPositions[1] = GetPositionFromDistance(4.8f);
                _slideStartPositions[2] = GetPositionFromDistance(4.8f);
            }

            Transform.rotation = Quaternion.Euler(0f, 0f, -45f * (StartPos - 1));

            LoadSkin();
            _slideOK!.transform.SetParent(transform.parent);
            for (var i = 0; i < _stars.Length; i++)
            {
                var star = _stars[i];
                if (star is null)
                    continue;
                _starTransforms[i] = star.transform;
                star.transform.position = _slideStartPositions[i];
                star.transform.localScale = new Vector3(0f, 0f, 1f);
            }

            State = NoteStatus.Initialized;
        }
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
                for (int i = 0; i < 3; i++)
                {
                    Check(ref _judgeQueues[i]);
                }
            }
            finally
            {
                _isChecking = false;
            }
        }
        void Check(ref Memory<SlideArea> queueMemory)
        {
            if (queueMemory.IsEmpty)
                return;

            var queue = queueMemory.Span;
            var first = queue[0];
            SlideArea? second = null;

            if (queueMemory.Length >= 2)
                second = queue[1];
            var fAreas = first.IncludedAreas;
            foreach (var t in fAreas)
            {
                var sensorState = _ioManager.CheckSensorStatus(t, SensorStatus.On) ? SensorStatus.On : SensorStatus.Off;
                first.Check(t, sensorState);
            }

            if (first.On)
                PlaySFX();

            if (second is not null && (first.IsSkippable || first.On))
            {
                var sAreas = second.IncludedAreas;
                foreach (var t in sAreas)
                {
                    var sensorState = _ioManager.CheckSensorStatus(t, SensorStatus.On) ? SensorStatus.On : SensorStatus.Off;
                    second.Check(t, sensorState);
                }

                if (second.IsFinished)
                {
                    queueMemory = queueMemory.Slice(2);
                    HideBar(GetIndex());
                    return;
                }
                else if (second.On)
                {
                    queueMemory = queueMemory.Slice(1);
                    HideBar(GetIndex());
                    return;
                }
            }

            if (first.IsFinished)
            {
                queueMemory = queueMemory.Slice(1);
                HideBar(GetIndex());
                return;
            }

        }
        void SlideCheck()
        {
            /// time      是Slide启动的时间点
            /// timeStart 是Slide完全显示但未启动
            /// LastFor   是Slide的时值
            var thisFrameSec = ThisFrameSec;
            var startTiming = thisFrameSec - _startTiming;
            var tooLateTiming = _timing + Length + 0.6 + MathF.Min(_gameSetting.Judge.JudgeOffset, 0);
            var isTooLate = thisFrameSec - tooLateTiming >= 0;

            if (startTiming >= -0.05f)
                _isCheckable = true;

            if (!_isJudged)
            {
                if (IsFinished)
                {
                    HideAllBar();
                    if (IsClassic)
                        Judge_Classic(thisFrameSec);
                    else
                        Judge(thisFrameSec);
                }
                else if (isTooLate)
                    TooLateJudge();
            }
            else
            {
                if (_lastWaitTime <= 0)
                    End();
                else
                    _lastWaitTime -= Time.deltaTime;
            }
        }
        int GetIndex()
        {
            if(_judgeQueues.IsEmpty())
                return int.MaxValue;
            else if(IsClassic)
            {
                var isRemainingOne = _judgeQueues.All(x => x.Length <= 1);
                if (isRemainingOne)
                    return 8;
            }
            else if (_judgeQueues[1].IsEmpty)
            {
                if (_judgeQueues[0].Length <= 1 && _judgeQueues[2].Length <= 1)
                    return 9;
            }
            var nums = new int[3];
            foreach(var (i,queue) in _judgeQueues.WithIndex())
                nums[i] = queue.Length;
            var max = nums.Max();
            var index = nums.FindIndex(x => x == max);

            return _judgeQueues[index].Span[0].ArrowProgressWhenFinished;
        }
        void OnUpdate()
        {
            SlideBarFadeIn();
            SlideCheck();
            CheckSensor();

            switch (State)
            {
                case NoteStatus.Initialized:
                    SetStarActive(false);
                    if (ThisFixedUpdateSec - StartTiming > 0)
                    {
                        SetStarActive(true);
                        for (var i = 0; i < _stars.Length; i++)
                        {
                            var starTransform = _starTransforms[i];

                            starTransform.position = _slideStartPositions[i];
                        }
                        State = NoteStatus.Scaling;
                        goto case NoteStatus.Scaling;
                    }
                    break;
                case NoteStatus.Scaling:
                    var timing = ThisFixedUpdateSec - Timing;
                    if (timing > 0f)
                    {
                        for (var i = 0; i < _stars.Length; i++)
                        {
                            var starTransform = _starTransforms[i];

                            _starRenderers[i].color = new Color(1, 1, 1, 1);
                            starTransform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
                        }
                        State = NoteStatus.Running;
                        goto case NoteStatus.Running;
                    }
                    var alpha = (1f - -timing / (_timing - _startTiming)).Clamp(0, 1);

                    for (var i = 0; i < _stars.Length; i++)
                    {
                        var starTransform = _starTransforms[i];

                        _starRenderers[i].color = new Color(1, 1, 1, alpha);
                        starTransform.localScale = new Vector3(alpha + 0.5f, alpha + 0.5f, alpha + 0.5f);
                    }
                    break;
                case NoteStatus.Running:
                    if (GetRemainingTimeWithoutOffset() == 0)
                    {
                        for (var i = 0; i < _stars.Length; i++)
                        {
                            var starTransform = _starTransforms[i];
                            starTransform.position = _slideEndPositions[i];
                        }
                        State = NoteStatus.Arrived;
                        goto case NoteStatus.Arrived;
                    }
                    var process = ((Length - GetRemainingTimeWithoutOffset()) / Length).Clamp(0, 1);

                    for (var i = 0; i < _stars.Length; i++)
                    {
                        var starTransform = _starTransforms[i];
                        var a = _slideEndPositions[i];
                        var b = _slideStartPositions[i];
                        var ba = a - b;
                        var newPos = ba * process + b;

                        starTransform.position = newPos; //TODO add some runhua
                    }
                    Autoplay();
                    break;
                case NoteStatus.Arrived:
                    Autoplay();
                    break;
            }
        }
        protected override void Autoplay()
        {
            if (!IsAutoplay)
                return;
            var process = ((Length - GetRemainingTimeWithoutOffset()) / Length).Clamp(0, 1);
            var queueMemory = _judgeQueues[0];
            var queue = queueMemory.Span;
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
            else if (process > 0)
                PlaySFX();
            var areaIndex = (int)(process * queueMemory.Length) - 1;
            if (areaIndex < 0)
                return;
            var barIndex = queue[areaIndex].ArrowProgressWhenFinished;
            HideBar(barIndex);
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
            //foreach (var sensor in ArrayHelper.ToEnumerable(_judgeAreas))
            //    _ioManager.UnbindSensor(_noteChecker, sensor);
            State = NoteStatus.End;
            base.End();
            if (forceEnd)
            {
                Destroy(_slideOK);
                Destroy(gameObject);
                return;
            }
            ConvertJudgeGrade(ref _judgeResult);
            JudgeResultCorrection(ref _judgeResult);
            var result = new JudgeResult()
            {
                Grade = _judgeResult,
                Diff = _judgeDiff,
                IsEX = IsEX,
                IsBreak = IsBreak
            };

            _objectCounter.ReportResult(this, result);
            if(PlaySlideOK(result))
            {
                if (IsClassic)
                {
                    _slideOKAnim!.SetTrigger("classic");
                }
                else if (IsBreak && _judgeResult == JudgeGrade.Perfect)
                {
                    _slideOKAnim!.runtimeAnimatorController = MajInstances.SkinManager.JustBreak;
                }
                _slideOKController!.SetResult(_judgeResult);
            }
            PlayJudgeSFX(result);
            //PlaySlideOK(result);
            //Destroy(gameObject);
        }
        protected override void LoadSkin()
        {
            var bars = _slideBars;
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
            foreach(var (i,bar) in bars.WithIndex())
            {
                var barRenderer = bar.GetComponent<SpriteRenderer>();

                barRenderer.color = new Color(1f, 1f, 1f, 0f);
                barRenderer.sortingOrder = _sortOrder--;
                barRenderer.sortingLayerName = "Slides";

                barRenderer.sprite = barSprites[i];
                if (breakMaterial is not null)
                {
                    barRenderer.sharedMaterial = breakMaterial;
                    //var controller = bar.AddComponent<BreakShineController>();
                    //controller.Parent = this;
                }
            }
            foreach(var (i, star) in _stars.WithIndex())
            {
                var starRenderer = _starRenderers[i];
                starRenderer.sprite = starSprite;
                if (breakMaterial is not null)
                {
                    starRenderer.sharedMaterial = breakMaterial;
                }
                star!.transform.rotation = Quaternion.Euler(0, 0, -22.5f * (8 + i + 2 * (StartPos - 1)));
            }
            

            if (_isJustR)
            {
                _slideOKController!.SetR();
            }
            else
            {
                _slideOKController!.SetL();
                _slideOK!.transform.Rotate(new Vector3(0f, 0f, 180f));
            }
        }
    }
}