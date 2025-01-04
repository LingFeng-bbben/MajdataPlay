using MajdataPlay.Types;
using UnityEngine;
#nullable enable
namespace MajdataPlay.Game
{
    public class HoldEffectDisplayer: MonoBehaviour
    {
        public Vector3 Position
        {
            get => effectObject.transform.position;
            set => effectObject.transform.position = value;
        }
        public Vector3 LocalPosition
        {
            get => effectObject.transform.localPosition;
            set => effectObject.transform.localPosition = value;
        }
        [SerializeField]
        GameObject effectObject;
        [SerializeField]
        ParticleSystemRenderer psRenderer;

        bool isPlaying = false;
        public void Reset()
        {
            effectObject.SetActive(false);
            isPlaying = false;
        }
        public void Play(in JudgeGrade judgeType)
        {
            if (isPlaying)
                return;
            isPlaying = true;
            var material = psRenderer.material;
            switch (judgeType)
            {
                case JudgeGrade.LatePerfect2:
                case JudgeGrade.FastPerfect2:
                case JudgeGrade.LatePerfect1:
                case JudgeGrade.FastPerfect1:
                case JudgeGrade.Perfect:
                    material.SetColor("_Color", new Color(1f, 0.93f, 0.61f)); // Yellow
                    break;
                case JudgeGrade.LateGreat:
                case JudgeGrade.LateGreat1:
                case JudgeGrade.LateGreat2:
                case JudgeGrade.FastGreat2:
                case JudgeGrade.FastGreat1:
                case JudgeGrade.FastGreat:
                    material.SetColor("_Color", new Color(1f, 0.70f, 0.94f)); // Pink
                    break;
                case JudgeGrade.LateGood:
                case JudgeGrade.FastGood:
                    material.SetColor("_Color", new Color(0.56f, 1f, 0.59f)); // Green
                    break;
                case JudgeGrade.TooFast:
                case JudgeGrade.Miss:
                    material.SetColor("_Color", new Color(1f, 1f, 1f)); // White
                    break;
                default:
                    break;
            }
            effectObject.SetActive(true);
        }
    }
}
