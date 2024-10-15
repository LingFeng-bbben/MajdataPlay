using MajdataPlay.Buffers;
using MajdataPlay.Extensions;
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
                        foreach (var renderer in fanRenderers)
                            renderer.forceRenderingOff = true;
                        break;
                    case RendererStatus.On:
                        foreach (var renderer in fanRenderers)
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

        readonly SpriteRenderer[] fanRenderers = new SpriteRenderer[4];
        readonly GameObject[] fans = new GameObject[4];

        GameObject point;
        SpriteRenderer pointRenderer;
        GameObject justBorder;
        SpriteRenderer justBorderRenderer;
        MultTouchHandler multTouchHandler;
        NotePoolManager notePoolManager;
        BreakShineController?[] breakShineControllers = new BreakShineController[4];

        public void Initialize(TouchPoolingInfo poolingInfo)
        {
            if (State >= NoteStatus.Initialized && State < NoteStatus.Destroyed)
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
            if (State == NoteStatus.Start)
                Start();
            else
            {
                wholeDuration = 3.209385682f * Mathf.Pow(Speed, -0.9549621752f);
                moveDuration = 0.8f * wholeDuration;
                displayDuration = 0.2f * wholeDuration;

                LoadSkin();

                transform.position = GetAreaPos(StartPos, areaPosition);
                point.SetActive(false);
                justBorder.SetActive(false);

                SetFansColor(new Color(1f, 1f, 1f, 0f));
                _ioManager.BindSensor(Check, GetSensor());
                _sensorPos = GetSensor();
                SetFansPosition(0.4f);
                State = NoteStatus.Initialized;
                RendererState = RendererStatus.Off;
            }
        }
        public void End(bool forceEnd = false)
        {
            _ioManager.UnbindSensor(Check, GetSensor());
            State = NoteStatus.Destroyed;
            if (!_isJudged || forceEnd)
                return;

            multTouchHandler.Unregister(_sensorPos);
            var result = new JudgeResult()
            {
                Result = _judgeResult,
                Diff = _judgeDiff,
                IsEX = IsEX,
                IsBreak = IsBreak
            };
            // disable SpriteRenderer
            RendererState = RendererStatus.Off;
            DisableBreakShine();
            CanShine = false;
            point.SetActive(false);
            justBorder.SetActive(false);

            _effectManager.PlayTouchEffect(_sensorPos, result);

            if (GroupInfo is not null && _judgeResult != JudgeType.Miss)
            {
                GroupInfo.JudgeResult = _judgeResult;
                GroupInfo.JudgeDiff = _judgeDiff;
                GroupInfo.RegisterResult(_judgeResult);
            }

            if (_judgeResult != JudgeType.Miss)
                _audioEffMana.PlayTouchSound();
            _objectCounter.ReportResult(this, result);
            _noteManager.NextTouch(QueueInfo);

            if (isFirework && _judgeResult != JudgeType.Miss)
            {
                _effectManager.PlayFireworkEffect(transform.position);
                _audioEffMana.PlayHanabiSound();
            }
            notePoolManager.Collect(this);
        }
        protected override void Start()
        {
            if (IsInitialized)
                return;
            base.Start();
            wholeDuration = 3.209385682f * Mathf.Pow(Speed, -0.9549621752f);
            moveDuration = 0.8f * wholeDuration;
            displayDuration = 0.2f * wholeDuration;

            notePoolManager = FindObjectOfType<NotePoolManager>();
            multTouchHandler = GameObject.Find("MultTouchHandler").GetComponent<MultTouchHandler>();
            
            fans[0] = transform.GetChild(3).gameObject;
            fans[1] = transform.GetChild(2).gameObject;
            fans[2] = transform.GetChild(1).gameObject;
            fans[3] = transform.GetChild(4).gameObject;

            point = transform.GetChild(0).gameObject;
            pointRenderer = point.GetComponent<SpriteRenderer>();
            justBorder = transform.GetChild(5).gameObject;
            justBorderRenderer = justBorder.GetComponent<SpriteRenderer>();

            LoadSkin();
            transform.position = GetAreaPos(StartPos, areaPosition);
            point.SetActive(false);
            justBorder.SetActive(false);
            
            SetFansColor(new Color(1f, 1f, 1f, 0f));
            _ioManager.BindSensor(Check, GetSensor());
            _sensorPos = GetSensor();
            SetFansPosition(0.4f);
            State = NoteStatus.Initialized;
            RendererState = RendererStatus.Off;
        }
        protected override void LoadSkin()
        {
            var skin = MajInstances.SkinManager.GetTouchSkin();
            for (var i = 0; i < 4; i++)
            {
                fanRenderers[i] = fans[i].GetComponent<SpriteRenderer>();
                fanRenderers[i].sortingOrder += SortOrder;
                var controller = breakShineControllers[i];
                if(controller is null)
                {
                    controller = gameObject.AddComponent<BreakShineController>();
                    controller.enabled = false;
                    controller.Parent = this;
                    controller.Renderer = fanRenderers[i];
                    breakShineControllers[i] = controller;
                }
            }
            DisableBreakShine();
            SetFansMaterial(skin.DefaultMaterial);
            if (IsBreak)
            {
                EnableBreakShine();
                SetFansSprite(skin.Break);
                SetFansMaterial(skin.BreakMaterial);
                pointRenderer.sprite = skin.Point_Break;
            }
            else if (IsEach)
            {
                SetFansSprite(skin.Each);
                pointRenderer.sprite = skin.Point_Each;
            }
            else
            {
                SetFansSprite(skin.Normal);
                pointRenderer.sprite = skin.Point_Normal;
            }

            justBorderRenderer.sprite = skin.JustBorder;
        }
        protected override void Check(object sender, InputEventArgs arg)
        {
            var type = GetSensor();
            if (State < NoteStatus.Running)
                return;
            else if (arg.Type != type)
                return;
            else if (_isJudged || !_noteManager.CanJudge(QueueInfo))
                return;
            else if (arg.IsClick)
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
        private void FixedUpdate()
        {
            if (State < NoteStatus.Running || IsDestroyed)
                return;
            var isTooLate = GetTimeSpanToJudgeTiming() >= 0.316667f;
            if (!_isJudged && !isTooLate)
            {
                if (GroupInfo is not null)
                {
                    if (GroupInfo.Percent > 0.5f && GroupInfo.JudgeResult != null)
                    {
                        _isJudged = true;
                        _judgeResult = (JudgeType)GroupInfo.JudgeResult;
                        _judgeDiff = GroupInfo.JudgeDiff;
                        End();
                    }
                }
            }
            else if (!_isJudged)
            {
                _judgeResult = JudgeType.Miss;
                _isJudged = true;
                End();
            }
            else if (_isJudged)
                End();
        }
        protected override void Judge(float currentSec)
        {

            const float JUDGE_GOOD_AREA = 316.667f;
            const int JUDGE_GREAT_AREA = 250;
            const int JUDGE_PERFECT_AREA = 200;

            const float JUDGE_SEG_PERFECT = 150f;

            if (_isJudged)
                return;

            var timing = currentSec - JudgeTiming;
            var isFast = timing < 0;
            _judgeDiff = timing * 1000;
            var diff = MathF.Abs(timing * 1000);
            JudgeType result;
            if (diff > JUDGE_SEG_PERFECT && isFast)
                return;
            else if (diff < JUDGE_SEG_PERFECT)
                result = JudgeType.Perfect;
            else if (diff < JUDGE_PERFECT_AREA)
                result = JudgeType.LatePerfect2;
            else if (diff < JUDGE_GREAT_AREA)
                result = JudgeType.LateGreat;
            else if (diff < JUDGE_GOOD_AREA)
                result = JudgeType.LateGood;
            else
                result = JudgeType.Miss;

            _judgeResult = result;
            _isJudged = true;
        }
        void Update()
        {
            var timing = GetTimeSpanToArriveTiming();
            
            switch(State)
            {
                case NoteStatus.Initialized:
                    if((-timing).InRange(moveDuration, wholeDuration))
                    {
                        multTouchHandler.Register(_sensorPos,IsEach,IsBreak);
                        RendererState = RendererStatus.On;
                        point.SetActive(true);
                        CanShine = true;
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
                        var pow = -Mathf.Exp(8 * (timing * 0.425f / moveDuration) - 0.85f) + 0.42f;
                        var distance = Mathf.Clamp(pow, 0f, 0.4f);
                        if (float.IsNaN(distance)) 
                            distance = 0f;

                        if (timing > -0.02f)
                            justBorder.SetActive(true);
                        if (timing >= 0)
                        {
                            var _pow = -Mathf.Exp(-0.85f) + 0.42f;
                            var _distance = Mathf.Clamp(_pow, 0f, 0.4f);
                            SetFansPosition(_distance);
                            State = NoteStatus.End;
                        }
                        else
                            SetFansPosition(distance);
                    }
                    return;
                case NoteStatus.End:
                    return;
            }
        }
        void SetFansPosition(in float distance)
        {
            for (var i = 0; i < 4; i++)
            {
                var pos = (0.226f + distance) * GetAngle(i);
                fans[i].transform.localPosition = pos;
            }
        }
        Vector3 GetAngle(int index)
        {
            var angle = index * (Mathf.PI / 2);
            return new Vector3(Mathf.Sin(angle), Mathf.Cos(angle));
        }
        void SetFansColor(Color color)
        {
            foreach (var fan in fanRenderers) fan.color = color;
        }
        void SetFansSprite(Sprite sprite)
        {
            for (var i = 0; i < 4; i++) 
                fanRenderers[i].sprite = sprite;
        }
        void SetFansMaterial(Material material)
        {
            for (var i = 0; i < 4; i++)
                fanRenderers[i].material = material;
        }
        void DisableBreakShine()
        {
            for (var i = 0; i < 4; i++)
            {
                var controller = breakShineControllers[i];
                if (controller is not null)
                    controller.enabled = false;
            }
        }
        void EnableBreakShine()
        {
            for (var i = 0; i < 4; i++)
            {
                var controller = breakShineControllers[i];
                if (controller is not null)
                    controller.enabled = true;
            }
        }

        RendererStatus _rendererState = RendererStatus.Off;
    }
}