using MajdataPlay.Extensions;
using MajdataPlay.Game.Notes;
using MajdataPlay.Types;
using MajdataPlay.Utils;
using UnityEngine;
#nullable enable
namespace MajdataPlay.Game
{
    public sealed class NoteEffectPool : MonoBehaviour
    {
        [SerializeField]
        GameObject tapEffectPrefab;
        [SerializeField]
        GameObject touchEffectPrefab;
        [SerializeField]
        GameObject holdEffectPrefab;
        [SerializeField]
        GameObject _touchFeedbackEffectPrefab;

        TapEffectDisplayer[] _tapJudgeEffects = new TapEffectDisplayer[8];
        TapEffectDisplayer[] _touchHoldJudgeEffects = new TapEffectDisplayer[33];
        TouchEffectDisplayer[] _touchJudgeEffects = new TouchEffectDisplayer[33];

        HoldEffectDisplayer[] _holdEffects = new HoldEffectDisplayer[8];
        HoldEffectDisplayer[] _touchHoldEffects = new HoldEffectDisplayer[33];

        TouchFeedbackDisplayer[] _touchFeedbackEffects = new TouchFeedbackDisplayer[33];

        GamePlayManager _gpManager;
        public NoteEffectPool()
        {
            MajInstanceHelper<NoteEffectPool>.Instance = this;
        }
        void Awake()
        {
            _gpManager = MajInstanceHelper<GamePlayManager>.Instance!;
        }
        void OnDestroy()
        {
            MajInstanceHelper<NoteEffectPool>.Free();
        }
        void Start () 
        {
            var tapParent = transform.GetChild(0);
            var touchParent = transform.GetChild(1);
            var touchHoldParent = transform.GetChild(2);
            var touchFeedbackParent = transform.GetChild(3);
            // Judge Effect
            for(int i = 0;i < 8;i++)
            {
                var rotation = Quaternion.Euler(0, 0, -22.5f + -45f * i);
                var obj = Instantiate(tapEffectPrefab, tapParent);
                obj.name = $"TapEffect_{i + 1}";
                obj.transform.rotation = rotation;
                if(_gpManager.IsClassicMode)
                    obj.transform.GetChild(0).localScale = new Vector3(1.4f, 1.4f, 0);
                var displayer = obj.GetComponent<TapEffectDisplayer>();
                displayer.DistanceRatio = MajInstances.Setting.Display.OuterJudgeDistance;
                displayer.ResetAll();
                _tapJudgeEffects[i] = displayer;
            }
            for(int i = 0;i < 33;i++)
            {
                var sensorPos = (SensorType)i;
                var obj = Instantiate(touchEffectPrefab, touchParent);
                var displayer = obj.GetComponent<TouchEffectDisplayer>();
                displayer.DistanceRatio= MajInstances.Setting.Display.InnerJudgeDistance;
                obj.name = $"TouchEffect_{sensorPos}";
                displayer.SensorPos = sensorPos;
                displayer.ResetAll();
                _touchJudgeEffects[i] = displayer;

                var obj4Hold = Instantiate(tapEffectPrefab, touchHoldParent);
                var distance = TouchBase.GetDistance(sensorPos.GetGroup());
                var position = Vector3.zero;
                position.y += distance;
                var rotation = TouchBase.GetRoation(TouchBase.GetAreaPos(sensorPos), sensorPos);
                var displayer4Hold = obj4Hold.GetComponent<TapEffectDisplayer>();
                obj4Hold.transform.rotation = rotation;
                displayer4Hold.DistanceRatio = MajInstances.Setting.Display.InnerJudgeDistance;
                displayer4Hold.LocalPosition = position;
                obj4Hold.name = $"TouchHoldEffect_{sensorPos}";
                displayer4Hold.ResetAll();
                _touchHoldJudgeEffects[i] = displayer4Hold;
            }
            // Hold Effect
            for (int i = 0; i < 8; i++)
            {
                var obj = Instantiate(holdEffectPrefab, tapParent);
                obj.name = $"HoldEffect_{i + 1}";
                var position = NoteDrop.GetPositionFromDistance(4.8f, i + 1);
                var displayer = obj.GetComponent<HoldEffectDisplayer>();
                displayer.Position = position;
                displayer.Reset();
                _holdEffects[i] = displayer;
            }
            for (int i = 0; i < 33; i++)
            {
                var sensorPos = (SensorType)i;
                var obj = Instantiate(holdEffectPrefab, touchHoldParent);
                obj.name = $"TouchHold_HoldingEffect_{sensorPos}";
                var position = TouchBase.GetAreaPos(sensorPos);
                var displayer = obj.GetComponent<HoldEffectDisplayer>();
                displayer.Position = position;
                displayer.Reset();
                _touchHoldEffects[i] = displayer;
            }
            // Touch Feedback Effect
            for (var i = 0; i < 33; i++)
            {
                var pos = (SensorType)i;
                var obj = Instantiate(_touchFeedbackEffectPrefab, touchFeedbackParent);
                obj.name = $"{pos}";
                if(pos < SensorType.B1)
                {
                    var position = NoteDrop.GetPositionFromDistance(4.8f, i + 1);
                    obj.transform.position = position;
                }
                else
                {
                    var position = TouchBase.GetAreaPos(pos);
                    obj.transform.position = position;
                }
                var displayer = obj.GetComponent<TouchFeedbackDisplayer>();
                displayer.Reset();
                obj.transform.localScale = new Vector3(0.5f, 0.5f, 1);
                _touchFeedbackEffects[i] = displayer;
            }
        }
        /// <summary>
        /// Tap、Hold、Star
        /// </summary>
        /// <param name="judgeResult"></param>
        /// <param name="keyIndex"></param>
        public void Play(in JudgeResult judgeResult,int keyIndex)
        {
            var effectDisplayer = _tapJudgeEffects[keyIndex - 1];
            effectDisplayer.Play(judgeResult);
        }
        /// <summary>
        /// Touch
        /// </summary>
        /// <param name="judgeResult"></param>
        /// <param name="sensorPos"></param>
        public void Play(in JudgeResult judgeResult, SensorType sensorPos)
        {
            var effectDisplayer = _touchJudgeEffects[(int)sensorPos];
            effectDisplayer.Play(judgeResult);
        }
        /// <summary>
        /// TouchHold
        /// </summary>
        /// <param name="judgeResult"></param>
        /// <param name="sensorPos"></param>
        public void PlayTouchHoldEffect(in JudgeResult judgeResult, SensorType sensorPos)
        {
            var effectDisplayer = _touchHoldJudgeEffects[(int)sensorPos];
            effectDisplayer.Play(judgeResult);
        }
        public void PlayHoldEffect(in JudgeGrade judgeType,int keyIndex)
        {
            var displayer = _holdEffects[keyIndex - 1];
            displayer.Play(judgeType);
        }
        public void PlayHoldEffect(in JudgeGrade judgeType, SensorType sensorPos)
        {
            var displayer = _touchHoldEffects[(int)sensorPos];
            displayer.Play(judgeType);
        }
        public void ResetHoldEffect(int keyIndex)
        {
            var displayer = _holdEffects[keyIndex - 1];
            displayer.Reset();
        }
        public void ResetHoldEffect(SensorType sensorPos)
        {
            var displayer = _touchHoldEffects[(int)sensorPos];
            displayer.Reset();
        }
        /// <summary>
        /// Tap、Hold、Star
        /// </summary>
        /// <param name="keyIndex"></param>
        public void Reset(int keyIndex)
        {
            var effectDisplayer = _tapJudgeEffects[keyIndex - 1];
            effectDisplayer.Reset();
        }
        public void PlayFeedbackEffect(SensorType sensorPos)
        {
            var effect = _touchFeedbackEffects[(int)sensorPos];
            effect.Play();
        }
        public void ResetFeedbackEffect(SensorType sensorPos)
        {
            var effect = _touchFeedbackEffects[(int)sensorPos];
            effect.Reset();
        }
    }
}
