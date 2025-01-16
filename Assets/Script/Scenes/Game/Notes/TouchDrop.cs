using MajdataPlay.Extensions;
using MajdataPlay.Game.Buffers;
using MajdataPlay.Game.Controllers;
using MajdataPlay.Game.Types;
using MajdataPlay.IO;
using MajdataPlay.Types;
using MajdataPlay.Utils;
using System;
using UnityEngine;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ProgressBar;
#nullable enable
namespace MajdataPlay.Game.Notes
{
    public sealed class TouchDrop : TouchBase , IRendererContainer, IPoolableNote<TouchPoolingInfo,TouchQueueInfo>
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
        /// <summary>
        /// Undefined
        /// </summary>
        float displayDuration;
        /// <summary>
        /// Touch淡入结束，开始移动
        /// </summary>
        float moveDuration;
        /// <summary>
        /// Touch开始淡入的时刻
        /// </summary>
        float wholeDuration;

        readonly SpriteRenderer[] _fanRenderers = new SpriteRenderer[4];
        readonly GameObject[] _fans = new GameObject[4];
        readonly Transform[] _fanTransforms = new Transform[4];

        GameObject _pointObject;
        GameObject _justBorderObject;

        SpriteRenderer _pointRenderer;
        SpriteRenderer _justBorderRenderer;

        MultTouchHandler _multTouchHandler;
        NotePoolManager _notePoolManager;

        const int _fanSpriteSortOrder = 3;
        const int _justBorderSortOrder = 1;
        const int _pointBorderSortOrder = 2;

