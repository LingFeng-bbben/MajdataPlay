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
using MajdataPlay.Settings;
using UnityEngine;
#nullable enable
namespace MajdataPlay.Scenes.Game.Notes.Behaviours
{
    using Unsafe = System.Runtime.CompilerServices.Unsafe;
    internal sealed class TouchDrop : NoteDrop, IRendererContainer, IPoolableNote<TouchPoolingInfo, TouchQueueInfo>, INoteQueueMember<TouchQueueInfo>, IMajComponent
    {
        public TouchGroup? GroupInfo { get; set; }
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
                        break;
                    case RendererStatus.On:
                        foreach (var renderer in _fanRenderers)
                            renderer.forceRenderingOff = false;
                        break;
                    default:
                        return;
                }
                _rendererState = value;
            }
        }

        bool _isFirework = false;
        /// <summary>
        /// Undefined
        /// </summary>
        float _displayDuration;
        /// <summary>
        /// Touch淡入结束，开始移动
        /// </summary>
        float _moveDuration;
        /// <summary>
        /// Touch开始淡入的时刻
        /// </summary>
        float _wholeDuration;

        readonly SpriteRenderer[] _fanRenderers = new SpriteRenderer[4];
        readonly GameObject[] _fans = new GameObject[4];
        readonly Transform[] _fanTransforms = new Transform[4];

        ButtonZone? _buttonPos;

        GameObject _pointObject;
        GameObject _justBorderObject;

        SpriteRenderer _pointRenderer;
        SpriteRenderer _justBorderRenderer;

        MultTouchHandler _multTouchHandler;
        NotePoolManager _notePoolManager;

        //readonly float _touchPanelOffset = MajEnv.UserSetting?.Judge.TouchPanelOffset ?? 0;

        const int _fanSpriteSortOrder = 3;
        const int _justBorderSortOrder = 1;
        const int _pointBorderSortOrder = 2;

        protected override void Awake()
        {
            base.Awake();
            _notePoolManager = Majdata<NotePoolManager>.Instance!;
            _multTouchHandler = Majdata<MultTouchHandler>.Instance!;

            _fanTransforms[0] = Transform.GetChild(3);
            _fanTransforms[1] = Transform.GetChild(2);
            _fanTransforms[2] = Transform.GetChild(1);
            _fanTransforms[3] = Transform.GetChild(4);

            _fans[0] = _fanTransforms[0].gameObject;
            _fans[1] = _fanTransforms[1].gameObject;
            _fans[2] = _fanTransforms[2].gameObject;
            _fans[3] = _fanTransforms[3].gameObject;

            for (var i = 0; i < 4; i++)
            {
                _fanRenderers[i] = _fans[i].GetComponent<SpriteRenderer>();
            }

            _pointObject = Transform.GetChild(0).gameObject;
            _pointRenderer = _pointObject.GetComponent<SpriteRenderer>();

            _justBorderObject = Transform.GetChild(5).gameObject;
            _justBorderRenderer = _justBorderObject.GetComponent<SpriteRenderer>();

            _pointObject.SetActive(true);
            _justBorderObject.SetActive(true);

            Transform.position = new Vector3(0, 0, 0);
            SetFansColor(new Color(1f, 1f, 1f, 0f));
            SetFansPosition(0.4f);

            base.SetActive(false);
            SetFanActive(false);
            SetJustBorderActive(false);
            SetPointActive(false);
            Active = false;
            //_noteChecker = new(Check);

            //if(!IsAutoplay)
            //    _noteManager.OnGameIOUpdate += GameIOListener;
            RendererState = RendererStatus.Off;
            Transform.localScale *= USERSETTING_TOUCH_SCALE;
        }
        public void Initialize(TouchPoolingInfo poolingInfo)
        {
            if (State >= NoteStatus.Initialized && State < NoteStatus.End)
                return;

            StartPos = poolingInfo.StartPos;
            Timing = poolingInfo.Timing - TOUCH_DISPLAY_OFFSET_SEC;
            _judgeTiming = poolingInfo.Timing;
            SortOrder = poolingInfo.NoteSortOrder;
            Speed = poolingInfo.Speed;
            IsEach = poolingInfo.IsEach;
            IsBreak = poolingInfo.IsBreak;
            IsEX = poolingInfo.IsEX;
            QueueInfo = poolingInfo.QueueInfo;
            _isJudged = false;
            _isFirework = poolingInfo.IsFirework;
            GroupInfo = poolingInfo.GroupInfo;
            _sensorPos = poolingInfo.SensorPos;
            if (_sensorPos < SensorArea.B1 && _sensorPos >= SensorArea.A1)
            {
                _buttonPos = _sensorPos.ToButtonZone();
            }
            else
            {
                _buttonPos = null;
            }
            _judgableRange = new(JudgeTiming - 0.15f, JudgeTiming + 0.316667f, ContainsType.Closed);

            _wholeDuration = 3.209385682f * Mathf.Pow(Speed, -0.9549621752f);
            _moveDuration = 0.8f * _wholeDuration;
            _displayDuration = 0.2f * _wholeDuration;

            LoadSkin();

            Transform.position = NoteHelper.GetTouchAreaPosition(_sensorPos);
            //_pointObject.SetActive(false);
            //_justBorderObject.SetActive(false);

            SetFansColor(new Color(1f, 1f, 1f, 0f));
            SetFansPosition(0.4f);
            RendererState = RendererStatus.Off;

            for (var i = 0; i < 4; i++)
            {
                _fanRenderers[i].sortingOrder = SortOrder - (_fanSpriteSortOrder + i);
            }
            _pointRenderer.sortingOrder = SortOrder - _pointBorderSortOrder;
            _justBorderRenderer.sortingOrder = SortOrder - _justBorderSortOrder;

            SetActive(true);
            SetFanActive(false);
            SetJustBorderActive(false);
            SetPointActive(false);

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
            var result = new NoteJudgeResult()
            {
                Grade = _judgeResult,
                Diff = _judgeDiff,
                IsEX = IsEX,
                IsBreak = IsBreak
            };
            // disable SpriteRenderer
            RendererState = RendererStatus.Off;
            SetActive(false);

            if (_isFirework && !result.IsMissOrTooFast)
            {
                _effectManager.PlayFireworkEffect(Transform.position);
            }

            PlayJudgeSFX(result);
            _noteManager.NextTouch(QueueInfo);
            _effectManager.PlayTouchEffect(_sensorPos, result);
            _objectCounter.ReportResult(this, result);
            _notePoolManager.Collect(this);
        }
        protected override void LoadSkin()
        {
            var skin = MajInstances.SkinManager.GetTouchSkin();

            SetFansMaterial(DefaultMaterial);
            if (IsBreak)
            {
                SetFansSprite(skin.Break);
                SetFansMaterial(BreakMaterial);
                _pointRenderer.sprite = skin.Point_Break;
            }
            else if (IsEach)
            {
                SetFansSprite(skin.Each);
                _pointRenderer.sprite = skin.Point_Each;
            }
            else
            {
                SetFansSprite(skin.Normal);
                _pointRenderer.sprite = skin.Point_Normal;
            }

            _justBorderRenderer.sprite = skin.JustBorder;
        }
        void TooLateCheck()
        {
            // Too late check
            if (IsEnded || _isJudged || AutoplayMode == AutoplayModeOption.Enable)
            {
                return;
            }

            var isTooLate = GetTimeSpanToJudgeTiming() > TOUCH_JUDGE_GOOD_AREA_MSEC / 1000;

            if (!isTooLate)
            {
                if (GroupInfo is not null)
                {
                    if (GroupInfo.Percent > 0.5f && GroupInfo.JudgeResult != null)
                    {
                        _isJudged = true;
                        _judgeResult = (JudgeGrade)GroupInfo.JudgeResult;
                        _judgeDiff = GroupInfo.JudgeDiff;
                        End();
                    }
                }
            }
            else
            {
                _judgeResult = JudgeGrade.Miss;
                _isJudged = true;
                End();
            }
        }
        void Check()
        {
            if (IsEnded || !IsInitialized)
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
                RegisterGrade();
                End();
            }
        }
        protected override void Autoplay()
        {
            switch (AutoplayMode)
            {
                case AutoplayModeOption.Enable:
                    base.Autoplay();
                    if (_isJudged)
                    {
                        End();
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
            if (_isJudged)
            {
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
        void RegisterGrade()
        {
            if (GroupInfo is not null && !_judgeResult.IsMissOrTooFast())
            {
                GroupInfo.JudgeResult = _judgeResult;
                GroupInfo.JudgeDiff = _judgeDiff;
                GroupInfo.RegisterResult(_judgeResult);
            }
        }
        [OnPreUpdate]
        void OnPreUpdate()
        {
            TooLateCheck();
            Check();
            Autoplay();
        }
        [OnUpdate]
        void OnUpdate()
        {
            var timing = GetTimeSpanToArriveTiming();

            switch (State)
            {
                case NoteStatus.Initialized:
                    if (-timing < _wholeDuration)
                    {
                        _multTouchHandler.Register(_sensorPos, IsEach, IsBreak);
                        RendererState = RendererStatus.On;
                        //_pointObject.SetActive(true);
                        SetPointActive(true);
                        SetFanActive(true);
                        State = NoteStatus.Scaling;
                        goto case NoteStatus.Scaling;
                    }
                    return;
                case NoteStatus.Scaling:
                    {
                        var newColor = Color.white;
                        if (-timing < _moveDuration)
                        {
                            SetFansColor(Color.white);
                            State = NoteStatus.Running;
                            goto case NoteStatus.Running;
                        }
                        var alpha = ((_wholeDuration + timing) / _displayDuration).Clamp(0, 1);
                        newColor.a = alpha;
                        SetFansColor(newColor);
                    }
                    return;
                case NoteStatus.Running:
                    {
                        var pow = -Mathf.Exp(8 * (timing * 0.43f / _moveDuration) - 0.85f) + 0.42f;
                        var distance = Mathf.Clamp(pow, 0f, 0.4f);
                        if (float.IsNaN(distance))
                            distance = 0f;

                        if (timing >= 0)
                        {
                            var _pow = -Mathf.Exp(-0.85f) + 0.42f;
                            var _distance = Mathf.Clamp(_pow, 0f, 0.4f);
                            SetFansPosition(_distance);
                            SetJustBorderActive(true);
                            State = NoteStatus.Arrived;
                        }
                        else
                        {
                            SetFansPosition(distance);
                        }
                    }
                    return;
                case NoteStatus.Arrived:
                    return;
            }
        }
        protected override void Judge(float currentSec)
        {
            if (_isJudged)
                return;

            var diffSec = currentSec - JudgeTiming;
            var isFast = diffSec < 0;
            _judgeDiff = diffSec * 1000;
            var diffMSec = MathF.Abs(diffSec * 1000);

            if (isFast && diffMSec > TOUCH_JUDGE_SEG_1ST_PERFECT_MSEC)
                return;

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
        }

        public override void SetActive(bool state)
        {
            if (Active == state)
                return;
            base.SetActive(state);
            SetFanActive(state);
            SetJustBorderActive(state);
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
        void SetJustBorderActive(bool state)
        {
            switch (state)
            {
                case true:
                    _justBorderObject.layer = MajEnv.DEFAULT_LAYER;
                    break;
                case false:
                    _justBorderObject.layer = MajEnv.HIDDEN_LAYER;
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
        Vector3 GetAngle(int index)
        {
            var angle = index * (Mathf.PI / 2);
            return new Vector3(Mathf.Sin(angle), Mathf.Cos(angle));
        }
        void SetFansColor(Color color)
        {
            foreach (var fan in _fanRenderers) fan.color = color;
        }
        void SetFansSprite(Sprite sprite)
        {
            for (var i = 0; i < 4; i++)
                _fanRenderers[i].sprite = sprite;
        }
        void SetFansMaterial(Material material)
        {
            for (var i = 0; i < 4; i++)
                _fanRenderers[i].sharedMaterial = material;
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
            if (judgeResult.IsMissOrTooFast)
                return;
            if (judgeResult.IsBreak)
                _audioEffMana.PlayTapSound(judgeResult);
            else
                _audioEffMana.PlayTouchSound();
            if (_isFirework)
                _audioEffMana.PlayHanabiSound();
        }
        RendererStatus _rendererState = RendererStatus.Off;
    }
}