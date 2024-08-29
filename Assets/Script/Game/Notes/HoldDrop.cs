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

        GameObject tapLine;

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
        }
        private void FixedUpdate()
        {
            var timing = GetJudgeTiming();
            var remainingTime = GetRemainingTime();

            if (remainingTime == 0 && isJudged) // Hold完成后Destroy
            {
                Destroy(tapLine);
                Destroy(holdEffect);
                Destroy(gameObject);
            }
            

            if (isJudged) // 头部判定完成后开始累计按压时长
            {
                if (timing <= 0.1f) // 忽略头部6帧
                    return;
                else if (remainingTime <= 0.2f) // 忽略尾部12帧
                    return;
                else if (!gpManager.isStart) // 忽略暂停
                    return;
                var on = ioManager.CheckAreaStatus(sensorPos, SensorStatus.On);
                if (on)
                    PlayHoldEffect();
                else
                {
                    playerIdleTime += Time.fixedDeltaTime;
                    StopHoldEffect();
                }
            }
            else if (timing > 0.15f && !isJudged) // 头部Miss
            {
                judgeDiff = 150;
                judgeResult = JudgeType.Miss;
                isJudged = true;
                objectCounter.NextNote(startPosition);
            }
        }
        protected override void Check(object sender, InputEventArgs arg)
        {
            if (arg.Type != sensorPos)
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

            var timing = gpManager.AudioTime - time;
            var isFast = timing < 0;
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
        private void Update()
        {
            var timing = GetJudgeTiming();
            var distance = timing * speed + 4.8f;
            var destScale = distance * 0.4f + 0.51f;
            if (destScale < 0f)
            {
                destScale = 0f;
                return;
            }

            thisRenderer.forceRenderingOff = false;
            if (isEX) exRenderer.forceRenderingOff = false;

            thisRenderer.size = new Vector2(1.22f, 1.4f);

            var holdTime = timing - LastFor;
            var holdDistance = holdTime * speed + 4.8f;
            if (holdTime >= 0 ||
                holdTime >= 0 && LastFor <= 0.15f)
            {
                tapLine.transform.localScale = new Vector3(1f, 1f, 1f);
                transform.position = GetPositionFromDistance(4.8f);
                return;
            }


            transform.rotation = Quaternion.Euler(0, 0, -22.5f + -45f * (startPosition - 1));
            tapLine.transform.rotation = transform.rotation;
            holdEffect.transform.position = GetPositionFromDistance(4.8f);

            if (isBreak && !holdAnimStart && !isJudged)
            {
                var (brightness, contrast) = gpManager.BreakParams;
                thisRenderer.material.SetFloat("_Brightness", brightness);
                thisRenderer.material.SetFloat("_Contrast", contrast);
            }


            if (destScale > 0.3f) tapLine.SetActive(true);

            if (distance < 1.225f)
            {
                transform.localScale = new Vector3(destScale, destScale);
                thisRenderer.size = new Vector2(1.22f, 1.42f);
                distance = 1.225f;
                var pos = GetPositionFromDistance(distance);
                transform.position = pos;
            }
            else
            {
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
                transform.position = GetPositionFromDistance(dis); //0.325
                var size = distance - holdDistance + 1.4f;
                thisRenderer.size = new Vector2(1.22f, size);
                endRenderer.transform.localPosition = new Vector3(0f, 0.6825f - size / 2);
                transform.localScale = new Vector3(1f, 1f);
            }

            var lineScale = Mathf.Abs(distance / 4.8f);
            lineScale = lineScale >= 1f ? 1f : lineScale;
            tapLine.transform.localScale = new Vector3(lineScale, lineScale, 1f);
            exRenderer.size = thisRenderer.size;
        }
        private void OnDestroy()
        {
            ioManager.UnbindArea(Check, sensorPos);
            if (!isJudged) return;

            var realityHT = LastFor - 0.3f - judgeDiff / 1000f;
            var percent = MathF.Min(1, (realityHT - playerIdleTime) / realityHT);
            JudgeType result = judgeResult;
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

            var audioEffMana = GameObject.Find("NoteAudioManager").GetComponent<NoteAudioManager>();
            audioEffMana.PlayTapSound(false, false, result);
            var effectManager = GameObject.Find("NoteEffects").GetComponent<NoteEffectManager>();
            effectManager.PlayEffect(startPosition, isBreak, result);
            effectManager.PlayFastLate(startPosition, result);
            print($"Hold: {MathF.Round(percent * 100, 2)}%\nTotal Len : {MathF.Round(realityHT * 1000, 2)}ms");

            objectCounter.ReportResult(this, result, isBreak);
            if (!isJudged)
                objectCounter.NextNote(startPosition);

            
        }
        protected override void PlayHoldEffect()
        {
            base.PlayHoldEffect();
            effectManager.ResetEffect(startPosition);
            if (LastFor <= 0.3)
                return;
            else if (!holdAnimStart && GetJudgeTiming() >= 0.1f)//忽略开头6帧与结尾12帧
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

            renderer.sprite = holdSprite;
        }

    }
}