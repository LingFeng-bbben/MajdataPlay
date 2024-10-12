using MajdataPlay.Game.Controllers;
using MajdataPlay.Interfaces;
using MajdataPlay.Utils;
using MajdataPlay.Types;
using System;
using UnityEngine;
#nullable enable
namespace MajdataPlay.Game.Notes
{
    public sealed class StarDrop : TapBase, ISlideLauncher, IPoolableNote<TapPoolingInfo, TapQueueInfo>
    {
        public float RotateSpeed { get; set; } = 1f;
        public bool IsDouble { get; set; } = false;
        public bool IsNoHead { get; set; } = false;
        public bool IsFakeStar { get; set; } = false;
        public bool IsForceRotate { get; set; } = false;

        public GameObject? SlideObject { get; set; }
        //protected override void Start()
        //{
        //    base.Start();

        //    if (SlideObject is null)
        //        throw new NullReferenceException("Slide launcher has no slide reference");
        //    LoadSkin();

        //    if (!IsNoHead)
        //    {
        //        sensorPos = (SensorType)(startPosition - 1);
        //        ioManager.BindArea(Check, sensorPos);
        //    }
        //    State = NoteStatus.Initialized;
        //}
        public override void Initialize(TapPoolingInfo poolingInfo)
        {
            base.Initialize(poolingInfo);

            RotateSpeed = poolingInfo.RotateSpeed;
            IsNoHead = poolingInfo.IsNoHead;
            IsDouble = poolingInfo.IsDouble;
            IsFakeStar = poolingInfo.IsFakeStar;
            IsForceRotate = poolingInfo.IsForceRotate;
            SlideObject = poolingInfo.Slide;
            if (SlideObject is null && !IsFakeStar)
                throw new NullReferenceException("Slide launcher has no slide reference");
            LoadSkin();
            if (!IsNoHead)
            {
                _sensorPos = (SensorType)(startPosition - 1);
                _ioManager.BindArea(Check, _sensorPos);
            }
            State = NoteStatus.Initialized;
        }
        public override void End(bool forceEnd = false)
        {
            if (!IsNoHead || IsFakeStar)
                base.End(forceEnd);
            else
                State = NoteStatus.Destroyed;
            if (forceEnd)
                return;
            RendererState = RendererStatus.Off;
            notePoolManager.Collect(this);
        }
        protected override void Update()
        {
            var songSpeed = _gpManager.CurrentSpeed;
            var judgeTiming = GetTimeSpanToArriveTiming();
            var distance = judgeTiming * speed + 4.8f;
            var scaleRate = _gameSetting.Debug.NoteAppearRate;
            var destScale = distance * scaleRate + (1 - (scaleRate * 1.225f));

            switch (State)
            {
                case NoteStatus.Initialized:
                    if (destScale >= 0f)
                    {
                        if (!IsNoHead)
                        {
                            tapLine.transform.rotation = Quaternion.Euler(0, 0, -22.5f + -45f * (startPosition - 1));
                            RendererState = RendererStatus.On;
                        }
                        State = NoteStatus.Scaling;
                        goto case NoteStatus.Scaling;
                    }
                    else
                        transform.localScale = new Vector3(0, 0);
                    return;
                case NoteStatus.Scaling:
                    {
                        if (destScale > 0.3f && !IsNoHead)
                            tapLine.SetActive(true);
                        if (distance < 1.225f)
                        {
                            transform.localScale = new Vector3(destScale, destScale);
                            transform.position = GetPositionFromDistance(1.225f);
                            var lineScale = Mathf.Abs(1.225f / 4.8f);
                            tapLine.transform.localScale = new Vector3(lineScale, lineScale, 1f);
                        }
                        else
                        {
                            if (!IsFakeStar && !SlideObject!.activeSelf)
                            {
                                SlideObject.SetActive(true);
                                if (IsNoHead)
                                {
                                    End();
                                    return;
                                }
                            }
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

            if (_gpManager.IsStart && !IsFakeStar && _gameSetting.Game.StarRotation)
                transform.Rotate(0f, 0f, -180f * Time.deltaTime * songSpeed / RotateSpeed);
            else if (IsForceRotate)
                transform.Rotate(0f, 0f, 400f * Time.deltaTime);
        }
        protected override void LoadSkin()
        {
            var renderer = GetComponent<SpriteRenderer>();
            var exRenderer = transform.GetChild(0).GetComponent<SpriteRenderer>();
            var tapLineRenderer = tapLine.GetComponent<SpriteRenderer>();
            if (breakShineController is null)
                breakShineController = gameObject.AddComponent<BreakShineController>();

            RendererState = RendererStatus.Off;

            var skin = MajInstances.SkinManager.GetStarSkin();
            renderer.material = skin.DefaultMaterial;
            exRenderer.color = skin.ExEffects[0];
            tapLineRenderer.sprite = skin.NoteLines[0];
            breakShineController.enabled = false;

            if (IsDouble)
            {
                renderer.sprite = skin.Double;
                exRenderer.sprite = skin.ExDouble;
                
                if (isEach)
                {
                    renderer.sprite = skin.EachDouble;
                    tapLineRenderer.sprite = skin.NoteLines[1];
                    exRenderer.color = skin.ExEffects[1];
                }
                if (isBreak)
                {
                    renderer.sprite = skin.BreakDouble;
                    renderer.material = skin.BreakMaterial;
                    tapLineRenderer.sprite = skin.NoteLines[2];
                    breakShineController.enabled = true;
                    breakShineController.Parent = this;
                    exRenderer.color = skin.ExEffects[2];
                }
            }
            else
            {
                renderer.sprite = skin.Normal;
                exRenderer.sprite = skin.Ex;

                if (isEach)
                {
                    renderer.sprite = skin.Each;
                    tapLineRenderer.sprite = skin.NoteLines[1];
                    exRenderer.color = skin.ExEffects[1];
                }
                if (isBreak)
                {
                    renderer.sprite = skin.Break;
                    renderer.material = skin.BreakMaterial;
                    tapLineRenderer.sprite = skin.NoteLines[2];
                    breakShineController.enabled = true;
                    breakShineController.Parent = this;
                    exRenderer.color = skin.ExEffects[2];
                }
            }
        }
    }
}