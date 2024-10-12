using MajdataPlay.Extensions;
using MajdataPlay.Game.Controllers;
using MajdataPlay.Interfaces;
using MajdataPlay.IO;
using MajdataPlay.Types;
using MajdataPlay.Utils;
using System;
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

        public void Initialize(TouchHoldPoolingInfo poolingInfo)
        {
            if (State >= NoteStatus.Initialized && State < NoteStatus.Destroyed)
                return;

            startPosition = poolingInfo.StartPos;
            areaPosition = poolingInfo.AreaPos;
            timing = poolingInfo.Timing;
            judgeTiming = timing;
            noteSortOrder = poolingInfo.NoteSortOrder;
            speed = poolingInfo.Speed;
            isEach = poolingInfo.IsEach;
            isBreak = poolingInfo.IsBreak;
            isEX = poolingInfo.IsEX;
            QueueInfo = poolingInfo.QueueInfo;
            isJudged = false;
            LastFor = poolingInfo.LastFor;
            isFirework = poolingInfo.IsFirework;
            sensorPos = poolingInfo.SensorPos;
            if (State == NoteStatus.Start)
                Start();
            else
            {
                wholeDuration = 3.209385682f * Mathf.Pow(speed, -0.9549621752f);
                moveDuration = 0.8f * wholeDuration;
                displayDuration = 0.2f * wholeDuration;

                LoadSkin();

                SetfanColor(new Color(1f, 1f, 1f, 0f));
                mask.enabled = false;
                mask.alphaCutoff = 0;
                point.SetActive(false);
                border.SetActive(false);

                sensorPos = TouchBase.GetSensor(areaPosition, startPosition);
                var pos = TouchBase.GetAreaPos(sensorPos);
                transform.position = pos;
                SetFansPosition(0.4f);
                ioManager.BindSensor(Check, sensorPos);
                State = NoteStatus.Initialized;
                RendererState = RendererStatus.Off;
            }
        }
        public void End(bool forceEnd = false)
        {
            ioManager.UnbindSensor(Check, sensorPos);
            State = NoteStatus.Destroyed;
            if (forceEnd)
                return;
            EndJudge(ref judgeResult);
            var result = new JudgeResult()
            {
                Result = judgeResult,
                IsBreak = isBreak,
                IsEX = isEX,
                Diff = judgeDiff
            };
            DisableBreakShine();
            CanShine = false;
            point.SetActive(false);
            RendererState = RendererStatus.Off;

            objectCounter.ReportResult(this, result);
            if (!isJudged)
                noteManager.NextTouch(QueueInfo);
            if (isFirework && !result.IsMiss)
            {
                effectManager.PlayFireworkEffect(transform.position);
                audioEffMana.PlayHanabiSound();
            }
            audioEffMana.PlayTapSound(result);
            audioEffMana.StopTouchHoldSound();

            effectManager.PlayTouchHoldEffect(sensorPos, result);
            effectManager.ResetHoldEffect(sensorPos);
            notePoolManager.Collect(this);
        }
        protected override void Start()
        {
            base.Start();
            wholeDuration = 3.209385682f * Mathf.Pow(speed, -0.9549621752f);
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

            sensorPos = TouchBase.GetSensor(areaPosition,startPosition);
            var pos = TouchBase.GetAreaPos(sensorPos);
            transform.position = pos;
            SetFansPosition(0.4f);
            ioManager.BindSensor(Check, sensorPos);
            State = NoteStatus.Initialized;
            RendererState = RendererStatus.Off;
        }
        protected override void Check(object sender, InputEventArgs arg)
        {
            if (isJudged || !noteManager.CanJudge(QueueInfo))
                return;
            else if (arg.IsClick)
            {
                if (!ioManager.IsIdle(arg))
                    return;
                else
                    ioManager.SetBusy(arg);
                Judge(gpManager.ThisFrameSec);
                //ioManager.SetIdle(arg);
                if (isJudged)
                {
                    ioManager.UnbindSensor(Check, sensorPos);
                    noteManager.NextTouch(QueueInfo);
                }
            }
        }
        protected override void LoadSkin()
        {
            var skin = MajInstances.SkinManager.GetTouchHoldSkin();
            for (var i = 0; i < 4; i++)
            {
                fanRenderers[i] = fans[i].GetComponent<SpriteRenderer>();
                fanRenderers[i].sortingOrder += noteSortOrder;
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
            borderRenderer.sortingOrder += noteSortOrder;
            DisableBreakShine();
            SetFansMaterial(skin.DefaultMaterial);
            if(isBreak)
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

            const float JUDGE_SEG_PERFECT = 150f;

            if (isJudged)
                return;

            var timing = currentSec - JudgeTiming;
            var isFast = timing < 0;
            judgeDiff = timing * 1000;
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

            judgeResult = result;
            isJudged = true;
            PlayHoldEffect();
        }
        void FixedUpdate()
        {
            if (State < NoteStatus.Running || IsDestroyed)
                return;
            var remainingTime = GetRemainingTime();
            var timing = GetTimeSpanToJudgeTiming();
            var isTooLate = timing > 0.316667f;

            if (remainingTime == 0 && isJudged)
                End();

            if (isJudged)
            {
                if (timing <= 0.25f) // 忽略头部15帧
                    return;
                else if (remainingTime <= 0.2f) // 忽略尾部12帧
                    return;
                else if (!gpManager.IsStart) // 忽略暂停
                    return;

                var on = ioManager.CheckSensorStatus(sensorPos, SensorStatus.On);
                if (on)
                    PlayHoldEffect();
                else
                {
                    playerIdleTime += Time.fixedDeltaTime;
                    StopHoldEffect();
                }
            }
            else if (isTooLate)
            {
                judgeDiff = 316.667f;
                judgeResult = JudgeType.Miss;
                ioManager.UnbindSensor(Check, SensorType.C);
                isJudged = true;
                noteManager.NextTouch(QueueInfo);
            }
        }
        void Update()
        {
            var timing = GetTimeSpanToArriveTiming();

            switch(State)
            {
                case NoteStatus.Initialized:
                    if ((-timing).InRange(wholeDuration, moveDuration))
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
                        var pow = -Mathf.Exp(8 * (timing * 0.4f / moveDuration) - 0.85f) + 0.42f;
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
                        var value = 0.91f * (1 - (LastFor - timing) / LastFor);
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
            if (!isJudged) 
                return;
            var offset = (int)judgeResult > 7 ? 0 : judgeDiff;
            var realityHT = LastFor - 0.45f - offset / 1000f;
            var percent = MathF.Min(1, (realityHT - playerIdleTime) / realityHT);
            result = judgeResult;
            if (realityHT > 0)
            {
                if (percent >= 1f)
                {
                    if (judgeResult == JudgeType.Miss)
                        result = JudgeType.LateGood;
                    else if (MathF.Abs((int)judgeResult - 7) == 6)
                        result = (int)judgeResult < 7 ? JudgeType.LateGreat : JudgeType.FastGreat;
                    else
                        result = judgeResult;
                }
                else if (percent >= 0.67f)
                {
                    if (judgeResult == JudgeType.Miss)
                        result = JudgeType.LateGood;
                    else if (MathF.Abs((int)judgeResult - 7) == 6)
                        result = (int)judgeResult < 7 ? JudgeType.LateGreat : JudgeType.FastGreat;
                    else if (judgeResult == JudgeType.Perfect)
                        result = (int)judgeResult < 7 ? JudgeType.LatePerfect1 : JudgeType.FastPerfect1;
                }
                else if (percent >= 0.33f)
                {
                    if (MathF.Abs((int)judgeResult - 7) >= 6)
                        result = (int)judgeResult < 7 ? JudgeType.LateGood : JudgeType.FastGood;
                    else
                        result = (int)judgeResult < 7 ? JudgeType.LateGreat : JudgeType.FastGreat;
                }
                else if (percent >= 0.05f)
                    result = (int)judgeResult < 7 ? JudgeType.LateGood : JudgeType.FastGood;
                else if (percent >= 0)
                {
                    if (judgeResult == JudgeType.Miss)
                        result = JudgeType.Miss;
                    else
                        result = (int)judgeResult < 7 ? JudgeType.LateGood : JudgeType.FastGood;
                }
            }
            print($"TouchHold: {MathF.Round(percent * 100, 2)}%\nTotal Len : {MathF.Round(realityHT * 1000, 2)}ms");
        }
        void PlayHoldEffect()
        {
            if(isBreak)
            {
                foreach(var fanRenderer in fanRenderers)
                {
                    fanRenderer.material.SetFloat("_Brightness", 1);
                    fanRenderer.material.SetFloat("_Contrast", 1);
                }
                DisableBreakShine();
            }
            CanShine = false;
            effectManager.PlayHoldEffect(sensorPos, judgeResult);
            audioEffMana.PlayTouchHoldSound();
            borderRenderer.sprite = board_On;
        }
        void StopHoldEffect()
        {
            if (isBreak)
            {
                foreach (var fanRenderer in fanRenderers)
                {
                    fanRenderer.material.SetFloat("_Brightness", 1);
                    fanRenderer.material.SetFloat("_Contrast", 1);
                }
                DisableBreakShine();
            }
            CanShine = false;
            effectManager.ResetHoldEffect(sensorPos);
            audioEffMana.StopTouchHoldSound();
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


        RendererStatus _rendererState = RendererStatus.Off;
    }
}