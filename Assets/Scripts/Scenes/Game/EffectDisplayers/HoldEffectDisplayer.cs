using MajdataPlay.Types;
using MajdataPlay.Utils;
using System.Windows.Forms;
using UnityEngine;
using static Unity.VisualScripting.Metadata;
#nullable enable
namespace MajdataPlay.Game
{
    internal sealed class HoldEffectDisplayer: MajComponent
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

        ParticleSystemRenderer _psRenderer;
        Material _material;

        bool _isPlaying = false;
        Color _color = WHITE;

        static readonly int MATERIAL_COLOR_ID = Shader.PropertyToID("_Color");
        static readonly Color YELLOW = new Color(1f, 1f, 1f);
        static readonly Color PINK = new Color(1f, 1f, 1f);
        static readonly Color GREEN = new Color(1f, 1f, 1f);
        static readonly Color WHITE = new Color(1f, 1f, 1f);
        protected override void Awake()
        {
            base.Awake();
            _psRenderer = GetComponent<ParticleSystemRenderer>();
            _material = _psRenderer.sharedMaterial;
            SetActiveInternal(false);
        }
        public void Reset()
        {
            SetActive(false);
            _isPlaying = false;
        }
        public void Play(in JudgeGrade judgeType)
        {
            if (_isPlaying)
                return;
            _isPlaying = true;

            var newColor = WHITE;
            switch (judgeType)
            {
                case JudgeGrade.LatePerfect2:
                case JudgeGrade.FastPerfect2:
                case JudgeGrade.LatePerfect1:
                case JudgeGrade.FastPerfect1:
                case JudgeGrade.Perfect:
                    newColor = YELLOW;
                    break;
                case JudgeGrade.LateGreat:
                case JudgeGrade.LateGreat1:
                case JudgeGrade.LateGreat2:
                case JudgeGrade.FastGreat2:
                case JudgeGrade.FastGreat1:
                case JudgeGrade.FastGreat:
                    newColor = PINK;
                    break;
                case JudgeGrade.LateGood:
                case JudgeGrade.FastGood:
                    newColor = GREEN;
                    break;
                case JudgeGrade.TooFast:
                case JudgeGrade.Miss:
                    break;
                default:
                    break;
            }
            if(_color == newColor)
            {
                SetActive(true);
                return;
            }
            SetActive(true);
            _material.SetColor(MATERIAL_COLOR_ID, newColor);
            _color = newColor;
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
                    GameObject.layer = MajEnv.DEFAULT_LAYER;
                    break;
                case false:
                    GameObject.layer = MajEnv.HIDDEN_LAYER;
                    break;
            }
        }
    }
}
