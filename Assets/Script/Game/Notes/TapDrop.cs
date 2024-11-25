using MajdataPlay.Buffers;
using MajdataPlay.Game.Controllers;
using MajdataPlay.Interfaces;
using MajdataPlay.IO;
using MajdataPlay.Types;
using MajdataPlay.Utils;
using System;
using UnityEngine;
#nullable enable
namespace MajdataPlay.Game.Notes
{
    public sealed class TapDrop : NoteDrop, IDistanceProvider, INoteQueueMember<TapQueueInfo>, IRendererContainer, IPoolableNote<TapPoolingInfo, TapQueueInfo>
    {
        public RendererStatus RendererState
        {
            get => _rendererState;
            set 
            {
                if (State < NoteStatus.Initialized)
                    return;

                switch(value)
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
        public float RotateSpeed { get; set; } = 0.0000000000000000000000000000001f;
        public bool IsDouble { get; set; } = false;
        public bool IsStar { get; set; } = false;
        public float Distance { get; set; } = -100;

        [SerializeField]
        GameObject _tapLinePrefab;

        GameObject _tapLineObject;
        GameObject _exObject;

        
        SpriteRenderer _thisRenderer;
        SpriteRenderer _exRenderer;
        SpriteRenderer _tapLineRenderer;
        NotePoolManager _notePoolManager;
        BreakShineController? _breakShineController = null;

        const int _spriteSortOrder = 1;
        const int _exSortOrder = 0;


        public void Initialize(TapPoolingInfo poolingInfo)
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
            IsStar = poolingInfo.IsStar;
            IsDouble = poolingInfo.IsDouble;
            RotateSpeed = poolingInfo.RotateSpeed;
            _isJudged = false;
            Distance = -100;
            if (State == NoteStatus.Start)
                Start();

            Transform.rotation = Quaternion.Euler(0, 0, -22.5f + -45f * (StartPos - 1));
            _tapLineObject.transform.rotation = Quaternion.Euler(0, 0, -22.5f + -45f * (StartPos - 1));
            _thisRenderer.sortingOrder = SortOrder - _spriteSortOrder;
            _exRenderer.sortingOrder = SortOrder - _exSortOrder;

            LoadSkin();
            SetActive(true);
            SetTapLineActive(false);
            _sensorPos = (SensorType)(StartPos - 1);
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
            if (!_isJudged || forceEnd) 
                return;

            SetActive(false);
            RendererState = RendererStatus.Off;
            var result = new JudgeResult()
            {
                Result = _judgeResult,
                IsBreak = IsBreak,
                IsEX = IsEX,
                Diff = _judgeDiff
            };
            CanShine = false;
            if (_breakShineController is not null)
                _breakShineController.enabled = false;
            PlayJudgeSFX(result);
            _effectManager.PlayEffect(StartPos, result);
            _noteManager.NextNote(QueueInfo);
            _objectCounter.ReportResult(this, result);
            _notePoolManager.Collect(this);
        }
        protected override void Start()
        {
            if (IsInitialized)
                return;
            base.Start();
            Active = true;
            _notePoolManager = FindObjectOfType<NotePoolManager>();
            _thisRenderer = GetComponent<SpriteRenderer>();

            _exObject = transform.GetChild(0).gameObject;
            _exRenderer = _exObject.GetComponent<SpriteRenderer>();

            _tapLineObject = Instantiate(_tapLinePrefab, _noteManager.gameObject.transform.GetChild(7));
            _tapLineObject.SetActive(true);
            _tapLineRenderer = _tapLineObject.GetComponent<SpriteRenderer>();

            transform.localScale = new Vector3(0, 0);
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
        public override void ComponentFixedUpdate()
        {
            if (State < NoteStatus.Running|| IsDestroyed)
                return;
            var timing = GetTimeSpanToJudgeTiming();
            var isTooLate = timing > 0.15f;
            if (!_isJudged && isTooLate)
            {
                _judgeResult = JudgeType.Miss;
                _isJudged = true;
                End();
            }
            else if (_isJudged)
                End();
        }
        // Update is called once per frame
        public override void ComponentUpdate()
        {
            var timing = GetTimeSpanToArriveTiming();
            var distance = timing * Speed + 4.8f;
            var scaleRate = _gameSetting.Debug.NoteAppearRate;
            var destScale = distance * scaleRate + (1 - (scaleRate * 1.225f));

            switch (State)
            {
                case NoteStatus.Initialized:
                    if (destScale >= 0f)
                    {
                        RendererState = RendererStatus.On;
                        CanShine = true;
                        State = NoteStatus.Scaling;
                        goto case NoteStatus.Scaling;
                    }
                    else
                        transform.localScale = new Vector3(0, 0);
                    return;
                case NoteStatus.Scaling:
                    {
                        if (destScale > 0.3f)
                            SetTapLineActive(true);
                        if (distance < 1.225f)
                        {
                            Distance = distance;
                            Transform.localScale = new Vector3(destScale, destScale);
                            Transform.position = GetPositionFromDistance(1.225f);
                            var lineScale = Mathf.Abs(1.225f / 4.8f);
                            _tapLineObject.transform.localScale = new Vector3(lineScale, lineScale, 1f);
                        }
                        else
                        {
                            State = NoteStatus.Running;
                            goto case NoteStatus.Running;
                        }
                    }
                    break;
                case NoteStatus.Running:
                    {
                        Distance = distance;
                        Transform.position = GetPositionFromDistance(distance);
                        Transform.localScale = new Vector3(1f, 1f);
                        var lineScale = Mathf.Abs(distance / 4.8f);
                        _tapLineObject.transform.localScale = new Vector3(lineScale, lineScale, 1f);
                    }
                    break;
                default:
                    return;
            }
            if(IsStar)
            {
                if (_gpManager.IsStart && _gameSetting.Game.StarRotation)
                    Transform.Rotate(0f, 0f, -180f * Time.deltaTime / RotateSpeed);
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
                    End();
            }
        }
        protected override void LoadSkin()
        {
            if (_breakShineController is null)
                _breakShineController = gameObject.AddComponent<BreakShineController>();

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
            switch(state)
            {
                case true:
                    _exObject.layer = 0;
                    break;
                case false:
                    _exObject.layer = 0;
                    break;
            }
            if (_breakShineController is not null)
                _breakShineController.Active = state;
            SetTapLineActive(state);
            Active = state;
        }
        void SetTapLineActive(bool state)
        {
            switch (state)
            {
                case true:
                    _tapLineObject.layer = 0;
                    break;
                case false:
                    _tapLineObject.layer = 3;
                    break;
            }
        }
        void SubscribeEvent()
        {
            _ioManager.BindArea(Check, _sensorPos);
        }
        void UnsubscribeEvent()
        {
            _ioManager.UnbindArea(Check, _sensorPos);
        }
        void LoadTapSkin()
        {
            var skin = MajInstances.SkinManager.GetTapSkin();
            var renderer = GetComponent<SpriteRenderer>();
            var exRenderer = transform.GetChild(0).GetComponent<SpriteRenderer>();
            var tapLineRenderer = _tapLineObject.GetComponent<SpriteRenderer>();

            renderer.sprite = skin.Normal;
            renderer.material = skin.DefaultMaterial;
            exRenderer.sprite = skin.Ex;
            exRenderer.color = skin.ExEffects[0];
            tapLineRenderer.sprite = skin.NoteLines[0];

            if (_breakShineController is null)
                throw new MissingComponentException(nameof(_breakShineController));
            _breakShineController.enabled = false;
            if (IsEach)
            {
                renderer.sprite = skin.Each;
                tapLineRenderer.sprite = skin.NoteLines[1];
                exRenderer.color = skin.ExEffects[1];

            }

            if (IsBreak)
            {
                renderer.sprite = skin.Break;
                renderer.material = skin.BreakMaterial;
                tapLineRenderer.sprite = skin.NoteLines[2];
                _breakShineController.enabled = true;
                _breakShineController.Parent = this;
                exRenderer.color = skin.ExEffects[2];

            }
        }
        void LoadStarSkin()
        {
            var renderer = GetComponent<SpriteRenderer>();
            var exRenderer = transform.GetChild(0).GetComponent<SpriteRenderer>();
            var tapLineRenderer = _tapLineObject.GetComponent<SpriteRenderer>();
            var skin = MajInstances.SkinManager.GetStarSkin();
            renderer.material = skin.DefaultMaterial;
            exRenderer.color = skin.ExEffects[0];
            tapLineRenderer.sprite = skin.NoteLines[0];

            if (_breakShineController is null)
                throw new MissingComponentException(nameof(_breakShineController));

            _breakShineController.enabled = false;

            if (IsDouble)
            {
                renderer.sprite = skin.Double;
                exRenderer.sprite = skin.ExDouble;

                if (IsEach)
                {
                    renderer.sprite = skin.EachDouble;
                    tapLineRenderer.sprite = skin.NoteLines[1];
                    exRenderer.color = skin.ExEffects[1];
                }
                if (IsBreak)
                {
                    renderer.sprite = skin.BreakDouble;
                    renderer.material = skin.BreakMaterial;
                    tapLineRenderer.sprite = skin.NoteLines[2];
                    _breakShineController.enabled = true;
                    _breakShineController.Parent = this;
                    exRenderer.color = skin.ExEffects[2];
                }
            }
            else
            {
                renderer.sprite = skin.Normal;
                exRenderer.sprite = skin.Ex;

                if (IsEach)
                {
                    renderer.sprite = skin.Each;
                    tapLineRenderer.sprite = skin.NoteLines[1];
                    exRenderer.color = skin.ExEffects[1];
                }
                if (IsBreak)
                {
                    renderer.sprite = skin.Break;
                    renderer.material = skin.BreakMaterial;
                    tapLineRenderer.sprite = skin.NoteLines[2];
                    _breakShineController.enabled = true;
                    _breakShineController.Parent = this;
                    exRenderer.color = skin.ExEffects[2];
                }
            }
        }
        RendererStatus _rendererState = RendererStatus.Off;
    }
}
