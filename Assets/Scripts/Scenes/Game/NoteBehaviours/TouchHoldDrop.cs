using MajdataPlay.Buffers;
using MajdataPlay.Extensions;
using MajdataPlay.IO;
using MajdataPlay.Numerics;
using MajdataPlay.Scenes.Game.Buffers;
using MajdataPlay.Scenes.Game.Notes.Controllers;
using MajdataPlay.Scenes.Game.Notes.Touch;
using MajdataPlay.Scenes.Game.Utils;
using MajdataPlay.Settings;
using MajdataPlay.Utils;
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UI;
#nullable enable
namespace MajdataPlay.Scenes.Game.Notes.Behaviours
{
    internal sealed class TouchHoldDrop : NoteLongDrop, INoteQueueMember<TouchQueueInfo>, IRendererContainer, IPoolableNote<TouchHoldPoolingInfo, TouchQueueInfo>, IMajComponent
    {
        public TouchGroup? GroupInfo { get; private set; } = null;
        public TouchHoldGroup? BodyGroupInfo { get; private set; } = null;
        public TouchQueueInfo QueueInfo { get; private set; } = TouchQueueInfo.Default;
        public RendererStatus RendererState
        {
            get => _rendererState;
            set
            {
                if (State < NoteStatus.Inited)
                {
                    return;
                }

                switch (value)
                {
                    case RendererStatus.Off:
                        for (var i = 0; i < _fanRenderers.Length; i++)
                        {
                            var renderer = _fanRenderers[i];
                            renderer.enabled = false;
                        }
                        _borderRenderer.enabled = false;
                        _borderMask.enabled = false;
                        break;
                    case RendererStatus.On:
                        for (var i = 0; i < _fanRenderers.Length; i++)
                        {
                            var renderer = _fanRenderers[i];
                            renderer.enabled = true;
                        }
                        _borderRenderer.enabled = true;
                        _borderMask.enabled = true;
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

            for (var i = 0; i < _fanRenderers.Length; i++)
            {
                var renderer = _fanRenderers[i];
                renderer.enabled = false;
            }
            _borderRenderer.enabled = false;
            _borderMask.enabled = false;
            _borderMask.alphaCutoff = 0;

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
                    else if (IsJudged)
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
                            JudgeResult = autoplayGrade;
                        }
                        else
                        {
                            JudgeResult = (JudgeGrade)Randomizer.Next(0, 15);
                        }
                        ConvertJudgeGrade(ref JudgeResult);
                        IsJudged = true;
                        JudgeDiff = JudgeResult switch
                        {
                            < JudgeGrade.Perfect => 1,
                            > JudgeGrade.Perfect => -1,
                            _ => 0
                        };
                        PlayJudgeSFX(new NoteJudgeResult()
                        {
                            Grade = JudgeResult,
                            IsBreak = IsBreak,
                            IsEX = IsEX,
                            Diff = JudgeDiff
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
            else if (IsJudged)
            {
                NoteManager.SimulateSensorPress(SensorPos);
                return;
            }
            else if (!NoteManager.IsCurrentNoteJudgeable(QueueInfo))
            {
                return;
            }
            else if (GetTimeSpanToArriveTiming() < -FRAME_LENGTH_SEC)
            {
                return;
            }
            NoteManager.SimulateSensorClick(SensorPos);
        }
        public void Init(TouchHoldPoolingInfo poolingInfo)
        {
            if (State >= NoteStatus.Inited && State < NoteStatus.End)
            {
                return;
            }

            StartPos = poolingInfo.StartPos;
            areaPosition = poolingInfo.AreaPos;
            Timing = poolingInfo.Timing - TOUCH_HOLD_DISPLAY_OFFSET_SEC;
            JudgeTiming = poolingInfo.Timing;
            SortOrder = poolingInfo.NoteSortOrder;
            Speed = poolingInfo.Speed;
            IsEach = poolingInfo.IsEach;
            IsBreak = poolingInfo.IsBreak;
            IsEX = poolingInfo.IsEX;
            QueueInfo = poolingInfo.QueueInfo;
            GroupInfo = poolingInfo.GroupInfo;
            BodyGroupInfo = poolingInfo.TouchHoldGroupInfo;
            IsJudged = false;
            _lastHoldState = HOLD_STATE_HEAD_MISS_OR_NOT_JUDGED;
            Length = poolingInfo.LastFor;
            isFirework = poolingInfo.IsFirework;
            SensorPos = poolingInfo.SensorPos;
            JudgeResult = JudgeGrade.Miss;
            if (SensorPos < SensorArea.B1 && SensorPos >= SensorArea.A1)
            {
                _buttonPos = SensorPos.ToButtonZone();
            }
            else
            {
                _buttonPos = null;
            }
            _playerReleaseTimeSec = 0;
            JudgableRange = new(JudgeTimingWithOffset - 0.15f, JudgeTimingWithOffset + 0.316667f, ContainsType.Closed);
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

            Transform.position = NoteHelper.GetTouchAreaPosition(SensorPos);
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

            State = NoteStatus.Inited;
        }
        void End()
        {
            if (IsEnded)
            {
                return;
            }

            State = NoteStatus.End;
            _multTouchHandler.Unregister(SensorPos);
            BodyGroupInfo?.UnregisterTrigger(InstanceID);
            BodyGroupInfo?.Exit();
            JudgeResult = HoldEndJudge(JudgeResult, TOUCH_HOLD_HEAD_IGNORE_LENGTH_SEC + TOUCH_HOLD_TAIL_IGNORE_LENGTH_SEC);
            ConvertJudgeGrade(ref JudgeResult);
            var result = new NoteJudgeResult()
            {
                Grade = JudgeResult,
                IsBreak = IsBreak,
                IsEX = IsEX,
                Diff = JudgeDiff,
            };
            //_pointObject.SetActive(false);
            SetActive(false);
            RendererState = RendererStatus.Off;

            ObjectCounter.ReportResult(this, result);
            if (!IsJudged)
            {
                NoteManager.NextTouch(QueueInfo);
            }
            if (isFirework && !result.IsMissOrTooFast)
            {
                EffectManager.PlayFireworkEffect(transform.position);
            }

            PlayJudgeSFX(new NoteJudgeResult()
            {
                Grade = JudgeResult,
                IsBreak = false,
                IsEX = false,
                Diff = JudgeDiff
            });
            _lastHoldState = HOLD_STATE_HEAD_MISS_OR_NOT_JUDGED;
            AudioEffMana.StopTouchHoldSound();
            EffectManager.PlayTouchHoldEffect(SensorPos, result);
            EffectManager.ResetHoldEffect(SensorPos);
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
            if (IsJudged)
            {
                return;
            }

            var diffSec = currentSec - JudgeTimingWithOffset;
            var isFast = diffSec < 0;
            JudgeDiff = diffSec * 1000;
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
            JudgeResult = result;
            IsJudged = true;
            BodyGroupInfo?.RegisterTrigger(InstanceID);
            _lastHoldState = HOLD_STATE_HEAD_JUDGED_AND_NOT_FEEDBACK;
        }
        [OnPreUpdate]
        void OnPreUpdate()
        {
            using (UnityProfiler.Create("TouchHoldDrop.OnPreUpdate"))
            {
                TooLateCheck();
                Check();
                BodyCheck();
                ForceEndCheck();
                Autoplay();
            }
        }
        [OnUpdate]
        void OnUpdate()
        {
            using (UnityProfiler.Create("TouchHoldDrop.OnUpdate"))
            {
                var timing = GetTimeSpanToArriveTiming();

                switch (State)
                {
                    case NoteStatus.Inited:
                        if (-timing < wholeDuration)
                        {
                            _multTouchHandler.Register(SensorPos, IsEach, IsBreak);
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
        }
        void RegisterGrade()
        {
            if (GroupInfo is not null && !JudgeResult.IsMissOrTooFast())
            {
                GroupInfo.JudgeResult = JudgeResult;
                GroupInfo.JudgeDiff = JudgeDiff;
                GroupInfo.RegisterResult(JudgeResult);
            }
        }
        void TooLateCheck()
        {
            // Too late check
            if (IsEnded || IsJudged)
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
                        IsJudged = true;
                        JudgeResult = (JudgeGrade)GroupInfo.JudgeResult;
                        JudgeDiff = GroupInfo.JudgeDiff;
                        _lastHoldState = HOLD_STATE_HEAD_JUDGED_AND_NOT_FEEDBACK;
                        NoteManager.NextTouch(QueueInfo);
                        BodyGroupInfo?.RegisterTrigger(InstanceID);
                    }
                }
            }
            else
            {
                JudgeResult = JudgeGrade.Miss;
                IsJudged = true;
                JudgeDiff = TOUCH_JUDGE_GOOD_AREA_MSEC;
                _lastHoldState = HOLD_STATE_HEAD_MISS_OR_NOT_JUDGED;
                _releaseTime = 114514;
                NoteManager.NextTouch(QueueInfo);
            }
        }
        void Check()
        {
            if (IsEnded || !IsInited || IsJudged || AutoplayMode == AutoplayModeOption.Enable)
            {
                return;
            }
            else if (!JudgableRange.InRange(ThisFrameSec) || !NoteManager.IsCurrentNoteJudgeable(QueueInfo))
            {
                return;
            }

#if UNITY_ANDROID || UNITY_IOS
            if (NoteManager.IsSensorClickedInThisFrame(SensorPos) && NoteManager.TryUseSensorClickEvent(SensorPos))
            {
                Judge(ThisFrameSec - USERSETTING_TOUCHPANEL_OFFSET_SEC);
            }
            else
            {
                return;
            }
#else
            if (IsUseButtonRingForTouch &&
                NoteManager.IsButtonClickedInThisFrame(_buttonPos) &&
                NoteManager.TryUseButtonClickEvent(_buttonPos))
            {
                Judge(ThisFrameSec);
            }
            else if (NoteManager.IsSensorClickedInThisFrame(SensorPos) && NoteManager.TryUseSensorClickEvent(SensorPos))
            {
                Judge(ThisFrameSec - USERSETTING_TOUCHPANEL_OFFSET_SEC);
            }
            else
            {
                return;
            }
#endif
            if (IsJudged)
            {
                NoteManager.NextTouch(QueueInfo);
                EffectManager.PlayHoldEffect(SensorPos, JudgeResult);
                RegisterGrade();
            }
        }
        void BodyCheck()
        {
            if (!IsInited || IsEnded)
            {
                return;
            }
            if (_lastHoldState is HOLD_STATE_HEAD_JUDGED or HOLD_STATE_PRESSED)
            {
                AudioEffMana.PlayTouchHoldSound();
            }

            if (!_bodyCheckRange.InRange(ThisFrameSec) || !NoteController.IsStart)
            {
                if (_lastHoldState == HOLD_STATE_HEAD_JUDGED_AND_NOT_FEEDBACK && GetRemainingTime() < Length)
                {
                    EffectManager.PlayHoldEffect(SensorPos, JudgeResult);
                    _lastHoldState = HOLD_STATE_HEAD_JUDGED;
                }
                return;
            }
            var on = NoteManager.CheckSensorStatusInThisFrame(SensorPos, SwitchStatus.On);

            if(BodyGroupInfo is not null)
            {
                if (on)
                {
                    BodyGroupInfo.RegisterTrigger(InstanceID);
                }
                else
                {
                    BodyGroupInfo.UnregisterTrigger(InstanceID);
                }
                on |= BodyGroupInfo.Percent > 0.5f;
            }

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
            if (!IsJudged || IsEnded)
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
                EffectManager.PlayHoldEffect(SensorPos, JudgeResult);
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
                EffectManager.ResetHoldEffect(SensorPos);
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
            AudioEffMana.PlayTouchHoldSound();
        }
        protected override void PlayJudgeSFX(in NoteJudgeResult judgeResult)
        {
            if (judgeResult.IsMissOrTooFast)
                return;
            AudioEffMana.PlayTapSound(judgeResult);
            if (isFirework)
                AudioEffMana.PlayHanabiSound();
        }

        RendererStatus _rendererState = RendererStatus.Off;
    }
}