using MajdataPlay.IO;
using MajdataPlay.Utils;
using System;
using UnityEngine;
using System.Threading.Tasks;
using MajdataPlay.Scenes.Game.Buffers;
using System.Runtime.CompilerServices;
using MajdataPlay.Scenes.Game.Utils;
using MajdataPlay.Scenes.Game.Notes.Controllers;
using MajdataPlay.Numerics;
using MajdataPlay.Buffers;
using MajdataPlay.Settings;

#nullable enable
namespace MajdataPlay.Scenes.Game.Notes.Behaviours
{
    using Unsafe = System.Runtime.CompilerServices.Unsafe;
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
        int _lastHoldState = HOLD_STATE_HEAD_MISS_OR_NOT_JUDGED;
        float _releaseTime = 0;
        ButtonZone? _buttonPos;
        Range<float> _bodyCheckRange;

        readonly float _noteAppearRate = MajInstances.Settings?.Debug.NoteAppearRate ?? 0.265f;
        //readonly float _touchPanelOffset = MajEnv.UserSetting?.Judge.TouchPanelOffset ?? 0;

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

            //if (!IsAutoplay)
            //    _noteManager.OnGameIOUpdate += GameIOListener;
            //_noteChecker = new(Check);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void Autoplay()
        {
            switch (AutoplayMode)
            {
                case AutoplayModeOption.Enable:
                    if (!IsAutoplay)
                    {
                        return;
                    }
                    else if(_isJudged)
                    {
                        if (GetRemainingTime() == 0)
                        {
                            End();
                        }
                        return;
                    }
                    if (GetTimeSpanToJudgeTiming() >= -0.016667f)
                    {
                        var autoplayGrade = AutoplayGrade;
                        if (((int)autoplayGrade).InRange(0, 14))
                        {
                            _judgeResult = autoplayGrade;
                        }
                        else
                        {
                            _judgeResult = (JudgeGrade)_randomizer.Next(0, 15);
                        }
                        ConvertJudgeGrade(ref _judgeResult);
                        _isJudged = true;
                        _judgeDiff = _judgeResult switch
                        {
                            < JudgeGrade.Perfect => 1,
                            > JudgeGrade.Perfect => -1,
                            _ => 0
                        };
                        PlaySFX();
                        _lastHoldState = HOLD_STATE_HEAD_JUDGED_AND_NOT_FEEDBACK;
                    }
                    break;
                case AutoplayModeOption.DJAuto_TouchPanel_First:
                case AutoplayModeOption.DJAuto_ButtonRing_First:
                    DJAutoplay();
                    break;
            }
        }
        void DJAutoplay()
        {
            var isBtnFirst = AutoplayMode == AutoplayModeOption.DJAuto_ButtonRing_First;
            if (!IsAutoplay || IsEnded)
            {
                return;
            }
            else if (_isJudged)
            {
                var remainingTime = GetRemainingTime();
                if(remainingTime <= 2 * FRAME_LENGTH_SEC)
                {
                    return;
                }
                if (isBtnFirst)
                {
                    _noteManager.SimulateButtonPress(_buttonPos);
                }
                else
                {
                    _noteManager.SimulateSensorPress(_sensorPos);
                }
                return;
            }
            else if (!_noteManager.IsCurrentNoteJudgeable(QueueInfo))
            {
                return;
            }
            else if (GetTimeSpanToArriveTiming() < (-FRAME_LENGTH_SEC * 2 + FRAME_LENGTH_SEC / 2))
            {
                return;
            }

            if (isBtnFirst)
            {
                _ = _noteManager.SimulateButtonClick(_buttonPos) ||
                    (USERSETTING_DJAUTO_POLICY == DJAutoPolicyOption.Permissive && _noteManager.SimulateSensorClick(_sensorPos));
            }
            else
            {
                _ = _noteManager.SimulateSensorClick(_sensorPos) ||
                    (USERSETTING_DJAUTO_POLICY == DJAutoPolicyOption.Permissive &&  _noteManager.SimulateButtonClick(_buttonPos));
            }
        }
        public void Initialize(HoldPoolingInfo poolingInfo)
        {
            if (State >= NoteStatus.Initialized && State < NoteStatus.End)
            {
                return;
            }
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
            _buttonPos = _sensorPos.ToButtonZone();
            _playerReleaseTimeSec = 0;
            _judgeResult = JudgeGrade.Miss;
            _judgableRange = new(JudgeTiming - 0.15f, JudgeTiming + 0.15f, ContainsType.Closed);
            _lastHoldState = HOLD_STATE_HEAD_MISS_OR_NOT_JUDGED;
            _releaseTime = 0;

            if(IsClassic)
            {
                _bodyCheckRange = new Range<float>(JudgeTiming - TAP_JUDGE_GOOD_AREA_MSEC / 1000, float.MaxValue, ContainsType.Closed);
            }
            else if (Length <= HOLD_HEAD_IGNORE_LENGTH_SEC + HOLD_TAIL_IGNORE_LENGTH_SEC)
            {
                _bodyCheckRange = DEFAULT_HOLD_BODY_CHECK_RANGE;
            }
            else
            {
                _bodyCheckRange = new Range<float>(JudgeTiming + HOLD_HEAD_IGNORE_LENGTH_SEC, JudgeTiming + Length - HOLD_TAIL_IGNORE_LENGTH_SEC, ContainsType.Closed);
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
            {
                return;
            }

            State = NoteStatus.End;

            if (IsClassic)
            {
                _judgeResult = HoldClassicEndJudge(_judgeResult, endJudgeOffset);
            }
            else
            {
                _judgeResult = HoldEndJudge(_judgeResult, HOLD_HEAD_IGNORE_LENGTH_SEC + HOLD_TAIL_IGNORE_LENGTH_SEC);
            }
            ConvertJudgeGrade(ref _judgeResult);

            var result = new NoteJudgeResult()
            {
                Grade = _judgeResult,
                IsBreak = IsBreak,
                IsEX = IsEX,
                Diff = _judgeDiff
            };
            PlayJudgeSFX(new NoteJudgeResult()
            {
                Grade = _judgeResult,
                IsBreak = false,
                IsEX = false,
                Diff = _judgeDiff
            });
            _lastHoldState = HOLD_STATE_HEAD_MISS_OR_NOT_JUDGED;
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
            {
                return;
            }
            _lastHoldState = HOLD_STATE_HEAD_JUDGED_AND_NOT_FEEDBACK;
        }
        protected override void PlaySFX()
        {
            PlayJudgeSFX(new NoteJudgeResult()
            {
                Grade = _judgeResult,
                IsBreak = IsBreak,
                IsEX = IsEX,
                Diff = _judgeDiff
            });
        }
        protected override void PlayJudgeSFX(in NoteJudgeResult judgeResult)
        {
            _audioEffMana.PlayTapSound(judgeResult);
        }
        [OnPreUpdate]
        void OnPreUpdate()
        {
            TooLateCheck();
            Check();
            BodyCheck();
            ForceEndCheck();
            Autoplay();
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
                        Transform.localScale = new Vector3(destScale, destScale) * USERSETTING_HOLD_SCALE;
                    }
                    else
                    {
                        Transform.localScale = new Vector3(1f, 1f) * USERSETTING_HOLD_SCALE;
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
                    var size = (distance - holdDistance + 1.4f * USERSETTING_HOLD_SCALE) / USERSETTING_HOLD_SCALE;
                    var lineScale = Mathf.Abs(distance / 4.8f);

                    lineScale = lineScale >= 1f ? 1f : lineScale;

                    Transform.position = _outerPos * (dis / 4.8f); //0.325
                    _tapLineTransform.localScale = new Vector3(lineScale, lineScale, 1f);
                    _thisRenderer.size = new Vector2(1.22f, size);
                    _exRenderer.size = new Vector2(1.22f, size);
                    _endTransform.localPosition = new Vector3(0f, 0.6825f - size / 2);
                    
                    break;
                case NoteStatus.Arrived:
                    var endTiming = timing - Length;
                    var endDistance = endTiming * Speed + 4.8f;
                    var ratio = endDistance / 4.8f;
                    var scale = Mathf.Abs(ratio);
                    _tapLineTransform.localScale = new Vector3(1f, 1f, 1f);
                    Distance = endDistance;
                    Transform.position = _outerPos * ratio;
                    _tapLineTransform.localScale = new Vector3(scale, scale, 1f);
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
            if (IsEnded || _isJudged || AutoplayMode == AutoplayModeOption.Enable)
            {
                return;
            }

            var timing = GetTimeSpanToJudgeTiming();
            var isTooLate = timing > TAP_JUDGE_GOOD_AREA_MSEC / 1000;

            if (isTooLate)
            {
                _judgeResult = JudgeGrade.Miss;
                _isJudged = true;
                _judgeDiff = 150;
                _lastHoldState = HOLD_STATE_HEAD_MISS_OR_NOT_JUDGED;
                _noteManager.NextNote(QueueInfo);
                _releaseTime = 114514;
                if (USERSETTING_DISPLAY_HOLD_HEAD_JUDGE_RESULT)
                {
                    _effectManager.PlayEffect(StartPos, new NoteJudgeResult()
                    {
                        Grade = _judgeResult,
                        IsBreak = IsBreak,
                        IsEX = IsEX,
                        Diff = _judgeDiff
                    });
                }
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
#if UNITY_ANDROID || UNITY_IOS
            if (_noteManager.IsSensorClickedInThisFrame(_sensorPos) && _noteManager.TryUseSensorClickEvent(_sensorPos))
            {
                Judge(ThisFrameSec - USERSETTING_TOUCHPANEL_OFFSET_SEC);
            }
            else
            {
                return;
            }
#else
            if (_noteManager.IsButtonClickedInThisFrame(_buttonPos) && _noteManager.TryUseButtonClickEvent(_buttonPos))
            {
                Judge(ThisFrameSec);
            }
            else if (_noteManager.IsSensorClickedInThisFrame(_sensorPos) && _noteManager.TryUseSensorClickEvent(_sensorPos))
            {
                Judge(ThisFrameSec - USERSETTING_TOUCHPANEL_OFFSET_SEC);
            }
            else
            {
                return;
            }
#endif
            if (_isJudged)
            {
                PlaySFX();
                if (USERSETTING_DISPLAY_HOLD_HEAD_JUDGE_RESULT)
                {
                    _effectManager.PlayEffect(StartPos, new NoteJudgeResult()
                    {
                        Grade = _judgeResult,
                        IsBreak = IsBreak,
                        IsEX = IsEX,
                        Diff = _judgeDiff
                    });
                }
                _effectManager.PlayHoldEffect(StartPos, _judgeResult);
                _effectManager.ResetEffect(StartPos);
                _noteManager.NextNote(QueueInfo);
            }
        }
        void BodyCheck()
        {
            if (!IsInitialized || IsEnded)
            {
                return;
            }

            if (_lastHoldState is HOLD_STATE_HEAD_JUDGED or HOLD_STATE_PRESSED)
            {
                _effectManager.ResetEffect(StartPos);
            }

            
            if (!_bodyCheckRange.InRange(ThisFrameSec) || !NoteController.IsStart)
            {
                if(_lastHoldState == HOLD_STATE_HEAD_JUDGED_AND_NOT_FEEDBACK && GetRemainingTime() < Length)
                {
                    _effectManager.PlayHoldEffect(StartPos, _judgeResult);
                    _effectManager.ResetEffect(StartPos);
                    _lastHoldState = HOLD_STATE_HEAD_JUDGED;
                }
                return;
            }
            var isButtonPressed = _noteManager.CheckButtonStatusInThisFrame(_buttonPos, SwitchStatus.On);
            var isSensorPressed = _noteManager.CheckSensorStatusInThisFrame(_sensorPos, SwitchStatus.On);
            var isPressed = isButtonPressed || isSensorPressed;

            if (isPressed || AutoplayMode == AutoplayModeOption.Enable)
            {
                PlayHoldEffect();
                _releaseTime = 0;
                _lastHoldState = HOLD_STATE_PRESSED;
            }
            else
            {
                if (IsClassic)
                {
                    if(!_isJudged)
                    {
                        return;
                    }
                    var isButtonReleased = _noteManager.CheckSensorStatusInPreviousFrame(_sensorPos, SwitchStatus.On) && 
                                           !isButtonPressed;
                    var offset = isButtonReleased ? 0 : USERSETTING_TOUCHPANEL_OFFSET_SEC;
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
                _lastHoldState = HOLD_STATE_RELEASED;
            }
        }
        void ForceEndCheck()
        {
            if (!_isJudged || IsEnded)
            {
                return;
            }

            var timing = GetTimeSpanToJudgeTiming();
            var endTiming = timing - Length;
            var remainingTime = GetRemainingTime();

            if (IsClassic)
            {
                if (AutoplayMode == AutoplayModeOption.Enable && remainingTime == 0)
                {
                    End();
                }
                else if (endTiming >= CLASSIC_HOLD_ALLOW_OVER_LENGTH_SEC || _judgeResult.IsMissOrTooFast())
                {
                    End();
                }
            }
            else if (remainingTime == 0)
            {
                End();
            }
        }
        void PlayHoldEffect()
        {
            if (_lastHoldState != HOLD_STATE_PRESSED)
            {
                _effectManager.PlayHoldEffect(StartPos, _judgeResult);
                _thisRenderer.sprite = _holdOnSprite;
                _thisRenderer.sharedMaterial = HoldShineMaterial;
            }
        }
        void StopHoldEffect()
        {
            if (_lastHoldState != HOLD_STATE_RELEASED)
            {
                _effectManager.ResetHoldEffect(StartPos);
                _thisRenderer.sprite = _holdOffSprite;
                _thisRenderer.sharedMaterial = DefaultMaterial;
            }
        }
        public override void SetActive(bool state)
        {
            if (Active == state)
            {
                return;
            }
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
            _tapLineRenderer.sprite = skin.GuideLines[0];

            if (IsEach)
            {
                _holdSprite = skin.Each;
                _holdOnSprite = skin.Each_On;
                _endRenderer.sprite = skin.Ends[1];
                _tapLineRenderer.sprite = skin.GuideLines[1];
                _exRenderer.color = skin.ExEffects[1];
            }

            if (IsBreak)
            {
                _holdSprite = skin.Break;
                _holdOnSprite = skin.Break_On;
                _endRenderer.sprite = skin.Ends[2];
                _thisRenderer.sharedMaterial = BreakMaterial;
                _tapLineRenderer.sprite = skin.GuideLines[2];
                _exRenderer.color = skin.ExEffects[2];
            }

            RendererState = RendererStatus.Off;
            //_endRenderer.enabled = false;
            _thisRenderer.sprite = _holdSprite;
        }

        RendererStatus _rendererState = RendererStatus.Off;
    }
}