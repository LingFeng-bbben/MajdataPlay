using MajdataPlay.Buffers;
using MajdataPlay.Extensions;
using MajdataPlay.Game.Controllers;
using MajdataPlay.Interfaces;
using MajdataPlay.IO;
using MajdataPlay.Types;
using MajdataPlay.Utils;
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
#nullable enable
namespace MajdataPlay.Game.Notes
{
    public sealed class TouchHoldDrop : NoteLongDrop, INoteQueueMember<TouchQueueInfo>, IRendererContainer,IPoolableNote<TouchHoldPoolingInfo, TouchQueueInfo>
    {
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
                        foreach (var renderer in fanRenderers)
                            renderer.forceRenderingOff = true;
                        borderRenderer.forceRenderingOff = true;
                        mask.forceRenderingOff = true;
                        break;
                    case RendererStatus.On:
                        foreach (var renderer in fanRenderers)
                            renderer.forceRenderingOff = false;
                        borderRenderer.forceRenderingOff = false;
                        mask.forceRenderingOff = false;
                        break;
                    default:
                        return;
                }
                _rendererState = value;
            }
        }
        public char areaPosition;
        public bool isFirework;
        public GameObject tapEffect;
        public GameObject judgeEffect;

        Sprite board_On;
        Sprite board_Off;

        GameObject[] fans = new GameObject[4];
        readonly SpriteRenderer[] fanRenderers = new SpriteRenderer[4];

        float displayDuration;
        float moveDuration;
        float wholeDuration;

        GameObject point;
        GameObject border;
        SpriteMask mask;
        SpriteRenderer pointRenderer;
        SpriteRenderer borderRenderer;
        NotePoolManager notePoolManager;
        BreakShineController?[] breakShineControllers = new BreakShineController[4];

        const int _fanSpriteSortOrder = 2;
        const int _borderSortOrder = 6;
        const int _pointBorderSortOrder = 1;

        protected override async void Autoplay()
        {
            while (!_isJudged)
            {
                if (_gpManager is null)
                    return;
                else if (GetTimeSpanToJudgeTiming() >= 0)
                {
                    _judgeResult = JudgeType.Perfect;
                    _isJudged = true;
                    _judgeDiff = 0;
                    PlayJudgeSFX(new JudgeResult()
                    {
                        Result = _judgeResult,
                        IsBreak = IsBreak,
                        IsEX = IsEX,
                        Diff = _judgeDiff
                    });
                    PlayHoldEffect();
                }
                await Task.Delay(1);
            }
        }
        public void Initialize(TouchHoldPoolingInfo poolingInfo)
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
            Length = poolingInfo.LastFor;
            isFirework = poolingInfo.IsFirework;
            _sensorPos = poolingInfo.SensorPos;
            if (State == NoteStatus.Start)
                Start();
            else
            {
                wholeDuration = 3.209385682f * Mathf.Pow(Speed, -0.9549621752f);
                moveDuration = 0.8f * wholeDuration;
                displayDuration = 0.2f * wholeDuration;

                LoadSkin();

                SetfanColor(new Color(1f, 1f, 1f, 0f));
                mask.enabled = false;
                mask.alphaCutoff = 0;
                point.SetActive(false);
                border.SetActive(false);

                _sensorPos = TouchBase.GetSensor(areaPosition, StartPos);
                var pos = TouchBase.GetAreaPos(_sensorPos);
                transform.position = pos;
                SetFansPosition(0.4f);
                RendererState = RendererStatus.Off;
            }
            for (var i = 0; i < 4; i++)
                fanRenderers[i].sortingOrder = SortOrder - (_fanSpriteSortOrder + i);
            pointRenderer.sortingOrder = SortOrder - _pointBorderSortOrder;
            borderRenderer.sortingOrder = SortOrder - _borderSortOrder;

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
            if (forceEnd)
                return;
            EndJudge(ref _judgeResult);
            ConvertJudgeResult(ref _judgeResult);
            var result = new JudgeResult()
            {
                Result = _judgeResult,
                IsBreak = IsBreak,
                IsEX = IsEX,
                Diff = _judgeDiff
            };
            DisableBreakShine();
            CanShine = false;
            point.SetActive(false);
            RendererState = RendererStatus.Off;

            _objectCounter.ReportResult(this, result);
            if (!_isJudged)
                _noteManager.NextTouch(QueueInfo);
            if (isFirework && !result.IsMiss)
                _effectManager.PlayFireworkEffect(transform.position);

            PlayJudgeSFX(result);
            _audioEffMana.StopTouchHoldSound();
            _effectManager.PlayTouchHoldEffect(_sensorPos, result);
            _effectManager.ResetHoldEffect(_sensorPos);
            notePoolManager.Collect(this);
        }
        protected override void Start()
        {
            base.Start();
            wholeDuration = 3.209385682f * Mathf.Pow(Speed, -0.9549621752f);
            moveDuration = 0.8f * wholeDuration;
            displayDuration = 0.2f * wholeDuration;

            fans[0] = transform.GetChild(5).gameObject;
            fans[1] = transform.GetChild(4).gameObject;
            fans[2] = transform.GetChild(3).gameObject;
            fans[3] = transform.GetChild(2).gameObject;

            point = transform.GetChild(6).gameObject;
            border = transform.GetChild(1).gameObject;
            pointRenderer = point.GetComponent<SpriteRenderer>();
            borderRenderer = border.GetComponent<SpriteRenderer>();
            mask = transform.GetChild(0).GetComponent<SpriteMask>();

            notePoolManager = FindObjectOfType<NotePoolManager>();

            LoadSkin();

            SetfanColor(new Color(1f, 1f, 1f, 0f));
            mask.enabled = false;
            point.SetActive(false);
            border.SetActive(false);

            _sensorPos = TouchBase.GetSensor(areaPosition,StartPos);
            var pos = TouchBase.GetAreaPos(_sensorPos);
            transform.position = pos;
            SetFansPosition(0.4f);
            State = NoteStatus.Initialized;
            RendererState = RendererStatus.Off;
        }
        protected override void Check(object sender, InputEventArgs arg)
        {
            if (_isJudged || !_noteManager.CanJudge(QueueInfo))
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
                {
                    _ioManager.UnbindSensor(Check, _sensorPos);
                    _noteManager.NextTouch(QueueInfo);
                }
            }
        }
        protected override void LoadSkin()
        {
            var skin = MajInstances.SkinManager.GetTouchHoldSkin();
            for (var i = 0; i < 4; i++)
            {
                fanRenderers[i] = fans[i].GetComponent<SpriteRenderer>();
                
                var controller = breakShineControllers[i];
                if (controller is null)
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
            if(IsBreak)
            {
                EnableBreakShine();
                for (var i = 0; i < 4; i++)
                    fanRenderers[i].sprite = skin.Fans_Break[i];
                borderRenderer.sprite = skin.Boader_Break; // TouchHold Border
                pointRenderer.sprite = skin.Point_Break;
                board_On = skin.Boader_Break;
                SetFansMaterial(skin.BreakMaterial);
            }
            else
            {
                for (var i = 0; i < 4; i++)
                    fanRenderers[i].sprite = skin.Fans[i];
                borderRenderer.sprite = skin.Boader; // TouchHold Border
                pointRenderer.sprite = skin.Point;
                board_On = skin.Boader;
            }
            board_Off = skin.Off;
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

            JudgeType result = diff switch
            {
                < JUDGE_SEG_PERFECT1 => JudgeType.Perfect,
                < JUDGE_SEG_PERFECT2 => JudgeType.LatePerfect1,
                < JUDGE_PERFECT_AREA => JudgeType.LatePerfect2,
                < JUDGE_SEG_GREAT1 => JudgeType.LateGreat,
                < JUDGE_SEG_GREAT2 => JudgeType.LateGreat1,
                < JUDGE_GREAT_AREA => JudgeType.LateGreat2,
                < JUDGE_GOOD_AREA => JudgeType.LateGood,
                _ => JudgeType.Miss
            };

            ConvertJudgeResult(ref result);
            _judgeResult = result;
            _isJudged = true;
            PlayHoldEffect();
        }
        void FixedUpdate()
        {
            if (State < NoteStatus.Running || IsDestroyed)
                return;
            var remainingTime = GetRemainingTime();
            var timing = GetTimeSpanToJudgeTiming();
            var isTooLate = timing > 0.316667f;

            if (remainingTime == 0 && _isJudged)
                End();

            if (_isJudged)
            {
                if (timing <= 0.25f) // 忽略头部15帧
                    return;
                else if (remainingTime <= 0.2f) // 忽略尾部12帧
                    return;
                else if (!_gpManager.IsStart) // 忽略暂停
                    return;

                var on = _ioManager.CheckSensorStatus(_sensorPos, SensorStatus.On);
                if (on || _gpManager.IsAutoplay)
                    PlayHoldEffect();
                else
                {
                    _playerIdleTime += Time.fixedDeltaTime;
                    StopHoldEffect();
                }
            }
            else if (isTooLate)
            {
                _judgeDiff = 316.667f;
                _judgeResult = JudgeType.Miss;
                _ioManager.UnbindSensor(Check, SensorType.C);
                _isJudged = true;
                _noteManager.NextTouch(QueueInfo);
            }
        }
        void Update()
        {
            var timing = GetTimeSpanToArriveTiming();

            switch(State)
            {
                case NoteStatus.Initialized:
                    if ((-timing).InRange(moveDuration, wholeDuration))
                    {
                        point.SetActive(true);
                        RendererState = RendererStatus.On;
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
                            SetfanColor(Color.white);
                            State = NoteStatus.Running;
                            goto case NoteStatus.Running;
                        }
                        var alpha = ((wholeDuration + timing) / displayDuration).Clamp(0, 1);
                        newColor.a = alpha;
                        SetfanColor(newColor);
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
                            border.SetActive(true);
                            mask.enabled = true;
                            State = NoteStatus.End;
                            goto case NoteStatus.End;
                        }
                        else
                            SetFansPosition(distance);
                    }
                    return;
                case NoteStatus.End:
                    {
                        var value = 0.91f * (1 - (Length - timing) / Length);
                        var alpha = value.Clamp(0, 1f);
                        mask.alphaCutoff = alpha;
                    }
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
        void EndJudge(ref JudgeType result)
        {
            if (!_isJudged) 
                return;
            var offset = (int)_judgeResult > 7 ? 0 : _judgeDiff;
            var realityHT = Length - 0.45f - offset / 1000f;
            var percent = MathF.Min(1, (realityHT - _playerIdleTime) / realityHT);
            result = _judgeResult;
            if (realityHT > 0)
            {
                if (percent >= 1f)
                {
                    if (_judgeResult == JudgeType.Miss)
                        result = JudgeType.LateGood;
                    else if (MathF.Abs((int)_judgeResult - 7) == 6)
                        result = (int)_judgeResult < 7 ? JudgeType.LateGreat : JudgeType.FastGreat;
                    else
                        result = _judgeResult;
                }
                else if (percent >= 0.67f)
                {
                    if (_judgeResult == JudgeType.Miss)
                        result = JudgeType.LateGood;
                    else if (MathF.Abs((int)_judgeResult - 7) == 6)
                        result = (int)_judgeResult < 7 ? JudgeType.LateGreat : JudgeType.FastGreat;
                    else if (_judgeResult == JudgeType.Perfect)
                        result = (int)_judgeResult < 7 ? JudgeType.LatePerfect1 : JudgeType.FastPerfect1;
                }
                else if (percent >= 0.33f)
                {
                    if (MathF.Abs((int)_judgeResult - 7) >= 6)
                        result = (int)_judgeResult < 7 ? JudgeType.LateGood : JudgeType.FastGood;
                    else
                        result = (int)_judgeResult < 7 ? JudgeType.LateGreat : JudgeType.FastGreat;
                }
                else if (percent >= 0.05f)
                    result = (int)_judgeResult < 7 ? JudgeType.LateGood : JudgeType.FastGood;
                else if (percent >= 0)
                {
                    if (_judgeResult == JudgeType.Miss)
                        result = JudgeType.Miss;
                    else
                        result = (int)_judgeResult < 7 ? JudgeType.LateGood : JudgeType.FastGood;
                }
            }
            print($"TouchHold: {MathF.Round(percent * 100, 2)}%\nTotal Len : {MathF.Round(realityHT * 1000, 2)}ms");
        }
        void PlayHoldEffect()
        {
            if(IsBreak)
            {
                foreach(var fanRenderer in fanRenderers)
                {
                    fanRenderer.material.SetFloat("_Brightness", 1);
                    fanRenderer.material.SetFloat("_Contrast", 1);
                }
                DisableBreakShine();
            }
            CanShine = false;
            _effectManager.PlayHoldEffect(_sensorPos, _judgeResult);
            _audioEffMana.PlayTouchHoldSound();
            borderRenderer.sprite = board_On;
        }
        void StopHoldEffect()
        {
            if (IsBreak)
            {
                foreach (var fanRenderer in fanRenderers)
                {
                    fanRenderer.material.SetFloat("_Brightness", 1);
                    fanRenderer.material.SetFloat("_Contrast", 1);
                }
                DisableBreakShine();
            }
            CanShine = false;
            _effectManager.ResetHoldEffect(_sensorPos);
            _audioEffMana.StopTouchHoldSound();
            borderRenderer.sprite = board_Off;
        }
        Vector3 GetAngle(int index)
        {
            var angle = Mathf.PI / 4 + index * (Mathf.PI / 2);
            return new Vector3(Mathf.Sin(angle), Mathf.Cos(angle));
        }
        void SetfanColor(Color color)
        {
            foreach (var fan in fanRenderers) fan.color = color;
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
        void SubscribeEvent()
        {
            _ioManager.BindSensor(Check, _sensorPos);
        }
        void UnsubscribeEvent()
        {
            _ioManager.UnbindSensor(Check, _sensorPos);
        }
        protected override void PlaySFX()
        {
            _audioEffMana.PlayTouchHoldSound();
        }
        protected override void PlayJudgeSFX(in JudgeResult judgeResult)
        {
            if (judgeResult.IsMiss)
                return;
            if (judgeResult.IsBreak)
                _audioEffMana.PlayTapSound(judgeResult);
            else
                _audioEffMana.PlayTouchSound();
            if (isFirework)
                _audioEffMana.PlayHanabiSound();
        }

        RendererStatus _rendererState = RendererStatus.Off;
    }
}