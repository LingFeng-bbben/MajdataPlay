using MajdataPlay.Types;
using MajdataPlay.Utils;
using UnityEngine;
#nullable enable
namespace MajdataPlay.Game
{
    public sealed class FastLateDisplayer: MonoBehaviour
    {
        public Vector3 Position
        {
            get => _gameObject.transform.position;
            set => _gameObject.transform.position = value;
        }
        public Vector3 LocalPosition
        {
            get => _gameObject.transform.localPosition;
            set => _gameObject.transform.localPosition = value;
        }
        GameObject _gameObject;
        Animator _animator;

        [SerializeField]
        SpriteRenderer textRenderer;

        Sprite fastSprite;
        Sprite lateSprite;
        void Awake()
        {
            _gameObject = gameObject;
            _animator = gameObject.GetComponent<Animator>();
            var skin = MajInstances.SkinManager.GetJudgeTextSkin();
            fastSprite = skin.Fast;
            lateSprite = skin.Late;
        }
        public void Reset()
        {
            _gameObject.SetActive(false);
        }
        public void Play(in JudgeResult judgeResult)
        {
            if (judgeResult.IsMissOrTooFast || judgeResult.Diff == 0)
            {
                Reset();
                return;
            }
            _gameObject.SetActive(true);
            if (judgeResult.IsFast)
                textRenderer.sprite = fastSprite;
            else
                textRenderer.sprite = lateSprite;
            if (judgeResult.IsBreak)
                _animator.SetTrigger("break");
            else
                _animator.SetTrigger("perfect");
        }
    }
}
