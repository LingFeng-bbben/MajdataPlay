using MajdataPlay.Buffers;
using MajdataPlay.IO;
using MajdataPlay.Numerics;
using MajdataPlay.Scenes.Game.Buffers;
using MajdataPlay.Scenes.Game.Notes.Controllers;
using MajdataPlay.Scenes.Game.Utils;
using MajdataPlay.Settings;
using MajdataPlay.Utils;
using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Profiling;
#nullable enable
namespace MajdataPlay.Scenes.Game.Notes.Behaviours
{
    internal sealed class TapDrop : NoteDrop, IDistanceProvider, INoteQueueMember<TapQueueInfo>, IRendererContainer, IPoolableNote<TapPoolingInfo, TapQueueInfo>, IMajComponent
    {
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
                        _thisRenderer.enabled = false;
                        _exRenderer.enabled = false;
                        _tapLineRenderer.enabled = false;
                        //_thisRenderer.forceRenderingOff = true;
                        //_exRenderer.forceRenderingOff = true;
                        //_tapLineRenderer.forceRenderingOff = true;
                        break;
                    case RendererStatus.On:
                        _thisRenderer.enabled = true;
                        _exRenderer.enabled = IsEX;
                        _tapLineRenderer.enabled = true;
                        //_thisRenderer.forceRenderingOff = false;
                        //_exRenderer.forceRenderingOff = !IsEX;
                        //_tapLineRenderer.forceRenderingOff = false;
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

        float _noteAppearRate = 0.265f;
        //readonly float _touchPanelOffset = MajEnv.UserSetting?.Judge.TouchPanelOffset ?? 0;

        const int TAP_SPRITE_SORT_ORDER = 1;
        const int TAP_EX_SORT_ORDER = 0;

