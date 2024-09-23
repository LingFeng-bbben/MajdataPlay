using MajdataPlay.Extensions;
using MajdataPlay.Game.Notes;
using MajdataPlay.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MajdataPlay.Game
{
    public sealed class NoteEffectPool : MonoBehaviour
    {
        [SerializeField]
        GameObject tapEffectPrefab;
        [SerializeField]
        GameObject touchEffectPrefab;

        TapEffectDisplayer[] tapEffects = new TapEffectDisplayer[8];
        TapEffectDisplayer[] touchHoldEffects = new TapEffectDisplayer[33];
        TouchEffectDisplayer[] touchEffects = new TouchEffectDisplayer[33];

        void Start () 
        {
            var tapParent = transform.GetChild(0);
            var touchParent = transform.GetChild(1);
            var touchHoldParent = transform.GetChild(2);
            for(int i = 0;i < 8;i++)
            {
                var rotation = Quaternion.Euler(0, 0, -22.5f + -45f * i);
                var obj = Instantiate(tapEffectPrefab, tapParent);
                obj.name = $"TapEffect_{i + 1}";
                obj.transform.rotation = rotation;
                var displayer = obj.GetComponent<TapEffectDisplayer>();
                displayer.DistanceRatio = GameManager.Instance.Setting.Display.OuterJudgeDistance;
                displayer.ResetAll();
                tapEffects[i] = displayer;
            }
            for(int i = 0;i < 33;i++)
            {
                var sensorPos = (SensorType)i;
                var obj = Instantiate(touchEffectPrefab, touchParent);
                var displayer = obj.GetComponent<TouchEffectDisplayer>();
                displayer.DistanceRatio= GameManager.Instance.Setting.Display.InnerJudgeDistance;
                obj.name = $"TouchEffect_{sensorPos}";
                displayer.SensorPos = sensorPos;
                displayer.ResetAll();
                touchEffects[i] = displayer;

                var obj4Hold = Instantiate(tapEffectPrefab, touchHoldParent);
                var distance = TouchBase.GetDistance(sensorPos.GetGroup());
                var position = Vector3.zero;
                position.y += distance;
                var rotation = TouchBase.GetRoation(TouchBase.GetAreaPos(sensorPos), sensorPos);
                var displayer4Hold = obj4Hold.GetComponent<TapEffectDisplayer>();
                obj4Hold.transform.rotation = rotation;
                displayer4Hold.DistanceRatio = GameManager.Instance.Setting.Display.InnerJudgeDistance;
                displayer4Hold.LocalPosition = position;
                obj4Hold.name = $"TouchHoldEffect_{sensorPos}";
                displayer4Hold.ResetAll();
                touchHoldEffects[i] = displayer4Hold;
            }
        }
        /// <summary>
        /// Tap、Hold、Star
        /// </summary>
        /// <param name="judgeResult"></param>
        /// <param name="keyIndex"></param>
        public void Play(in JudgeResult judgeResult,int keyIndex)
        {
            var effectDisplayer = tapEffects[keyIndex - 1];
            effectDisplayer.PlayEffect(judgeResult);
            effectDisplayer.PlayResult(judgeResult);
            effectDisplayer.PlayFastLate(judgeResult);
        }
        /// <summary>
        /// Touch
        /// </summary>
        /// <param name="judgeResult"></param>
        /// <param name="sensorPos"></param>
        public void Play(in JudgeResult judgeResult, SensorType sensorPos)
        {
            var effectDisplayer = touchEffects[(int)sensorPos];
            effectDisplayer.PlayEffect(judgeResult);
            effectDisplayer.PlayResult(judgeResult);
            effectDisplayer.PlayFastLate(judgeResult);
        }
        public void PlayTouchHoldEffect(in JudgeResult judgeResult, SensorType sensorPos)
        {
            var effectDisplayer = touchHoldEffects[(int)sensorPos];
            effectDisplayer.PlayEffect(judgeResult);
            effectDisplayer.PlayResult(judgeResult);
            effectDisplayer.PlayFastLate(judgeResult);
        }
        /// <summary>
        /// Tap、Hold、Star
        /// </summary>
        /// <param name="keyIndex"></param>
        public void Reset(int keyIndex)
        {
            var effectDisplayer = tapEffects[keyIndex - 1];
            effectDisplayer.Reset();
        }
        /// <summary>
        /// Touch
        /// </summary>
        /// <param name="sensorPos"></param>
        public void Reset(SensorType sensorPos)
        {

        }
    }
}
