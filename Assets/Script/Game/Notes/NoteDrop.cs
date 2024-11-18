using MajdataPlay.Extensions;
using MajdataPlay.Interfaces;
using MajdataPlay.IO;
using MajdataPlay.Types;
using MajdataPlay.Types.Attribute;
using MajdataPlay.Utils;
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;
using Random = System.Random;
#nullable enable
namespace MajdataPlay.Game.Notes
{
    public abstract class NoteDrop : MonoBehaviour, IFlasher, IStatefulNote, IGameObjectProvider, IUpdatableComponent<NoteStatus>, IFixedUpdatableComponent<NoteStatus>
    {
        public bool Active { get; protected set; } = false;
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
        public NoteStatus State 
        { 
            get => _state; 
            protected set => _state = value; 
        }
        public bool CanShine { get; protected set; } = false;
        public float JudgeTiming => _judgeTiming + _gameSetting.Judge.JudgeOffset;
        public float CurrentSec => _gpManager.AudioTime;

        [ReadOnlyField]
        [SerializeField]
        protected NoteStatus _state = NoteStatus.Start;
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
        protected static readonly Random _randomizer = new();
        protected virtual void Start()
        {
            _effectManager = MajInstanceHelper<NoteEffectManager>.Instance!;
            _objectCounter = MajInstanceHelper<ObjectCounter>.Instance!;
            _noteManager = MajInstanceHelper<NoteManager>.Instance!;
            _audioEffMana = MajInstanceHelper<NoteAudioManager>.Instance!;
            _gpManager = MajInstanceHelper<GamePlayManager>.Instance!;
            _judgeTiming = Timing;
        }
        void OnDestroy()
        {
            Active = false;
        }
        protected abstract void LoadSkin();
        protected abstract void Check(object sender, InputEventArgs arg);
        public abstract void ComponentUpdate();
        public abstract void ComponentFixedUpdate();
        protected abstract void PlaySFX();
        protected abstract void PlayJudgeSFX(in JudgeResult judgeResult);
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

            if (diff > JUDGE_GOOD_AREA && isFast)
                return;
            var result = diff switch
            {
                < JUDGE_SEG_PERFECT1 => JudgeType.Perfect,
                < JUDGE_SEG_PERFECT2 => JudgeType.LatePerfect1,
                < JUDGE_PERFECT_AREA => JudgeType.LatePerfect2,
                < JUDGE_SEG_GREAT1 => JudgeType.LateGreat,
                < JUDGE_SEG_GREAT2 => JudgeType.LateGreat1,
                < JUDGE_GREAT_AREA => JudgeType.LateGreat,
                < JUDGE_GOOD_AREA => JudgeType.LateGood,
                _ => JudgeType.Miss
            };

            if (result != JudgeType.Miss && isFast)
                result = 14 - result;
            if (result != JudgeType.Miss && IsEX)
                result = JudgeType.Perfect;

            ConvertJudgeResult(ref result);
            _judgeResult = result;
            _isJudged = true;
        }
        protected virtual async void Autoplay()
        {
            while(!_isJudged)
            {
                if (_gpManager is null)
                    return;
                else if (GetTimeSpanToJudgeTiming() >= 0)
                {
                    var autoplayParam = _gpManager.AutoplayParam;
                    if (autoplayParam.InRange(0, 14))
                        _judgeResult = (JudgeType)autoplayParam;
                    else
                        _judgeResult = (JudgeType)_randomizer.Next(0, 15);
                    ConvertJudgeResult(ref _judgeResult);
                    _isJudged = true;
                    _judgeDiff = _judgeResult switch
                    {
                        < JudgeType.Perfect => 1,
                        > JudgeType.Perfect => -1,
                        _ => 0
                    };
                }
                await Task.Delay(1);
            }
        }
        /// <summary>
        /// Sets whether the camera renders this GameObject
        /// </summary>
        /// <param name="state"></param>
        public virtual void SetActive(bool state)
        {
            if (state)
                GameObject.layer = 0;
            else
                GameObject.layer = 3;
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void ConvertJudgeResult(ref JudgeType judgeType)
        {
            var judgeStyle = _gpManager.JudgeStyle;
            switch(judgeStyle)
            {
                case JudgeStyleType.MAJI:
                    ConvertToMAJI(ref judgeType); 
                    break;
                case JudgeStyleType.GACHI:
                    ConvertToGACHI(ref judgeType);
                    break;
                case JudgeStyleType.GORI:
                    ConvertToGORI(ref judgeType);
                    break;
                case JudgeStyleType.DEFAULT:
                default:
                    return;
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void ConvertToMAJI(ref JudgeType judgeType)
        {
            var isFast = (int)judgeType > 7;
            switch(judgeType)
            {
                case JudgeType.LateGreat:
                case JudgeType.LateGreat1:
                case JudgeType.LateGreat2:
                case JudgeType.FastGreat:
                case JudgeType.FastGreat1:
                case JudgeType.FastGreat2:
                    if (isFast)
                        judgeType = JudgeType.FastGood;
                    else
                        judgeType = JudgeType.LateGood;
                    break;
                case JudgeType.LatePerfect2:
                case JudgeType.FastPerfect2:
                    if (isFast)
                        judgeType = JudgeType.FastGreat;
                    else
                        judgeType = JudgeType.LateGreat;
                    break;
                default:
                    if (judgeType > JudgeType.Perfect)
                        judgeType = JudgeType.TooFast;
                    else if (judgeType < JudgeType.Perfect)
                        judgeType = JudgeType.Miss;
                    break;
                case JudgeType.LatePerfect1:
                case JudgeType.FastPerfect1:
                case JudgeType.Perfect:
                case JudgeType.Miss:
                case JudgeType.TooFast:
                    return;
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void ConvertToGACHI(ref JudgeType judgeType)
        {
            var isFast = (int)judgeType > 7;
            switch (judgeType)
            {                    
                case JudgeType.LatePerfect2:
                case JudgeType.FastPerfect2:
                    if (isFast)
                        judgeType = JudgeType.FastGood;
                    else
                        judgeType = JudgeType.LateGood;
                    break;
                case JudgeType.LatePerfect1:
                case JudgeType.FastPerfect1:
                    if (isFast)
                        judgeType = JudgeType.FastGreat;
                    else
                        judgeType = JudgeType.LateGreat;
                    break;
                default:
                    if (judgeType > JudgeType.Perfect)
                        judgeType = JudgeType.TooFast;
                    else if (judgeType < JudgeType.Perfect)
                        judgeType = JudgeType.Miss;
                    break;
                case JudgeType.Perfect:
                case JudgeType.Miss:
                    return;
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void ConvertToGORI(ref JudgeType judgeType)
        {
            switch (judgeType)
            { 
                case JudgeType.Perfect:
                case JudgeType.Miss:
                    return;
                default:
                    if (judgeType > JudgeType.Perfect)
                        judgeType = JudgeType.TooFast;
                    else if (judgeType < JudgeType.Perfect)
                        judgeType = JudgeType.Miss;
                    break;
            }
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