using MajdataPlay.Extensions;
using MajdataPlay.Game.Controllers;
using MajdataPlay.IO;
using MajdataPlay.Types;
using System;
using System.Security.Claims;
using UnityEngine;
#nullable enable
namespace MajdataPlay.Game.Notes
{
    public sealed class TouchDrop : TouchBase
    {
        public RendererStatus RendererState 
        {
            get => _rendererState;
            private set
            {
                if (State < NoteStatus.Initialized)
                    return;
                switch(value)
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
        float displayDuration;
        float moveDuration;
        float wholeDuration;

        readonly SpriteRenderer[] fanRenderers = new SpriteRenderer[4];
        readonly GameObject[] fans = new GameObject[4];

        GameObject point;
        SpriteRenderer pointRenderer;
        GameObject justBorder;
        SpriteRenderer justBorderRenderer;
        MultTouchHandler multTouchHandler;
        

        // Start is called before the first frame update
        protected override void Start()
        {
            base.Start();
            wholeDuration = 3.209385682f * Mathf.Pow(speed, -0.9549621752f);
            moveDuration = 0.8f * wholeDuration;
            displayDuration = 0.2f * wholeDuration;

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

            transform.position = GetAreaPos(startPosition, areaPosition);
            point.SetActive(false);
            justBorder.SetActive(false);
            
            SetfanColor(new Color(1f, 1f, 1f, 0f));
            var customSkin = SkinManager.Instance;
            ioManager.BindSensor(Check, GetSensor());
            sensorPos = GetSensor();
            SetFansPosition(0.4f);
            State = NoteStatus.Initialized;
            RendererState = RendererStatus.Off;
        }
        protected override void LoadSkin()
        {
            var skin = SkinManager.Instance.GetTouchSkin();
            for (var i = 0; i < 4; i++)
            {
                fanRenderers[i] = fans[i].GetComponent<SpriteRenderer>();
                fanRenderers[i].sortingOrder += noteSortOrder;
            }

            if (isEach)
            {
                SetfanSprite(skin.Each);
                pointRenderer.sprite = skin.Point_Each;
            }
            else
            {
                SetfanSprite(skin.Normal);
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
            else if (isJudged || !noteManager.CanJudge(gameObject, type))
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
                    Destroy(gameObject);
                }
            }
        }
        private void FixedUpdate()
        {
            var isTooLate = GetTimeSpanToJudgeTiming() >= 0.316667f;
            if (!isJudged && !isTooLate)
            {
                if (GroupInfo is not null)
                {
                    if (GroupInfo.Percent > 0.5f && GroupInfo.JudgeResult != null)
                    {
                        isJudged = true;
                        judgeResult = (JudgeType)GroupInfo.JudgeResult;
                        Destroy(gameObject);
                    }
                }
            }
            else if (!isJudged)
            {
                judgeResult = JudgeType.Miss;
                isJudged = true;
                Destroy(gameObject);
            }
            else if (isJudged)
                Destroy(gameObject);
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
        }
        // Update is called once per frame
        private void Update()
        {
            var timing = GetTimeSpanToArriveTiming();
            
            switch(State)
            {
                case NoteStatus.Initialized:
                    if((-timing).InRange(wholeDuration, moveDuration))
                    {
                        multTouchHandler.Register(sensorPos,isEach,isBreak);
                        RendererState = RendererStatus.On;
                        point.SetActive(true);
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
        private void OnDestroy()
        {
            ioManager.UnbindSensor(Check, GetSensor());
            multTouchHandler.Unregister(sensorPos);
            if (!isJudged) 
                return;
            State = NoteStatus.Destroyed;
            var result = new JudgeResult()
            {
                Result = judgeResult,
                Diff = judgeDiff,
                IsEX = isEX,
                IsBreak = isBreak
            };
            
            effectManager.PlayTouchEffect(sensorPos, result);

            if (GroupInfo is not null && judgeResult != JudgeType.Miss)
                GroupInfo.JudgeResult = judgeResult;

            if(judgeResult != JudgeType.Miss)
                audioEffMana.PlayTouchSound();
            objectCounter.ReportResult(this, result);
            objectCounter.NextTouch(sensorPos);

            if (isFirework && judgeResult != JudgeType.Miss)
            {
                effectManager.PlayFireworkEffect(transform.position);
                audioEffMana.PlayHanabiSound();
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
        private Vector3 GetAngle(int index)
        {
            var angle = index * (Mathf.PI / 2);
            return new Vector3(Mathf.Sin(angle), Mathf.Cos(angle));
        }
        private void SetfanColor(Color color)
        {
            foreach (var fan in fanRenderers) fan.color = color;
        }

        private void SetfanSprite(Sprite sprite)
        {
            for (var i = 0; i < 4; i++) fanRenderers[i].sprite = sprite;
        }
        RendererStatus _rendererState = RendererStatus.Off;
    }
}