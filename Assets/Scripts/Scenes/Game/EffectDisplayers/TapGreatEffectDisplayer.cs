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
    internal sealed class TapGreatEffectDisplayer: MajComponent
    {
        Animator _animator;
        GameObject[] _children = Array.Empty<GameObject>();

        static readonly int TAP_GREAT_ANIM_HASH = Animator.StringToHash("great");
        const float ANIM_LENGTH_SEC = 1 / 60f * 26;

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
        public void Reset()
        {
            SetActive(false);
        }
        internal void OnLateUpdate()
        {
            if (!Active || _animRemainingTime < 0)
            {
                return;
            }
            var deltaTime = MajTimeline.DeltaTime;
            _animator.Update(deltaTime);
            _animRemainingTime -= deltaTime.Clamp(0, _animRemainingTime);
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
                case JudgeGrade.LateGreat:
                case JudgeGrade.LateGreat2nd:
                case JudgeGrade.LateGreat3rd:
                case JudgeGrade.FastGreat3rd:
                case JudgeGrade.FastGreat2nd:
                case JudgeGrade.FastGreat:
                    SetActive(true);
                    _animator.SetTrigger(TAP_GREAT_ANIM_HASH);
                    _animRemainingTime = ANIM_LENGTH_SEC;
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