        protected override void Awake()
        {
            base.Awake();
            _notePoolManager = FindObjectOfType<NotePoolManager>();
            _multTouchHandler = FindObjectOfType<MultTouchHandler>();

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
            
            if(!IsAutoplay)
                _noteManager.OnGameIOUpdate += GameIOListener;
            RendererState = RendererStatus.Off;
        }
        public void Initialize(TouchPoolingInfo poolingInfo)
        {
            if (State >= NoteStatus.Initialized && State < NoteStatus.End)
                return;

            StartPos = poolingInfo.StartPos;
            areaPosition = poolingInfo.AreaPos;
            Timing = poolingInfo.Timing;
            _judgeTiming = Timing;
            SortOrder = poolingInfo.NoteSortOrder;
            Speed = poolingInfo.Speed;
            IsEach = poolingInfo.IsEach;
            IsBreak = poolingInfo.IsBreak;
            IsEX = poolingInfo.IsEX;
            QueueInfo = poolingInfo.QueueInfo;
            _isJudged = false;
            isFirework = poolingInfo.IsFirework;
            GroupInfo = poolingInfo.GroupInfo;
            _sensorPos = poolingInfo.SensorPos;
            _judgableRange = new(JudgeTiming - 0.15f, JudgeTiming + 0.316667f, ContainsType.Closed);

            wholeDuration = 3.209385682f * Mathf.Pow(Speed, -0.9549621752f);
            moveDuration = 0.8f * wholeDuration;
            displayDuration = 0.2f * wholeDuration;

            LoadSkin();

            Transform.position = GetAreaPos(StartPos, areaPosition);
            //_pointObject.SetActive(false);
            //_justBorderObject.SetActive(false);

            SetFansColor(new Color(1f, 1f, 1f, 0f));
            _sensorPos = GetSensor();
            SetFansPosition(0.4f);
            RendererState = RendererStatus.Off;

            for (var i = 0; i < 4; i++)
                _fanRenderers[i].sortingOrder = SortOrder - (_fanSpriteSortOrder + i);
            _pointRenderer.sortingOrder = SortOrder - _pointBorderSortOrder;
            _justBorderRenderer.sortingOrder= SortOrder - _justBorderSortOrder;

            SetActive(true);
            SetFanActive(false);
            SetJustBorderActive(false);
            SetPointActive(false);

            if (_gpManager.IsAutoplay)
                Autoplay();

            State = NoteStatus.Initialized;
        }
        public void End(bool forceEnd = false)
        {
            State = NoteStatus.End;
            if (!_isJudged || forceEnd)
                return;

            _multTouchHandler.Unregister(_sensorPos);
            var result = new JudgeResult()
            {
                Grade = _judgeResult,
                Diff = _judgeDiff,
                IsEX = IsEX,
                IsBreak = IsBreak
            };
            // disable SpriteRenderer
            RendererState = RendererStatus.Off;
            SetActive(false);
            //_pointObject.SetActive(false);
            //_justBorderObject.SetActive(false);

            
            if (isFirework && !result.IsMissOrTooFast)
                _effectManager.PlayFireworkEffect(transform.position);

            PlayJudgeSFX(result);
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
        void Check()
        {
            if (IsEnded)
                return;
            else if(_isJudged)
            {
                End();
                return;
            }    
        }
        void GameIOListener(GameInputEventArgs args)
        {
            if (_isJudged || IsEnded)
                return;
            else if (args.IsButton)
                return;
            else if (args.Area != _sensorPos)
                return;
            else if (!args.IsClick)
                return;
            else if (!_judgableRange.InRange(ThisFrameSec))
                return;
            else if (!_noteManager.CanJudge(QueueInfo))
                return;

            ref var isUsed = ref args.IsUsed.Target;

            if (isUsed)
                return;
            Judge(ThisFrameSec);

            if (_isJudged)
            {
                isUsed = true;
                _noteManager.NextTouch(QueueInfo);
                RegisterGrade();
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
        public override void ComponentFixedUpdate()
        {
            // Too late check
            if (IsEnded || _isJudged)
                return;

            var isTooLate = GetTimeSpanToJudgeTiming() >= 0.316667f;

            if (!isTooLate)
            {
                if (GroupInfo is not null)
                {
                    if (GroupInfo.Percent > 0.5f && GroupInfo.JudgeResult != null)
                    {
                        _isJudged = true;
                        _judgeResult = (JudgeGrade)GroupInfo.JudgeResult;
                        _judgeDiff = GroupInfo.JudgeDiff;
                        _noteManager.NextTouch(QueueInfo);
                    }
                }
            }
            else
            {
                _judgeResult = JudgeGrade.Miss;
                _isJudged = true;
                _noteManager.NextTouch(QueueInfo);
            }
        }
        public override void ComponentUpdate()
        {
            var timing = GetTimeSpanToArriveTiming();

            Check();

            switch (State)
            {
                case NoteStatus.Initialized:
                    if (-timing < wholeDuration)
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

                        if (timing > -0.02f)
                        {
                            //_justBorderObject.SetActive(true);
                            SetJustBorderActive(true);
                        }
                        if (timing >= 0)
                        {
                            var _pow = -Mathf.Exp(-0.85f) + 0.42f;
                            var _distance = Mathf.Clamp(_pow, 0f, 0.4f);
                            SetFansPosition(_distance);
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

            const float JUDGE_GOOD_AREA = 316.667f;
            const int JUDGE_GREAT_AREA = 250;
            const int JUDGE_PERFECT_AREA = 200;

            const float JUDGE_SEG_PERFECT1 = 150f;
            const float JUDGE_SEG_PERFECT2 = 175f;
            const float JUDGE_SEG_GREAT1 = 216.6667f;
            const float JUDGE_SEG_GREAT2 = 233.3334f;

            if (_isJudged)
                return;

            var timing = currentSec - JudgeTiming;
            var isFast = timing < 0;
            _judgeDiff = timing * 1000;
            var diff = MathF.Abs(timing * 1000);

            if (diff > JUDGE_SEG_PERFECT1 && isFast)
                return;

            JudgeGrade result = diff switch
            {
                <= JUDGE_SEG_PERFECT1 => JudgeGrade.Perfect,
                <= JUDGE_SEG_PERFECT2 => JudgeGrade.LatePerfect1,
                <= JUDGE_PERFECT_AREA => JudgeGrade.LatePerfect2,
                <= JUDGE_SEG_GREAT1 => JudgeGrade.LateGreat,
                <= JUDGE_SEG_GREAT2 => JudgeGrade.LateGreat1,
                <= JUDGE_GREAT_AREA => JudgeGrade.LateGreat2,
                <= JUDGE_GOOD_AREA => JudgeGrade.LateGood,
                _ => JudgeGrade.Miss
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
            switch(state)
            {
                case true:
                    foreach(var fanObj in _fans.AsSpan())
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
            if (judgeResult.IsMissOrTooFast)
                return;
            if (judgeResult.IsBreak)
                _audioEffMana.PlayTapSound(judgeResult);
            else
                _audioEffMana.PlayTouchSound();
            if(isFirework)
                _audioEffMana.PlayHanabiSound();
        }
        RendererStatus _rendererState = RendererStatus.Off;
    }
}