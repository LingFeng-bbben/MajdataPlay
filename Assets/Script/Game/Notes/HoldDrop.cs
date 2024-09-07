using MajdataPlay.Game.Controllers;
using MajdataPlay.IO;
using MajdataPlay.Types;
using System;
using UnityEngine;
#nullable enable
namespace MajdataPlay.Game.Notes
{
    public class HoldDrop : NoteLongDrop
    {
        bool holdAnimStart;

        public GameObject tapLine;

        Sprite holdSprite;
        Sprite holdOnSprite;
        Sprite holdOffSprite;

        Animator shineAnimator;

        SpriteRenderer exRenderer;
        SpriteRenderer endRenderer;
        SpriteRenderer thisRenderer;

        BreakShineController? breakShineController = null;

        protected override void Start()
        {
            base.Start();
            var notes = noteManager.gameObject.transform;

            holdEffect = Instantiate(holdEffect, notes);
            tapLine = Instantiate(tapLine, notes);

            holdEffect.SetActive(false);
            tapLine.SetActive(false);

            exRenderer = transform.GetChild(0).GetComponent<SpriteRenderer>();
            thisRenderer = GetComponent<SpriteRenderer>();
            endRenderer = transform.GetChild(1).GetComponent<SpriteRenderer>();
            shineAnimator = gameObject.GetComponent<Animator>();

            LoadSkin();

            thisRenderer.sortingOrder += noteSortOrder;
            exRenderer.sortingOrder += noteSortOrder;
            endRenderer.sortingOrder += noteSortOrder;

            sensorPos = (SensorType)(startPosition - 1);
            ioManager.BindArea(Check, sensorPos);
            transform.localScale = new Vector3(0, 0);

            State = NoteStatus.Initialized;
        }
        private void FixedUpdate()
        {
            if (State < NoteStatus.Running)
                return;

            var timing = GetTimeSpanToJudgeTiming();
            var endTiming = timing - LastFor;
            var remainingTime = GetRemainingTime();
            var isTooLate = timing > 0.15f;

            if (isJudged) // Hold完成后Destroy
            {
                if(IsClassic)
                {
                    if (endTiming >= 0.333334f)
                    {
                        Destroy(tapLine);
                        Destroy(holdEffect);
                        Destroy(gameObject);

                        return;
                    }
                }
                else if(remainingTime == 0)
                {
                    Destroy(tapLine);
                    Destroy(holdEffect);
                    Destroy(gameObject);
                    return;
                }
                
            }
            

            if (isJudged) // 头部判定完成后开始累计按压时长
            {
                if(!IsClassic)
                {
                    if (timing <= 0.1f) // 忽略头部6帧
                        return;
                    else if (remainingTime <= 0.2f) // 忽略尾部12帧
                        return;
                }

                if (!gpManager.isStart) // 忽略暂停
                    return;

                var on = ioManager.CheckAreaStatus(sensorPos, SensorStatus.On);
                if (on)
                {
                    if (remainingTime == 0)
                        base.StopHoldEffect();
                    else
                        PlayHoldEffect();
                            
                }
                else
                {
                    playerIdleTime += Time.fixedDeltaTime;
                    StopHoldEffect();

                    if (IsClassic)
                    {
                        Destroy(tapLine);
                        Destroy(holdEffect);
                        Destroy(gameObject);
                    }
                }
            }
            else if (isTooLate) // 头部Miss
            {
                judgeDiff = 150;
                judgeResult = JudgeType.Miss;
                isJudged = true;
                objectCounter.NextNote(startPosition);
                if (IsClassic)
                {
                    Destroy(tapLine);
                    Destroy(holdEffect);
                    Destroy(gameObject);
                }
            }
        }
        protected override void Check(object sender, InputEventArgs arg)
        {
            if (State < NoteStatus.Running)
                return;
            else if (arg.Type != sensorPos)
                return;
            else if (isJudged || !noteManager.CanJudge(gameObject, startPosition))
                return;
            if (arg.IsClick)
            {
                if (!ioManager.IsIdle(arg))
                    return;
                else
                    ioManager.SetBusy(arg);
                Judge();
                ioManager.SetIdle(arg);
                if (isJudged)
                {
                    ioManager.UnbindArea(Check, sensorPos);
                    objectCounter.NextNote(startPosition);
                }
            }
        }
        void Judge()
        {

            const int JUDGE_GOOD_AREA = 150;
            const int JUDGE_GREAT_AREA = 100;
            const int JUDGE_PERFECT_AREA = 50;

            const float JUDGE_SEG_PERFECT1 = 16.66667f;
            const float JUDGE_SEG_PERFECT2 = 33.33334f;
            const float JUDGE_SEG_GREAT1 = 66.66667f;
            const float JUDGE_SEG_GREAT2 = 83.33334f;

            if (isJudged)
                return;

            var timing = GetTimeSpanToJudgeTiming();
            var isFast = timing < 0;
            judgeDiff = timing * 1000;
            var diff = MathF.Abs(timing * 1000);

            JudgeType result;
            if (diff > JUDGE_GOOD_AREA && isFast)
                return;
            else if (diff < JUDGE_SEG_PERFECT1)
                result = JudgeType.Perfect;
            else if (diff < JUDGE_SEG_PERFECT2)
                result = JudgeType.LatePerfect1;
            else if (diff < JUDGE_PERFECT_AREA)
                result = JudgeType.LatePerfect2;
            else if (diff < JUDGE_SEG_GREAT1)
                result = JudgeType.LateGreat;
            else if (diff < JUDGE_SEG_GREAT2)
                result = JudgeType.LateGreat1;
            else if (diff < JUDGE_GREAT_AREA)
                result = JudgeType.LateGreat;
            else if (diff < JUDGE_GOOD_AREA)
                result = JudgeType.LateGood;
            else
                result = JudgeType.Miss;

            if (result != JudgeType.Miss && isFast)
                result = 14 - result;
            if (result != JudgeType.Miss && isEX)
                result = JudgeType.Perfect;
            if (isFast)
                judgeDiff = 0;
            else
                judgeDiff = diff;

            judgeResult = result;
            isJudged = true;

            var audioEffMana = GameObject.Find("NoteAudioManager").GetComponent<NoteAudioManager>();
            audioEffMana.PlayTapSound(isBreak, isEX, result);
            PlayHoldEffect();
        }
        // Update is called once per frame
        void Update()
        {
            var timing = GetTimeSpanToArriveTiming();
            var distance = timing * speed + 4.8f;
            var scaleRate = gameSetting.Debug.NoteAppearRate;
            var destScale = distance * scaleRate + (1 - (scaleRate * 1.225f));

            var remaining = GetRemainingTimeWithoutOffset();
            var holdTime = timing - LastFor;
            var holdDistance = holdTime * speed + 4.8f;

            switch (State)
            {
                case NoteStatus.Initialized:
                    if (destScale >= 0f)
                    {
                        transform.rotation = Quaternion.Euler(0, 0, -22.5f + -45f * (startPosition - 1));
                        tapLine.transform.rotation = transform.rotation;
                        holdEffect.transform.position = GetPositionFromDistance(4.8f);
                        thisRenderer.size = new Vector2(1.22f, 1.4f);

                        thisRenderer.forceRenderingOff = false;
                        if (isEX) 
                            exRenderer.forceRenderingOff = false;

                        State = NoteStatus.Scaling;
                        goto case NoteStatus.Scaling;
                    }
                    else
                        transform.localScale = new Vector3(0, 0);
                    return;
                case NoteStatus.Scaling:
                    if (destScale > 0.3f)
                        tapLine.SetActive(true);
                    if (distance < 1.225f)
                    {
                        transform.localScale = new Vector3(destScale, destScale);
                        thisRenderer.size = new Vector2(1.22f, 1.42f);
                        distance = 1.225f;
                        var pos = GetPositionFromDistance(distance);
                        tapLine.transform.localScale = new Vector3(0.2552f, 0.2552f, 1f);
                        transform.position = pos;
                    }
                    else
                    {
                        State = NoteStatus.Running;
                        goto case NoteStatus.Running;
                    }
                    break;
                case NoteStatus.Running:
                    if(remaining == 0)
                    {
                        State = NoteStatus.End;
                        goto case NoteStatus.End;
                    }
                    if (holdDistance < 1.225f && distance >= 4.8f) // 头到达 尾未出现
                    {
                        holdDistance = 1.225f;
                        distance = 4.8f;
                    }
                    else if (holdDistance < 1.225f && distance < 4.8f) // 头未到达 尾未出现
                    {
                        holdDistance = 1.225f;
                    }
                    else if (holdDistance >= 1.225f && distance >= 4.8f) // 头到达 尾出现
                    {
                        distance = 4.8f;

                        endRenderer.enabled = true;
                    }
                    else if (holdDistance >= 1.225f && distance < 4.8f) // 头未到达 尾出现
                    {
                        endRenderer.enabled = true;
                    }

                    var dis = (distance - holdDistance) / 2 + holdDistance;
                    var size = distance - holdDistance + 1.4f;
                    var lineScale = Mathf.Abs(distance / 4.8f);

                    lineScale = lineScale >= 1f ? 1f : lineScale;

                    transform.position = GetPositionFromDistance(dis); //0.325
                    tapLine.transform.localScale = new Vector3(lineScale, lineScale, 1f);
                    thisRenderer.size = new Vector2(1.22f, size);
                    endRenderer.transform.localPosition = new Vector3(0f, 0.6825f - size / 2);
                    transform.localScale = new Vector3(1f, 1f);
                    break;
                case NoteStatus.End:
                    var endTiming = timing - LastFor;
                    var endDistance = endTiming * speed + 4.8f;
                    tapLine.transform.localScale = new Vector3(1f, 1f, 1f);

                    if (IsClassic)
                    {
                        var scale = Mathf.Abs(endDistance / 4.8f);
                        transform.position = GetPositionFromDistance(endDistance);
                        tapLine.transform.localScale = new Vector3(scale, scale, 1f);
                    }
                    else
                        transform.position = GetPositionFromDistance(4.8f);
                    break;
            }

            if (isEX)
                exRenderer.size = thisRenderer.size;
        }
        void OnDestroy()
        {
            ioManager.UnbindArea(Check, sensorPos);
            if (!isJudged) return;

            if (IsClassic)
                EndJudge_Classic(ref judgeResult);
            else
                EndJudge(ref judgeResult);

            var audioEffMana = GameObject.Find("NoteAudioManager").GetComponent<NoteAudioManager>();
            audioEffMana.PlayTapSound(false, false, judgeResult);
            var result = new JudgeResult()
            {
                Result = judgeResult,
                IsBreak = isBreak,
                Diff = judgeDiff
            };

            effectManager.PlayEffect(startPosition, result);
            effectManager.PlayFastLate(startPosition, result);
            objectCounter.ReportResult(this, result);
            if (!isJudged)
                objectCounter.NextNote(startPosition);

            
        }
        void EndJudge(ref JudgeType result)
        {
            if (!isJudged)
                return;

            var realityHT = LastFor - 0.3f - judgeDiff / 1000f;
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
            print($"Hold: {MathF.Round(percent * 100, 2)}%\nTotal Len : {MathF.Round(realityHT * 1000, 2)}ms");
        }
        void EndJudge_Classic(ref JudgeType result)
        {
            if (!isJudged)
                return;
            else if (result == JudgeType.Miss)
                return;

            var releaseTiming = gpManager.AudioTime;
            var diff = (time + LastFor) - releaseTiming;
            var isFast = diff > 0;
            diff = MathF.Abs(diff);

            JudgeType endResult = diff switch
            {
                <= 0.044445f => JudgeType.Perfect,
                <= 0.088889f => isFast ? JudgeType.FastPerfect1 : JudgeType.LatePerfect1,
                <= 0.133336f => isFast ? JudgeType.FastPerfect2 : JudgeType.LatePerfect2,
                <= 0.150f =>    isFast ? JudgeType.FastGreat : JudgeType.LateGreat,
                <= 0.16667f =>  isFast ? JudgeType.FastGreat1 : JudgeType.LateGreat1,
                <= 0.183337f => isFast ? JudgeType.FastGreat2 : JudgeType.LateGreat2,
                _ => isFast ? JudgeType.FastGood : JudgeType.LateGood
            };

            var num = Math.Abs(7 - (int)result);
            var endNum = Math.Abs(7 - (int)endResult);
            if (endNum > num) // 取最差判定
                result = endResult;
        }
        protected override void PlayHoldEffect()
        {
            base.PlayHoldEffect();
            effectManager.ResetEffect(startPosition);
            if (LastFor <= 0.3)
                return;
            else if (!holdAnimStart && GetTimeSpanToArriveTiming() >= 0.1f)//忽略开头6帧与结尾12帧
            {
                holdAnimStart = true;
                shineAnimator.enabled = true;
                var sprRenderer = GetComponent<SpriteRenderer>();
                
                if(breakShineController != null)
                {
                    Destroy(breakShineController);
                    thisRenderer.material.SetFloat("_Brightness", 1);
                    thisRenderer.material.SetFloat("_Contrast", 1);
                }
                thisRenderer.sprite = holdOnSprite;
            }
        }
        protected override void StopHoldEffect()
        {
            base.StopHoldEffect();
            holdAnimStart = false;
            shineAnimator.enabled = false;
            var sprRenderer = GetComponent<SpriteRenderer>();
            sprRenderer.sprite = holdOffSprite;
            if (breakShineController != null)
            {
                Destroy(breakShineController);
                thisRenderer.material.SetFloat("_Brightness", 1);
                thisRenderer.material.SetFloat("_Contrast", 1);
            }
        }

