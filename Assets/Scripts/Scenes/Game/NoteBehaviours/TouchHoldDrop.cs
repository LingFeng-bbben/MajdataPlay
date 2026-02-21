using MajdataPlay.Buffers;
using MajdataPlay.Extensions;
using MajdataPlay.Scenes.Game.Buffers;
using MajdataPlay.Scenes.Game.Notes.Controllers;
using MajdataPlay.Scenes.Game.Notes.Touch;
using MajdataPlay.Scenes.Game.Utils;
using MajdataPlay.IO;
using MajdataPlay.Numerics;
using MajdataPlay.Utils;
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using MajdataPlay.Settings;
using UnityEngine;
using UnityEngine.UI;
#nullable enable
namespace MajdataPlay.Scenes.Game.Notes.Behaviours
{
    using Unsafe = System.Runtime.CompilerServices.Unsafe;
    internal sealed class TouchHoldDrop : NoteLongDrop, INoteQueueMember<TouchQueueInfo>, IRendererContainer, IPoolableNote<TouchHoldPoolingInfo, TouchQueueInfo>, IMajComponent
    {
        public TouchGroup? GroupInfo { get; set; } = null;
        public TouchQueueInfo QueueInfo { get; set; } = TouchQueueInfo.Default;
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
                        foreach (var renderer in _fanRenderers)
                            renderer.forceRenderingOff = true;
                        _borderRenderer.forceRenderingOff = true;
                        _borderMask.forceRenderingOff = true;
                        break;
                    case RendererStatus.On:
                        foreach (var renderer in _fanRenderers)
                            renderer.forceRenderingOff = false;
                        _borderRenderer.forceRenderingOff = false;
                        _borderMask.forceRenderingOff = false;
                        break;
                    default:
                        return;
                }
                _rendererState = value;
            }
        }
        public char areaPosition;
        public bool isFirework;

        Sprite board_On;
        Sprite board_Off;

        readonly GameObject[] _fans = new GameObject[4];
        readonly Transform[] _fanTransforms = new Transform[4];
        readonly SpriteRenderer[] _fanRenderers = new SpriteRenderer[4];

        ButtonZone? _buttonPos;

        float displayDuration;
        float moveDuration;
        float wholeDuration;

        GameObject _pointObject;
        GameObject _borderObject;
        SpriteMask _borderMask;
        SpriteRenderer _pointRenderer;
        SpriteRenderer _borderRenderer;
        NotePoolManager _notePoolManager;
        MultTouchHandler _multTouchHandler;

        // -2 => Head miss or not judged yet
        // -1 => Head judged
        // 0  => Released
        // 1  => Pressed
        int _lastHoldState = HOLD_STATE_HEAD_MISS_OR_NOT_JUDGED;
        float _releaseTime = 0;
        Range<float> _bodyCheckRange;
        //readonly float _touchPanelOffset = MajEnv.UserSetting?.Judge.TouchPanelOffset ?? 0;

        const int _fanSpriteSortOrder = 2;
        const int _borderSortOrder = 6;
        const int _pointBorderSortOrder = 1;

        protected override void Awake()
        {
            base.Awake();
            _notePoolManager = Majdata<NotePoolManager>.Instance!;
            _multTouchHandler = Majdata<MultTouchHandler>.Instance!;

            _fanTransforms[0] = Transform.GetChild(5);
            _fanTransforms[1] = Transform.GetChild(4);
            _fanTransforms[2] = Transform.GetChild(3);
            _fanTransforms[3] = Transform.GetChild(2);

            _fans[0] = _fanTransforms[0].gameObject;
            _fans[1] = _fanTransforms[1].gameObject;
            _fans[2] = _fanTransforms[2].gameObject;
            _fans[3] = _fanTransforms[3].gameObject;

            for (var i = 0; i < 4; i++)
            {
                _fanRenderers[i] = _fans[i].GetComponent<SpriteRenderer>();
            }

            _pointObject = transform.GetChild(6).gameObject;
            _borderObject = transform.GetChild(1).gameObject;
            _pointRenderer = _pointObject.GetComponent<SpriteRenderer>();
            _borderRenderer = _borderObject.GetComponent<SpriteRenderer>();
            _borderMask = Transform.GetChild(0).GetComponent<SpriteMask>();

            _pointObject.SetActive(true);
            _borderObject.SetActive(true);

            Transform.position = new Vector3(0, 0, 0);
            SetFansColor(new Color(1f, 1f, 1f, 0f));
            SetFansPosition(0.4f);

            base.SetActive(false);
            SetFanActive(false);
            SetBorderActive(false);
            SetPointActive(false);
            Active = false;

            //if (!IsAutoplay)
            //    _noteManager.OnGameIOUpdate += GameIOListener;

            RendererState = RendererStatus.Off;
            Transform.localScale *= USERSETTING_TOUCH_SCALE;
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
                    else if (_isJudged)
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
                        PlayJudgeSFX(new NoteJudgeResult()
                        {
                            Grade = _judgeResult,
                            IsBreak = IsBreak,
                            IsEX = IsEX,
                            Diff = _judgeDiff
                        });
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
            if (!IsAutoplay || IsEnded)
            {
                return;
            }
            else if (_isJudged)
            {
                _noteManager.SimulateSensorPress(_sensorPos);
                return;
            }
            else if (!_noteManager.IsCurrentNoteJudgeable(QueueInfo))
            {
                return;
            }
            else if (GetTimeSpanToArriveTiming() < -FRAME_LENGTH_SEC)
            {
                return;
            }
            _noteManager.SimulateSensorClick(_sensorPos);
        }
        public void Initialize(TouchHoldPoolingInfo poolingInfo)
        {
            if (State >= NoteStatus.Initialized && State < NoteStatus.End)
            {
                return;
            }

            StartPos = poolingInfo.StartPos;
            areaPosition = poolingInfo.AreaPos;
            Timing = poolingInfo.Timing - TOUCH_HOLD_DISPLAY_OFFSET_SEC;
            _judgeTiming = poolingInfo.Timing;
            SortOrder = poolingInfo.NoteSortOrder;
            Speed = poolingInfo.Speed;
            IsEach = poolingInfo.IsEach;
            IsBreak = poolingInfo.IsBreak;
            IsEX = poolingInfo.IsEX;
            QueueInfo = poolingInfo.QueueInfo;
            GroupInfo = poolingInfo.GroupInfo;
            _isJudged = false;
            _lastHoldState = HOLD_STATE_HEAD_MISS_OR_NOT_JUDGED;
            Length = poolingInfo.LastFor;
            isFirework = poolingInfo.IsFirework;
            _sensorPos = poolingInfo.SensorPos;
            _judgeResult = JudgeGrade.Miss;
            if (_sensorPos < SensorArea.B1 && _sensorPos >= SensorArea.A1)
            {
                _buttonPos = _sensorPos.ToButtonZone();
            }
            else
            {
                _buttonPos = null;
            }
            _playerReleaseTimeSec = 0;
            _judgableRange = new(JudgeTiming - 0.15f, JudgeTiming + 0.316667f, ContainsType.Closed);
            _releaseTime = 0;

            if (Length <= TOUCH_HOLD_HEAD_IGNORE_LENGTH_SEC + TOUCH_HOLD_TAIL_IGNORE_LENGTH_SEC)
            {
                _bodyCheckRange = DEFAULT_HOLD_BODY_CHECK_RANGE;
            }
            else
            {
                _bodyCheckRange = new Range<float>(Timing + TOUCH_HOLD_HEAD_IGNORE_LENGTH_SEC, Timing + Length - TOUCH_HOLD_TAIL_IGNORE_LENGTH_SEC, ContainsType.Closed);
            }

            wholeDuration = 3.209385682f * Mathf.Pow(Speed, -0.9549621752f);
            moveDuration = 0.8f * wholeDuration;
            displayDuration = 0.2f * wholeDuration;

            LoadSkin();

            SetFansColor(new Color(1f, 1f, 1f, 0f));
            _borderMask.enabled = false;
            _borderMask.alphaCutoff = 0;
            SetActive(true);
            SetFanActive(false);
            SetBorderActive(false);
            SetPointActive(false);

            Transform.position = NoteHelper.GetTouchAreaPosition(_sensorPos);
            SetFansPosition(0.4f);
            RendererState = RendererStatus.Off;

            for (var i = 0; i < 4; i++)
            {
                _fanRenderers[i].sortingOrder = SortOrder - (_fanSpriteSortOrder + i);
            }
            _pointRenderer.sortingOrder = SortOrder - _pointBorderSortOrder;
            _borderRenderer.sortingOrder = SortOrder - _borderSortOrder;
            _borderMask.frontSortingOrder = SortOrder - _borderSortOrder;
            _borderMask.backSortingOrder = SortOrder - _borderSortOrder - 1;

            State = NoteStatus.Initialized;
        }
        void End()
        {
            if (IsEnded)
            {
                return;
            }

            State = NoteStatus.End;
            _multTouchHandler.Unregister(_sensorPos);
            _judgeResult = HoldEndJudge(_judgeResult, TOUCH_HOLD_HEAD_IGNORE_LENGTH_SEC + TOUCH_HOLD_TAIL_IGNORE_LENGTH_SEC);
            ConvertJudgeGrade(ref _judgeResult);
            var result = new NoteJudgeResult()
            {
                Grade = _judgeResult,
                IsBreak = IsBreak,
                IsEX = IsEX,
                Diff = _judgeDiff,
            };
            //_pointObject.SetActive(false);
            SetActive(false);
            RendererState = RendererStatus.Off;

            _objectCounter.ReportResult(this, result);
            if (!_isJudged)
            {
                _noteManager.NextTouch(QueueInfo);
            }
            if (isFirework && !result.IsMissOrTooFast)
            {
                _effectManager.PlayFireworkEffect(transform.position);
            }

            PlayJudgeSFX(new NoteJudgeResult()
            {
                Grade = _judgeResult,
                IsBreak = false,
                IsEX = false,
                Diff = _judgeDiff
            });
            _lastHoldState = HOLD_STATE_HEAD_MISS_OR_NOT_JUDGED;
            _audioEffMana.StopTouchHoldSound();
            _effectManager.PlayTouchHoldEffect(_sensorPos, result);
            _effectManager.ResetHoldEffect(_sensorPos);
            _notePoolManager.Collect(this);
        }

        protected override void LoadSkin()
        {
            var skin = MajInstances.SkinManager.GetTouchHoldSkin();

            SetFansMaterial(DefaultMaterial);
            if (IsBreak)
            {
                for (var i = 0; i < 4; i++)
                {
                    _fanRenderers[i].sprite = skin.Fans_Break[i];
                }
                _borderRenderer.sprite = skin.Boader_Break; // TouchHold Border
                _pointRenderer.sprite = skin.Point_Break;
                board_On = skin.Boader_Break;
                SetFansMaterial(BreakMaterial);
            }
            else
            {
                for (var i = 0; i < 4; i++)
                {
                    _fanRenderers[i].sprite = skin.Fans[i];
                }
                _borderRenderer.sprite = skin.Boader; // TouchHold Border
                if(IsEach)
                {
                    _pointRenderer.sprite = skin.Point_Each;
                }
                else
                {
                    _pointRenderer.sprite = skin.Point;
                }
                board_On = skin.Boader;
            }
            board_Off = skin.Off;
        }
        protected override void Judge(float currentSec)
        {
            if (_isJudged)
            {
                return;
            }

            var diffSec = currentSec - JudgeTiming;
            var isFast = diffSec < 0;
            _judgeDiff = diffSec * 1000;
            var diffMSec = MathF.Abs(diffSec * 1000);

            if (isFast && diffMSec > TOUCH_JUDGE_SEG_1ST_PERFECT_MSEC)
            {
                return;
            }

            var result = diffMSec switch
            {
                <= TOUCH_JUDGE_SEG_1ST_PERFECT_MSEC => JudgeGrade.Perfect,
                <= TOUCH_JUDGE_SEG_2ND_PERFECT_MSEC => JudgeGrade.LatePerfect2nd,
                <= TOUCH_JUDGE_SEG_3RD_PERFECT_MSEC => JudgeGrade.LatePerfect3rd,
                <= TOUCH_JUDGE_SEG_1ST_GREAT_MSEC => JudgeGrade.LateGreat,
                <= TOUCH_JUDGE_SEG_2ND_GREAT_MSEC => JudgeGrade.LateGreat2nd,
                <= TOUCH_JUDGE_SEG_3RD_GREAT_MSEC => JudgeGrade.LateGreat3rd,
                _ => JudgeGrade.LateGood
            };

            ConvertJudgeGrade(ref result);
            _judgeResult = result;
            _isJudged = true;
            _lastHoldState = HOLD_STATE_HEAD_JUDGED_AND_NOT_FEEDBACK;
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

            switch (State)
            {
                case NoteStatus.Initialized:
                    if (-timing < wholeDuration)
                    {
                        _multTouchHandler.Register(_sensorPos, IsEach, IsBreak);
                        SetPointActive(true);
                        SetFanActive(true);
                        RendererState = RendererStatus.On;
                        State = NoteStatus.Scaling;
                        goto case NoteStatus.Scaling;
                    }
                    return;
                case NoteStatus.Scaling:
                    {
                        var newColor = Color.white;
                        if (-timing < moveDuration)
                        {
                            SetFansColor(Color.white);
                            State = NoteStatus.Running;
                            goto case NoteStatus.Running;
                        }
                        var alpha = ((wholeDuration + timing) / displayDuration).Clamp(0, 1);
                        newColor.a = alpha;
                        SetFansColor(newColor);
                    }
                    return;
                case NoteStatus.Running:
                    {
                        var pow = -Mathf.Exp(8 * (timing * 0.43f / moveDuration) - 0.85f) + 0.42f;
                        var distance = Mathf.Clamp(pow, 0f, 0.4f);
                        if (float.IsNaN(distance))
                            distance = 0f;
                        if (timing >= 0)
                        {
                            var _pow = -Mathf.Exp(-0.85f) + 0.42f;
                            var _distance = Mathf.Clamp(_pow, 0f, 0.4f);
                            SetFansPosition(_distance);
                            SetBorderActive(true);
                            _borderMask.enabled = true;
                            State = NoteStatus.Arrived;
                            goto case NoteStatus.Arrived;
                        }
                        else
                            SetFansPosition(distance);
                    }
                    return;
                case NoteStatus.Arrived:
                    {
                        var value = 0.91f * (1 - (Length - timing) / Length);
                        var alpha = value.Clamp(0, 1f);
                        _borderMask.alphaCutoff = alpha;
                    }
                    return;
            }
        }
        void RegisterGrade()
        {
            if (GroupInfo is not null && !_judgeResult.IsMissOrTooFast())
            {
                GroupInfo.JudgeResult = _judgeResult;
                GroupInfo.JudgeDiff = _judgeDiff;
                GroupInfo.RegisterResult(_judgeResult);
            }
        }
        void TooLateCheck()
        {
            // Too late check
            if (IsEnded || _isJudged)
            {
                return;
            }

            var timing = GetTimeSpanToJudgeTiming();
            var isTooLate = timing > TOUCH_JUDGE_GOOD_AREA_MSEC / 1000;

            if (!isTooLate)
            {
                if (GroupInfo is not null)
                {
                    if (GroupInfo.Percent > 0.5f && GroupInfo.JudgeResult != null)
                    {
                        _isJudged = true;
                        _judgeResult = (JudgeGrade)GroupInfo.JudgeResult;
                        _judgeDiff = GroupInfo.JudgeDiff;
                        _lastHoldState = HOLD_STATE_HEAD_JUDGED_AND_NOT_FEEDBACK;
                        _noteManager.NextTouch(QueueInfo);
                    }
                }
            }
            else
            {
                _judgeResult = JudgeGrade.Miss;
                _isJudged = true;
                _judgeDiff = TOUCH_JUDGE_GOOD_AREA_MSEC;
                _lastHoldState = HOLD_STATE_HEAD_MISS_OR_NOT_JUDGED;
                _releaseTime = 114514;
                _noteManager.NextTouch(QueueInfo);
            }
        }
        void Check()
        {
            if (IsEnded || !IsInitialized || _isJudged || AutoplayMode == AutoplayModeOption.Enable)
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
            if (IsUseButtonRingForTouch &&
                _noteManager.IsButtonClickedInThisFrame(_buttonPos) &&
                _noteManager.TryUseButtonClickEvent(_buttonPos))
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
                _noteManager.NextTouch(QueueInfo);
                _effectManager.PlayHoldEffect(_sensorPos, _judgeResult);
                RegisterGrade();
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
                _audioEffMana.PlayTouchHoldSound();
            }

            if (!_bodyCheckRange.InRange(ThisFrameSec) || !NoteController.IsStart)
            {
                if (_lastHoldState == HOLD_STATE_HEAD_JUDGED_AND_NOT_FEEDBACK && GetRemainingTime() < Length)
                {
                    _effectManager.PlayHoldEffect(_sensorPos, _judgeResult);
                    _lastHoldState = HOLD_STATE_HEAD_JUDGED;
                }
                return;
            }
            var on = _noteManager.CheckSensorStatusInThisFrame(_sensorPos, SwitchStatus.On);
            if (on || IsAutoplay)
            {
                PlayHoldEffect();
                _releaseTime = 0;
                _lastHoldState = HOLD_STATE_PRESSED;
            }
            else
            {
                if (_releaseTime <= DELUXE_HOLD_RELEASE_IGNORE_TIME_SEC)
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

            var remainingTime = GetRemainingTime();

            if (remainingTime == 0)
            {
                End();
            }
        }
        public override void SetActive(bool state)
        {
            if (Active == state)
            {
                return;
            }
            base.SetActive(state);
            SetFanActive(state);
            SetBorderActive(state);
            SetPointActive(state);
            Active = state;
        }
        void SetFanActive(bool state)
        {
            switch (state)
            {
                case true:
                    foreach (var fanObj in _fans.AsSpan())
                    {
                        fanObj.layer = MajEnv.DEFAULT_LAYER;
                    }
                    break;
                case false:
                    foreach (var fanObj in _fans.AsSpan())
                    {
                        fanObj.layer = MajEnv.HIDDEN_LAYER;
                    }
                    break;
            }
        }
        void SetPointActive(bool state)
        {
            switch (state)
            {
                case true:
                    _pointObject.layer = MajEnv.DEFAULT_LAYER;
                    break;
                case false:
                    _pointObject.layer = MajEnv.HIDDEN_LAYER;
                    break;
            }
        }
        void SetBorderActive(bool state)
        {
            switch (state)
            {
                case true:
                    _borderObject.layer = MajEnv.DEFAULT_LAYER;
                    break;
                case false:
                    _borderObject.layer = MajEnv.HIDDEN_LAYER;
                    break;
            }
        }

        void SetFansPosition(in float distance)
        {
            for (var i = 0; i < 4; i++)
            {
                var pos = (0.226f + distance) * GetAngle(i);
                _fanTransforms[i].localPosition = pos;
            }
        }
        void PlayHoldEffect()
        {
            //var r = MajInstances.AudioManager.GetSFX("touch_Hold_riser.wav");
            //MajDebug.Log($"IsPlaying:{r.IsPlaying}\nCurrent second: {r.CurrentSec}s");
            if (_lastHoldState != HOLD_STATE_PRESSED)
            {
                _effectManager.PlayHoldEffect(_sensorPos, _judgeResult);
                _borderRenderer.sprite = board_On;
                if (_lastHoldState < HOLD_STATE_RELEASED)
                {
                    SetFansMaterial(DefaultMaterial);
                }
            }
        }
        void StopHoldEffect()
        {
            if (_lastHoldState != HOLD_STATE_RELEASED)
            {
                _effectManager.ResetHoldEffect(_sensorPos);
                _borderRenderer.sprite = board_Off;
                if (_lastHoldState < HOLD_STATE_RELEASED)
                {
                    SetFansMaterial(DefaultMaterial);
                }
            }
        }
        Vector3 GetAngle(int index)
        {
            var angle = Mathf.PI / 4 + index * (Mathf.PI / 2);
            return new Vector3(Mathf.Sin(angle), Mathf.Cos(angle));
        }
        void SetFansColor(Color color)
        {
            foreach (var fan in _fanRenderers.AsSpan())
                fan.color = color;
        }
        void SetFansMaterial(Material material)
        {
            for (var i = 0; i < 4; i++)
            {
                _fanRenderers[i].sharedMaterial = material;
            }
        }
        protected override void PlaySFX()
        {
            _audioEffMana.PlayTouchHoldSound();
        }
        protected override void PlayJudgeSFX(in NoteJudgeResult judgeResult)
        {
            if (judgeResult.IsMissOrTooFast)
                return;
            _audioEffMana.PlayTapSound(judgeResult);
            if (isFirework)
                _audioEffMana.PlayHanabiSound();
        }

        RendererStatus _rendererState = RendererStatus.Off;
    }
}