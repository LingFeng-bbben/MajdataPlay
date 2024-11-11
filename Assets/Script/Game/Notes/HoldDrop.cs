using MajdataPlay.Game.Controllers;
using MajdataPlay.Interfaces;
using MajdataPlay.IO;
using MajdataPlay.Utils;
using MajdataPlay.Types;
using System;
using UnityEngine;
using MajdataPlay.Buffers;
using System.Threading.Tasks;
using MajdataPlay.Extensions;
#nullable enable
namespace MajdataPlay.Game.Notes
{
    public sealed class HoldDrop : NoteLongDrop, IDistanceProvider , INoteQueueMember<TapQueueInfo>, IPoolableNote<HoldPoolingInfo,TapQueueInfo>, IRendererContainer
    {
        public RendererStatus RendererState
        {
            get => _rendererState;
            set
            {
                if (State < NoteStatus.Initialized)
                    return;

                switch (value)
                {
                    case RendererStatus.Off:
                        thisRenderer.forceRenderingOff = true;
                        exRenderer.forceRenderingOff = true;
                        tapLineRenderer.forceRenderingOff = true;
                        endRenderer.forceRenderingOff = true;
                        break;
                    case RendererStatus.On:
                        thisRenderer.forceRenderingOff = false;
                        exRenderer.forceRenderingOff = !IsEX;
                        tapLineRenderer.forceRenderingOff = false;
                        endRenderer.forceRenderingOff = false;
                        break;
                }
            }
        }
        public TapQueueInfo QueueInfo { get; set; } = TapQueueInfo.Default;
        public float Distance { get; private set; } = -100;
        bool holdAnimStart;

        public GameObject tapLine;

        Sprite holdSprite;
        Sprite holdOnSprite;
        Sprite holdOffSprite;

        Animator shineAnimator;

        SpriteRenderer exRenderer;
        SpriteRenderer endRenderer;
        SpriteRenderer thisRenderer;
        SpriteRenderer tapLineRenderer;

        NotePoolManager poolManager;
        BreakShineController? breakShineController = null;

        const int _spriteSortOrder = 1;
        const int _exSortOrder = 0;
        const int _endSortOrder = 2;

