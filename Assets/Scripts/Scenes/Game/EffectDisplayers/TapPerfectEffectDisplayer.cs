using MajdataPlay.Extensions;
using MajdataPlay.Types;
using MajdataPlay.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MajdataPlay.Game
{
    internal sealed class TapPerfectEffectDisplayer : MajComponent
    {
        Animator _animator;
        GameObject[] _children = Array.Empty<GameObject>();

        static readonly int BREAK_GOOD_ANIM_HASH = Animator.StringToHash("bGood");
        static readonly int BREAK_GREAT_ANIM_HASH = Animator.StringToHash("bGreat");
        static readonly int BREAK_PERFECT_ANIM_HASH = Animator.StringToHash("break");
        static readonly int TAP_PERFECT_ANIM_HASH = Animator.StringToHash("tap");
        protected override void Awake()
        {
            base.Awake();

            _animator = GameObject.GetComponent<Animator>();
            _children = Transform.GetChildren()
                                 .Select(x => x.gameObject)
                                 .ToArray();
            _animator.speed = 0.9f;
            SetActiveInternal(false);
        }
        public void Reset()
        {
            SetActive(false);
        }
        public void PlayEffect(in JudgeResult judgeResult)
        {
            if(judgeResult.IsBreak)
            {
                PlayBreakEffect(judgeResult);
            }
            else
            {
                PlayTapEffect(judgeResult);
            }
        }
        void PlayTapEffect(in JudgeResult judgeResult)
        {
            var grade = judgeResult.Grade;
            switch (grade)
            {
                case JudgeGrade.LatePerfect2:
                case JudgeGrade.FastPerfect2:
                case JudgeGrade.LatePerfect1:
                case JudgeGrade.FastPerfect1:
                case JudgeGrade.Perfect:
                    SetActive(true);
                    _animator.SetTrigger(TAP_PERFECT_ANIM_HASH);
                    break;
            }
        }
        void PlayBreakEffect(in JudgeResult judgeResult)
        {
            var grade = judgeResult.Grade;
            switch (grade)
            {
                case JudgeGrade.LateGood:
                case JudgeGrade.FastGood:
                    SetActive(true);
                    _animator.SetTrigger(BREAK_GOOD_ANIM_HASH);
                    break;
                case JudgeGrade.LateGreat:
                case JudgeGrade.LateGreat1:
                case JudgeGrade.LateGreat2:
                case JudgeGrade.FastGreat2:
                case JudgeGrade.FastGreat1:
                case JudgeGrade.FastGreat:
                    SetActive(true);
                    _animator.SetTrigger(BREAK_GREAT_ANIM_HASH);
                    break;
                case JudgeGrade.LatePerfect2:
                case JudgeGrade.FastPerfect2:
                case JudgeGrade.LatePerfect1:
                case JudgeGrade.FastPerfect1:
                case JudgeGrade.Perfect:
                    SetActive(true);
                    _animator.SetTrigger(BREAK_PERFECT_ANIM_HASH);
                    break;
                default:
                    break;
            }
        }
        public override void SetActive(bool state)
        {
            if (Active == state)
                return;
            SetActiveInternal(state);
        }
        void SetActiveInternal(bool state)
        {
            Active = state;
            base.SetActive(state);
            switch (state)
            {
                case true:
                    foreach (var child in ArrayHelper.ToEnumerable(_children))
                    {
                        if (child is null)
                            continue;
                        child.layer = MajEnv.DEFAULT_LAYER;
                    }
                    GameObject.layer = MajEnv.DEFAULT_LAYER;
                    break;
                case false:
                    foreach (var child in ArrayHelper.ToEnumerable(_children))
                    {
                        if (child is null)
                            continue;
                        child.layer = MajEnv.HIDDEN_LAYER;
                    }
                    GameObject.layer = MajEnv.HIDDEN_LAYER;
                    break;
            }
        }
    }
}