        protected override void LoadSkin()
        {
            var skin = SkinManager.Instance.GetHoldSkin();
            var renderer = GetComponent<SpriteRenderer>();
            var exRenderer = transform.GetChild(0).GetComponent<SpriteRenderer>();
            var tapLineRenderer = tapLine.GetComponent<SpriteRenderer>();

            holdSprite = skin.Normal;
            holdOnSprite = skin.Normal_On;
            holdOffSprite = skin.Off;

            exRenderer.sprite = skin.Ex;
            exRenderer.color = skin.ExEffects[0];
            endRenderer.sprite = skin.Ends[0];

            if (isEach)
            {
                holdSprite = skin.Each;
                holdOnSprite = skin.Each_On;
                endRenderer.sprite = skin.Ends[1];
                tapLineRenderer.sprite = skin.NoteLines[1];
                exRenderer.color = skin.ExEffects[1];
            }

            if (isBreak)
            {
                holdSprite = skin.Break;
                holdOnSprite = skin.Break_On;
                endRenderer.sprite = skin.Ends[2];
                renderer.material = skin.BreakMaterial;
                tapLineRenderer.sprite = skin.NoteLines[2];
                breakShineController = gameObject.AddComponent<BreakShineController>();
                exRenderer.color = skin.ExEffects[2];
            }

            if (!isEX)
                Destroy(exRenderer);
            endRenderer.enabled = false;
            renderer.sprite = holdSprite;
        }

    }
}