        protected override async void Autoplay()
        {
            while (!_isJudged)
            {
                if (_gpManager is null)
                    return;
                else if (GetTimeSpanToJudgeTiming() >= 0)
                {
                    var autoplayParam = _gpManager.AutoplayParam;
                    if (autoplayParam.InRange(0, 14))
                        _judgeResult = (JudgeType)autoplayParam;
                    else
                        _judgeResult = (JudgeType)_randomizer.Next(0, 15);
                    ConvertJudgeResult(ref _judgeResult);
                    _isJudged = true;
                    _judgeDiff = _judgeResult switch
                    {
                        < JudgeType.Perfect => 1,
                        > JudgeType.Perfect => -1,
                        _ => 0
                    };
                    PlaySFX();
                    PlayHoldEffect();
                }
                await Task.Delay(1);
            }
        }
        public void Initialize(HoldPoolingInfo poolingInfo)
        {
            if (State >= NoteStatus.Initialized && State < NoteStatus.Destroyed)
                return;
            StartPos = poolingInfo.StartPos;
            Timing = poolingInfo.Timing;
            _judgeTiming = Timing;
            SortOrder = poolingInfo.NoteSortOrder;
            Speed = poolingInfo.Speed;
            IsEach = poolingInfo.IsEach;
            IsBreak = poolingInfo.IsBreak;
            IsEX = poolingInfo.IsEX;
            QueueInfo = poolingInfo.QueueInfo;
            _isJudged = false;
            Distance = -100;
            Length = poolingInfo.LastFor;
            _sensorPos = (SensorType)(StartPos - 1);
            if (State == NoteStatus.Start)
                Start();
            else
                LoadSkin();

            if(_gpManager.IsAutoplay)
                Autoplay();
            else
                SubscribeEvent();

            thisRenderer.sortingOrder = SortOrder - _spriteSortOrder;
            exRenderer.sortingOrder = SortOrder - _exSortOrder;
            endRenderer.sortingOrder = SortOrder - _endSortOrder;

            State = NoteStatus.Initialized;
        }
        public void End(bool forceEnd = false)
        {
            State = NoteStatus.Destroyed;
            UnsubscribeEvent();
            if (forceEnd)
                return;
            else if (!_isJudged)
            {
                _noteManager.NextNote(QueueInfo);
                return;
            }
            
            if (IsClassic)
                EndJudge_Classic(ref _judgeResult);
            else
                EndJudge(ref _judgeResult);
            ConvertJudgeResult(ref _judgeResult);

            var result = new JudgeResult()
            {
                Result = _judgeResult,
                IsBreak = IsBreak,
                IsEX = IsEX,
                Diff = _judgeDiff
            };
            PlayJudgeSFX(result);
            RendererState = RendererStatus.Off;
            _effectManager.ResetHoldEffect(StartPos);
            _effectManager.PlayEffect(StartPos, result);
            _objectCounter.ReportResult(this, result);
            poolManager.Collect(this);
        }
        protected override void Start()
        {
            if (IsInitialized)
                return;
            base.Start();
            var notes = _noteManager.gameObject.transform;

            tapLine = Instantiate(tapLine, notes.GetChild(7));
            tapLine.SetActive(false);

            tapLineRenderer = tapLine.GetComponent<SpriteRenderer>();
            exRenderer = transform.GetChild(0).GetComponent<SpriteRenderer>();
            thisRenderer = GetComponent<SpriteRenderer>();
            endRenderer = transform.GetChild(1).GetComponent<SpriteRenderer>();
            shineAnimator = gameObject.GetComponent<Animator>();
            poolManager = FindObjectOfType<NotePoolManager>();

            LoadSkin();

            _sensorPos = (SensorType)(StartPos - 1);
            transform.localScale = new Vector3(0, 0);

            State = NoteStatus.Initialized;
        }
        private void FixedUpdate()
        {
            if (State < NoteStatus.Running || IsDestroyed)
                return;

            var timing = GetTimeSpanToJudgeTiming();
            var endTiming = timing - Length;
            var remainingTime = GetRemainingTime();
            var isTooLate = timing > 0.15f;

            if (_isJudged) // Hold完成后Destroy
            {
                if(IsClassic)
                {
                    if (endTiming >= 0.333334f || _judgeResult.IsMissOrTooFast())
                    {
                        End();
                        return;
                    }
                }
                else if(remainingTime == 0)
                {
                    End();
                    return;
                }
                
            }
            

            if (_isJudged) // 头部判定完成后开始累计按压时长
            {
                if(!IsClassic)
                {
                    if (timing <= 0.1f) // 忽略头部6帧
                        return;
                    else if (remainingTime <= 0.2f) // 忽略尾部12帧
                        return;
                }

                if (!_gpManager.IsStart) // 忽略暂停
                    return;

                var on = _ioManager.CheckAreaStatus(_sensorPos, SensorStatus.On);
                if (on || _gpManager.IsAutoplay)
                {
                    if (remainingTime == 0)
                    {
                        _effectManager.ResetHoldEffect(StartPos);
                        if(_gpManager.IsAutoplay)
                        {
                            End();
                            return;
                        }
                    }
                    else
                        PlayHoldEffect();
                            
                }
                else
                {
                    _playerIdleTime += Time.fixedDeltaTime;
                    StopHoldEffect();

                    if (IsClassic)
                        End();
                }
            }
            else if (isTooLate) // 头部Miss
            {
                _judgeDiff = 150;
                _judgeResult = JudgeType.Miss;
                _isJudged = true;
                _noteManager.NextNote(QueueInfo);
                if (IsClassic)
                    End();
            }
        }
        protected override void Check(object sender, InputEventArgs arg)
        {
            if (State < NoteStatus.Running)
                return;
            else if (arg.Type != _sensorPos)
                return;
            else if (_isJudged || !_noteManager.CanJudge(QueueInfo))
                return;
            if (arg.IsClick)
            {
                if (!_ioManager.IsIdle(arg))
                    return;
                else
                    _ioManager.SetBusy(arg);
                Judge(_gpManager.ThisFrameSec);
                //ioManager.SetIdle(arg);
                if (_isJudged)
                {
                    _ioManager.UnbindArea(Check, _sensorPos);
                    _noteManager.NextNote(QueueInfo);
                }
            }
        }
        protected override void Judge(float currentSec)
        {
            base.Judge(currentSec);
            if (!_isJudged)
                return;
            PlaySFX();
            PlayHoldEffect();
        }
        protected override void PlaySFX()
        {
            PlayJudgeSFX(new JudgeResult()
            {
                Result = _judgeResult,
                IsBreak = IsBreak,
                IsEX = IsEX,
                Diff = _judgeDiff
            });
        }
        protected override void PlayJudgeSFX(in JudgeResult judgeResult)
        {
            _audioEffMana.PlayTapSound(judgeResult);
        }
        void Update()
        {
            var timing = GetTimeSpanToArriveTiming();
            var distance = timing * Speed + 4.8f;
            var scaleRate = _gameSetting.Debug.NoteAppearRate;
            var destScale = distance * scaleRate + (1 - (scaleRate * 1.225f));

            var remaining = GetRemainingTimeWithoutOffset();
            var holdTime = timing - Length;
            var holdDistance = holdTime * Speed + 4.8f;

            switch (State)
            {
                case NoteStatus.Initialized:
                    if (destScale >= 0f)
                    {
                        transform.rotation = Quaternion.Euler(0, 0, -22.5f + -45f * (StartPos - 1));
                        tapLine.transform.rotation = transform.rotation;
                        thisRenderer.size = new Vector2(1.22f, 1.4f);

                        RendererState = RendererStatus.On;

                        CanShine = true;
                        State = NoteStatus.Scaling;
                        goto case NoteStatus.Scaling;
                    }
                    else
                        transform.localScale = new Vector3(0, 0);
                    return;
                case NoteStatus.Scaling:
                    if (destScale > 0.3f)
                        tapLine.SetActive(true);
                    if (distance < 1.225f)
                    {
                        Distance = distance;
                        transform.localScale = new Vector3(destScale, destScale);
                        thisRenderer.size = new Vector2(1.22f, 1.42f);
                        distance = 1.225f;
                        var pos = GetPositionFromDistance(distance);
                        tapLine.transform.localScale = new Vector3(0.2552f, 0.2552f, 1f);
                        transform.position = pos;
                    }
                    else
                    {
                        State = NoteStatus.Running;
                        goto case NoteStatus.Running;
                    }
                    break;
                case NoteStatus.Running:
                    if(remaining == 0)
                    {
                        State = NoteStatus.End;
                        goto case NoteStatus.End;
                    }
                    if (holdDistance < 1.225f && distance >= 4.8f) // 头到达 尾未出现
                    {
                        holdDistance = 1.225f;
                        distance = 4.8f;
                    }
                    else if (holdDistance < 1.225f && distance < 4.8f) // 头未到达 尾未出现
                    {
                        holdDistance = 1.225f;
                    }
                    else if (holdDistance >= 1.225f && distance >= 4.8f) // 头到达 尾出现
                    {
                        distance = 4.8f;

                        endRenderer.enabled = true;
                    }
                    else if (holdDistance >= 1.225f && distance < 4.8f) // 头未到达 尾出现
                    {
                        endRenderer.enabled = true;
                    }
                    Distance = distance;
                    var dis = (distance - holdDistance) / 2 + holdDistance;
                    var size = distance - holdDistance + 1.4f;
                    var lineScale = Mathf.Abs(distance / 4.8f);

                    lineScale = lineScale >= 1f ? 1f : lineScale;

                    transform.position = GetPositionFromDistance(dis); //0.325
                    tapLine.transform.localScale = new Vector3(lineScale, lineScale, 1f);
                    thisRenderer.size = new Vector2(1.22f, size);
                    endRenderer.transform.localPosition = new Vector3(0f, 0.6825f - size / 2);
                    transform.localScale = new Vector3(1f, 1f);
                    break;
                case NoteStatus.End:
                    var endTiming = timing - Length;
                    var endDistance = endTiming * Speed + 4.8f;
                    tapLine.transform.localScale = new Vector3(1f, 1f, 1f);

                    if (IsClassic)
                    {
                        Distance = endDistance;
                        var scale = Mathf.Abs(endDistance / 4.8f);
                        transform.position = GetPositionFromDistance(endDistance);
                        tapLine.transform.localScale = new Vector3(scale, scale, 1f);
                    }
                    else
                        transform.position = GetPositionFromDistance(4.8f);
                    break;
                default:
                    return;
            }

            if (IsEX)
                exRenderer.size = thisRenderer.size;
        }
        void EndJudge(ref JudgeType result)
        {
            if (!_isJudged)
                return;

            var offset = (int)_judgeResult > 7 ? 0 : _judgeDiff;
            var realityHT = Length - 0.3f - offset / 1000f;
            var percent = MathF.Min(1, (realityHT - _playerIdleTime) / realityHT);
            result = _judgeResult;
            if (realityHT > 0)
            {
                if (percent >= 1f)
                {
                    if (_judgeResult.IsMissOrTooFast())
                        result = JudgeType.LateGood;
                    else if (MathF.Abs((int)_judgeResult - 7) == 6)
                        result = (int)_judgeResult < 7 ? JudgeType.LateGreat : JudgeType.FastGreat;
                    else
                        result = _judgeResult;
                }
                else if (percent >= 0.67f)
                {
                    if (_judgeResult.IsMissOrTooFast())
                        result = JudgeType.LateGood;
                    else if (MathF.Abs((int)_judgeResult - 7) == 6)
                        result = (int)_judgeResult < 7 ? JudgeType.LateGreat : JudgeType.FastGreat;
                    else if (_judgeResult == JudgeType.Perfect)
                        result = (int)_judgeResult < 7 ? JudgeType.LatePerfect1 : JudgeType.FastPerfect1;
                }
                else if (percent >= 0.33f)
                {
                    if (MathF.Abs((int)_judgeResult - 7) >= 6)
                        result = (int)_judgeResult < 7 ? JudgeType.LateGood : JudgeType.FastGood;
                    else
                        result = (int)_judgeResult < 7 ? JudgeType.LateGreat : JudgeType.FastGreat;
                }
                else if (percent >= 0.05f)
                    result = (int)_judgeResult < 7 ? JudgeType.LateGood : JudgeType.FastGood;
                else if (percent >= 0)
                {
                    if (_judgeResult.IsMissOrTooFast())
                        result = JudgeType.Miss;
                    else
                        result = (int)_judgeResult < 7 ? JudgeType.LateGood : JudgeType.FastGood;
                }
            }
            print($"Hold: {MathF.Round(percent * 100, 2)}%\nTotal Len : {MathF.Round(realityHT * 1000, 2)}ms");
        }
        void EndJudge_Classic(ref JudgeType result)
        {
            if (!_isJudged)
                return;
            else if (result.IsMissOrTooFast())
                return;

            var releaseTiming = _gpManager.AudioTime;
            var diff = (Timing + Length) - releaseTiming;
            var isFast = diff > 0;
            diff = MathF.Abs(diff);

            JudgeType endResult = diff switch
            {
                <= 0.044445f => JudgeType.Perfect,
                <= 0.088889f => isFast ? JudgeType.FastPerfect1 : JudgeType.LatePerfect1,
                <= 0.133336f => isFast ? JudgeType.FastPerfect2 : JudgeType.LatePerfect2,
                <= 0.150f =>    isFast ? JudgeType.FastGreat : JudgeType.LateGreat,
                <= 0.16667f =>  isFast ? JudgeType.FastGreat1 : JudgeType.LateGreat1,
                <= 0.183337f => isFast ? JudgeType.FastGreat2 : JudgeType.LateGreat2,
                _ => isFast ? JudgeType.FastGood : JudgeType.LateGood
            };

            var num = Math.Abs(7 - (int)result);
            var endNum = Math.Abs(7 - (int)endResult);
            if (endNum > num) // 取最差判定
                result = endResult;
        }
        void PlayHoldEffect()
        {
            _effectManager.PlayHoldEffect(StartPos, _judgeResult);
            _effectManager.ResetEffect(StartPos);
            if (Length <= 0.3)
                return;
            else if (!holdAnimStart && GetTimeSpanToArriveTiming() >= 0.1f)//忽略开头6帧与结尾12帧
            {
                holdAnimStart = true;
                shineAnimator.enabled = true;
                
                if(breakShineController is not null && breakShineController.enabled)
                {
                    breakShineController.enabled = false;
                    thisRenderer.material.SetFloat("_Brightness", 1);
                    thisRenderer.material.SetFloat("_Contrast", 1);
                }
                thisRenderer.sprite = holdOnSprite;
            }
        }
        void StopHoldEffect()
        {
            _effectManager.ResetHoldEffect(StartPos);
            holdAnimStart = false;
            shineAnimator.enabled = false;
            var sprRenderer = GetComponent<SpriteRenderer>();
            sprRenderer.sprite = holdOffSprite;
            if (breakShineController is not null && breakShineController.enabled)
            {
                breakShineController.enabled = false;
                thisRenderer.material.SetFloat("_Brightness", 1);
                thisRenderer.material.SetFloat("_Contrast", 1);
            }
        }

