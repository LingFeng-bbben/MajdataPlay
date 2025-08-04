using MajdataPlay.Scenes.Game.Buffers;
using MajdataPlay.Utils;
using System.Collections.Generic;
using UnityEngine;
using MajdataPlay.IO;
using System.Runtime.CompilerServices;
using System;
using MajdataPlay.Editor;
using MajdataPlay.Numerics;
using MajdataPlay.Settings;
using MajdataPlay.Unsafe;
using Unity.VisualScripting;
using Cysharp.Threading.Tasks;

#nullable enable
namespace MajdataPlay.Scenes.Game.Notes.Controllers
{
    internal class NoteManager : MonoBehaviour
    {

        TapUpdater _tapUpdater;
        HoldUpdater _holdUpdater;
        SlideUpdater _slideUpdater;
        TouchUpdater _touchUpdater;
        TouchHoldUpdater _touchHoldUpdater;
        EachLineUpdater _eachLineUpdater;

        readonly int[] _noteCurrentIndex = new int[8];
        readonly int[] _touchCurrentIndex = new int[33];

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

        //DJAuto
        readonly SwitchStatus[] _btnStatusInNextFrame = new SwitchStatus[8];
        readonly SwitchStatus[] _btnStatusInThisFrame = new SwitchStatus[8];
        readonly SwitchStatus[] _btnStatusInPreviousFrame = new SwitchStatus[8];
        //DJAuto
        readonly SwitchStatus[] _sensorStatusInNextFrame = new SwitchStatus[33];
        readonly SwitchStatus[] _sensorStatusInThisFrame = new SwitchStatus[33];
        readonly SwitchStatus[] _sensorStatusInPreviousFrame = new SwitchStatus[33];
        
        readonly bool[] _isBtnUsedInThisFrame = new bool[8];
        readonly bool[] _isSensorUsedInThisFrame = new bool[33];

        readonly Ref<bool>[] _btnUsageStatusRefs = new Ref<bool>[8];
        readonly Ref<bool>[] _sensorUsageStatusRefs = new Ref<bool>[33];

        bool _defaultButtonStatusUsage = false;
        bool _defaultSensorStatusUsage = false;

        bool _isUseButtonRingForTouch = false;

        readonly Ref<bool> _defaultButtonUsageStatusRef;
        readonly Ref<bool> _defaultSensorUsageStatusRef;

        const string SENSOR_IS_NULL = "Sensor index requested by Note is null";
        const string SENSOR_OUT_OF_RANGE = "Sensor index requested by Note is out of range";
        const string BUTTON_IS_NULL = "Button index requested by Note is null";
        const string BUTTON_OUT_OF_RANGE = "Button index requested by Note is out of range";
        readonly bool USERSETTING_IS_AUTOPLAY = (MajEnv.UserSettings?.Mod.AutoPlay ?? AutoplayModeOption.Disable) != AutoplayModeOption.Disable;