        protected override void Awake()
        {
            base.Awake();
            _noteAppearRate = MajEnv.Settings.Debug.NoteAppearRate;
            _isStarRotation = Settings.Game.StarRotation;
            _notePoolManager = FindObjectOfType<NotePoolManager>();
            _thisRenderer = GetComponent<SpriteRenderer>();

            _exObject = Transform.GetChild(0).gameObject;
            _exRenderer = _exObject.GetComponent<SpriteRenderer>();

            _tapLineObject = Instantiate(_tapLinePrefab, NoteManager.gameObject.transform.GetChild(7));
            _tapLineObject.SetActive(true);
            _tapLineRenderer = _tapLineObject.GetComponent<SpriteRenderer>();
            _tapLineTransform = _tapLineObject.transform;

            Transform.localScale = new Vector3(0, 0);

            base.SetActive(false);
            _tapLineObject.layer = MajEnv.HIDDEN_LAYER;
            _exObject.layer = MajEnv.HIDDEN_LAYER;

            _thisRenderer.enabled = false;
            _exRenderer.enabled = false;
            _tapLineRenderer.enabled = false;

            Active = false;
        }
        public void Init(TapPoolingInfo poolingInfo)
        {
            if (State >= NoteStatus.Inited && State < NoteStatus.End)
            {
                return;
            }
            StartPos = poolingInfo.StartPos;
            Timing = poolingInfo.Timing;
            JudgeTiming = Timing;
            SortOrder = poolingInfo.NoteSortOrder;
            Speed = poolingInfo.Speed;
            IsEach = poolingInfo.IsEach;
            IsBreak = poolingInfo.IsBreak;
            IsEX = poolingInfo.IsEX;
            IsMine = poolingInfo.IsMine;
            QueueInfo = poolingInfo.QueueInfo;
            IsStar = poolingInfo.IsStar;
            IsDouble = poolingInfo.IsDouble;
            RotateSpeed = poolingInfo.RotateSpeed;
            IsJudged = false;
            Distance = -100;
            _innerPos = NoteHelper.GetTapPosition(StartPos, 1.225f);
            _outerPos = NoteHelper.GetTapPosition(StartPos, 4.8f);
            SensorPos = (SensorArea)(StartPos - 1);
            _buttonPos = SensorPos.ToButtonZone();
            if (IsMine)
            {
                JudgableRange = new(JudgeTimingWithOffset - (TAP_JUDGE_SEG_3RD_PERFECT_MSEC / 1000), JudgeTimingWithOffset + (TAP_JUDGE_SEG_3RD_PERFECT_MSEC / 1000), ContainsType.Closed);
            }
            else
            {
                JudgableRange = new(JudgeTimingWithOffset - (TAP_JUDGE_GOOD_AREA_MSEC / 1000), JudgeTimingWithOffset + (TAP_JUDGE_GOOD_AREA_MSEC / 1000), ContainsType.Closed);
            }

            Transform.rotation = Quaternion.Euler(0, 0, -22.5f + (-45f * (StartPos - 1)));
            Transform.localScale = new Vector3(0, 0);

            _tapLineObject.transform.rotation = Quaternion.Euler(0, 0, -22.5f + (-45f * (StartPos - 1)));
            _thisRenderer.sortingOrder = SortOrder - TAP_SPRITE_SORT_ORDER;
            _exRenderer.sortingOrder = SortOrder - TAP_EX_SORT_ORDER;

            LoadSkin();
            SetActive(true);
            SetTapLineActive(false);

            State = NoteStatus.Inited;
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
                Grade = JudgeResult,
                IsBreak = IsBreak,
                IsMine = IsMine,
                IsEX = IsEX,
                Diff = JudgeDiff
            };
            PlayJudgeSFX(result);
            EffectManager.PlayTapJudgeResult(StartPos, result);
            //MajDebug.LogDebug($"Note index: {QueueInfo.Index}");
            NoteManager.NextNote(QueueInfo);
            ObjectCounter.ReportResult(this, result);
            _notePoolManager.Collect(this);
        }
        protected override void PlaySFX()
        {
            PlayJudgeSFX(new NoteJudgeResult()
            {
                Grade = JudgeResult,
                IsBreak = IsBreak,
                IsEX = IsEX,
                Diff = JudgeDiff
            });
        }
        protected override void PlayJudgeSFX(in NoteJudgeResult judgeResult)
        {
            if (judgeResult.IsMine)
            {
                return;
            }
            AudioEffMana.PlayTapSound(judgeResult);
        }
        [OnPreUpdate]
        void OnPreUpdate()
        {
            using (UnityProfiler.Create("TapDrop.OnPreUpdate"))
            {
                TooLateCheck();
                Check();
                MineCheck();
                Autoplay();
            }
        }
        protected override void Autoplay()
        {
            if (IsMine)
            {
                return;
            }
            switch(AutoplayMode)
            {
                case AutoplayModeOption.Enable:
                    base.Autoplay();
                    if(IsJudged)
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
            if (IsJudged || !IsAutoplay)
            {
                return;
            }
            else if (!NoteManager.IsCurrentNoteJudgeable(QueueInfo))
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
                _ = NoteManager.SimulateButtonClick(_buttonPos) ||
                    (USERSETTING_DJAUTO_POLICY == DJAutoPolicyOption.Permissive && NoteManager.SimulateSensorClick(SensorPos));
            }
            else
            {
                _ = NoteManager.SimulateSensorClick(SensorPos) ||
                    (USERSETTING_DJAUTO_POLICY == DJAutoPolicyOption.Permissive && NoteManager.SimulateButtonClick(_buttonPos));
            }
        }
        [OnUpdate]
        void OnUpdate()
        {
            using (UnityProfiler.Create("TapDrop.OnUpdate"))
            {
                var timing = GetTimeSpanToArriveTiming();
                var distance = timing * Speed + 4.8f;
                var scaleRate = _noteAppearRate;
                var destScale = distance * scaleRate + (1 - scaleRate * 1.225f);

                switch (State)
                {
                    case NoteStatus.Inited:
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
        }
        void TooLateCheck()
        {
            // Too late check
            if (IsJudged || IsEnded || AutoplayMode == AutoplayModeOption.Enable)
            {
                return;
            }

            var timing = GetTimeSpanToJudgeTiming();
            var isTooLate = timing > TAP_JUDGE_GOOD_AREA_MSEC / 1000;

            if (isTooLate)
            {
                //MajDebug.LogWarning("Note too late");
                JudgeResult = JudgeGrade.Miss;
                IsJudged = true;
                End();
            }
        }
        void Check()
        {
            if (IsEnded || !IsInited)
            {
                return;
            }
            else if (!JudgableRange.InRange(ThisFrameSec) || !NoteManager.IsCurrentNoteJudgeable(QueueInfo))
            {
                return;
            }

            if (NoteManager.IsButtonClickedInThisFrame(_buttonPos) && NoteManager.TryUseButtonClickEvent(_buttonPos))
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

            if (IsJudged)
            {
                if(IsMine)
                {
                    if(JudgeResult >= JudgeGrade.Perfect)
                    {
                        JudgeResult = JudgeGrade.TooFast;
                    }
                    else
                    {
                        JudgeResult = JudgeGrade.Miss;
                    }
                }
                //MajDebug.LogError("Note is judged");
                End();
            }
        }
        void MineCheck()
        {
            if (!IsMine || IsEnded || !IsInited || IsJudged)
            {
                return;
            }
            if (GetTimeSpanToJudgeTiming() > TAP_JUDGE_SEG_3RD_PERFECT_MSEC / 1000)
            {
                IsJudged = true;
                JudgeResult = JudgeGrade.Perfect;
                End();
            }
        }
        protected override void LoadSkin()
        {

            RendererState = RendererStatus.Off;

            if (IsStar)
            {
                LoadStarSkin();
            }
            else
            {
                LoadTapSkin();
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

            if (IsMine)
            {
                _thisRenderer.sprite = skin.Mine;
                _thisRenderer.sharedMaterial = DefaultMaterial;
                _exRenderer.sprite = skin.Ex;
                _exRenderer.color = skin.ExEffects[0];
                _tapLineRenderer.sprite = skin.GuideLines[0];
                if (IsEach)
                {
                    _tapLineRenderer.sprite = skin.GuideLines[1];
                }
                if (IsBreak)
                {
                    _thisRenderer.sprite = skin.BreakMine;
                    _thisRenderer.sharedMaterial = BreakMaterial;
                    _tapLineRenderer.sprite = skin.GuideLines[2];
                    _exRenderer.color = skin.ExEffects[2];
                }
            }
            else
            {
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
        }
        void LoadStarSkin()
        {
            var skin = MajInstances.SkinManager.GetStarSkin();
            _thisRenderer.sharedMaterial = DefaultMaterial;
            _exRenderer.color = skin.ExEffects[0];
            _tapLineRenderer.sprite = skin.GuideLines[0];

            if (IsMine)
            {
                if (IsDouble)
                {
                    _thisRenderer.sprite = skin.DoubleMine;
                    _exRenderer.sprite = skin.ExDouble;
                    if (IsBreak)
                    {
                        _thisRenderer.sprite = skin.BreakDoubleMine;
                        _thisRenderer.sharedMaterial = BreakMaterial;
                        _tapLineRenderer.sprite = skin.GuideLines[2];
                        _exRenderer.color = skin.ExEffects[2];
                    }
                }
                else
                {
                    _thisRenderer.sprite = skin.Mine;
                    _exRenderer.sprite = skin.Ex;

                    if (IsBreak)
                    {
                        _thisRenderer.sprite = skin.BreakMine;
                        _thisRenderer.sharedMaterial = BreakMaterial;
                        _tapLineRenderer.sprite = skin.GuideLines[2];
                        _exRenderer.color = skin.ExEffects[2];
                    }
                }
            }
            else
            {
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
        }
        RendererStatus _rendererState = RendererStatus.Off;
    }
}
