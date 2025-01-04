using MajdataPlay.Types;
using MajdataPlay.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#nullable enable
public class XxlbAnimationController : MonoBehaviour
{
    Image image;
    Animator animator;
    public Sprite[] sprites;

    short dir=0;

    private void Awake()
    {
        MajInstanceHelper<XxlbAnimationController>.Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        image = GetComponent<Image>();
        animator = GetComponent<Animator>();
    }

    public void Stepping()
    {
        animator.SetTrigger("dance");
        image.sprite = sprites[1];
    }

    public void Dance(JudgeGrade result)
    {
        switch (result)
        {
            case JudgeGrade.Perfect:
            case JudgeGrade.FastPerfect1:
            case JudgeGrade.FastPerfect2:
            case JudgeGrade.LatePerfect1:
            case JudgeGrade.LatePerfect2:
                animator.SetTrigger("dance");
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
                if(dir>3)dir = 0;
                break;
            default:
                animator.SetTrigger("dance");
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
        MajInstanceHelper<XxlbAnimationController>.Free();
    }
}
