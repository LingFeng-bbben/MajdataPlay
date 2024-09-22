using MajdataPlay.IO;
using MajdataPlay.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
#nullable enable
namespace MajdataPlay.Game
{
    public sealed class TapEffectDisplayer: MonoBehaviour
    {
        [SerializeField]
        GameObject perfectDisplayer;
        [SerializeField]
        GameObject greatDisplayer;
        [SerializeField]
        GameObject goodDisplayer;

        [SerializeField]
        Animator perfectAnim;
        [SerializeField]
        Animator greatAnim;
        [SerializeField]
        Animator goodAnim;

        [SerializeField]
        JudgeTextDisplayer judgeTextDisplayer;
        [SerializeField]
        FastLateDisplayer fastLateDisplayer;

        void Start()
        {
            Reset();
        }
        public void Reset()
        {
            perfectDisplayer.SetActive(false);
            greatDisplayer.SetActive(false);
            goodDisplayer.SetActive(false);
        }
        public void PlayEffect(in JudgeResult judgeResult)
        {
            var isBreak = judgeResult.IsBreak;
            var result = judgeResult.Result;
            if(!judgeResult.IsMiss)
                Reset();
            switch (result)
            {
                case JudgeType.LateGood:
                case JudgeType.FastGood:
                    if (isBreak)
                    {
                        perfectDisplayer.SetActive(true);
                        perfectAnim.speed = 0.9f;
                        perfectAnim.SetTrigger("bGood");
                    }
                    else
                        goodDisplayer.SetActive(true);
                    break;
                case JudgeType.LateGreat:
                case JudgeType.LateGreat1:
                case JudgeType.LateGreat2:
                case JudgeType.FastGreat2:
                case JudgeType.FastGreat1:
                case JudgeType.FastGreat:
                    if (isBreak)
                    {
                        perfectDisplayer.SetActive(true);
                        perfectAnim.speed = 0.9f;
                        perfectAnim.SetTrigger("bGreat");
                    }
                    else
                    {
                        greatDisplayer.SetActive(true);
                        greatAnim.SetTrigger("great");
                    }
                    break;
                case JudgeType.LatePerfect2:
                case JudgeType.FastPerfect2:
                case JudgeType.LatePerfect1:
                case JudgeType.FastPerfect1:
                case JudgeType.Perfect:
                    perfectDisplayer.SetActive(true);
                    if (isBreak)
                    {
                        perfectAnim.speed = 0.9f;
                        perfectAnim.SetTrigger("break");
                    }
                    break;
                default:
                    break;
            }
        }
    }
}
