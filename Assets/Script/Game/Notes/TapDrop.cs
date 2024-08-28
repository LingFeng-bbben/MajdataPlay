using MajdataPlay.Game.Controllers;
using MajdataPlay.IO;
using MajdataPlay.Types;
using UnityEngine;
#nullable enable
namespace MajdataPlay.Game.Notes
{
    public class TapDrop : TapBase
    {
        protected override void Start()
        {
            base.Start();

            LoadSkin();

            sensorPos = (SensorType)(startPosition - 1);
            ioManager.BindArea(Check, sensorPos);
            State = NoteStatus.Initialized;
        }
        protected override void LoadSkin()
        {
            var skin = SkinManager.Instance.GetTapSkin();
            var renderer = GetComponent<SpriteRenderer>();
            var exRenderer = transform.GetChild(0).GetComponent<SpriteRenderer>();
            var tapLineRenderer = tapLine.GetComponent<SpriteRenderer>();

            renderer.sprite = skin.Normal;
            exRenderer.sprite = skin.Ex;
            exRenderer.color = skin.ExEffects[0];
            
                

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
                gameObject.AddComponent<BreakShineController>();
                exRenderer.color = skin.ExEffects[2];

            }

            if (!isEX)
                Destroy(exRenderer);
        }
    }
}