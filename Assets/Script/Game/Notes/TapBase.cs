using MajdataPlay.IO;
using MajdataPlay.Types;
using System;
using UnityEngine;
#nullable enable
namespace MajdataPlay.Game.Notes
{
    public abstract class TapBase : NoteDrop
    {
        public GameObject tapLine;

        protected virtual void Start()
        {
            var notes = GameObject.Find("Notes").transform;
            var spriteRenderer = GetComponent<SpriteRenderer>();
            var exSpriteRender = transform.GetChild(0).GetComponent<SpriteRenderer>();

            noteManager = notes.GetComponent<NoteManager>();
            tapLine = Instantiate(tapLine, notes);
            tapLine.SetActive(false);
            objectCounter = GameObject.Find("ObjectCounter").GetComponent<ObjectCounter>();

            spriteRenderer.sortingOrder += noteSortOrder;
            exSpriteRender.sortingOrder += noteSortOrder;

            transform.localScale = new Vector3(0, 0);
        }
        protected abstract void LoadSkin();
        protected void FixedUpdate()
        {
            var timing = GetJudgeTiming();
            if (!isJudged && timing > 0.15f)
            {
                judgeResult = JudgeType.Miss;
                isJudged = true;
                Destroy(tapLine);
                Destroy(gameObject);
            }
            else if (isJudged)
            {
                Destroy(tapLine);
                Destroy(gameObject);
            }
        }
        // Update is called once per frame
        protected virtual void Update()
        {
            var timing = GetJudgeTiming();
            var distance = timing * speed + 4.8f;
            var destScale = distance * 0.4f + 0.51f;

            switch (State)
            {
                case NoteStatus.Initialized:
                    if (destScale >= 0f)
                    {
                        transform.rotation = Quaternion.Euler(0, 0, -22.5f + -45f * (startPosition - 1));
                        tapLine.transform.rotation = Quaternion.Euler(0, 0, -22.5f + -45f * (startPosition - 1));
                        State = NoteStatus.Pending;
                        goto case NoteStatus.Pending;
                    }
                    else
                        transform.localScale = new Vector3(0, 0);
                    return;
                case NoteStatus.Pending:
                    {
                        if (destScale > 0.3f)
                            tapLine.SetActive(true);
                        if (distance < 1.225f)
                        {
                            transform.localScale = new Vector3(destScale, destScale);
                            transform.position = getPositionFromDistance(1.225f);
                            var lineScale = Mathf.Abs(1.225f / 4.8f);
                            tapLine.transform.localScale = new Vector3(lineScale, lineScale, 1f);
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
                        transform.position = getPositionFromDistance(distance);
                        transform.localScale = new Vector3(1f, 1f);
                        var lineScale = Mathf.Abs(distance / 4.8f);
                        tapLine.transform.localScale = new Vector3(lineScale, lineScale, 1f);
                    }
                    break;
            }

            //spriteRenderer.forceRenderingOff = false;
            //if (isEX) exSpriteRender.forceRenderingOff = false;
            //if (isBreak)
            //{
            //    var (brightness, contrast) = gpManager.BreakParams;
            //    spriteRenderer.material.SetFloat("_Brightness", brightness);
            //    spriteRenderer.material.SetFloat("_Contrast", contrast);
            //}
        }
        protected void Check(object sender, InputEventArgs arg)
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
                    Destroy(tapLine);
                    Destroy(gameObject);
                }
            }
        }
        protected void Judge()
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

            judgeResult = result;
            isJudged = true;
            
        }
        protected virtual void OnDestroy()
        {
            ioManager.UnbindArea(Check, sensorPos);
            if (!isJudged) return;
            var effectManager = GameObject.Find("NoteEffects").GetComponent<NoteEffectManager>();
            effectManager.PlayEffect(startPosition, isBreak, judgeResult);
            effectManager.PlayFastLate(startPosition, judgeResult);
            var audioEffMana = GameObject.Find("NoteAudioManager").GetComponent<NoteAudioManager>();
            audioEffMana.PlayTapSound(isBreak,isEX, judgeResult);
            objectCounter.NextNote(startPosition);
            objectCounter.ReportResult(this, judgeResult, isBreak);
            
        }
    }
}
