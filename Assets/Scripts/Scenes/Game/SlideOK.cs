using MajdataPlay.Game.Types;
using MajdataPlay.Interfaces;
using MajdataPlay.Types;
using MajdataPlay.Utils;
using System;
using UnityEngine;
#nullable enable
namespace MajdataPlay.Game
{
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
        protected override void Awake()
        {
            base.Awake();
            _displayCP = MajInstances.Setting.Display.DisplayCriticalPerfect;
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _animator = GetComponent<Animator>();
            _defaultMaterial = _spriteRenderer.sharedMaterial;
            _justSprites = MajInstances.SkinManager.SelectedSkin.Just;

            SetActiveInternal(false);
        }
        public void PlayResult(in JudgeResult result)
        {
            var isBreak = false;
            switch (result.Grade)
            {
                case JudgeGrade.LatePerfect3rd:
                case JudgeGrade.LatePerfect2nd:
                case JudgeGrade.Perfect:
                case JudgeGrade.FastPerfect2nd:
                case JudgeGrade.FastPerfect3rd:
                    if (_displayCP)
                        SetJustCP();
                    else
                        SetJustP();
                    isBreak = result.IsBreak;
                    break;
                case JudgeGrade.FastGreat3rd:
                case JudgeGrade.FastGreat2nd:
                case JudgeGrade.FastGreat:
                    SetFastGr();
                    break;
                case JudgeGrade.FastGood:
                    SetFastGd();
                    break;
                case JudgeGrade.LateGood:
                    SetLateGd();
                    break;
                case JudgeGrade.LateGreat2nd:
                case JudgeGrade.LateGreat3rd:
                case JudgeGrade.LateGreat:
                    SetLateGr();
                    break;
                case JudgeGrade.TooFast:
                    SetTooFast();
                    break;
                default:
                    SetMiss();
                    break;
            }
            Play(isBreak);
            State = NoteStatus.Running;
            SetActive(true);
        }
        void Play(bool isBreak)
        {
            if(IsClassic)
                _animator.SetTrigger(CLASSIC_ANIM_HASH);
            else if(isBreak)
                _animator.SetTrigger(BREAK_ANIM_HASH);
            else
                _animator.SetTrigger(MODERN_ANIM_HASH);
        }
        void OnUpdate()
        {
            if(_elapsedTime > 0.5f)
            {
                State = NoteStatus.End;
                _spriteRenderer.sharedMaterial = _defaultMaterial;
                SetActiveInternal(false);
            }
            else
            {
                _elapsedTime += Time.deltaTime;
            }
        }
        public int SetR()
        {
            _indexOffset = 0;
            RefreshSprite();
            return (int)Shape;
        }
        public int SetL()
        {
            _indexOffset = 3;
            RefreshSprite();
            return (int)Shape;
        }
        public void SetJustCP()
        {
            _judgeOffset = 0;
            RefreshSprite();
        }
        public void SetJustP()
        {
            _judgeOffset = 6;
            RefreshSprite();
        }
        public void SetFastP()
        {
            _judgeOffset = 12;
            RefreshSprite();
        }
        public void SetFastGr()
        {
            _judgeOffset = 18;
            RefreshSprite();
        }
        public void SetFastGd()
        {
            _judgeOffset = 24;
            RefreshSprite();
        }
        public void SetLateP()
        {
            _judgeOffset = 30;
            RefreshSprite();
        }
        public void SetLateGr()
        {
            _judgeOffset = 36;
            RefreshSprite();
        }
        public void SetLateGd()
        {
            _judgeOffset = 42;
            RefreshSprite();
        }
        public void SetMiss()
        {
            _judgeOffset = 48;
            RefreshSprite();
        }
        public void SetTooFast()
        {
            _judgeOffset = 54;
            RefreshSprite();
        }
        private void RefreshSprite()
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
            switch(state)
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