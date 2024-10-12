using MajdataPlay.Game.Controllers;
using MajdataPlay.Interfaces;
using MajdataPlay.Utils;
using MajdataPlay.IO;
using MajdataPlay.Types;
using UnityEngine;
#nullable enable
namespace MajdataPlay.Game.Notes
{
    public sealed class TapDrop : TapBase, IPoolableNote<TapPoolingInfo, TapQueueInfo>
    {
        //protected override void Start()
        //{
        //    base.Start();

        //    LoadSkin();

        //    sensorPos = (SensorType)(startPosition - 1);
        //    ioManager.BindArea(Check, sensorPos);
        //    State = NoteStatus.Initialized;
        //}
        protected override void LoadSkin()
        {
            var skin = MajInstances.SkinManager.GetTapSkin();
            var renderer = GetComponent<SpriteRenderer>();
            var exRenderer = transform.GetChild(0).GetComponent<SpriteRenderer>();
            var tapLineRenderer = tapLine.GetComponent<SpriteRenderer>();
            if (breakShineController is null)
                breakShineController = gameObject.AddComponent<BreakShineController>();

            renderer.sprite = skin.Normal;
            renderer.material = skin.DefaultMaterial;
            exRenderer.sprite = skin.Ex;
            exRenderer.color = skin.ExEffects[0];
            tapLineRenderer.sprite = skin.NoteLines[0];
            breakShineController.enabled = false;


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

            RendererState = RendererStatus.Off;
        }
        public override void Initialize(TapPoolingInfo poolingInfo)
        {
            base.Initialize(poolingInfo);

            LoadSkin();
            _sensorPos = (SensorType)(startPosition - 1);
            _ioManager.BindArea(Check, _sensorPos);
            State = NoteStatus.Initialized;
        }
        public override void End(bool forceEnd = false)
        {
            base.End(forceEnd);
            if (forceEnd)
                return;
            RendererState = RendererStatus.Off;
            notePoolManager.Collect(this);
        }
    }
}