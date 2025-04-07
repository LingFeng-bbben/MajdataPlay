﻿using MajdataPlay.Extensions;
using MajdataPlay.Types;
using MajdataPlay.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
#nullable enable
namespace MajdataPlay.Game
{
    internal sealed class FastLateDisplayer: MajComponent
    {
        public Vector3 Position
        {
            get => Transform.position;
            set => Transform.position = value;
        }
        public Vector3 LocalPosition
        {
            get => Transform.localPosition;
            set => Transform.localPosition = value;
        }
        Animator _animator;

        [SerializeField]
        SpriteRenderer textRenderer;

        Sprite fastSprite;
        Sprite lateSprite;

        GameObject[] _children = Array.Empty<GameObject>();

        static readonly int PERFECT_ANIM_HASH = Animator.StringToHash("perfect");
        static readonly int BREAK_ANIM_HASH = Animator.StringToHash("break");
        protected override void Awake()
        {
            base.Awake();
            _animator = GameObject.GetComponent<Animator>();
            var skin = MajInstances.SkinManager.GetJudgeTextSkin();
            fastSprite = skin.Fast;
            lateSprite = skin.Late;
            _children = Transform.GetChildren()
                                 .Select(x => x.gameObject)
                                 .ToArray();
            SetActiveInternal(false);
        }
        public void Reset()
        {
            SetActive(false);
        }
        public void Play(in JudgeResult judgeResult)
        {
            if (judgeResult.IsMissOrTooFast || judgeResult.Diff == 0)
            {
                Reset();
                return;
            }
            SetActive(true);
            if (judgeResult.IsFast)
                textRenderer.sprite = fastSprite;
            else
                textRenderer.sprite = lateSprite;
            if (judgeResult.IsBreak)
                _animator.SetTrigger(BREAK_ANIM_HASH);
            else
                _animator.SetTrigger(PERFECT_ANIM_HASH);
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
            switch(state)
            {
                case true:
                    foreach(var child in ArrayHelper.ToEnumerable(_children))
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
