using MajdataPlay.IO;
using MajdataPlay.Types;
using UnityEngine;
#nullable enable
namespace MajdataPlay.Game.Notes
{
    public class TapDrop : TapBase
    {
        private void Start()
        {
            PreLoad();

            spriteRenderer.sprite = tapSpr;
            exSpriteRender.sprite = exSpr;

            if (isEX) exSpriteRender.color = exEffectTap;
            if (isEach)
            {
                spriteRenderer.sprite = eachSpr;
                lineSpriteRender.sprite = eachLine;
                if (isEX) exSpriteRender.color = exEffectEach;
            }

            if (isBreak)
            {
                spriteRenderer.sprite = breakSpr;
                lineSpriteRender.sprite = breakLine;
                if (isEX) exSpriteRender.color = exEffectBreak;
                spriteRenderer.material = breakMaterial;
            }

            spriteRenderer.forceRenderingOff = true;
            exSpriteRender.forceRenderingOff = true;

            sensorPos = (SensorType)(startPosition - 1);
            ioManager.BindArea(Check, sensorPos);
            State = NoteStatus.Initialized;
        }
        protected override void LoadSkin()
        {
            var skin = SkinManager.Instance.GetTapSkin();
            var renderer = GetComponent<SpriteRenderer>();

            renderer.sprite = skin.Normal;
        }
    }
}