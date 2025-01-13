using MajdataPlay.Game.Controllers;
using MajdataPlay.IO;
using MajdataPlay.Utils;
using MajdataPlay.Types;
using System;
using UnityEngine;
using System.Threading.Tasks;
using MajdataPlay.Extensions;
using MajdataPlay.Game.Buffers;
using MajdataPlay.Game.Types;
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
                        _thisRenderer.forceRenderingOff = true;
                        _exRenderer.forceRenderingOff = true;
                        _tapLineRenderer.forceRenderingOff = true;
                        _endRenderer.forceRenderingOff = true;
                        break;
                    case RendererStatus.On:
                        _thisRenderer.forceRenderingOff = false;
                        _exRenderer.forceRenderingOff = !IsEX;
                        _tapLineRenderer.forceRenderingOff = false;
                        _endRenderer.forceRenderingOff = false;
                        break;
                }
            }
        }
        public TapQueueInfo QueueInfo { get; set; } = TapQueueInfo.Default;
        public float Distance { get; private set; } = -100;
        bool _holdAnimStart;

        [SerializeField]
        GameObject _tapLinePrefab;

        Sprite _holdSprite;
        Sprite _holdOnSprite;
        Sprite _holdOffSprite;

        GameObject _exObject;
        GameObject _endObject;
        GameObject _tapLineObject;

        Transform _exTransform;
        Transform _endTransform;
        Transform _tapLineTransform;

        SpriteRenderer _exRenderer;
        SpriteRenderer _endRenderer;
        SpriteRenderer _thisRenderer;
        SpriteRenderer _tapLineRenderer;

        NotePoolManager _poolManager;

        const int _spriteSortOrder = 1;
        const int _exSortOrder = 0;
        const int _endSortOrder = 2;

        protected override void Awake()
        {
            base.Awake();
            _poolManager = FindObjectOfType<NotePoolManager>();
            var notes = _noteManager.gameObject.transform;

            _tapLineObject = Instantiate(_tapLinePrefab, notes.GetChild(7));
            _tapLineObject.SetActive(true);
            _tapLineTransform = _tapLineObject.transform;
            _tapLineRenderer = _tapLineObject.GetComponent<SpriteRenderer>();

            _exObject = Transform.GetChild(0).gameObject;
            _exTransform = _exObject.transform;
            _exRenderer = _exObject.GetComponent<SpriteRenderer>();

            _thisRenderer = GetComponent<SpriteRenderer>();

            _endObject = Transform.GetChild(1).gameObject;
            _endTransform = _endObject.transform;
            _endRenderer = _endObject.GetComponent<SpriteRenderer>();
            
            Transform.localScale = new Vector3(0, 0);

            base.SetActive(false);
            _tapLineObject.layer = MajEnv.HIDDEN_LAYER;
            _exObject.layer = MajEnv.HIDDEN_LAYER;
            _endObject.layer = MajEnv.HIDDEN_LAYER;
            Active = false;
        }
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
                        _judgeResult = (JudgeGrade)autoplayParam;
                    else
                        _judgeResult = (JudgeGrade)_randomizer.Next(0, 15);
                    ConvertJudgeGrade(ref _judgeResult);
                    _isJudged = true;
                    _judgeDiff = _judgeResult switch
                    {
                        < JudgeGrade.Perfect => 1,
                        > JudgeGrade.Perfect => -1,
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
            _holdAnimStart = false;
            _playerIdleTime = 0;
            _judgableRange = new(JudgeTiming - 0.15f, JudgeTiming + 0.15f, ContainsType.Closed);

            Transform.rotation = Quaternion.Euler(0, 0, -22.5f + -45f * (StartPos - 1));
            Transform.localScale = new Vector3(0, 0);

            _tapLineTransform.rotation = Quaternion.Euler(0, 0, -22.5f + -45f * (StartPos - 1));
            _thisRenderer.size = new Vector2(1.22f, 1.4f);
            _exRenderer.size = new Vector2(1.22f, 1.4f);
            _thisRenderer.sortingOrder = SortOrder - _spriteSortOrder;
            _exRenderer.sortingOrder = SortOrder - _exSortOrder;
            _endRenderer.sortingOrder = SortOrder - _endSortOrder;

            LoadSkin();
            SetActive(true);
            SetTapLineActive(false);
            SetEndActive(false);
            if (_gpManager.IsAutoplay)
                Autoplay();
            else
                SubscribeEvent();

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
                _judgeResult = EndJudge_Classic(_judgeResult);
            else
                _judgeResult = EndJudge(_judgeResult);
            ConvertJudgeGrade(ref _judgeResult);

            var result = new JudgeResult()
            {
                Grade = _judgeResult,
                IsBreak = IsBreak,
                IsEX = IsEX,
                Diff = _judgeDiff
            };
            PlayJudgeSFX(new JudgeResult()
            {
                Grade = _judgeResult,
                IsBreak = false,
                IsEX = false,
                Diff = _judgeDiff
            });
            _holdAnimStart = false;
            _thisRenderer.sharedMaterial = DefaultMaterial;
            SetActive(false);
            RendererState = RendererStatus.Off;
            _effectManager.ResetHoldEffect(StartPos);
            _effectManager.PlayEffect(StartPos, result);
            _objectCounter.ReportResult(this, result);
            _poolManager.Collect(this);
        }

        void Check()
        {
            if (_isJudged)
                return;
            else if (!_noteManager.CanJudge(QueueInfo))
                return;

            var timing = GetTimeSpanToJudgeTiming();
            var isTooLate = timing > 0.15f;
            
            if (_judgableRange.InRange(_gpManager.ThisFrameSec))
            {
                var btnState = _noteManager.GetButtonStateInThisFrame(_sensorPos);
                var sensorState = _noteManager.GetSensorStateInThisFrame(_sensorPos);

                Check(btnState, ref _noteManager.IsButtonUsedInThisFrame(_sensorPos));
                Check(sensorState, ref _noteManager.IsSensorUsedInThisFrame(_sensorPos));
            }
            else if (isTooLate)
            {
                _judgeResult = JudgeGrade.Miss;
                _isJudged = true;
                _judgeDiff = 150;
            }

            if (_isJudged)
            {
                _noteManager.NextNote(QueueInfo);
            }
        }
        void Check(in InputEventArgs args, ref bool isUsedInThisFrame)
        {
            if (_isJudged)
                return;
            else if (!args.IsClick)
                return;
            else if (isUsedInThisFrame)
                return;

            var thisFrameSec = _gpManager.ThisFrameSec;
            isUsedInThisFrame = true;

            Judge(thisFrameSec);
        }
        void BodyCheck()
        {
            if (!_isJudged)
                return;

            var timing = GetTimeSpanToJudgeTiming();
            var endTiming = timing - Length;
            var remainingTime = GetRemainingTime();

            if (IsClassic)
            {
                if (_gpManager.IsAutoplay && remainingTime == 0)
                {
                    End();
                    return;
                }
                if (endTiming >= 0.333334f || _judgeResult.IsMissOrTooFast())
                {
                    End();
                    return;
                }
            }
            else if (remainingTime == 0)
            {
                End();
                return;
            }


            if (!IsClassic)
            {
                if (timing <= 0.1f) // 忽略头部6帧
                    return;
                else if (remainingTime <= 0.2f) // 忽略尾部12帧
                    return;
            }

            if (!_gpManager.IsStart) // 忽略暂停
                return;

            var on = _noteManager.CheckAreaStateInThisFrame(_sensorPos, SensorStatus.On);
            if (on || _gpManager.IsAutoplay)
            {
                PlayHoldEffect();
            }
            else
            {
                _playerIdleTime += Time.deltaTime;
                StopHoldEffect();

                if (IsClassic)
                {
                    End();
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
                Grade = _judgeResult,
                IsBreak = IsBreak,
                IsEX = IsEX,
                Diff = _judgeDiff
            });
        }
        protected override void PlayJudgeSFX(in JudgeResult judgeResult)
        {
            _audioEffMana.PlayTapSound(judgeResult);
        }
        public override void ComponentFixedUpdate()
        {
            
        }
        public override void ComponentUpdate()
        {
            var timing = GetTimeSpanToArriveTiming();
            var distance = timing * Speed + 4.8f;
            var scaleRate = _gameSetting.Debug.NoteAppearRate;
            var destScale = distance * scaleRate + (1 - (scaleRate * 1.225f));

            var remaining = GetRemainingTimeWithoutOffset();
            var holdTime = timing - Length;
            var holdDistance = holdTime * Speed + 4.8f;

            Check();
            BodyCheck();

            switch (State)
            {
                case NoteStatus.Initialized:
                    if (destScale >= 0f)
                    {
                        //transform.rotation = Quaternion.Euler(0, 0, -22.5f + -45f * (StartPos - 1));
                        //_tapLineTransform.rotation = transform.rotation;
                        //_thisRenderer.size = new Vector2(1.22f, 1.4f);
                        _exRenderer.size = new Vector2(1.22f, 1.42f);
                        _thisRenderer.size = new Vector2(1.22f, 1.42f);
                        _tapLineTransform.localScale = new Vector3(0.2552f, 0.2552f, 1f);

                        RendererState = RendererStatus.On;

                        State = NoteStatus.Scaling;
                        goto case NoteStatus.Scaling;
                    }
                    //else
                    //{
                    //    Transform.localScale = new Vector3(0, 0);
                    //}
                    return;
                case NoteStatus.Scaling:
                    if (destScale > 0.3f)
                        SetTapLineActive(true);
                    if (distance < 1.225f)
                    {
                        Distance = distance;
                        Transform.localScale = new Vector3(destScale, destScale);
                        //_thisRenderer.size = new Vector2(1.22f, 1.42f);
                        distance = 1.225f;
                        var pos = GetPositionFromDistance(distance);
                        //_tapLineTransform.localScale = new Vector3(0.2552f, 0.2552f, 1f);
                        Transform.position = pos;
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

                        SetEndActive(true);
                        //_endRenderer.enabled = true;
                    }
                    else if (holdDistance >= 1.225f && distance < 4.8f) // 头未到达 尾出现
                    {
                        SetEndActive(true);
                        //_endRenderer.enabled = true;
                    }
                    Distance = distance;
                    var dis = (distance - holdDistance) / 2 + holdDistance;
                    var size = distance - holdDistance + 1.4f;
                    var lineScale = Mathf.Abs(distance / 4.8f);

                    lineScale = lineScale >= 1f ? 1f : lineScale;

                    Transform.position = GetPositionFromDistance(dis); //0.325
                    _tapLineTransform.localScale = new Vector3(lineScale, lineScale, 1f);
                    _thisRenderer.size = new Vector2(1.22f, size);
                    _exRenderer.size = new Vector2(1.22f, size);
                    _endTransform.localPosition = new Vector3(0f, 0.6825f - size / 2);
                    Transform.localScale = new Vector3(1f, 1f);
                    break;
                case NoteStatus.End:
                    var endTiming = timing - Length;
                    var endDistance = endTiming * Speed + 4.8f;
                    _tapLineTransform.localScale = new Vector3(1f, 1f, 1f);

                    if (IsClassic)
                    {
                        Distance = endDistance;
                        var scale = Mathf.Abs(endDistance / 4.8f);
                        Transform.position = GetPositionFromDistance(endDistance);
                        _tapLineTransform.localScale = new Vector3(scale, scale, 1f);
                    }
                    else
                    {
                        Transform.position = GetPositionFromDistance(4.8f);
                    }
                    break;
                default:
                    return;
            }

            //if (IsEX)
            //    _exRenderer.size = _thisRenderer.size;
        }
        JudgeGrade EndJudge(in JudgeGrade result)
        {
            if (!_isJudged)
                return result;

            var offset = (int)_judgeResult > 7 ? 0 : _judgeDiff;
            var realityHT = (Length - 0.3f - offset / 1000f).Clamp(0, Length - 0.3f);
            var percent = ((realityHT - _playerIdleTime) / realityHT).Clamp(0, 1);

            if (realityHT > 0)
            {
                if (percent >= 1f)
                {
                    if (result.IsMissOrTooFast())
                        return JudgeGrade.LateGood;
                    else if (MathF.Abs((int)result - 7) == 6)
                        return (int)result < 7 ? JudgeGrade.LateGreat : JudgeGrade.FastGreat;
                    else
                        return result;
                }
                else if (percent >= 0.67f)
                {
                    if (result.IsMissOrTooFast())
                        return JudgeGrade.LateGood;
                    else if (MathF.Abs((int)result - 7) == 6)
                        return (int)result < 7 ? JudgeGrade.LateGreat : JudgeGrade.FastGreat;
                    else if (result == JudgeGrade.Perfect)
                        return (int)result < 7 ? JudgeGrade.LatePerfect1 : JudgeGrade.FastPerfect1;
                }
                else if (percent >= 0.33f)
                {
                    if (MathF.Abs((int)result - 7) >= 6)
                        return (int)result < 7 ? JudgeGrade.LateGood : JudgeGrade.FastGood;
                    else
                        return (int)result < 7 ? JudgeGrade.LateGreat : JudgeGrade.FastGreat;
                }
                else if (percent >= 0.05f)
                    return (int)result < 7 ? JudgeGrade.LateGood : JudgeGrade.FastGood;
                else if (percent >= 0)
                {
                    if (result.IsMissOrTooFast())
                        return JudgeGrade.Miss;
                    else
                        return (int)result < 7 ? JudgeGrade.LateGood : JudgeGrade.FastGood;
                }
            }
            MajDebug.Log($"Hold: {MathF.Round(percent * 100, 2)}%\nTotal Len : {MathF.Round(realityHT * 1000, 2)}ms");
            return result;
        }
        JudgeGrade EndJudge_Classic(in JudgeGrade result)
        {
            if (!_isJudged)
                return result;
            else if (result.IsMissOrTooFast())
                return result;

            var releaseTiming = _gpManager.AudioTime - _gameSetting.Judge.JudgeOffset;
            var diff = (Timing + Length) - releaseTiming;
            var isFast = diff > 0;
            diff = MathF.Abs(diff);

            JudgeGrade endResult = diff switch
            {
                <= 0.05f => JudgeGrade.Perfect,
                <= 0.1f => isFast ? JudgeGrade.FastPerfect1 : JudgeGrade.LatePerfect1,
                <= 0.15f => isFast ? JudgeGrade.FastPerfect2 : JudgeGrade.LatePerfect2,
                //<= 0.150f =>    isFast ? JudgeType.FastGreat : JudgeType.LateGreat,
                //<= 0.16667f =>  isFast ? JudgeType.FastGreat1 : JudgeType.LateGreat1,
                //<= 0.183337f => isFast ? JudgeType.FastGreat2 : JudgeType.LateGreat2,
                _ => isFast ? JudgeGrade.FastGood : JudgeGrade.LateGood
            };

            var num = Math.Abs(7 - (int)result);
            var endNum = Math.Abs(7 - (int)endResult);
            if (endNum > num) // 取最差判定
                return endResult;
            else
                return result;
        }
        void PlayHoldEffect()
        {
            _effectManager.PlayHoldEffect(StartPos, _judgeResult);
            _effectManager.ResetEffect(StartPos);
            if (Length <= 0.3)
                return;
            else if (!_holdAnimStart && GetTimeSpanToArriveTiming() >= 0.1f)//忽略开头6帧与结尾12帧
            {
                _holdAnimStart = true;

                _thisRenderer.sharedMaterial = HoldShineMaterial;
                _thisRenderer.sprite = _holdOnSprite;
            }
        }
        void StopHoldEffect()
        {
            _effectManager.ResetHoldEffect(StartPos);
            _holdAnimStart = false;

            _thisRenderer.sprite = _holdOffSprite;
            _thisRenderer.sharedMaterial = DefaultMaterial;
        }
        public override void SetActive(bool state)
        {
            if (Active == state)
                return;
            base.SetActive(state);
            switch (state)
            {
                case true:
                    _exObject.layer = MajEnv.DEFAULT_LAYER;
                    break;
                case false:
                    _exObject.layer = MajEnv.HIDDEN_LAYER;
                    break;
            }
            SetTapLineActive(state);
            SetEndActive(state);
            Active = state;
        }
        void SetTapLineActive(bool state)
        {
            switch (state)
            {
                case true:
                    _tapLineObject.layer = MajEnv.DEFAULT_LAYER;
                    break;
                case false:
                    _tapLineObject.layer = MajEnv.HIDDEN_LAYER;
                    break;
            }
        }
        void SetEndActive(bool state)
        {
            switch(state)
            {
                case true:
                    _endObject.layer = MajEnv.DEFAULT_LAYER;
                    break;
                case false:
                    _endObject.layer = MajEnv.HIDDEN_LAYER;
                    break;
            }
        }
        protected override void LoadSkin()
        {
            var skin = MajInstances.SkinManager.GetHoldSkin();
            //var _thisRenderer = GetComponent<SpriteRenderer>();
            //var _exRenderer = transform.GetChild(0).GetComponent<SpriteRenderer>();
            //var _tapLineRenderer = tapLine.GetComponent<SpriteRenderer>();

            _holdSprite = skin.Normal;
            _holdOnSprite = skin.Normal_On;
            _holdOffSprite = skin.Off;

            _exRenderer.sprite = skin.Ex;
            _exRenderer.color = skin.ExEffects[0];
            _endRenderer.sprite = skin.Ends[0];
            _thisRenderer.sharedMaterial = DefaultMaterial;
            _tapLineRenderer.sprite = skin.NoteLines[0];

            if (IsEach)
            {
                _holdSprite = skin.Each;
                _holdOnSprite = skin.Each_On;
                _endRenderer.sprite = skin.Ends[1];
                _tapLineRenderer.sprite = skin.NoteLines[1];
                _exRenderer.color = skin.ExEffects[1];
            }

            if (IsBreak)
            {
                _holdSprite = skin.Break;
                _holdOnSprite = skin.Break_On;
                _endRenderer.sprite = skin.Ends[2];
                _thisRenderer.sharedMaterial = BreakMaterial;
                _tapLineRenderer.sprite = skin.NoteLines[2];
                _exRenderer.color = skin.ExEffects[2];
            }

            RendererState = RendererStatus.Off;
            //_endRenderer.enabled = false;
            _thisRenderer.sprite = _holdSprite;
        }
        void SubscribeEvent()
        {
            //_ioManager.BindArea(_noteChecker, _sensorPos);
        }
        void UnsubscribeEvent()
        {
           // _ioManager.UnbindArea(_noteChecker, _sensorPos);
        }
        RendererStatus _rendererState = RendererStatus.Off;
    }
}