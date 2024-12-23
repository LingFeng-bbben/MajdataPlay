using MajdataPlay.Extensions;
using MajdataPlay.IO;
using MajdataPlay.Types;
using MajdataPlay.Utils;
using System;
using System.Collections.Generic;
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

        InputManager _inputManager;

        public Color buttonGoodColor = Color.green;
        public Color buttonGreatColor = Color.red;
        public Color buttonPerfectColor = Color.yellow;

        Dictionary<SensorType, TimeSpan> _lastTriggerTimes = new();
        GameSetting _setting;
        Range<int> _touchFeedbackLevel = new Range<int>(0, 0, ContainsType.Open);
        void Awake()
        {
            MajInstanceHelper<NoteEffectManager>.Instance = this;
            _inputManager = MajInstances.InputManager;
            _setting = MajInstances.Setting;
            if(_setting.Display.TouchFeedback != TouchFeedbackLevel.Disable)
            {
                _inputManager.BindAnyArea(OnAnyAreaClick);
                switch(_setting.Display.TouchFeedback)
                {
                    case TouchFeedbackLevel.All:
                        _touchFeedbackLevel = new Range<int>(0, 32, ContainsType.Closed);
                        break;
                    case TouchFeedbackLevel.Outer_Only:
                        _touchFeedbackLevel = new Range<int>(0, 7, ContainsType.Closed);
                        break;
                    case TouchFeedbackLevel.Inner_Only:
                        _touchFeedbackLevel = new Range<int>(8, 32, ContainsType.Closed);
                        break;
                }
            }


            for (var i = 0; i < 33; i++)
            {
                var pos = (SensorType)i;
                _lastTriggerTimes[pos] = TimeSpan.Zero; 
            }
        }
        void OnDestroy()
        {
            MajInstanceHelper<NoteEffectManager>.Free();
            _inputManager.UnbindAnyArea(OnAnyAreaClick);
        }
        void OnAnyAreaClick(object? sender, InputEventArgs args)
        {
            var pos = args.Type;
            if (!args.IsClick)
                return;
            else if (pos > SensorType.E8)
                return;
            else if (pos.GetGroup() == SensorGroup.D)
                return;
            else if (!_touchFeedbackLevel.InRange((int)pos))
                return;

            var now = MajTimeline.Time;
            var lastTriggerTime = _lastTriggerTimes[pos];

            if ((now - lastTriggerTime).TotalMilliseconds < 416.6675f)
                return;

            _effectPool.PlayFeedbackEffect(args.Type);
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
            var pos = (SensorType)(position - 1);
            MajInstances.LightManager.SetButtonLightWithTimeout(GetColor(judgeResult.Result), position - 1);

            if(!judgeResult.IsMissOrTooFast)
            {
                _lastTriggerTimes[pos] = MajTimeline.Time;
                _effectPool.ResetFeedbackEffect(pos);
            }
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
            if(!judgeResult.IsMissOrTooFast)
            {
                _lastTriggerTimes[sensorPos] = MajTimeline.Time;
                _effectPool.ResetFeedbackEffect(sensorPos);
            }
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
                JudgeDisplayType.MissOnly => judgeResult.IsMissOrTooFast,
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