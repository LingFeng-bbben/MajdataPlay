using MajdataPlay.Game.Buffers;
using MajdataPlay.Game.Controllers;
using MajdataPlay.Game.Types;
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

        const int _spriteSortOrder = 1;
        const int _exSortOrder = 0;

        protected override void Awake()
        {
            base.Awake();
            _noteChecker = new(Check);
            _notePoolManager = FindObjectOfType<NotePoolManager>();
            _thisRenderer = GetComponent<SpriteRenderer>();

            _exObject = Transform.GetChild(0).gameObject;
            _exRenderer = _exObject.GetComponent<SpriteRenderer>();

            _tapLineObject = Instantiate(_tapLinePrefab, _noteManager.gameObject.transform.GetChild(7));
            _tapLineObject.SetActive(true);
            _tapLineRenderer = _tapLineObject.GetComponent<SpriteRenderer>();

            Transform.localScale = new Vector3(0, 0);

            base.SetActive(false);
            _tapLineObject.layer = HIDDEN_LAYER;
            _exObject.layer = HIDDEN_LAYER;
            Active = false;
        }
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
            _sensorPos = (SensorType)(StartPos - 1);

            Transform.rotation = Quaternion.Euler(0, 0, -22.5f + -45f * (StartPos - 1));
            Transform.localScale = new Vector3(0, 0);

            _tapLineObject.transform.rotation = Quaternion.Euler(0, 0, -22.5f + -45f * (StartPos - 1));
            _thisRenderer.sortingOrder = SortOrder - _spriteSortOrder;
            _exRenderer.sortingOrder = SortOrder - _exSortOrder;

            LoadSkin();
            SetActive(true);
            SetTapLineActive(false);
            
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
                Grade = _judgeResult,
                IsBreak = IsBreak,
                IsEX = IsEX,
                Diff = _judgeDiff
            };
            PlayJudgeSFX(result);
            _effectManager.PlayEffect(StartPos, result);
            _noteManager.NextNote(QueueInfo);
            _objectCounter.ReportResult(this, result);
            _notePoolManager.Collect(this);
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
            if (State < NoteStatus.Running|| IsDestroyed)
                return;
            var timing = GetTimeSpanToJudgeTiming();
            var isTooLate = timing > 0.15f;
            if (!_isJudged && isTooLate)
            {
                _judgeResult = JudgeGrade.Miss;
                _isJudged = true;
                End();
            }
            else if (_isJudged)
                End();
        }
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
                        Transform.position = GetPositionFromDistance(1.225f);
                        _tapLineObject.transform.localScale = new Vector3(1.225f / 4.8f, 1.225f / 4.8f, 1f);

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
                    {
                        if (destScale > 0.3f)
                            SetTapLineActive(true);
                        if (distance < 1.225f)
                        {
                            Distance = distance;
                            Transform.localScale = new Vector3(destScale, destScale);
                            //Transform.position = GetPositionFromDistance(1.225f);
                            //var lineScale = Mathf.Abs(1.225f / 4.8f);
                            //_tapLineObject.transform.localScale = new Vector3(lineScale, lineScale, 1f);
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
                    Transform.Rotate(0f, 0f, RotateSpeed * Time.deltaTime);
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
                    _exObject.layer = DEFAULT_LAYER;
                    break;
                case false:
                    _exObject.layer = HIDDEN_LAYER;
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
                    _tapLineObject.layer = DEFAULT_LAYER;
                    break;
                case false:
                    _tapLineObject.layer = HIDDEN_LAYER;
                    break;
            }
        }
        void SubscribeEvent()
        {
            _ioManager.BindArea(_noteChecker, _sensorPos);
        }
        void UnsubscribeEvent()
        {
            _ioManager.UnbindArea(_noteChecker, _sensorPos);
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
            _tapLineRenderer.sprite = skin.NoteLines[0];

            if (IsEach)
            {
                _thisRenderer.sprite = skin.Each;
                _tapLineRenderer.sprite = skin.NoteLines[1];
                _exRenderer.color = skin.ExEffects[1];
            }

            if (IsBreak)
            {
                _thisRenderer.sprite = skin.Break;
                _thisRenderer.sharedMaterial = BreakMaterial;
                _tapLineRenderer.sprite = skin.NoteLines[2];
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
            _tapLineRenderer.sprite = skin.NoteLines[0];

            if (IsDouble)
            {
                _thisRenderer.sprite = skin.Double;
                _exRenderer.sprite = skin.ExDouble;

                if (IsEach)
                {
                    _thisRenderer.sprite = skin.EachDouble;
                    _tapLineRenderer.sprite = skin.NoteLines[1];
                    _exRenderer.color = skin.ExEffects[1];
                }
                if (IsBreak)
                {
                    _thisRenderer.sprite = skin.BreakDouble;
                    _thisRenderer.sharedMaterial = BreakMaterial;
                    _tapLineRenderer.sprite = skin.NoteLines[2];
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
                    _tapLineRenderer.sprite = skin.NoteLines[1];
                    _exRenderer.color = skin.ExEffects[1];
                }
                if (IsBreak)
                {
                    _thisRenderer.sprite = skin.Break;
                    _thisRenderer.sharedMaterial = BreakMaterial;
                    _tapLineRenderer.sprite = skin.NoteLines[2];
                    _exRenderer.color = skin.ExEffects[2];
                }
            }
        }
        RendererStatus _rendererState = RendererStatus.Off;
    }
}
