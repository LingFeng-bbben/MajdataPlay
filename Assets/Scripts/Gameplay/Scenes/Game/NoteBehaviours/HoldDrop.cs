﻿using MajdataPlay.IO;
using MajdataPlay.Utils;
using MajdataPlay.Types;
using System;
using UnityEngine;
using System.Threading.Tasks;
using MajdataPlay.Extensions;
using MajdataPlay.Game.Buffers;
using System.Runtime.CompilerServices;
using MajdataPlay.Game.Utils;
using MajdataPlay.Game.Notes.Controllers;
#nullable enable
namespace MajdataPlay.Game.Notes.Behaviours
{
    internal sealed class HoldDrop : NoteLongDrop, IDistanceProvider, INoteQueueMember<TapQueueInfo>, IPoolableNote<HoldPoolingInfo, TapQueueInfo>, IRendererContainer, IMajComponent
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

        Vector3 _innerPos = NoteHelper.GetTapPosition(1, 1.225f);
        Vector3 _outerPos = NoteHelper.GetTapPosition(1, 4.8f);

        // -2 => Head miss or not judged yet
        // -1 => Head judged
        // 0  => Released
        // 1  => Pressed
        int _lastHoldState = -2;
        float _releaseTime = 0;
        Range<float> _bodyCheckRange;

        readonly float _noteAppearRate = MajInstances.Setting?.Debug.NoteAppearRate ?? 0.265f;
        //readonly float _touchPanelOffset = MajEnv.UserSetting?.Judge.TouchPanelOffset ?? 0;

        const int _spriteSortOrder = 1;
        const int _exSortOrder = 0;
        const int _endSortOrder = 2;

        readonly static Range<float> DEFAULT_BODY_CHECK_RANGE = new Range<float>(float.MinValue, float.MinValue, ContainsType.Closed);


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

