using MajdataPlay.Scenes.Game.Notes;
using MajdataPlay.Utils;
using System.Collections;
using System.Collections.Generic;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;
using UnityEngine.UI;
#nullable enable
namespace MajdataPlay.Scenes.Game
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    public class XxlbAnimationController : MonoBehaviour
    {
        Image image;
        Animator animator;
        public Sprite[] sprites;

        short dir = 0;

        readonly int ANIMATOR_DANCE_HASH = Animator.StringToHash("dance");

        private void Awake()
        {
            Majdata<XxlbAnimationController>.Instance = this;
            image = GetComponent<Image>();
            animator = GetComponent<Animator>();
        }

        public void Stepping()
        {
            animator.SetTrigger(ANIMATOR_DANCE_HASH);
            image.sprite = sprites[1];
        }

        public void Dance(JudgeGrade result)
        {
            switch (result)
            {
                case JudgeGrade.Perfect:
                case JudgeGrade.FastPerfect2nd:
                case JudgeGrade.FastPerfect3rd:
                case JudgeGrade.LatePerfect2nd:
                case JudgeGrade.LatePerfect3rd:
                    animator.SetTrigger(ANIMATOR_DANCE_HASH);
                    if (dir == 0)//left
                    {
                        image.sprite = sprites[0];
                    }
                    if (dir == 1)//center
                    {
                        image.sprite = sprites[1];
                    }
                    if (dir == 2)//right
                    {
                        image.sprite = sprites[2];
                    }
                    if (dir == 3)//center
                    {
                        image.sprite = sprites[1];
                    }
                    dir++;
                    if (dir > 3) dir = 0;
                    break;
                default:
                    animator.SetTrigger(ANIMATOR_DANCE_HASH);
                    if (dir == 0)//left
                    {
                        image.sprite = sprites[3];
                    }
                    if (dir == 1)//center
                    {
                        image.sprite = sprites[1];
                    }
                    if (dir == 2)//right
                    {
                        image.sprite = sprites[4];
                    }
                    if (dir == 3)//center
                    {
                        image.sprite = sprites[1];
                    }
                    dir++;
                    if (dir > 3) dir = 0;
                    break;
            }
        }

        private void OnDestroy()
        {
            Majdata<XxlbAnimationController>.Free();
        }
    }
}