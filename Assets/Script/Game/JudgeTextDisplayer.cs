using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
#nullable enable
namespace MajdataPlay.Game
{
    public sealed class JudgeTextDisplayer: MonoBehaviour
    {
        [SerializeField]
        GameObject effectObject;
        [SerializeField]
        Animator animator;

        [SerializeField]
        SpriteRenderer textRenderer;
        [SerializeField]
        SpriteRenderer breakRenderer;

        Sprite breakSprite;
        Sprite perfectSprite;
        Sprite greatSprite;
        Sprite goodSprite;
        Sprite missSprite;
        void Start() 
        {
            var skin = SkinManager.Instance.GetJudgeTextSkin();

            if(GameManager.Instance.Setting.Display.DisplayCriticalPerfect)
            {
                breakSprite = skin.CP_Break;
                perfectSprite = skin.CriticalPerfect;
            }
            else
            {
                breakSprite = skin.P_Break;
                perfectSprite = skin.Perfect;
            }
            greatSprite = skin.Great;
            goodSprite = skin.Good;
            missSprite = skin.Miss;
        }
        public void Reset()
        {
            effectObject.SetActive(false);
        }
    }
}
