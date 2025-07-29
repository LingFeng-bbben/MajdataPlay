﻿using MajdataPlay.Extensions;
using MajdataPlay.Scenes.Game.Notes;
using MajdataPlay.Numerics;
using MajdataPlay.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;

namespace MajdataPlay.Scenes.Game
{
    internal sealed class TapPerfectEffectDisplayer : MajComponent
    {
        Animator _animator;
        GameObject[] _children = Array.Empty<GameObject>();

        static readonly int BREAK_GOOD_ANIM_HASH = Animator.StringToHash("bGood");
        static readonly int BREAK_GREAT_ANIM_HASH = Animator.StringToHash("bGreat");
        static readonly int BREAK_PERFECT_ANIM_HASH = Animator.StringToHash("break");
        static readonly int TAP_PERFECT_ANIM_HASH = Animator.StringToHash("tap");
        const float ANIM_LENGTH_SEC = 1 / 60f * 48;

        float _animRemainingTime = 0;
        protected override void Awake()
        {
            base.Awake();

            _animator = GameObject.GetComponent<Animator>();
            _animator.enabled = false;
            _children = Transform.GetChildren()
                                 .Select(x => x.gameObject)
                                 .ToArray();
            SetActiveInternal(false);
        }
        internal void OnLateUpdate()
        {
            if (!Active || _animRemainingTime < 0)
            {
                return;
            }
            var deltaTime = MajTimeline.DeltaTime;
            _animator.Update(deltaTime);
            _animRemainingTime -= deltaTime;
        }
        public void Reset()
        {
            SetActive(false);
        }
        public void PlayEffect(in NoteJudgeResult judgeResult)
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
        void PlayTapEffect(in NoteJudgeResult judgeResult)
        {
            var grade = judgeResult.Grade;
            switch (grade)
            {
                case JudgeGrade.LatePerfect3rd:
                case JudgeGrade.FastPerfect3rd:
                case JudgeGrade.LatePerfect2nd:
                case JudgeGrade.FastPerfect2nd:
                case JudgeGrade.Perfect:
                    SetActive(true);
                    _animator.SetTrigger(TAP_PERFECT_ANIM_HASH);
                    _animRemainingTime = ANIM_LENGTH_SEC;
                    _animator.Update(0.000000114514f);
                    break;
            }
        }
        void PlayBreakEffect(in NoteJudgeResult judgeResult)
        {
            var grade = judgeResult.Grade;
            switch (grade)
            {
                case JudgeGrade.LateGood:
                case JudgeGrade.FastGood:
                    SetActive(true);
                    _animator.SetTrigger(BREAK_GOOD_ANIM_HASH);
                    _animRemainingTime = ANIM_LENGTH_SEC;
                    _animator.Update(0.000000114514f);
                    break;
                case JudgeGrade.LateGreat:
                case JudgeGrade.LateGreat2nd:
                case JudgeGrade.LateGreat3rd:
                case JudgeGrade.FastGreat3rd:
                case JudgeGrade.FastGreat2nd:
                case JudgeGrade.FastGreat:
                    SetActive(true);
                    _animator.SetTrigger(BREAK_GREAT_ANIM_HASH);
                    _animRemainingTime = ANIM_LENGTH_SEC;
                    _animator.Update(0.000000114514f);
                    break;
                case JudgeGrade.LatePerfect3rd:
                case JudgeGrade.FastPerfect3rd:
                case JudgeGrade.LatePerfect2nd:
                case JudgeGrade.FastPerfect2nd:
                case JudgeGrade.Perfect:
                    SetActive(true);
                    _animator.SetTrigger(BREAK_PERFECT_ANIM_HASH);
                    _animRemainingTime = ANIM_LENGTH_SEC;
                    _animator.Update(0.000000114514f);
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
