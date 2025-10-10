using MajdataPlay.Buffers;
using MajdataPlay.Scenes.Game.Notes.Slide;
using MajdataPlay.Utils;
using System;
using System.Runtime.CompilerServices;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;
#nullable enable
namespace MajdataPlay.Scenes.Game.Notes.Behaviours
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    internal class SlideOK : MajComponent, IStateful<NoteStatus>
    {
        public NoteStatus State { get; private set; } = NoteStatus.Start;
        public SlideOKShape Shape { get; set; } = SlideOKShape.Curv;
        public bool IsClassic { get; set; } = false;

        int _indexOffset;
        int _judgeOffset = 0;
        bool _displayCP = false;
        float _elapsedTime = 0f;

        Sprite[] _justSprites = Array.Empty<Sprite>();
        SpriteRenderer _spriteRenderer;
        Animator _animator;
        Material _defaultMaterial;

        readonly static int CLASSIC_ANIM_HASH = Animator.StringToHash("classic");
        readonly static int MODERN_ANIM_HASH = Animator.StringToHash("modern");
        readonly static int BREAK_ANIM_HASH = Animator.StringToHash("break");

        const int SORTING_ORDER_CRITICAL = 0;
        const int SORTING_ORDER_PERFECT = 1;
        const int SORTING_ORDER_GREAT = 2;
        const int SORTING_ORDER_GOOD = 3;
        const int SORTING_ORDER_MISS = 4;
        protected override void Awake()
        {
            base.Awake();
            _displayCP = MajInstances.Settings.Display.DisplayCriticalPerfect;
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _animator = GetComponent<Animator>();
            _defaultMaterial = _spriteRenderer.sharedMaterial;
            _justSprites = MajInstances.SkinManager.SelectedSkin.Just;

            SetActiveInternal(false);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PlayResult(in NoteJudgeResult result)
        {
            var isBreak = false;
            switch (result.Grade)
            {
                case JudgeGrade.Perfect:
                    if (_displayCP)
                    {
                        SetJustCP();
                    }
                    else
                    {
                        SetJustP();
                    }
                    _spriteRenderer.sortingOrder = SORTING_ORDER_CRITICAL;
                    isBreak = result.IsBreak;
                    break;
                case JudgeGrade.FastPerfect2nd:
                case JudgeGrade.FastPerfect3rd:
                    SetFastP();
                    _spriteRenderer.sortingOrder = SORTING_ORDER_PERFECT;
                    break;
                case JudgeGrade.FastGreat3rd:
                case JudgeGrade.FastGreat2nd:
                case JudgeGrade.FastGreat:
                    SetFastGr();
                    _spriteRenderer.sortingOrder = SORTING_ORDER_GREAT;
                    break;
                case JudgeGrade.FastGood:
                    SetFastGd();
                    _spriteRenderer.sortingOrder = SORTING_ORDER_GOOD;
                    break;
                case JudgeGrade.LateGood:
                    SetLateGd();
                    _spriteRenderer.sortingOrder = SORTING_ORDER_GOOD;
                    break;
                case JudgeGrade.LatePerfect3rd:
                case JudgeGrade.LatePerfect2nd:
                    SetLateP();
                    _spriteRenderer.sortingOrder = SORTING_ORDER_PERFECT;
                    break;
                case JudgeGrade.LateGreat2nd:
                case JudgeGrade.LateGreat3rd:
                case JudgeGrade.LateGreat:
                    SetLateGr();
                    _spriteRenderer.sortingOrder = SORTING_ORDER_GREAT;
                    break;
                case JudgeGrade.TooFast:
                    SetTooFast();
                    _spriteRenderer.sortingOrder = SORTING_ORDER_MISS;
                    break;
                default:
                    SetMiss();
                    _spriteRenderer.sortingOrder = SORTING_ORDER_MISS;
                    break;
            }
            Play(isBreak);
            State = NoteStatus.Running;
            SetActive(true);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void Play(bool isBreak)
        {
            if (IsClassic)
            {
                _animator.SetTrigger(CLASSIC_ANIM_HASH);
            }
            else if (isBreak)
            {
                _animator.SetTrigger(BREAK_ANIM_HASH);
            }
            else
            {
                _animator.SetTrigger(MODERN_ANIM_HASH);
            }
            _animator.Update(0.0000001f);
        }
        [OnUpdate]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void OnUpdate()
        {
            if (_elapsedTime > 0.5f)
            {
                State = NoteStatus.End;
                _spriteRenderer.sharedMaterial = _defaultMaterial;
                SetActiveInternal(false);
            }
            else
            {
                _elapsedTime += MajTimeline.DeltaTime;
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int SetR()
        {
            _indexOffset = 0;
            RefreshSprite();
            return (int)Shape;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int SetL()
        {
            _indexOffset = 3;
            RefreshSprite();
            return (int)Shape;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void SetJustCP()
        {
            _judgeOffset = 0;
            RefreshSprite();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void SetJustP()
        {
            _judgeOffset = 6;
            RefreshSprite();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void SetFastP()
        {
            _judgeOffset = 12;
            RefreshSprite();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void SetFastGr()
        {
            _judgeOffset = 18;
            RefreshSprite();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void SetFastGd()
        {
            _judgeOffset = 24;
            RefreshSprite();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void SetLateP()
        {
            _judgeOffset = 30;
            RefreshSprite();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void SetLateGr()
        {
            _judgeOffset = 36;
            RefreshSprite();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void SetLateGd()
        {
            _judgeOffset = 42;
            RefreshSprite();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void SetMiss()
        {
            _judgeOffset = 48;
            RefreshSprite();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void SetTooFast()
        {
            _judgeOffset = 54;
            RefreshSprite();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void RefreshSprite()
        {
            _spriteRenderer.sprite = _justSprites[(int)Shape + _indexOffset + _judgeOffset];
        }
        public override void SetActive(bool state)
        {
            if (Active == state)
                return;
            SetActiveInternal(state);
        }
        void SetActiveInternal(bool state)
        {
            base.SetActive(state);
            switch (state)
            {
                case true:
                    _spriteRenderer.forceRenderingOff = false;
                    _spriteRenderer.enabled = true;
                    _animator.enabled = true;
                    break;
                case false:
                    _spriteRenderer.forceRenderingOff = !false;
                    _spriteRenderer.enabled = !true;
                    _animator.enabled = !true;
                    break;
            }
        }
    }
}