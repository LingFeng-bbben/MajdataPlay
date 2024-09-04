using MajdataPlay.Game.Controllers;
using MajdataPlay.IO;
using MajdataPlay.Types;
using UnityEngine;
#nullable enable
namespace MajdataPlay.Game.Notes
{
    public class StarDrop : TapBase
    {
        public float rotateSpeed = 1f;

        public bool isDouble;
        public bool isNoHead;
        public bool isFakeStar = false;
        public bool isFakeStarRotate = false;

        public GameObject slide;
        protected override void Start()
        {
            base.Start();

            LoadSkin();

            if (!isNoHead)
            {
                sensorPos = (SensorType)(startPosition - 1);
                ioManager.BindArea(Check, sensorPos);
            }
            State = NoteStatus.Initialized;
        }
        // Update is called once per frame
        protected override void Update()
        {
            var songSpeed = gpManager.CurrentSpeed;
            var judgeTiming = GetTimeSpanToArriveTiming();
            var distance = judgeTiming * speed + 4.8f;
            var destScale = distance * 0.4f + 0.51f;

            switch (State)
            {
                case NoteStatus.Initialized:
                    if (destScale >= 0f)
                    {

                        if (!isNoHead)
                            tapLine.transform.rotation = Quaternion.Euler(0, 0, -22.5f + -45f * (startPosition - 1));
                        State = NoteStatus.Pending;
                        goto case NoteStatus.Pending;
                    }
                    else
                        transform.localScale = new Vector3(0, 0);
                    return;
                case NoteStatus.Pending:
                    {
                        if (destScale > 0.3f && !isNoHead)
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
                            if (!isFakeStar && !slide.activeSelf)
                            {
                                slide.SetActive(true);
                                if (isNoHead)
                                {
                                    Destroy(tapLine);
                                    Destroy(gameObject);
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
                        transform.position = GetPositionFromDistance(distance);
                        transform.localScale = new Vector3(1f, 1f);
                        var lineScale = Mathf.Abs(distance / 4.8f);
                        tapLine.transform.localScale = new Vector3(lineScale, lineScale, 1f);
                    }
                    break;
            }

            if (gpManager.isStart && !isFakeStar && gameSetting.Game.StarRotation)
                transform.Rotate(0f, 0f, -180f * Time.deltaTime * songSpeed / rotateSpeed);
            else if (isFakeStarRotate)
                transform.Rotate(0f, 0f, 400f * Time.deltaTime);
        }
        protected override void OnDestroy()
        {
            if (!isNoHead || isFakeStar)
                base.OnDestroy();
        }
        protected override void LoadSkin()
        {
            var renderer = GetComponent<SpriteRenderer>();
            var exRenderer = transform.GetChild(0).GetComponent<SpriteRenderer>();
            var tapLineRenderer = tapLine.GetComponent<SpriteRenderer>();

            if (isNoHead)
            {
                Destroy(tapLineRenderer);
                Destroy(renderer);
                Destroy(exRenderer);
                return;
            }

            var skin = SkinManager.Instance.GetStarSkin();
            exRenderer.color = skin.ExEffects[0];


            if (isDouble)
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
                    tapLineRenderer.sprite = skin.NoteLines[2];
                    gameObject.AddComponent<BreakShineController>();
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
                    tapLineRenderer.sprite = skin.NoteLines[2];
                    gameObject.AddComponent<BreakShineController>();
                    exRenderer.color = skin.ExEffects[2];
                }
            }

            if (!isEX)
                Destroy(exRenderer);

        }
    }
}