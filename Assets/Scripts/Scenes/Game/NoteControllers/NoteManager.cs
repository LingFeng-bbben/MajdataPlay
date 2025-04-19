using MajdataPlay.Game.Buffers;
using MajdataPlay.Utils;
using System.Collections.Generic;
using UnityEngine;
using MajdataPlay.References;
using MajdataPlay.IO;
using System.Runtime.CompilerServices;
using MajdataPlay.Extensions;
using System;
using MajdataPlay.Editor;
using MajdataPlay.Types;

#nullable enable
namespace MajdataPlay.Game.Notes.Controllers
{
    internal class NoteManager : MonoBehaviour
    {
        public bool IsUseButtonRingForTouch { get; set; } = false;

        [SerializeField]
        NoteUpdater[] _noteUpdaters = new NoteUpdater[8];
        int[] _noteCurrentIndex = new int[8];
        int[] _touchCurrentIndex = new int[33];

        [ReadOnlyField]
        [SerializeField]
        double _preUpdateElapsedMs = 0;
        [ReadOnlyField]
        [SerializeField]
        double _updateElapsedMs = 0;
        [ReadOnlyField]
        [SerializeField]
        double _fixedUpdateElapsedMs = 0;
        [ReadOnlyField]
        [SerializeField]
        double _lateUpdateElapsedMs = 0;

        internal delegate void IOListener(GameInputEventArgs args);

        internal event IOListener? OnGameIOUpdate;

        //DJAuto
        readonly SensorStatus[] _btnStatusInNextFrame = new SensorStatus[8];
        readonly SensorStatus[] _btnStatusInThisFrame = new SensorStatus[8];
        readonly SensorStatus[] _btnStatusInPreviousFrame = new SensorStatus[8];
        //DJAuto
        readonly SensorStatus[] _sensorStatusInNextFrame = new SensorStatus[33];
        readonly SensorStatus[] _sensorStatusInThisFrame = new SensorStatus[33];
        readonly SensorStatus[] _sensorStatusInPreviousFrame = new SensorStatus[33];
        
        readonly bool[] _isBtnUsedInThisFrame = new bool[8];
        readonly bool[] _isSensorUsedInThisFrame = new bool[33];

        readonly Ref<bool>[] _btnUsageStatusRefs = new Ref<bool>[8];
        readonly Ref<bool>[] _sensorUsageStatusRefs = new Ref<bool>[33];

        GamePlayManager? _gpManager;

        readonly bool USERSETTING_IS_AUTOPLAY = (MajEnv.UserSettings?.Mod.AutoPlay ?? AutoplayMode.Disable) != AutoplayMode.Disable;