        protected override void LoadSkin()
        {
            var skin = MajInstances.SkinManager.GetHoldSkin();
            var renderer = GetComponent<SpriteRenderer>();
            var exRenderer = transform.GetChild(0).GetComponent<SpriteRenderer>();
            var tapLineRenderer = tapLine.GetComponent<SpriteRenderer>();
            if(breakShineController is null)
                breakShineController = gameObject.AddComponent<BreakShineController>();

            holdSprite = skin.Normal;
            holdOnSprite = skin.Normal_On;
            holdOffSprite = skin.Off;

            exRenderer.sprite = skin.Ex;
            exRenderer.color = skin.ExEffects[0];
            endRenderer.sprite = skin.Ends[0];
            breakShineController.enabled = false;
            renderer.material = skin.DefaultMaterial;
            tapLineRenderer.sprite = skin.NoteLines[0];

            if (IsEach)
            {
                holdSprite = skin.Each;
                holdOnSprite = skin.Each_On;
                endRenderer.sprite = skin.Ends[1];
                tapLineRenderer.sprite = skin.NoteLines[1];
                exRenderer.color = skin.ExEffects[1];
            }

            if (IsBreak)
            {
                holdSprite = skin.Break;
                holdOnSprite = skin.Break_On;
                endRenderer.sprite = skin.Ends[2];
                renderer.material = skin.BreakMaterial;
                tapLineRenderer.sprite = skin.NoteLines[2];
                breakShineController.enabled = true;
                breakShineController.Parent = this;
                exRenderer.color = skin.ExEffects[2];
            }

            RendererState = RendererStatus.Off;
            endRenderer.enabled = false;
            renderer.sprite = holdSprite;
        }
        void SubscribeEvent()
        {
            _ioManager.BindArea(Check, _sensorPos);
        }
        void UnsubscribeEvent()
        {
            _ioManager.UnbindArea(Check, _sensorPos);
        }
        RendererStatus _rendererState = RendererStatus.Off;
    }
}