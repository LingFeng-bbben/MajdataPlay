using MajdataPlay.Extensions;
using MajdataPlay.Interfaces;
using MajdataPlay.IO;
using MajdataPlay.Types;
using MajdataPlay.Types.Attribute;
using MajdataPlay.Utils;
using System;
using UnityEngine;
#nullable enable
namespace MajdataPlay.Game.Notes
{
    public abstract class NoteDrop : MonoBehaviour, IFlasher, IStatefulNote, IGameObjectProvider
    {
        public int StartPos 
        { 
            get => _startPos; 
            set
            {
                if (value.InRange(1, 8))
                    _startPos = value;
                else
                    throw new ArgumentOutOfRangeException("Start position must be between 1 and 8");
            } 
        }
        public float Timing 
        { 
            get => _timing; 
            set => _timing = value; 
        }
        public int SortOrder 
        { 
            get => _sortOrder; 
            set => _sortOrder = value; 
        }
        public float Speed 
        { 
            get => _speed; 
            set => _speed = value; 
        }
        public bool IsEach 
        { 
            get => _isEach; 
            set => _isEach = value; 
        }
        public bool IsBreak 
        { 
            get => _isBreak; 
            set => _isBreak = value; 
        }
        public bool IsEX 
        { 
            get => _isEX;
            set => _isEX = value; 
        }

        public GameObject GameObject => gameObject;
        public bool IsInitialized => State >= NoteStatus.Initialized;
        public bool IsDestroyed => State == NoteStatus.Destroyed;
        public bool IsClassic => _gameSetting.Judge.Mode == JudgeMode.Classic;
        public NoteStatus State { get; protected set; } = NoteStatus.Start;
        public bool CanShine { get; protected set; } = false;
        public float JudgeTiming => _judgeTiming + _gameSetting.Judge.JudgeOffset;
        public float CurrentSec => _gpManager.AudioTime;

        protected GamePlayManager _gpManager;
        protected InputManager _ioManager = MajInstances.InputManager;
        protected bool _isJudged = false;
        /// <summary>
        /// 正解帧
        /// </summary>
        protected float _judgeTiming;
        protected float _judgeDiff = -1;
        protected JudgeType _judgeResult = JudgeType.Miss;

        protected SensorType _sensorPos;
        protected ObjectCounter _objectCounter;
        protected NoteManager _noteManager;
        protected NoteEffectManager _effectManager;
        protected NoteAudioManager _audioEffMana;
        protected GameSetting _gameSetting = MajInstances.Setting;
        protected virtual void Start()
        {
            _effectManager = MajInstanceHelper<NoteEffectManager>.Instance!;
            _objectCounter = MajInstanceHelper<ObjectCounter>.Instance!;
            _noteManager = MajInstanceHelper<NoteManager>.Instance!;
            _audioEffMana = MajInstanceHelper<NoteAudioManager>.Instance!;
            _gpManager = MajInstanceHelper<GamePlayManager>.Instance!;
            _judgeTiming = Timing;
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

            if (_isJudged)
                return;

            //var timing = GetTimeSpanToJudgeTiming();
            var timing = currentSec - JudgeTiming;
            var isFast = timing < 0;
            _judgeDiff = timing * 1000;
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
            if (result != JudgeType.Miss && IsEX)
                result = JudgeType.Perfect;

            _judgeResult = result;
            _isJudged = true;
        }
        /// <summary>
        /// 获取当前时刻距离抵达判定线的长度
        /// </summary>
        /// <returns>
        /// 当前时刻在判定线后方，结果为正数
        /// <para>当前时刻在判定线前方，结果为负数</para>
        /// </returns>
        protected float GetTimeSpanToArriveTiming() => _gpManager.AudioTime - Timing;
        /// <summary>
        /// 获取当前时刻距离正解帧的长度
        /// </summary>
        /// <returns>
        /// 当前时刻在正解帧后方，结果为正数
        /// <para>当前时刻在正解帧前方，结果为负数</para>
        /// </returns>
        protected float GetTimeSpanToJudgeTiming() => _gpManager.AudioTime - JudgeTiming;
        protected float GetTimeSpanToJudgeTiming(float baseTiming) => baseTiming - JudgeTiming;
        protected Vector3 GetPositionFromDistance(float distance) => GetPositionFromDistance(distance, StartPos);
        public static Vector3 GetPositionFromDistance(float distance, int position)
        {
            return new Vector3(
                distance * Mathf.Cos((position * -2f + 5f) * 0.125f * Mathf.PI),
                distance * Mathf.Sin((position * -2f + 5f) * 0.125f * Mathf.PI));
        }
        public static Vector3 GetPositionFromDistance(float distance, float position)
        {
            return new Vector3(
                distance * Mathf.Cos((position * -2f + 5f) * 0.125f * Mathf.PI),
                distance * Mathf.Sin((position * -2f + 5f) * 0.125f * Mathf.PI));
        }

        [ReadOnlyField]
        [SerializeField]
        protected int _startPos;
        [ReadOnlyField]
        [SerializeField]
        protected float _timing;
        [ReadOnlyField]
        [SerializeField]
        protected float _speed = 7;
        [ReadOnlyField]
        [SerializeField]
        protected int _sortOrder;
        [ReadOnlyField]
        [SerializeField]
        protected bool _isEach = false;
        [ReadOnlyField]
        [SerializeField]
        protected bool _isBreak = false;
        [ReadOnlyField]
        [SerializeField]
        protected bool _isEX = false;
    }

    public abstract class NoteLongDrop : NoteDrop
    {
        public float Length
        {
            get => _length;
            set => _length = value;
        }

        [ReadOnlyField]
        [SerializeField]
        protected float _playerIdleTime = 0;
        [ReadOnlyField]
        [SerializeField]
        protected float _length = 1f;
        /// <summary>
        /// 返回Hold的剩余长度
        /// </summary>
        /// <returns>
        /// Hold剩余长度
        /// </returns>
        protected float GetRemainingTime() => MathF.Max(Length - GetTimeSpanToJudgeTiming(), 0);
        protected float GetRemainingTimeWithoutOffset() => MathF.Max(Length - GetTimeSpanToArriveTiming(), 0);
    }
}