        void Awake()
        {
            Majdata<NoteManager>.Instance = this;

            for (var i = 0; i < 8; i++)
            {
                _isBtnUsedInThisFrame[i] = false;
                ref var state = ref _isBtnUsedInThisFrame[i];
                _btnUsageStatusRefs[i] = new Ref<bool>(ref state);
            }
            for (var i = 0; i < 33; i++)
            {
                _isSensorUsedInThisFrame[i] = false;
                ref var state = ref _isSensorUsedInThisFrame[i];
                _sensorUsageStatusRefs[i] = new Ref<bool>(ref state);
            }
            //InputManager.BindAnyArea(OnAnyAreaTrigger);
        }
        void Start()
        {
            _gpManager = Majdata<GamePlayManager>.Instance;
        }
        void OnDestroy()
        {
            Majdata<NoteManager>.Free();
        }
        internal void OnPreUpdate()
        {
            for (var i = 0; i < 8; i++)
            {
                _isBtnUsedInThisFrame[i] = false;
            }
            for (var i = 0; i < 33; i++)
            {
                _isSensorUsedInThisFrame[i] = false;
            }

            if(USERSETTING_IS_AUTOPLAY)
            {
                AutoplayIOUpdate();
            }
            else
            {
                GameIOUpdate();
            }
            for (var i = 0; i < _noteUpdaters.Length; i++)
            {
                var updater = _noteUpdaters[i];
                updater.OnPreUpdate();
            }
#if UNITY_EDITOR || DEBUG
            _preUpdateElapsedMs = 0;
            foreach (var updater in _noteUpdaters)
                _preUpdateElapsedMs += updater.UpdateElapsedMs;
#endif
        }
        internal void OnUpdate()
        {
            for (var i = 0; i < _noteUpdaters.Length; i++)
            {
                var updater = _noteUpdaters[i];
                updater.OnUpdate();
            }
#if UNITY_EDITOR || DEBUG
            _updateElapsedMs = 0;
            foreach (var updater in _noteUpdaters)
                _updateElapsedMs += updater.UpdateElapsedMs;
#endif
        }
        internal void OnLateUpdate()
        {
            for (var i = 0; i < _noteUpdaters.Length; i++)
            {
                var updater = _noteUpdaters[i];
                updater.OnLateUpdate();
            }
#if UNITY_EDITOR || DEBUG
            _lateUpdateElapsedMs = 0;
            foreach (var updater in _noteUpdaters)
                _lateUpdateElapsedMs += updater.LateUpdateElapsedMs;
#endif
        }
        internal void OnFixedUpdate()
        {
            for (var i = 0; i < _noteUpdaters.Length; i++)
            {
                var updater = _noteUpdaters[i];
                updater.OnFixedUpdate();
            }
#if UNITY_EDITOR || DEBUG
            _fixedUpdateElapsedMs = 0;
            foreach (var updater in _noteUpdaters)
                _fixedUpdateElapsedMs += updater.FixedUpdateElapsedMs;
#endif
        }
        public void InitializeUpdater()
        {
            foreach (var updater in _noteUpdaters)
            {
                updater.Initialize();
            }
        }
        internal void Clear()
        {
            foreach (var updater in _noteUpdaters)
            {
                updater.Clear();
            }
            for (var i = 0; i < 8; i++)
            {
                _isBtnUsedInThisFrame[i] = false;
            }
            for (var i = 0; i < 33; i++)
            {
                _isSensorUsedInThisFrame[i] = false;
            }
        }
        public void ResetCounter()
        {
            for (var i = 0; i < 8; i++)
                _noteCurrentIndex[i] = 0;
            for (var i = 0; i < 33; i++)
                _touchCurrentIndex[i] = 0;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void GameIOUpdate()
        {
            var previousButtonStatus = InputManager.ButtonStatusInPreviousFrame;
            var currentButtonStatus = InputManager.ButtonStatusInThisFrame;
            var previousSensorStatus = InputManager.SensorStatusInPreviousFrame;
            var currentSensorStatus = InputManager.SensorStatusInThisFrame;

            for (var i = 0; i < 8; i++)
            {
                _btnStatusInPreviousFrame[i] = previousButtonStatus[i];
                _btnStatusInThisFrame[i] = currentButtonStatus[i];
            }
            for (var i = 0; i < 33; i++)
            {
                _sensorStatusInPreviousFrame[i] = previousSensorStatus[i];
                _sensorStatusInThisFrame[i] = currentSensorStatus[i];
                if (IsUseButtonRingForTouch && i < 8)
                {
                    _sensorStatusInThisFrame[i] |= _btnStatusInThisFrame[i];
                }
            }
        }
        void AutoplayIOUpdate()
        {
            for (var i = 0; i < 8; i++)
            {
                var btnState = _btnStatusInNextFrame[i];
                
                _btnStatusInPreviousFrame[i] = _btnStatusInThisFrame[i];
                _btnStatusInThisFrame[i] = btnState;
                _btnStatusInNextFrame[i] = SensorStatus.Off;
            }
            for (var i = 0; i < 33; i++)
            {
                var senState = _sensorStatusInNextFrame[i];
                if(IsUseButtonRingForTouch && i < 8)
                {
                    senState |= _btnStatusInThisFrame[i];
                }
                _sensorStatusInPreviousFrame[i] = _sensorStatusInThisFrame[i];
                _sensorStatusInThisFrame[i] = senState;
                _sensorStatusInNextFrame[i] = SensorStatus.Off;
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsCurrentNoteJudgeable(in TapQueueInfo queueInfo)
        {
            var keyIndex = queueInfo.KeyIndex - 1;
            if (!keyIndex.InRange(0, 7))
                return false;

            var currentIndex = _noteCurrentIndex[keyIndex];
            var index = queueInfo.Index;

            return index <= currentIndex;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsCurrentNoteJudgeable(in TouchQueueInfo queueInfo)
        {
            var sensorPos = queueInfo.SensorPos;
            if (sensorPos > SensorArea.E8 || sensorPos < SensorArea.A1)
                return false;
            var pos = (int)sensorPos;
            var index = queueInfo.Index;
            var currentIndex = _touchCurrentIndex[pos];

            return index <= currentIndex;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void NextNote(in TapQueueInfo queueInfo)
        {
            var keyIndex = queueInfo.KeyIndex - 1;
            if (!keyIndex.InRange(0, 7))
                return;
            var currentIndex = _noteCurrentIndex[keyIndex];
            if (currentIndex > queueInfo.Index)
                return;
            _noteCurrentIndex[keyIndex]++;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void NextTouch(in TouchQueueInfo queueInfo)
        {
            var sensorPos = queueInfo.SensorPos;
            if (sensorPos > SensorArea.E8 || sensorPos < SensorArea.A1)
                return;
            var pos = (int)sensorPos;
            var currentIndex = _touchCurrentIndex[pos];
            if (currentIndex > queueInfo.Index)
                return;

            _touchCurrentIndex[pos]++;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Ref<bool> GetButtonUsageInThisFrame(SensorArea area)
        {
            ThrowIfButtonIndexOutOfRange(area);

            return _btnUsageStatusRefs[(int)area];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Ref<bool> GetSensorUsageInThisFrame(SensorArea area)
        {
            ThrowIfSensorIndexOutOfRange(area);

            return _sensorUsageStatusRefs[(int)area];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CheckSensorStatusInThisFrame(SensorArea area, SensorStatus targetState)
        {
            ThrowIfSensorIndexOutOfRange(area);

            return _sensorStatusInThisFrame[(int)area] == targetState;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CheckButtonStatusInThisFrame(SensorArea area, SensorStatus targetState)
        {
            ThrowIfButtonIndexOutOfRange(area);

            return _btnStatusInThisFrame[(int)area] == targetState;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CheckButtonStatusInPreviousFrame(SensorArea area, SensorStatus targetState)
        {
            ThrowIfButtonIndexOutOfRange(area);

            return _btnStatusInPreviousFrame[(int)area] == targetState;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CheckSensorStatusInPreviousFrame(SensorArea area, SensorStatus targetState)
        {
            ThrowIfSensorIndexOutOfRange(area);

            return _sensorStatusInPreviousFrame[(int)area] == targetState;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SensorStatus GetButtonStatusInThisFrame(SensorArea area)
        {
            ThrowIfButtonIndexOutOfRange(area);

            return _btnStatusInThisFrame[(int)area];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SensorStatus GetButtonStatusInPreviousFrame(SensorArea area)
        {
            ThrowIfButtonIndexOutOfRange(area);

            return _btnStatusInPreviousFrame[(int)area];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SensorStatus GetSensorStatusInThisFrame(SensorArea area)
        {
            ThrowIfSensorIndexOutOfRange(area);

            return _sensorStatusInThisFrame[(int)area];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SensorStatus GetSensorStatusInPreviousFrame(SensorArea area)
        {
            ThrowIfSensorIndexOutOfRange(area);

            return _sensorStatusInPreviousFrame[(int)area];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsButtonClickedInThisFrame(SensorArea area)
        {
            ThrowIfButtonIndexOutOfRange(area);
            var i = (int)area;

            //return _isBtnClickedInThisFrame[i] == SensorStatus.On;
            return _btnStatusInPreviousFrame[i] == SensorStatus.Off &&
                   _btnStatusInThisFrame[i] == SensorStatus.On;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsSensorClickedInThisFrame(SensorArea area)
        {
            ThrowIfSensorIndexOutOfRange(area);
            var i = (int)area;

            //return _isSensorClickedInThisFrame[i] == SensorStatus.On;
            return _sensorStatusInPreviousFrame[i] == SensorStatus.Off &&
                   _sensorStatusInThisFrame[i] == SensorStatus.On;
        }

        public bool SimulateSensorPress(SensorArea area)
        {
            if (area < SensorArea.A1 || area > SensorArea.E8)
                return false;
            var i = (int)area;
            var raw = _sensorStatusInNextFrame[i];
            _sensorStatusInNextFrame[i] = SensorStatus.On;

            return raw == SensorStatus.Off;
        }
        public bool SimulateButtonPress(SensorArea area)
        {
            if (area < SensorArea.A1 || area > SensorArea.A8)
                return false;

            var i = (int)area;
            var raw = _btnStatusInNextFrame[i];
            _btnStatusInNextFrame[i] = SensorStatus.On;

            return raw == SensorStatus.Off;
        }
        public bool SimulateSensorClick(SensorArea area)
        {
            if (area < SensorArea.A1 || area > SensorArea.E8)
                return false;
            var i = (int)area;
            var raw = _sensorStatusInThisFrame[i] == SensorStatus.Off &&
                      _sensorStatusInNextFrame[i] == SensorStatus.Off;
            _sensorStatusInNextFrame[i] = SensorStatus.On;

            return raw;
        }
        public bool SimulateButtonClick(SensorArea area)
        {
            if (area < SensorArea.A1 || area > SensorArea.A8)
                return false;

            var i = (int)area;
            var raw = _btnStatusInThisFrame[i] == SensorStatus.Off &&
                      _btnStatusInNextFrame[i] == SensorStatus.Off;
            _btnStatusInNextFrame[i] = SensorStatus.On;

            return raw;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void ThrowIfButtonIndexOutOfRange(SensorArea area)
        {
            if (area < SensorArea.A1 || area > SensorArea.A8)
                throw new ArgumentOutOfRangeException();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void ThrowIfSensorIndexOutOfRange(SensorArea area)
        {
            if (area < SensorArea.A1 || area > SensorArea.E8)
                throw new ArgumentOutOfRangeException();
        }
        
    }
}