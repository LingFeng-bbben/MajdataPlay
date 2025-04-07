﻿using MajdataPlay.Extensions;
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
    internal sealed class TouchJudgeEffectDisplayer: MajComponent
    {
        Animator _animator;
        GameObject[] _children = Array.Empty<GameObject>();

        static readonly int TOUCH_PERFECT_ANIM_HASH = Animator.StringToHash("perfect");
        static readonly int TOUCH_GREAT_ANIM_HASH = Animator.StringToHash("great");
        static readonly int TOUCH_GOOD_ANIM_HASH = Animator.StringToHash("good");
        protected override void Awake()
        {
            base.Awake();

            _animator = GameObject.GetComponent<Animator>();
            _children = Transform.GetChildren()
                                 .Select(x => x.gameObject)
                                 .ToArray();
            SetActiveInternal(false);
        }
        public void Reset()
        {
            SetActive(false);
        }
        public void PlayEffect(in JudgeResult judgeResult)
        {
            if (judgeResult.IsBreak)
            {
                return;
            }
            var grade = judgeResult.Grade;
            switch (grade)
            {
                case JudgeGrade.LateGood:
                case JudgeGrade.FastGood:
                    SetActive(true);
                    _animator.SetTrigger(TOUCH_GOOD_ANIM_HASH);
                    break;
                case JudgeGrade.LateGreat:
                case JudgeGrade.LateGreat2nd:
                case JudgeGrade.LateGreat3rd:
                case JudgeGrade.FastGreat3rd:
                case JudgeGrade.FastGreat2nd:
                case JudgeGrade.FastGreat:
                    SetActive(true);
                    _animator.SetTrigger(TOUCH_GREAT_ANIM_HASH);
                    break;
                case JudgeGrade.LatePerfect3rd:
                case JudgeGrade.FastPerfect3rd:
                case JudgeGrade.LatePerfect2nd:
                case JudgeGrade.FastPerfect2nd:
                case JudgeGrade.Perfect:
                    SetActive(true);
                    _animator.SetTrigger(TOUCH_PERFECT_ANIM_HASH);
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
