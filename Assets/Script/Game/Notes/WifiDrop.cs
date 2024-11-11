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

        readonly Vector3[] _slideEndPositions = new Vector3[3];

        readonly SpriteRenderer[] _starRenderers = new SpriteRenderer[3];

        Vector3[] _slideStartPositions = new Vector3[3];

        public override void Initialize()
        {
            if (IsInitialized)
                return;
            base.Start();
            State = NoteStatus.Initialized;
            ConnectInfo.StartTiming = Timing;
            _judgeQueues = SlideTables.GetWifiTable(StartPos);

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
            }
            else
            {
                _slideStartPositions[0] = GetPositionFromDistance(4.8f);
                _slideStartPositions[1] = GetPositionFromDistance(4.8f);
                _slideStartPositions[2] = GetPositionFromDistance(4.8f);
            }

            _slideOK = transform.GetChild(transform.childCount - 1).gameObject; //slideok is the last one

            transform.rotation = Quaternion.Euler(0f, 0f, -45f * (StartPos - 1));
            _slideBars = new GameObject[transform.childCount - 1];

            for (var i = 0; i < transform.childCount - 1; i++)
                _slideBars[i] = transform.GetChild(i).gameObject;

            LoadSkin();
        }
        protected override void Start()
        {
            
            Initialize();

            var wifiConst = 0.162870f;
            _judgeTiming = Timing + (Length * (1 - wifiConst));
            _lastWaitTime = Length * wifiConst;

            _judgeAreas = _judgeQueues.SelectMany(x => x.SelectMany(y => y.GetSensorTypes()))
                                    .GroupBy(x => x)
                                    .Select(x => x.Key);

            foreach (var sensor in _judgeAreas)
                _ioManager.BindSensor(Check, sensor);
            FadeIn().Forget();
        }
        private void FixedUpdate()
        {
            /// time      是Slide启动的时间点
            /// timeStart 是Slide完全显示但未启动
            /// LastFor   是Slide的时值
            var timing = _gpManager.AudioTime - _timing;
            var startTiming = _gpManager.AudioTime - _startTiming;
            var tooLateTiming = _timing + Length + 0.6 + MathF.Min(_gameSetting.Judge.JudgeOffset, 0);
            var isTooLate = _gpManager.AudioTime - tooLateTiming >= 0;

            if (startTiming >= -0.05f)
                _canCheck = true;

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
                if (_lastWaitTime < 0)
                    End();
                else
                    _lastWaitTime -= Time.fixedDeltaTime;
            }
        }
        protected override void Check(object sender, InputEventArgs arg) => CheckAll();
        void CheckAll()
        {
            if (IsDestroyed || !IsInitialized)
                return;
            else if (IsFinished || !_canCheck)
                return;
            else if (_isChecking)
                return;
            else if (_gpManager.IsAutoplay)
                return;
            _isChecking = true;
            for (int i = 0; i < 3; i++)
                Check(ref _judgeQueues[i]);
            _isChecking = false;
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

            if (first.On)
                PlaySFX();

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
            if(_judgeQueues.IsEmpty())
                return int.MaxValue;
            else if(IsClassic)
            {
                var isRemainingOne = _judgeQueues.All(x => x.Length <= 1);
                if (isRemainingOne)
                    return 8;
            }
            else if (_judgeQueues[1].IsEmpty())
            {
                if (_judgeQueues[0].Length <= 1 && _judgeQueues[2].Length <= 1)
                    return 9;
            }
            var nums = new int[3];
            foreach(var (i,queue) in _judgeQueues.WithIndex())
                nums[i] = queue.Length;
            var max = nums.Max();
            var index = nums.FindIndex(x => x == max);

            return _judgeQueues[index].First().SlideIndex;
        }
        void Update()
        {
            foreach (var star in _stars)
                star.SetActive(true);

            var timing = CurrentSec - _timing;
            if (timing <= 0f)
            {
                CanShine = true;
                float alpha;
                alpha = 1f - -timing / (_timing - _startTiming);
                alpha = alpha > 1f ? 1f : alpha;
                alpha = alpha < 0f ? 0f : alpha;

                for (var i = 0; i < _stars.Length; i++)
                {
                    _starRenderers[i].color = new Color(1, 1, 1, alpha);
                    _stars[i].transform.localScale = new Vector3(alpha + 0.5f, alpha + 0.5f, alpha + 0.5f);
                    _stars[i].transform.position = _slideStartPositions[i];
                }
            }
            else
                UpdateStar();
            CheckAll();
        }
        void UpdateStar()
        {
            var timing = _gpManager.AudioTime - _timing;
            var process = (Length - timing) / Length;
            process = 1f - process;

            if (process >= 1)
            {
                for (var i = 0; i < _stars.Length; i++)
                {
                    _starRenderers[i].color = Color.white;
                    _stars[i].transform.position = _slideEndPositions[i];
                    _stars[i].transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
                }
            }
            else
            {
                for (var i = 0; i < _stars.Length; i++)
                {
                    _starRenderers[i].color = Color.white;
                    _stars[i].transform.position =
                        (_slideEndPositions[i] - _slideStartPositions[i]) * process + _slideStartPositions[i]; //TODO add some runhua
                    _stars[i].transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
                }
            }
            if (_gpManager.IsAutoplay)
            {
                var queue = _judgeQueues?[0];
                if (queue is null || queue.IsEmpty())
                    return;
                else if (process >= 1)
                {
                    HideAllBar();
                    var autoplayParam = _gpManager.AutoplayParam;
                    if (autoplayParam.InRange(0, 14))
                        _judgeResult = (JudgeType)autoplayParam;
                    else
                        _judgeResult = (JudgeType)_randomizer.Next(0, 15);
                    _isJudged = true;
                    _lastWaitTime = 0;
                    _judgeDiff = _judgeResult switch
                    {
                        < JudgeType.Perfect => 1,
                        > JudgeType.Perfect => -1,
                        _ => 0
                    };
                    return;
                }
                else if (process > 0)
                    PlaySFX();
                var areaIndex = (int)(process * queue.Length) - 1;
                if (areaIndex < 0)
                    return;
                var barIndex = queue[areaIndex].SlideIndex;
                HideBar(barIndex);
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
            if (IsDestroyed)
                return;
            foreach (var sensor in _judgeAreas)
                _ioManager.UnbindSensor(Check, sensor);
            State = NoteStatus.Destroyed;
            base.End();
            if (forceEnd)
            {
                Destroy(_slideOK);
                Destroy(gameObject);
                return;
            }
            ConvertJudgeResult(ref _judgeResult);
            JudgeResultCorrection(ref _judgeResult);
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
                var anim = _slideOK.GetComponent<Animator>();
                anim.runtimeAnimatorController = MajInstances.SkinManager.JustBreak;
            }
            _slideOK.GetComponent<LoadJustSprite>().SetResult(_judgeResult);
            PlayJudgeSFX(result);
            PlaySlideOK(result);
            Destroy(gameObject);
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
                breakMaterial = skin.BreakMaterial;
                var controller = gameObject.AddComponent<BreakSlideShineController>();
                controller.Parent = this;
                controller.Initialize();
            }
            foreach(var (i,bar) in bars.WithIndex())
            {
                var barRenderer = bar.GetComponent<SpriteRenderer>();

                barRenderer.color = new Color(1f, 1f, 1f, 0f);
                barRenderer.sortingOrder = _sortOrder--;
                barRenderer.sortingLayerName = "Slides";

                barRenderer.sprite = barSprites[i];
                if (breakMaterial != null)
                {
                    barRenderer.material = breakMaterial;
                    //var controller = bar.AddComponent<BreakShineController>();
                    //controller.Parent = this;
                }
            }
            foreach(var (i, star) in _stars.WithIndex())
            {
                var starRenderer = star.GetComponent<SpriteRenderer>();
                _starRenderers[i] = starRenderer;
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
                _starRenderers[0].sortingOrder = -1;
                _starRenderers[2].sortingOrder = -1;
            }

            if (_isJustR)
                _slideOK.GetComponent<LoadJustSprite>().SetR();
            else
            {
                _slideOK.GetComponent<LoadJustSprite>().SetL();
                _slideOK.transform.Rotate(new Vector3(0f, 0f, 180f));
            }
            _slideOK.SetActive(false);
            _slideOK.transform.SetParent(transform.parent);
        }
    }
}