        NoteManager()
        {
            _defaultButtonUsageStatusRef = new(ref _defaultButtonStatusUsage);
            _defaultSensorUsageStatusRef = new(ref _defaultSensorStatusUsage);
        }
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
            _tapUpdater = Majdata<TapUpdater>.Instance!;
            _holdUpdater = Majdata<HoldUpdater>.Instance!;
            _slideUpdater = Majdata<SlideUpdater>.Instance!;
            _touchUpdater = Majdata<TouchUpdater>.Instance!;
            _touchHoldUpdater = Majdata<TouchHoldUpdater>.Instance!;
            _eachLineUpdater = Majdata<EachLineUpdater>.Instance!;
            _isUseButtonRingForTouch = Majdata<INoteController>.Instance?.ModInfo.ButtonRingForTouch ?? false;
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
            _tapUpdater.OnPreUpdate();
            _holdUpdater.OnPreUpdate();
            _slideUpdater.OnPreUpdate();
            _touchUpdater.OnPreUpdate();
            _touchHoldUpdater.OnPreUpdate();
            _eachLineUpdater.OnPreUpdate();
#if UNITY_EDITOR || DEBUG
            _preUpdateElapsedMs = 0;
            _preUpdateElapsedMs += _tapUpdater.PreUpdateElapsedMs;
            _preUpdateElapsedMs += _holdUpdater.PreUpdateElapsedMs;
            _preUpdateElapsedMs += _slideUpdater.PreUpdateElapsedMs;
            _preUpdateElapsedMs += _touchUpdater.PreUpdateElapsedMs;
            _preUpdateElapsedMs += _touchHoldUpdater.PreUpdateElapsedMs;
            _preUpdateElapsedMs += _eachLineUpdater.PreUpdateElapsedMs;
#endif
        }
        internal void OnUpdate()
        {
            _tapUpdater.OnUpdate();
            _holdUpdater.OnUpdate();
            _slideUpdater.OnUpdate();
            _touchUpdater.OnUpdate();
            _touchHoldUpdater.OnUpdate();
            _eachLineUpdater.OnUpdate();
#if UNITY_EDITOR || DEBUG
            _updateElapsedMs = 0;
            _updateElapsedMs += _tapUpdater.UpdateElapsedMs;
            _updateElapsedMs += _holdUpdater.UpdateElapsedMs;
            _updateElapsedMs += _slideUpdater.UpdateElapsedMs;
            _updateElapsedMs += _touchUpdater.UpdateElapsedMs;
            _updateElapsedMs += _touchHoldUpdater.UpdateElapsedMs;
            _updateElapsedMs += _eachLineUpdater.UpdateElapsedMs;
#endif
        }
        internal void OnLateUpdate()
        {
            _tapUpdater.OnLateUpdate();
            _holdUpdater.OnLateUpdate();
            _slideUpdater.OnLateUpdate();
            _touchUpdater.OnLateUpdate();
            _touchHoldUpdater.OnLateUpdate();
            _eachLineUpdater.OnLateUpdate();
#if UNITY_EDITOR || DEBUG
            _lateUpdateElapsedMs = 0;
            _lateUpdateElapsedMs += _tapUpdater.LateUpdateElapsedMs;
            _lateUpdateElapsedMs += _holdUpdater.LateUpdateElapsedMs;
            _lateUpdateElapsedMs += _slideUpdater.LateUpdateElapsedMs;
            _lateUpdateElapsedMs += _touchUpdater.LateUpdateElapsedMs;
            _lateUpdateElapsedMs += _touchHoldUpdater.LateUpdateElapsedMs;
            _lateUpdateElapsedMs += _eachLineUpdater.LateUpdateElapsedMs;
#endif
        }
        internal void OnFixedUpdate()
        {
            _tapUpdater.OnFixedUpdate();
            _holdUpdater.OnFixedUpdate();
            _slideUpdater.OnFixedUpdate();
            _touchUpdater.OnFixedUpdate();
            _touchHoldUpdater.OnFixedUpdate();
            _eachLineUpdater.OnFixedUpdate();
#if UNITY_EDITOR || DEBUG
            _fixedUpdateElapsedMs = 0;
            _fixedUpdateElapsedMs += _tapUpdater.FixedUpdateElapsedMs;
            _fixedUpdateElapsedMs += _holdUpdater.FixedUpdateElapsedMs;
            _fixedUpdateElapsedMs += _slideUpdater.FixedUpdateElapsedMs;
            _fixedUpdateElapsedMs += _touchUpdater.FixedUpdateElapsedMs;
            _fixedUpdateElapsedMs += _touchHoldUpdater.FixedUpdateElapsedMs;
            _fixedUpdateElapsedMs += _eachLineUpdater.FixedUpdateElapsedMs;
#endif
        }
        public async UniTask InitAsync()
        {
            await UniTask.WhenAll(
                _tapUpdater.InitAsync(),
                _holdUpdater.InitAsync(),
                _slideUpdater.InitAsync(),
                _touchUpdater.InitAsync(),
                _touchHoldUpdater.InitAsync(),
                _eachLineUpdater.InitAsync()
            );
        }
        internal void Clear()
        {
            _tapUpdater.Clear();
            _holdUpdater.Clear();
            _slideUpdater.Clear();
            _touchUpdater.Clear();
            _touchHoldUpdater.Clear();
            _eachLineUpdater.Clear();

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
                if (_isUseButtonRingForTouch && i < 8)
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
                _btnStatusInNextFrame[i] = SwitchStatus.Off;
            }
            for (var i = 0; i < 33; i++)
            {
                var senState = _sensorStatusInNextFrame[i];
                if(_isUseButtonRingForTouch && i < 8)
                {
                    senState |= _btnStatusInThisFrame[i];
                }
                _sensorStatusInPreviousFrame[i] = _sensorStatusInThisFrame[i];
                _sensorStatusInThisFrame[i] = senState;
                _sensorStatusInNextFrame[i] = SwitchStatus.Off;
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsCurrentNoteJudgeable(in TapQueueInfo queueInfo)
        {
            var keyIndex = queueInfo.KeyIndex - 1;
            if (!keyIndex.InRange(0, 7))
            {
                return false;
            }

            var currentIndex = _noteCurrentIndex[keyIndex];
            var index = queueInfo.Index;

            return index <= currentIndex;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsCurrentNoteJudgeable(in TouchQueueInfo queueInfo)
        {
            var sensorPos = queueInfo.SensorPos;
            if (sensorPos > SensorArea.E8 || sensorPos < SensorArea.A1)
            {
                return false;
            }
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
            {
                return;
            }
            var currentIndex = _noteCurrentIndex[keyIndex];
            if (currentIndex > queueInfo.Index)
            {
                return;
            }
            _noteCurrentIndex[keyIndex]++;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void NextTouch(in TouchQueueInfo queueInfo)
        {
            var sensorPos = queueInfo.SensorPos;
            if (sensorPos > SensorArea.E8 || sensorPos < SensorArea.A1)
            {
                return;
            }
            var pos = (int)sensorPos;
            var currentIndex = _touchCurrentIndex[pos];
            if (currentIndex > queueInfo.Index)
            {
                return;
            }

            _touchCurrentIndex[pos]++;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Ref<bool> GetButtonUsageInThisFrame(ButtonZone? zone)
        {
            if (zone is null)
            {
                MajDebug.LogWarning(BUTTON_IS_NULL);
                return _defaultButtonUsageStatusRef;
            }
            else if (zone < ButtonZone.A1 || zone > ButtonZone.A8)
            {
                MajDebug.LogWarning(BUTTON_OUT_OF_RANGE);
                return _defaultButtonUsageStatusRef;
            }

            return _btnUsageStatusRefs[(int)zone];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Ref<bool> GetSensorUsageInThisFrame(SensorArea? area)
        {
            if (area is null)
            {
                MajDebug.LogWarning(SENSOR_IS_NULL);
                return _defaultSensorUsageStatusRef;
            }
            else if (area < SensorArea.A1 || area > SensorArea.E8)
            {
                MajDebug.LogWarning(SENSOR_OUT_OF_RANGE);
                return _defaultSensorUsageStatusRef;
            }

            return _sensorUsageStatusRefs[(int)area];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CheckSensorStatusInThisFrame(SensorArea? area, SwitchStatus targetState)
        {
            if (area is null)
            {
                MajDebug.LogWarning(SENSOR_IS_NULL);
                return default;
            }
            else if (area < SensorArea.A1 || area > SensorArea.E8)
            {
                MajDebug.LogWarning(SENSOR_OUT_OF_RANGE);
                return default;
            }

            return _sensorStatusInThisFrame[(int)area] == targetState;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CheckButtonStatusInThisFrame(ButtonZone? zone, SwitchStatus targetState)
        {
            if (zone is null)
            {
                MajDebug.LogWarning(BUTTON_IS_NULL);
                return default;
            }
            else if (zone < ButtonZone.A1 || zone > ButtonZone.A8)
            {
                MajDebug.LogWarning(BUTTON_OUT_OF_RANGE);
                return default;
            }

            return _btnStatusInThisFrame[(int)zone] == targetState;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CheckButtonStatusInPreviousFrame(ButtonZone? zone, SwitchStatus targetState)
        {
            if (zone is null)
            {
                MajDebug.LogWarning(BUTTON_IS_NULL);
                return default;
            }
            else if (zone < ButtonZone.A1 || zone > ButtonZone.A8)
            {
                MajDebug.LogWarning(BUTTON_OUT_OF_RANGE);
                return default;
            }

            return _btnStatusInPreviousFrame[(int)zone] == targetState;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CheckSensorStatusInPreviousFrame(SensorArea? area, SwitchStatus targetState)
        {
            if (area is null)
            {
                MajDebug.LogWarning(SENSOR_IS_NULL);
                return default;
            }
            else if (area < SensorArea.A1 || area > SensorArea.E8)
            {
                MajDebug.LogWarning(SENSOR_OUT_OF_RANGE);
                return default;
            }

            return _sensorStatusInPreviousFrame[(int)area] == targetState;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SwitchStatus GetButtonStatusInThisFrame(ButtonZone? zone)
        {
            if (zone is null)
            {
                MajDebug.LogWarning(BUTTON_IS_NULL);
                return default;
            }
            else if (zone < ButtonZone.A1 || zone > ButtonZone.A8)
            {
                MajDebug.LogWarning(BUTTON_OUT_OF_RANGE);
                return default;
            }

            return _btnStatusInThisFrame[(int)zone];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SwitchStatus GetButtonStatusInPreviousFrame(ButtonZone? zone)
        {
            if (zone is null)
            {
                MajDebug.LogWarning(BUTTON_IS_NULL);
                return default;
            }
            else if (zone < ButtonZone.A1 || zone > ButtonZone.A8)
            {
                MajDebug.LogWarning(BUTTON_OUT_OF_RANGE);
                return default;
            }

            return _btnStatusInPreviousFrame[(int)zone];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SwitchStatus GetSensorStatusInThisFrame(SensorArea? area)
        {
            if (area is null)
            {
                MajDebug.LogWarning(SENSOR_IS_NULL);
                return default;
            }
            else if (area < SensorArea.A1 || area > SensorArea.E8)
            {
                MajDebug.LogWarning(SENSOR_OUT_OF_RANGE);
                return default;
            }

            return _sensorStatusInThisFrame[(int)area];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SwitchStatus GetSensorStatusInPreviousFrame(SensorArea? area)
        {
            if (area is null)
            {
                MajDebug.LogWarning(SENSOR_IS_NULL);
                return default;
            }
            else if (area < SensorArea.A1 || area > SensorArea.E8)
            {
                MajDebug.LogWarning(SENSOR_OUT_OF_RANGE);
                return default;
            }

            return _sensorStatusInPreviousFrame[(int)area];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsButtonClickedInThisFrame(ButtonZone? zone)
        {
            if (zone is null)
            {
                MajDebug.LogWarning(BUTTON_IS_NULL);
                return default;
            }
            else if (zone < ButtonZone.A1 || zone > ButtonZone.A8)
            {
                MajDebug.LogWarning(BUTTON_OUT_OF_RANGE);
                return default;
            }
            var i = (int)zone;

            //return _isBtnClickedInThisFrame[i] == SensorStatus.On;
            return _btnStatusInPreviousFrame[i] == SwitchStatus.Off &&
                   _btnStatusInThisFrame[i] == SwitchStatus.On;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsSensorClickedInThisFrame(SensorArea? area)
        {
            if(area is null)
            {
                MajDebug.LogWarning(SENSOR_IS_NULL);
                return default;
            }
            else if (area < SensorArea.A1 || area > SensorArea.E8)
            {
                MajDebug.LogWarning(SENSOR_OUT_OF_RANGE);
                return default;
            }
            var i = (int)area;

            //return _isSensorClickedInThisFrame[i] == SensorStatus.On;
            return _sensorStatusInPreviousFrame[i] == SwitchStatus.Off &&
                   _sensorStatusInThisFrame[i] == SwitchStatus.On;
        }

        public bool SimulateSensorPress(SensorArea? area)
        {
            if (area is null)
            {
                MajDebug.LogWarning(SENSOR_IS_NULL);
                return default;
            }
            else if (area < SensorArea.A1 || area > SensorArea.E8)
            {
                MajDebug.LogWarning(SENSOR_OUT_OF_RANGE);
                return default;
            }
            var i = (int)area;
            var raw = _sensorStatusInNextFrame[i];
            _sensorStatusInNextFrame[i] = SwitchStatus.On;

            return raw == SwitchStatus.Off;
        }
        public bool SimulateButtonPress(ButtonZone? zone)
        {
            if (zone is null)
            {
                MajDebug.LogWarning(BUTTON_IS_NULL);
                return default;
            }
            else if (zone < ButtonZone.A1 || zone > ButtonZone.A8)
            {
                MajDebug.LogWarning(BUTTON_OUT_OF_RANGE);
                return default;
            }

            var i = (int)zone;
            var raw = _btnStatusInNextFrame[i];
            _btnStatusInNextFrame[i] = SwitchStatus.On;

            return raw == SwitchStatus.Off;
        }
        public bool SimulateSensorClick(SensorArea? area)
        {
            if (area is null)
            {
                MajDebug.LogWarning(SENSOR_IS_NULL);
                return default;
            }
            else if (area < SensorArea.A1 || area > SensorArea.E8)
            {
                MajDebug.LogWarning(SENSOR_OUT_OF_RANGE);
                return default;
            }

            var i = (int)area;
            var raw = _sensorStatusInThisFrame[i] == SwitchStatus.Off &&
                      _sensorStatusInNextFrame[i] == SwitchStatus.Off;
            _sensorStatusInNextFrame[i] = SwitchStatus.On;

            return raw;
        }
        public bool SimulateButtonClick(ButtonZone? zone)
        {
            if (zone is null)
            {
                MajDebug.LogWarning(BUTTON_IS_NULL);
                return default;
            }
            else if (zone < ButtonZone.A1 || zone > ButtonZone.A8)
            {
                MajDebug.LogWarning(BUTTON_OUT_OF_RANGE);
                return default;
            }

            var i = (int)zone;
            var raw = _btnStatusInThisFrame[i] == SwitchStatus.Off &&
                      _btnStatusInNextFrame[i] == SwitchStatus.Off;
            _btnStatusInNextFrame[i] = SwitchStatus.On;

            return raw;
        }
        
    }
}