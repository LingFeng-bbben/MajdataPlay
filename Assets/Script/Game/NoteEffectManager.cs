using MajdataPlay.Types;
using MajdataPlay.Utils;
using System;
using UnityEngine;

namespace MajdataPlay.Game
{
#nullable enable
    public class NoteEffectManager : MonoBehaviour
    {
        NoteEffectPool _effectPool;
        GameObject _fireworkEffect;
        Animator _fireworkEffectAnimator;

        void Awake()
        {
            MajInstanceHelper<NoteEffectManager>.Instance = this;
        }
        void OnDestroy()
        {
            MajInstanceHelper<NoteEffectManager>.Free();
        }
        void Start()
        {
            _fireworkEffect = GameObject.Find("FireworkEffect");
            _fireworkEffectAnimator = _fireworkEffect.GetComponent<Animator>();

            _effectPool = MajInstanceHelper<NoteEffectPool>.Instance!;
        }
        public void PlayFireworkEffect(in Vector3 position)
        {
            _fireworkEffectAnimator.SetTrigger("Fire");
            _fireworkEffect.transform.position = position;
        }
        /// <summary>
        /// Tap, Hold, Star
        /// </summary>
        /// <param name="position"></param>
        /// <param name="isBreak"></param>
        /// <param name="judge"></param>
        public void PlayEffect(int position, in JudgeResult judgeResult)
        {
            _effectPool.Play(judgeResult, position);
        }
        public void PlayHoldEffect( int keyIndex, in JudgeType judgeType)
        {
            _effectPool.PlayHoldEffect(judgeType, keyIndex);
        }
        public void PlayHoldEffect( SensorType sensorPos, in JudgeType judgeType)
        {
            _effectPool.PlayHoldEffect(judgeType, sensorPos);
        }
        public void ResetHoldEffect(int keyIndex)
        {
            _effectPool.ResetHoldEffect(keyIndex);
        }
        public void ResetHoldEffect(SensorType sensorPos)
        {
            _effectPool.ResetHoldEffect(sensorPos);
        }
        public void PlayTouchEffect(SensorType sensorPos, in JudgeResult judgeResult)
        {
            _effectPool.Play(judgeResult, sensorPos);
        }
        public void PlayTouchHoldEffect(SensorType sensorPos, in JudgeResult judgeResult)
        {
            _effectPool.PlayTouchHoldEffect(judgeResult, sensorPos);
        }
        public static bool CheckJudgeDisplaySetting(in JudgeDisplayType setting, in JudgeResult judgeResult)
        {
            var result = judgeResult.Result;
            var resultValue = (int)result;
            var absValue = Math.Abs(7 - resultValue);

            return setting switch
            {
                JudgeDisplayType.All => true,
                JudgeDisplayType.BelowCP => resultValue != 7,
                JudgeDisplayType.BelowP => absValue > 2,
                JudgeDisplayType.BelowGR => absValue > 5,
                JudgeDisplayType.MissOnly => judgeResult.IsMiss,
                _ => false
            };
        }
        public void ResetEffect(int position)
        {
            _effectPool.Reset(position);
        }
    }
}