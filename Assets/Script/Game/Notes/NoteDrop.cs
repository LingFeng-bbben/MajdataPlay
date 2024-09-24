using MajdataPlay.Interfaces;
using MajdataPlay.IO;
using MajdataPlay.Types;
using System;
using UnityEngine;
#nullable enable
namespace MajdataPlay.Game.Notes
{
    public abstract class NoteDrop : MonoBehaviour, IFlasher, IStatefulNote
    {
        public int startPosition;
        public float timing;
        public int noteSortOrder;
        public float speed = 7;
        public bool isEach;
        public bool isBreak = false;
        public bool isEX = false;

        public bool IsInitialized => State >= NoteStatus.Initialized;
        public bool IsDestroyed => State == NoteStatus.Destroyed;
        public bool IsClassic => gameSetting.Judge.Mode == JudgeMode.Classic;
        protected GamePlayManager gpManager => GamePlayManager.Instance;
        protected InputManager ioManager => InputManager.Instance;
        public NoteStatus State { get; protected set; } = NoteStatus.Start;
        public bool CanShine { get; protected set; } = false;
        public float JudgeTiming { get => judgeTiming + gameSetting.Judge.JudgeOffset; }
        protected float CurrentSec => gpManager.AudioTime;


        protected bool isJudged = false;
        /// <summary>
        /// 正解帧
        /// </summary>
        protected float judgeTiming;
        protected float judgeDiff = -1;
        protected JudgeType judgeResult = JudgeType.Miss;

        protected SensorType sensorPos;
        protected ObjectCounter objectCounter;
        protected NoteManager noteManager;
        protected NoteEffectManager effectManager;
        protected NoteAudioManager audioEffMana;
        protected GameSetting gameSetting = new();
        protected virtual void Start()
        {
            effectManager = GameObject.Find("NoteEffects").GetComponent<NoteEffectManager>();
            objectCounter = GameObject.Find("ObjectCounter").GetComponent<ObjectCounter>();
            noteManager = GameObject.Find("Notes").GetComponent<NoteManager>();
            audioEffMana = GameObject.Find("NoteAudioManager").GetComponent<NoteAudioManager>();
            gameSetting = GameManager.Instance.Setting;
            judgeTiming = timing;
        }
        protected abstract void LoadSkin();
        protected abstract void Check(object sender, InputEventArgs arg);
        protected virtual void Judge(float currentSec)
        {
            const int JUDGE_GOOD_AREA = 150;
            const int JUDGE_GREAT_AREA = 100;
            const int JUDGE_PERFECT_AREA = 50;

            const float JUDGE_SEG_PERFECT1 = 16.66667f;
            const float JUDGE_SEG_PERFECT2 = 33.33334f;
            const float JUDGE_SEG_GREAT1 = 66.66667f;
            const float JUDGE_SEG_GREAT2 = 83.33334f;

            if (isJudged)
                return;

            //var timing = GetTimeSpanToJudgeTiming();
            var timing = currentSec - JudgeTiming;
            var isFast = timing < 0;
            judgeDiff = timing * 1000;
            var diff = MathF.Abs(timing * 1000);

            JudgeType result;
            if (diff > JUDGE_GOOD_AREA && isFast)
                return;
            else if (diff < JUDGE_SEG_PERFECT1)
                result = JudgeType.Perfect;
            else if (diff < JUDGE_SEG_PERFECT2)
                result = JudgeType.LatePerfect1;
            else if (diff < JUDGE_PERFECT_AREA)
                result = JudgeType.LatePerfect2;
            else if (diff < JUDGE_SEG_GREAT1)
                result = JudgeType.LateGreat;
            else if (diff < JUDGE_SEG_GREAT2)
                result = JudgeType.LateGreat1;
            else if (diff < JUDGE_GREAT_AREA)
                result = JudgeType.LateGreat;
            else if (diff < JUDGE_GOOD_AREA)
                result = JudgeType.LateGood;
            else
                result = JudgeType.Miss;

            if (result != JudgeType.Miss && isFast)
                result = 14 - result;
            if (result != JudgeType.Miss && isEX)
                result = JudgeType.Perfect;

            judgeResult = result;
            isJudged = true;
        }
        /// <summary>
        /// 获取当前时刻距离抵达判定线的长度
        /// </summary>
        /// <returns>
        /// 当前时刻在判定线后方，结果为正数
        /// <para>当前时刻在判定线前方，结果为负数</para>
        /// </returns>
        protected float GetTimeSpanToArriveTiming() => gpManager.AudioTime - timing;
        /// <summary>
        /// 获取当前时刻距离正解帧的长度
        /// </summary>
        /// <returns>
        /// 当前时刻在正解帧后方，结果为正数
        /// <para>当前时刻在正解帧前方，结果为负数</para>
        /// </returns>
        protected float GetTimeSpanToJudgeTiming() => gpManager.AudioTime - JudgeTiming;
        protected float GetTimeSpanToJudgeTiming(float baseTiming) => baseTiming - JudgeTiming;
        protected Vector3 GetPositionFromDistance(float distance) => GetPositionFromDistance(distance, startPosition);
        public static Vector3 GetPositionFromDistance(float distance, int position)
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
        protected float GetRemainingTime() => MathF.Max(LastFor - GetTimeSpanToJudgeTiming(), 0);
        protected float GetRemainingTimeWithoutOffset() => MathF.Max(LastFor - GetTimeSpanToArriveTiming(), 0);
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