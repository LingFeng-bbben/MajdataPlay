using MajdataPlay.IO;
using MajdataPlay.Types;
using MajdataPlay.Utils;
using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace MajdataPlay.Game
{
#nullable enable
    public class NoteEffectManager : MonoBehaviour
    {
        NoteEffectPool _effectPool;
        GameObject _fireworkEffect;
        Animator _fireworkEffectAnimator;

        public Color buttonGoodColor = Color.green;
        public Color buttonGreatColor = Color.red;
        public Color buttonPerfectColor = Color.yellow;

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
            MajInstances.LightManager.SetButtonLightWithTimeout(GetColor(judgeResult.Result), position - 1);
            _effectPool.Play(judgeResult, position);
        }
        public void PlayHoldEffect( int keyIndex, in JudgeType judgeType)
        {
            MajInstances.LightManager.SetButtonLight(GetColor(judgeType), keyIndex - 1);
            _effectPool.PlayHoldEffect(judgeType, keyIndex);
        }
        public void PlayHoldEffect( SensorType sensorPos, in JudgeType judgeType)
        {
            _effectPool.PlayHoldEffect(judgeType, sensorPos);
        }
        public void ResetHoldEffect(int keyIndex)
        {
            MajInstances.LightManager.SetButtonLight(Color.white, keyIndex - 1);
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
        public Color GetColor(JudgeType judgeType)
        {
            switch (judgeType)
            {
                case JudgeType.LateGood:
                case JudgeType.FastGood:
                    return buttonGoodColor;
                case JudgeType.LateGreat:
                case JudgeType.LateGreat1:
                case JudgeType.LateGreat2:
                case JudgeType.FastGreat2:
                case JudgeType.FastGreat1:
                case JudgeType.FastGreat:
                    return buttonGreatColor;
                case JudgeType.LatePerfect2:
                case JudgeType.FastPerfect2:
                case JudgeType.LatePerfect1:
                case JudgeType.FastPerfect1:
                case JudgeType.Perfect:
                    return buttonPerfectColor;
                default:
                    return Color.white;
            }
        }
    }
}