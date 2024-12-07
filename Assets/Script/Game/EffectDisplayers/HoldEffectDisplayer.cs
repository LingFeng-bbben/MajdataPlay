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
        public void Play(in JudgeType judgeType)
        {
            if (isPlaying)
                return;
            isPlaying = true;
            var material = psRenderer.material;
            switch (judgeType)
            {
                case JudgeType.LatePerfect2:
                case JudgeType.FastPerfect2:
                case JudgeType.LatePerfect1:
                case JudgeType.FastPerfect1:
                case JudgeType.Perfect:
                    material.SetColor("_Color", new Color(1f, 0.93f, 0.61f)); // Yellow
                    break;
                case JudgeType.LateGreat:
                case JudgeType.LateGreat1:
                case JudgeType.LateGreat2:
                case JudgeType.FastGreat2:
                case JudgeType.FastGreat1:
                case JudgeType.FastGreat:
                    material.SetColor("_Color", new Color(1f, 0.70f, 0.94f)); // Pink
                    break;
                case JudgeType.LateGood:
                case JudgeType.FastGood:
                    material.SetColor("_Color", new Color(0.56f, 1f, 0.59f)); // Green
                    break;
                case JudgeType.TooFast:
                case JudgeType.Miss:
                    material.SetColor("_Color", new Color(1f, 1f, 1f)); // White
                    break;
                default:
                    break;
            }
            effectObject.SetActive(true);
        }
    }
}
