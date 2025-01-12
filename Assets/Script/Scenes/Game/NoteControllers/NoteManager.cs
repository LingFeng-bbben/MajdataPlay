using MajdataPlay.Game.Buffers;
using MajdataPlay.Types;
using MajdataPlay.Attributes;
using MajdataPlay.Utils;
using System.Collections.Generic;
using UnityEngine;
using MajdataPlay.IO;
using Cysharp.Threading.Tasks;
using MajdataPlay.Game.Types;
using MajdataPlay.References;
using System;
#nullable enable
namespace MajdataPlay.Game
{
    public class NoteManager : MonoBehaviour
    {
        [SerializeField]
        NoteUpdater[] _noteUpdaters = new NoteUpdater[8];
        Dictionary<int, int> _noteCurrentIndex = new();
        Dictionary<SensorType, int> _touchCurrentIndex = new();

        [ReadOnlyField]
        [SerializeField]
        double _updateElapsedMs = 0;
        [ReadOnlyField]
        [SerializeField]
        double _fixedUpdateElapsedMs = 0;
        [ReadOnlyField]
        [SerializeField]
        double _lateUpdateElapsedMs = 0;

        InputManager _inputManager = MajInstances.InputManager;

        bool[] _isBtnUsedInThisFrame = new bool[8];
        bool[] _btnStatusInThisFrame = new bool[8];
        bool[] _btnStatusInLastFrame = new bool[8];

        bool[] _isSensorUsedInThisFrame = new bool[8];
        bool[] _sensorStatusInThisFrame = new bool[33];
        bool[] _sensorStatusInLastFrame = new bool[33];
        void Awake()
        {
            MajInstanceHelper<NoteManager>.Instance = this;
            for (var i = 0; i < 8; i++)
            {
                var area = (SensorType)i;
                _btnStatusInThisFrame[i] = _inputManager.CheckButtonStatus(area, SensorStatus.On);
                _btnStatusInLastFrame[i] = _btnStatusInThisFrame[i];
            }
            for (var i = 0; i < 33; i++)
            {
                var area = (SensorType)i;
                _sensorStatusInThisFrame[i] = _inputManager.CheckSensorStatus(area, SensorStatus.On);
                _sensorStatusInLastFrame[i] = _sensorStatusInThisFrame[i];
            }
        }
        void OnDestroy()
        {
            MajInstanceHelper<NoteManager>.Free();
        }
        private void Update()
        {
            for (var i = 0; i < 8; i++)
            {
                var area = (SensorType)i;
                _btnStatusInLastFrame[i] = _btnStatusInThisFrame[i];
                _btnStatusInThisFrame[i] = _inputManager.CheckButtonStatus(area, SensorStatus.On);
                _isBtnUsedInThisFrame[i] = false;
            }
            for (var i = 0; i < 33; i++)
            {
                var area = (SensorType)i;
                _sensorStatusInLastFrame[i] = _sensorStatusInThisFrame[i];
                _sensorStatusInThisFrame[i] = _inputManager.CheckSensorStatus(area, SensorStatus.On);
                _isSensorUsedInThisFrame[i] = false;
            }
#if UNITY_EDITOR || DEBUG
            _updateElapsedMs = 0;
            foreach (var updater in _noteUpdaters)
                _updateElapsedMs += updater.UpdateElapsedMs;
#endif
        }
#if UNITY_EDITOR || DEBUG
        private void FixedUpdate()
        {
            _fixedUpdateElapsedMs = 0;
            foreach (var updater in _noteUpdaters)
                _fixedUpdateElapsedMs += updater.FixedUpdateElapsedMs;
        }
        private void LateUpdate()
        {
            _lateUpdateElapsedMs = 0;
            foreach (var updater in _noteUpdaters)
                _lateUpdateElapsedMs += updater.LateUpdateElapsedMs;
        }
#endif
        public void InitializeUpdater()
        {
            foreach(var updater in _noteUpdaters)
                updater.Initialize();
        }
        public void ResetCounter()
        {
            _noteCurrentIndex.Clear();
            _touchCurrentIndex.Clear();
            //������� �ж����˹���ϵĵڼ���note��
            for (int i = 1; i < 9; i++)
                _noteCurrentIndex.Add(i, 0);
            for (int i = 0; i < 33; i++)
                _touchCurrentIndex.Add((SensorType)i, 0);
        }
        public bool CanJudge(in TapQueueInfo queueInfo)
        {
            if (!_noteCurrentIndex.ContainsKey(queueInfo.KeyIndex))
                return false;
            var index = queueInfo.Index;
            var currentIndex = _noteCurrentIndex[queueInfo.KeyIndex];

            return index <= currentIndex;
        }
        public bool CanJudge(in TouchQueueInfo queueInfo)
        {
            if (!_touchCurrentIndex.ContainsKey(queueInfo.SensorPos))
                return false;
            var index = queueInfo.Index;
            var currentIndex = _touchCurrentIndex[queueInfo.SensorPos];

            return index <= currentIndex;
        }
        public InputEventArgs GetButtonStateInThisFrame(SensorType area)
        {
            if (area > SensorType.A8)
                throw new ArgumentOutOfRangeException();
            var index = (int)area;
            return new InputEventArgs()
            {
                Type = area,
                OldStatus = _btnStatusInLastFrame[index] ? SensorStatus.On : SensorStatus.Off,
                Status = _btnStatusInThisFrame[index] ? SensorStatus.On : SensorStatus.Off,
                IsButton = true
            };
        }
        public InputEventArgs GetSensorStateInThisFrame(SensorType area)
        {
            if (area > SensorType.E8)
                throw new ArgumentOutOfRangeException();

            var index = (int)area;
            return new InputEventArgs()
            {
                Type = area,
                OldStatus = _sensorStatusInLastFrame[index] ? SensorStatus.On : SensorStatus.Off,
                Status = _sensorStatusInThisFrame[index] ? SensorStatus.On : SensorStatus.Off,
                IsButton = false
            };
        }
        public bool CheckAreaStateInThisFrame(SensorType area,SensorStatus state)
        {
            return CheckSensorStateInThisFrame(area,state) || CheckButtonStateInThisFrame(area, state);
        }
        public bool CheckButtonStateInThisFrame(SensorType area, SensorStatus state)
        {
            if (area > SensorType.A8)
                throw new ArgumentOutOfRangeException();
            var index = (int)area;
            var nowState = _btnStatusInThisFrame[index] ? SensorStatus.On: SensorStatus.Off;

            return nowState == state;
        }
        public bool CheckSensorStateInThisFrame(SensorType area, SensorStatus state)
        {
            if (area > SensorType.E8)
                throw new ArgumentOutOfRangeException();
            var index = (int)area;
            var nowState = _sensorStatusInThisFrame[index] ? SensorStatus.On : SensorStatus.Off;

            return nowState == state;
        }
        public ref bool IsButtonUsedInThisFrame(SensorType area)
        {
            if (area > SensorType.A8)
                throw new ArgumentOutOfRangeException();

            return ref _isBtnUsedInThisFrame[(int)area];
        }
        public ref bool IsSensorUsedInThisFrame(SensorType area)
        {
            if (area > SensorType.E8)
                throw new ArgumentOutOfRangeException();

            return ref _isSensorUsedInThisFrame[(int)area];
        }
        public void NextNote(in TapQueueInfo queueInfo) => _noteCurrentIndex[queueInfo.KeyIndex]++;
        public void NextTouch(in TouchQueueInfo queueInfo) => _touchCurrentIndex[queueInfo.SensorPos]++;
    }
}