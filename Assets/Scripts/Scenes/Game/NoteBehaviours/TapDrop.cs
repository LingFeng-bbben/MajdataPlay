using MajdataPlay.Buffers;
using MajdataPlay.Scenes.Game.Buffers;
using MajdataPlay.Scenes.Game.Notes.Controllers;
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
    internal sealed class TapDrop : NoteDrop, IDistanceProvider, INoteQueueMember<TapQueueInfo>, IRendererContainer, IPoolableNote<TapPoolingInfo, TapQueueInfo>, IMajComponent
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
                        break;
                    case RendererStatus.On:
                        _thisRenderer.forceRenderingOff = false;
                        _exRenderer.forceRenderingOff = !IsEX;
                        _tapLineRenderer.forceRenderingOff = false;
                        break;
                }
            }
        }
        public TapQueueInfo QueueInfo { get; set; } = TapQueueInfo.Default;
        public float RotateSpeed { get; set; } = 0f;
        public bool IsDouble { get; set; } = false;
        public bool IsStar { get; set; } = false;
        public float Distance { get; set; } = -100;

        [SerializeField]
        GameObject _tapLinePrefab;


        Transform _tapLineTransform;
        GameObject _tapLineObject;
        GameObject _exObject;


        SpriteRenderer _thisRenderer;
        SpriteRenderer _exRenderer;
        SpriteRenderer _tapLineRenderer;
        NotePoolManager _notePoolManager;

        bool _isStarRotation = false;

        ButtonZone? _buttonPos;

        Vector3 _innerPos = NoteHelper.GetTapPosition(1, 1.225f);
        Vector3 _outerPos = NoteHelper.GetTapPosition(1, 4.8f);

        readonly float _noteAppearRate = MajInstances.Settings?.Debug.NoteAppearRate ?? 0.265f;
        //readonly float _touchPanelOffset = MajEnv.UserSetting?.Judge.TouchPanelOffset ?? 0;

        const int _spriteSortOrder = 1;
        const int _exSortOrder = 0;

        protected override void Awake()
        {
            base.Awake();
            _isStarRotation = _settings.Game.StarRotation;
            _notePoolManager = FindObjectOfType<NotePoolManager>();
            _thisRenderer = GetComponent<SpriteRenderer>();

            _exObject = Transform.GetChild(0).gameObject;
            _exRenderer = _exObject.GetComponent<SpriteRenderer>();

            _tapLineObject = Instantiate(_tapLinePrefab, _noteManager.gameObject.transform.GetChild(7));
            _tapLineObject.SetActive(true);
            _tapLineRenderer = _tapLineObject.GetComponent<SpriteRenderer>();
            _tapLineTransform = _tapLineObject.transform;

            Transform.localScale = new Vector3(0, 0);

            base.SetActive(false);
            _tapLineObject.layer = MajEnv.HIDDEN_LAYER;
            _exObject.layer = MajEnv.HIDDEN_LAYER;
            Active = false;

            //if (!IsAutoplay)
            //    _noteManager.OnGameIOUpdate += GameIOListener;
        }
        public void Initialize(TapPoolingInfo poolingInfo)
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
            IsStar = poolingInfo.IsStar;
            IsDouble = poolingInfo.IsDouble;
            RotateSpeed = poolingInfo.RotateSpeed;
            _isJudged = false;
            Distance = -100;
            _innerPos = NoteHelper.GetTapPosition(StartPos, 1.225f);
            _outerPos = NoteHelper.GetTapPosition(StartPos, 4.8f);
            _sensorPos = (SensorArea)(StartPos - 1);
            _buttonPos = _sensorPos.ToButtonZone();
            _judgableRange = new(JudgeTiming - 0.15f, JudgeTiming + 0.15f, ContainsType.Closed);

            Transform.rotation = Quaternion.Euler(0, 0, -22.5f + -45f * (StartPos - 1));
            Transform.localScale = new Vector3(0, 0);

            _tapLineObject.transform.rotation = Quaternion.Euler(0, 0, -22.5f + -45f * (StartPos - 1));
            _thisRenderer.sortingOrder = SortOrder - _spriteSortOrder;
            _exRenderer.sortingOrder = SortOrder - _exSortOrder;

            LoadSkin();
            SetActive(true);
            SetTapLineActive(false);

            State = NoteStatus.Initialized;
        }
        void End()
        {
            if (IsEnded)
            {
                return;
            }
            State = NoteStatus.End;

            SetActive(false);
            RendererState = RendererStatus.Off;
            var result = new NoteJudgeResult()
            {
                Grade = _judgeResult,
                IsBreak = IsBreak,
                IsEX = IsEX,
                Diff = _judgeDiff
            };
            PlayJudgeSFX(result);
            //MajDebug.LogDebug($"Note index: {QueueInfo.Index}");
            _noteManager.NextNote(QueueInfo);
            
            _effectManager.PlayEffect(StartPos, result);
            _objectCounter.ReportResult(this, result);
            _notePoolManager.Collect(this);
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
            Autoplay();
        }
        protected override void Autoplay()
        {
            switch(AutoplayMode)
            {
                case AutoplayModeOption.Enable:
                    base.Autoplay();
                    if(_isJudged)
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
            if (_isJudged || !IsAutoplay)
            {
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
            var isBtnFirst = AutoplayMode == AutoplayModeOption.DJAuto_ButtonRing_First;

            if (isBtnFirst)
            {
                _ = _noteManager.SimulateButtonClick(_buttonPos) ||
                    (USERSETTING_DJAUTO_POLICY == DJAutoPolicyOption.Permissive && _noteManager.SimulateSensorClick(_sensorPos));
            }
            else
            {
                _ = _noteManager.SimulateSensorClick(_sensorPos) ||
                    (USERSETTING_DJAUTO_POLICY == DJAutoPolicyOption.Permissive && _noteManager.SimulateButtonClick(_buttonPos));
            }
        }
        [OnUpdate]
        void OnUpdate()
        {
            var timing = GetTimeSpanToArriveTiming();
            var distance = timing * Speed + 4.8f;
            var scaleRate = _noteAppearRate;
            var destScale = distance * scaleRate + (1 - scaleRate * 1.225f);

            switch (State)
            {
                case NoteStatus.Initialized:
                    if (destScale >= 0f)
                    {
                        Transform.position = _innerPos;
                        _tapLineTransform.localScale = new Vector3(1.225f / 4.8f, 1.225f / 4.8f, 1f);

                        RendererState = RendererStatus.On;
                        State = NoteStatus.Scaling;
                        goto case NoteStatus.Scaling;
                    }
                    return;
                case NoteStatus.Scaling:
                    {
                        if (destScale > 0.3f)
                        {
                            SetTapLineActive(true);
                        }
                        if (distance < 1.225f)
                        {
                            Distance = distance;
                            Transform.localScale = new Vector3(destScale, destScale) * USERSETTING_TAP_SCALE;
                        }
                        else
                        {
                            Transform.localScale = new Vector3(1f, 1f) * USERSETTING_TAP_SCALE;
                            State = NoteStatus.Running;
                            goto case NoteStatus.Running;
                        }
                    }
                    break;
                case NoteStatus.Running:
                    {
                        Distance = distance;
                        Transform.position = _outerPos * (distance / 4.8f);
                        var lineScale = Mathf.Abs(distance / 4.8f);
                        _tapLineTransform.localScale = new Vector3(lineScale, lineScale, 1f);
                    }
                    break;
                default:
                    return;
            }
            if (IsStar)
            {
                if (NoteController.IsStart && _isStarRotation)
                    Transform.Rotate(0f, 0f, RotateSpeed * MajTimeline.DeltaTime);
            }
        }
        void TooLateCheck()
        {
            // Too late check
            if (_isJudged || IsEnded || AutoplayMode == AutoplayModeOption.Enable)
            {
                return;
            }

            var timing = GetTimeSpanToJudgeTiming();
            var isTooLate = timing > TAP_JUDGE_GOOD_AREA_MSEC / 1000;

            if (isTooLate)
            {
                //MajDebug.LogWarning("Note too late");
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
                //MajDebug.LogError("Note is judged");
                End();
            }
        }
        protected override void LoadSkin()
        {

            RendererState = RendererStatus.Off;

            if (IsStar)
                LoadStarSkin();
            else
                LoadTapSkin();
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
        void LoadTapSkin()
        {
            var skin = MajInstances.SkinManager.GetTapSkin();
            //var _thisRenderer = GetComponent<SpriteRenderer>();
            //var _exRenderer = transform.GetChild(0).GetComponent<SpriteRenderer>();
            //var _tapLineRenderer = _tapLineObject.GetComponent<SpriteRenderer>();

            _thisRenderer.sprite = skin.Normal;
            _thisRenderer.sharedMaterial = DefaultMaterial;
            _exRenderer.sprite = skin.Ex;
            _exRenderer.color = skin.ExEffects[0];
            _tapLineRenderer.sprite = skin.GuideLines[0];

            if (IsEach)
            {
                _thisRenderer.sprite = skin.Each;
                _tapLineRenderer.sprite = skin.GuideLines[1];
                _exRenderer.color = skin.ExEffects[1];
            }

            if (IsBreak)
            {
                _thisRenderer.sprite = skin.Break;
                _thisRenderer.sharedMaterial = BreakMaterial;
                _tapLineRenderer.sprite = skin.GuideLines[2];
                _exRenderer.color = skin.ExEffects[2];
            }
        }
        void LoadStarSkin()
        {
            //var _thisRenderer = GetComponent<SpriteRenderer>();
            //var _exRenderer = transform.GetChild(0).GetComponent<SpriteRenderer>();
            //var _tapLineRenderer = _tapLineObject.GetComponent<SpriteRenderer>();
            var skin = MajInstances.SkinManager.GetStarSkin();
            _thisRenderer.sharedMaterial = DefaultMaterial;
            _exRenderer.color = skin.ExEffects[0];
            _tapLineRenderer.sprite = skin.GuideLines[0];

            if (IsDouble)
            {
                _thisRenderer.sprite = skin.Double;
                _exRenderer.sprite = skin.ExDouble;

                if (IsEach)
                {
                    _thisRenderer.sprite = skin.EachDouble;
                    _tapLineRenderer.sprite = skin.GuideLines[1];
                    _exRenderer.color = skin.ExEffects[1];
                }
                if (IsBreak)
                {
                    _thisRenderer.sprite = skin.BreakDouble;
                    _thisRenderer.sharedMaterial = BreakMaterial;
                    _tapLineRenderer.sprite = skin.GuideLines[2];
                    _exRenderer.color = skin.ExEffects[2];
                }
            }
            else
            {
                _thisRenderer.sprite = skin.Normal;
                _exRenderer.sprite = skin.Ex;

                if (IsEach)
                {
                    _thisRenderer.sprite = skin.Each;
                    _tapLineRenderer.sprite = skin.GuideLines[1];
                    _exRenderer.color = skin.ExEffects[1];
                }
                if (IsBreak)
                {
                    _thisRenderer.sprite = skin.Break;
                    _thisRenderer.sharedMaterial = BreakMaterial;
                    _tapLineRenderer.sprite = skin.GuideLines[2];
                    _exRenderer.color = skin.ExEffects[2];
                }
            }
        }
        RendererStatus _rendererState = RendererStatus.Off;
    }
}
