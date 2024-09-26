using MajdataPlay.Interfaces;
using MajdataPlay.IO;
using MajdataPlay.Types;
using System;
using UnityEngine;
#nullable enable
namespace MajdataPlay.Game.Notes
{
    public abstract class TapBase : NoteDrop, IDistanceProvider, INoteQueueMember<TapQueueInfo>, IRendererContainer
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
                        thisRenderer.forceRenderingOff = true;
                        exRenderer.forceRenderingOff = true;
                        tapLineRenderer.forceRenderingOff = true;
                        break;
                    case RendererStatus.On:
                        thisRenderer.forceRenderingOff = false;
                        exRenderer.forceRenderingOff = !isEX;
                        tapLineRenderer.forceRenderingOff = false;
                        break;
                }
            }
        }
        public TapQueueInfo QueueInfo { get; set; } = TapQueueInfo.Default;
        public float Distance { get; protected set; } = -100;
        public GameObject tapLine;

        protected SpriteRenderer thisRenderer;
        protected SpriteRenderer exRenderer;
        protected SpriteRenderer tapLineRenderer;
        protected NotePoolManager notePoolManager;

        public virtual void Initialize(TapPoolingInfo poolingInfo)
        {
            if (State >= NoteStatus.Initialized && State < NoteStatus.Destroyed)
                return;
            startPosition = poolingInfo.StartPos;
            timing = poolingInfo.Timing;
            judgeTiming = timing;
            noteSortOrder = poolingInfo.NoteSortOrder;
            speed = poolingInfo.Speed;
            isEach = poolingInfo.IsEach;
            isBreak = poolingInfo.IsBreak;
            isEX = poolingInfo.IsEX;
            QueueInfo = poolingInfo.QueueInfo;
            isJudged = false;
            Distance = -100;
            if (State == NoteStatus.Start)
                Start();
            State = NoteStatus.Initialized;
        }
        public virtual void End(bool forceEnd = false)
        {
            State = NoteStatus.Destroyed;
            ioManager.UnbindArea(Check, sensorPos);
            if (!isJudged || forceEnd) 
                return;

            var result = new JudgeResult()
            {
                Result = judgeResult,
                IsBreak = isBreak,
                IsEX = isEX,
                Diff = judgeDiff
            };
            // TODO: TapLine
            effectManager.PlayEffect(startPosition, result);
            audioEffMana.PlayTapSound(result);
            noteManager.NextNote(QueueInfo);
            objectCounter.ReportResult(this, result);
        }
        protected override void Start()
        {
            if (IsInitialized)
                return;
            base.Start();
            notePoolManager = FindObjectOfType<NotePoolManager>();
            thisRenderer = GetComponent<SpriteRenderer>();
            exRenderer = transform.GetChild(0).GetComponent<SpriteRenderer>();

            tapLine = Instantiate(tapLine, noteManager.gameObject.transform);
            tapLine.SetActive(false);
            tapLineRenderer = tapLine.GetComponent<SpriteRenderer>();

            thisRenderer.sortingOrder += noteSortOrder;
            exRenderer.sortingOrder += noteSortOrder;

            transform.localScale = new Vector3(0, 0);
        }
        
        protected void FixedUpdate()
        {
            if (State < NoteStatus.Running)
                return;
            var timing = GetTimeSpanToJudgeTiming();
            var isTooLate = timing > 0.15f;
            if (!isJudged && isTooLate)
            {
                judgeResult = JudgeType.Miss;
                isJudged = true;
                End();
            }
            else if (isJudged)
                End();
        }
        // Update is called once per frame
        protected virtual void Update()
        {
            var timing = GetTimeSpanToArriveTiming();
            var distance = timing * speed + 4.8f;
            var scaleRate = gameSetting.Debug.NoteAppearRate;
            var destScale = distance * scaleRate + (1 - (scaleRate * 1.225f));

            switch (State)
            {
                case NoteStatus.Initialized:
                    if (destScale >= 0f)
                    {
                        transform.rotation = Quaternion.Euler(0, 0, -22.5f + -45f * (startPosition - 1));
                        tapLine.transform.rotation = Quaternion.Euler(0, 0, -22.5f + -45f * (startPosition - 1));

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
                            tapLine.SetActive(true);
                        if (distance < 1.225f)
                        {
                            Distance = distance;
                            transform.localScale = new Vector3(destScale, destScale);
                            transform.position = GetPositionFromDistance(1.225f);
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
                        Distance = distance;
                        transform.position = GetPositionFromDistance(distance);
                        transform.localScale = new Vector3(1f, 1f);
                        var lineScale = Mathf.Abs(distance / 4.8f);
                        tapLine.transform.localScale = new Vector3(lineScale, lineScale, 1f);
                    }
                    break;
                default:
                    return;
            }
        }
        protected override void Check(object sender, InputEventArgs arg)
        {
            if (State < NoteStatus.Running)
                return;
            else if (arg.Type != sensorPos)
                return;
            else if (isJudged || !noteManager.CanJudge(QueueInfo))
                return;

            if (arg.IsClick)
            {
                if (!ioManager.IsIdle(arg))
                    return;
                else
                    ioManager.SetBusy(arg);

                Judge(gpManager.ThisFrameSec);
                ioManager.SetIdle(arg);
                if (isJudged)
                {
                    Destroy(tapLine);
                    Destroy(gameObject);
                }
            }
        }
        protected override void Judge(float currentSec)
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

            //var timing = GetTimeSpanToJudgeTiming();
            var timing = currentSec - JudgeTiming;
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

            judgeResult = result;
            isJudged = true;
            
        }
        RendererStatus _rendererState = RendererStatus.Off;
    }
}