            //if (!IsAutoplay)
            //    _noteManager.OnGameIOUpdate += GameIOListener;
            //_noteChecker = new(Check);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void Autoplay()
        {
            if (_isJudged || !IsAutoplay)
                return;
            if (GetTimeSpanToJudgeTiming() >= -0.016667f)
            {
                var autoplayGrade = AutoplayGrade;
                if (((int)autoplayGrade).InRange(0, 14))
                    _judgeResult = autoplayGrade;
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
                _effectManager.PlayHoldEffect(StartPos, _judgeResult);
                _effectManager.ResetEffect(StartPos);
                _lastHoldState = -1;
            }
        }
        public void Initialize(HoldPoolingInfo poolingInfo)
        {
            if (State >= NoteStatus.Initialized && State < NoteStatus.End)
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
            _innerPos = NoteHelper.GetTapPosition(StartPos, 1.225f);
            _outerPos = NoteHelper.GetTapPosition(StartPos, 4.8f);
            _sensorPos = (SensorArea)(StartPos - 1);
            _playerReleaseTimeSec = 0;
            _judgableRange = new(JudgeTiming - 0.15f, JudgeTiming + 0.15f, ContainsType.Closed);
            _lastHoldState = -2;
            _releaseTime = 0;

            if (Length < HOLD_HEAD_IGNORE_LENGTH_SEC + HOLD_TAIL_IGNORE_LENGTH_SEC)
            {
                _bodyCheckRange = DEFAULT_BODY_CHECK_RANGE;
            }
            else
            {
                _bodyCheckRange = new Range<float>(Timing + HOLD_HEAD_IGNORE_LENGTH_SEC, Timing + Length - HOLD_TAIL_IGNORE_LENGTH_SEC, ContainsType.Closed);
            }

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

            State = NoteStatus.Initialized;
        }
        void End(float endJudgeOffset = 0)
        {
            if (IsEnded)
                return;
            State = NoteStatus.End;

            if (IsClassic)
                _judgeResult = HoldEndJudgeClassic(_judgeResult, endJudgeOffset);
            else
                _judgeResult = HoldEndJudge(_judgeResult, HOLD_HEAD_IGNORE_LENGTH_SEC + HOLD_TAIL_IGNORE_LENGTH_SEC);
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
            _lastHoldState = -2;
            _thisRenderer.sharedMaterial = DefaultMaterial;
            SetActive(false);
            RendererState = RendererStatus.Off;
            _effectManager.ResetHoldEffect(StartPos);
            _effectManager.PlayEffect(StartPos, result);
            _objectCounter.ReportResult(this, result);
            _poolManager.Collect(this);
        }
        protected override void Judge(float currentSec)
        {
            base.Judge(currentSec);
            if (!_isJudged)
                return;
            if (_judgeResult is JudgeGrade.Miss or JudgeGrade.TooFast)
            {
                _lastHoldState = -2;
            }
            else
            {
                _lastHoldState = -1;
            }
            PlaySFX();
            _effectManager.PlayHoldEffect(StartPos, _judgeResult);
            _effectManager.ResetEffect(StartPos);
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
        [OnPreUpdate]
        void OnPreUpdate()
        {
            Autoplay();
            TooLateCheck();
            Check();
            BodyCheck();
        }
        [OnUpdate]
        void OnUpdate()
        {
            var timing = GetTimeSpanToArriveTiming();
            var distance = timing * Speed + 4.8f;
            var scaleRate = _noteAppearRate;
            var destScale = distance * scaleRate + (1 - scaleRate * 1.225f);

            var remaining = GetRemainingTimeWithoutOffset();
            var holdTime = timing - Length;
            var holdDistance = holdTime * Speed + 4.8f;

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
                        Transform.position = _innerPos;
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
                    }
                    else
                    {
                        State = NoteStatus.Running;
                        goto case NoteStatus.Running;
                    }
                    break;
                case NoteStatus.Running:
                    if (remaining == 0)
                    {
                        State = NoteStatus.Arrived;
                        goto case NoteStatus.Arrived;
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

                    Transform.position = _outerPos * (dis / 4.8f); //0.325
                    _tapLineTransform.localScale = new Vector3(lineScale, lineScale, 1f);
                    _thisRenderer.size = new Vector2(1.22f, size);
                    _exRenderer.size = new Vector2(1.22f, size);
                    _endTransform.localPosition = new Vector3(0f, 0.6825f - size / 2);
                    Transform.localScale = new Vector3(1f, 1f);
                    break;
                case NoteStatus.Arrived:
                    var endTiming = timing - Length;
                    var endDistance = endTiming * Speed + 4.8f;
                    _tapLineTransform.localScale = new Vector3(1f, 1f, 1f);

                    if (IsClassic)
                    {
                        Distance = endDistance;
                        var ratio = endDistance / 4.8f;
                        var scale = Mathf.Abs(ratio);
                        Transform.position = _outerPos * ratio;
                        _tapLineTransform.localScale = new Vector3(scale, scale, 1f);
                    }
                    else
                    {
                        Transform.position = _outerPos;
                    }
                    break;
                default:
                    return;
            }

            //if (IsEX)
            //    _exRenderer.size = _thisRenderer.size;
        }
        void TooLateCheck()
        {
            // Too late check
            if (IsEnded || _isJudged)
                return;

            var timing = GetTimeSpanToJudgeTiming();
            var isTooLate = timing > TAP_JUDGE_GOOD_AREA_MSEC / 1000;

            if (isTooLate)
            {
                _judgeResult = JudgeGrade.Miss;
                _isJudged = true;
                _judgeDiff = 150;
                _noteManager.NextNote(QueueInfo);
            }
        }
        void Check()
        {
            if (IsEnded || !IsInitialized || _isJudged)
            {
                return;
            }
            else if (!_judgableRange.InRange(ThisFrameSec) || !_noteManager.IsCurrentNoteJudgeable(QueueInfo))
            {
                return;
            }

            ref bool isDeviceUsedInThisFrame = ref Unsafe.NullRef<bool>();
            var isButton = false;
            if (_noteManager.IsButtonClickedInThisFrame(_sensorPos))
            {
                isDeviceUsedInThisFrame = ref _noteManager.GetButtonUsageInThisFrame(_sensorPos).Target;
                isButton = true;
            }
            else if (_noteManager.IsSensorClickedInThisFrame(_sensorPos))
            {
                isDeviceUsedInThisFrame = ref _noteManager.GetSensorUsageInThisFrame(_sensorPos).Target;
            }
            else
            {
                return;
            }

            if (isDeviceUsedInThisFrame)
            {
                return;
            }
            if (isButton)
            {
                Judge(ThisFrameSec);
            }
            else
            {
                Judge(ThisFrameSec - USERSETTING_TOUCHPANEL_OFFSET);
            }

            if (_isJudged)
            {
                isDeviceUsedInThisFrame = true;
                _noteManager.NextNote(QueueInfo);
            }
        }
        void BodyCheck()
        {
            if (!_isJudged || IsEnded)
                return;

            var timing = GetTimeSpanToJudgeTiming();
            var endTiming = timing - Length;
            var remainingTime = GetRemainingTime();

            if (_lastHoldState is -1 or 1)
            {
                _effectManager.ResetEffect(StartPos);
            }

            if (IsClassic)
            {
                if (IsAutoplay && remainingTime == 0)
                {
                    End();
                    return;
                }
                if (endTiming >= CLASSIC_HOLD_ALLOW_OVER_LENGTH_SEC || _judgeResult.IsMissOrTooFast())
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
            else if (!_bodyCheckRange.InRange(ThisFrameSec) || !NoteController.IsStart)
            {
                return;
            }
            var isButtonPressed = _noteManager.CheckButtonStatusInThisFrame(_sensorPos, SensorStatus.On);
            var isSensorPressed = _noteManager.CheckSensorStatusInThisFrame(_sensorPos, SensorStatus.On);
            var isPressed = isButtonPressed || isSensorPressed;

            if (isPressed || IsAutoplay)
            {
                if (remainingTime == 0)
                {
                    _effectManager.ResetHoldEffect(StartPos);
                }
                else
                {
                    PlayHoldEffect();
                }
                _releaseTime = 0;
                _lastHoldState = 1;
            }
            else
            {
                if (IsClassic)
                {
                    var isButtonReleased = _noteManager.CheckSensorStatusInPreviousFrame(_sensorPos, SensorStatus.On) && 
                                           !isButtonPressed;
                    var offset = isButtonReleased ? 0 : USERSETTING_TOUCHPANEL_OFFSET;
                    End(offset);
                    return;
                }
                else if (_releaseTime <= DELUXE_HOLD_RELEASE_IGNORE_TIME_SEC)
                {
                    _releaseTime += MajTimeline.DeltaTime;
                    return;
                }
                _playerReleaseTimeSec += MajTimeline.DeltaTime;
                StopHoldEffect();
                _lastHoldState = 0;
            }
        }
        void PlayHoldEffect()
        {
            if (_lastHoldState != 1)
            {
                _effectManager.PlayHoldEffect(StartPos, _judgeResult);
                _thisRenderer.sprite = _holdOnSprite;
                _thisRenderer.sharedMaterial = HoldShineMaterial;
            }
        }
        void StopHoldEffect()
        {
            if (_lastHoldState != 0)
            {
                _effectManager.ResetHoldEffect(StartPos);
                _thisRenderer.sprite = _holdOffSprite;
                _thisRenderer.sharedMaterial = DefaultMaterial;
            }
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
            switch (state)
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

        RendererStatus _rendererState = RendererStatus.Off;
    }
}