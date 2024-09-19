using MajdataPlay.Extensions;
using MajdataPlay.IO;
using MajdataPlay.Types;
using System;
using UnityEngine;
#nullable enable
namespace MajdataPlay.Game.Notes
{
    public sealed class TouchHoldDrop : NoteLongDrop
    {
        public RendererStatus RendererState
        {
            get => _rendererState;
            private set
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

        JudgeTextSkin judgeText;

        // Start is called before the first frame update
        protected override void Start()
        {
            base.Start();
            wholeDuration = 3.209385682f * Mathf.Pow(speed, -0.9549621752f);
            moveDuration = 0.8f * wholeDuration;
            displayDuration = 0.2f * wholeDuration;

            holdEffect = Instantiate(holdEffect, noteManager.transform);
            holdEffect.SetActive(false);

            fans[0] = transform.GetChild(5).gameObject;
            fans[1] = transform.GetChild(4).gameObject;
            fans[2] = transform.GetChild(3).gameObject;
            fans[3] = transform.GetChild(2).gameObject;

            point = transform.GetChild(6).gameObject;
            border = transform.GetChild(1).gameObject;
            pointRenderer = point.GetComponent<SpriteRenderer>();
            borderRenderer = border.GetComponent<SpriteRenderer>();
            mask = transform.GetChild(0).GetComponent<SpriteMask>();

            LoadSkin();

            SetfanColor(new Color(1f, 1f, 1f, 0f));
            mask.enabled = false;
            point.SetActive(false);
            border.SetActive(false);

            sensorPos = SensorType.C;
            var customSkin = SkinManager.Instance;
            judgeText = customSkin.GetJudgeTextSkin();
            SetFansPosition(0.4f);
            ioManager.BindSensor(Check, SensorType.C);
            State = NoteStatus.Initialized;
            RendererState = RendererStatus.Off;
        }
        protected override void Check(object sender, InputEventArgs arg)
        {
            if (isJudged || !noteManager.CanJudge(gameObject, sensorPos))
                return;
            else if (arg.IsClick)
            {
                if (!ioManager.IsIdle(arg))
                    return;
                else
                    ioManager.SetBusy(arg);
                Judge(gpManager.ThisFrameSec);
                ioManager.SetIdle(arg);
                if (isJudged)
                {
                    ioManager.UnbindSensor(Check, SensorType.C);
                    objectCounter.NextTouch(SensorType.C);
                }
            }
        }
        protected override void LoadSkin()
        {
            var skin = SkinManager.Instance.GetTouchHoldSkin();
            for (var i = 0; i < 4; i++)
            {
                fanRenderers[i] = fans[i].GetComponent<SpriteRenderer>();
                fanRenderers[i].sortingOrder += noteSortOrder;
            }
            borderRenderer.sortingOrder += noteSortOrder;

            for (var i = 0; i < 4; i++)
                fanRenderers[i].sprite = skin.Fans[i];
            borderRenderer.sprite = skin.Boader; // TouchHold Border
            pointRenderer.sprite = skin.Point;
            board_On = skin.Boader;
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
        private void FixedUpdate()
        {
            var remainingTime = GetRemainingTime();
            var timing = GetTimeSpanToJudgeTiming();
            var isTooLate = timing > 0.316667f;

            if (remainingTime == 0 && isJudged)
            {
                Destroy(holdEffect);
                Destroy(gameObject);
            }
            
            if (isJudged)
            {
                if (timing <= 0.25f) // 忽略头部15帧
                    return;
                else if (remainingTime <= 0.2f) // 忽略尾部12帧
                    return;
                else if (!gpManager.IsStart) // 忽略暂停
                    return;

                var on = ioManager.CheckSensorStatus(SensorType.C, SensorStatus.On);
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
                objectCounter.NextTouch(SensorType.C);
            }
        }
        // Update is called once per frame
        private void Update()
        {
            var timing = GetTimeSpanToArriveTiming();

            switch(State)
            {
                case NoteStatus.Initialized:
                    if ((-timing).InRange(wholeDuration, moveDuration))
                    {
                        point.SetActive(true);
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
                fans[i].transform.position = pos;
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
        private void OnDestroy()
        {
            ioManager.UnbindSensor(Check, SensorType.C);
            EndJudge(ref judgeResult);
            State = NoteStatus.Destroyed;
            var result = new JudgeResult()
            {
                Result = judgeResult,
                IsBreak = isBreak,
                Diff = judgeDiff
            };
            objectCounter.ReportResult(this, result);
            if (!isJudged)
                objectCounter.NextTouch(SensorType.C);
            if (isFirework && !result.IsMiss)
            {
                effectManager.PlayFireworkEffect(transform.position);
                audioEffMana.PlayHanabiSound();
            }
            audioEffMana.PlayTapSound(false,false,judgeResult);
            audioEffMana.StopTouchHoldSound();

            PlayJudgeEffect(result);
        }
        void PlayJudgeEffect(in JudgeResult judgeResult)
        {
            var obj = Instantiate(judgeEffect, Vector3.zero, transform.rotation);
            var _obj = Instantiate(judgeEffect, Vector3.zero, transform.rotation);
            var judgeObj = obj.transform.GetChild(0);
            var flObj = _obj.transform.GetChild(0);
            var distance = -0.6f;

            judgeObj.transform.position = new Vector3(0, distance, 0);
            flObj.transform.position = new Vector3(0, distance - 0.48f, 0);
            flObj.GetChild(0).transform.rotation = Quaternion.Euler(Vector3.zero);
            judgeObj.GetChild(0).transform.rotation = Quaternion.Euler(Vector3.zero);
            var anim = obj.GetComponent<Animator>();

            var effects = GameObject.Find("NoteEffects");
            var flAnim = _obj.GetComponent<Animator>();
            GameObject effect;
            switch (judgeResult.Result)
            {
                case JudgeType.LateGood:
                case JudgeType.FastGood:
                    judgeObj.GetChild(0).gameObject.GetComponent<SpriteRenderer>().sprite = judgeText.Good;
                    effect = Instantiate(effects.transform.GetChild(3).GetChild(0), transform.position, transform.rotation).gameObject;
                    effect.SetActive(true);
                    break;
                case JudgeType.LateGreat:
                case JudgeType.LateGreat1:
                case JudgeType.LateGreat2:
                case JudgeType.FastGreat2:
                case JudgeType.FastGreat1:
                case JudgeType.FastGreat:
                    judgeObj.GetChild(0).gameObject.GetComponent<SpriteRenderer>().sprite = judgeText.Great;
                    //transform.Rotate(0, 0f, 30f);
                    effect = Instantiate(effects.transform.GetChild(2).GetChild(0), transform.position, transform.rotation).gameObject;
                    effect.SetActive(true);
                    effect.gameObject.GetComponent<Animator>().SetTrigger("great");
                    break;
                case JudgeType.LatePerfect2:
                case JudgeType.FastPerfect2:
                case JudgeType.LatePerfect1:
                case JudgeType.FastPerfect1:
                    judgeObj.GetChild(0).gameObject.GetComponent<SpriteRenderer>().sprite = judgeText.Perfect;
                    transform.Rotate(0, 180f, 90f);
                    Instantiate(tapEffect, transform.position, transform.rotation);
                    break;
                case JudgeType.Perfect:
                    if (GameManager.Instance.Setting.Display.DisplayCriticalPerfect)
                        judgeObj.GetChild(0).gameObject.GetComponent<SpriteRenderer>().sprite = judgeText.CriticalPerfect;
                    else
                        judgeObj.GetChild(0).gameObject.GetComponent<SpriteRenderer>().sprite = judgeText.Perfect;
                    transform.Rotate(0, 180f, 90f);
                    Instantiate(tapEffect, transform.position, transform.rotation);
                    break;
                case JudgeType.Miss:
                    judgeObj.GetChild(0).gameObject.GetComponent<SpriteRenderer>().sprite = judgeText.Miss;
                    break;
                default:
                    break;
            }
            var canPlay = NoteEffectManager.CheckJudgeDisplaySetting(GameManager.Instance.Setting.Display.TouchJudgeType, judgeResult);
            effectManager.PlayFastLate(_obj, flAnim, judgeResult);

            if (canPlay)
                anim.SetTrigger("touch");
            else
                Destroy(obj);
        }
        protected override void PlayHoldEffect()
        {
            base.PlayHoldEffect();
            var audioEffMana = GameObject.Find("NoteAudioManager").GetComponent<NoteAudioManager>();
            audioEffMana.PlayTouchHoldSound();
            borderRenderer.sprite = board_On;
        }
        protected override void StopHoldEffect()
        {
            base.StopHoldEffect();
            var audioEffMana = GameObject.Find("NoteAudioManager").GetComponent<NoteAudioManager>();
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
        RendererStatus _rendererState = RendererStatus.Off;
    }
}