using MajdataPlay.Interfaces;
using MajdataPlay.IO;
using MajdataPlay.Types;
using System;
using UnityEngine;
#nullable enable
namespace MajdataPlay.Game.Notes
{
    public abstract class NoteDrop : MonoBehaviour, IFlasher
    {
        public int startPosition;
        public float time;
        public int noteSortOrder;
        public float speed = 7;
        public bool isEach;
        public bool isBreak = false;
        public bool isEX = false;

        protected GamePlayManager gpManager => GamePlayManager.Instance;
        protected InputManager ioManager => InputManager.Instance;
        public NoteStatus State { get; protected set; } = NoteStatus.Start;
        public bool CanShine { get; protected set; } = false;

        protected bool isJudged = false;
        protected float judgeDiff = -1;
        protected JudgeType judgeResult = JudgeType.Miss;

        protected SensorType sensorPos;
        protected ObjectCounter objectCounter;
        protected NoteManager noteManager;
        protected NoteEffectManager effectManager;
        protected virtual void Start()
        {
            effectManager = GameObject.Find("NoteEffects").GetComponent<NoteEffectManager>();
            objectCounter = GameObject.Find("ObjectCounter").GetComponent<ObjectCounter>();
            noteManager = GameObject.Find("Notes").GetComponent<NoteManager>();
        }
        protected abstract void LoadSkin();
        protected abstract void Check(object sender, InputEventArgs arg);
        /// <summary>
        /// 获取当前时刻距离正解帧的时间长度
        /// </summary>
        /// <returns>
        /// 当前时刻在正解帧后方，结果为正数
        /// <para>当前时刻在正解帧前方，结果为负数</para>
        /// </returns>
        protected float GetJudgeTiming() => gpManager.AudioTime - time;
        protected Vector3 GetPositionFromDistance(float distance) => GetPositionFromDistance(distance, startPosition);
        protected Vector3 GetPositionFromDistance(float distance, int position)
        {
            return new Vector3(
                distance * Mathf.Cos((position * -2f + 5f) * 0.125f * Mathf.PI),
                distance * Mathf.Sin((position * -2f + 5f) * 0.125f * Mathf.PI));
        }
    }

    public abstract class NoteLongDrop : NoteDrop
    {
        public float LastFor = 1f;
        public GameObject holdEffect;

        protected float playerIdleTime = 0;
        

        /// <summary>
        /// 返回Hold的剩余长度
        /// </summary>
        /// <returns>
        /// Hold剩余长度
        /// </returns>
        protected float GetRemainingTime() => MathF.Max(LastFor - GetJudgeTiming(), 0);
        protected virtual void PlayHoldEffect()
        {
            var material = holdEffect.GetComponent<ParticleSystemRenderer>().material;
            switch (judgeResult)
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
                case JudgeType.Miss:
                    material.SetColor("_Color", new Color(1f, 1f, 1f)); // White
                    break;
                default:
                    break;
            }
            holdEffect.SetActive(true);
        }
        protected virtual void StopHoldEffect()
        {
            holdEffect.SetActive(false);
        }
    }
}