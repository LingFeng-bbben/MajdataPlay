using MajdataPlay.Game.Buffers;
using MajdataPlay.Types;
using MajdataPlay.Attributes;
using MajdataPlay.Utils;
using System.Collections.Generic;
using UnityEngine;
using MajdataPlay.References;
using MajdataPlay.IO;
using System.Runtime.CompilerServices;
using MajdataPlay.Game.Types;
using MajdataPlay.Extensions;
using System;
#nullable enable
namespace MajdataPlay.Game
{
    internal class NoteManager : MonoBehaviour
    {
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

        bool[] _isBtnUsedInThisFixedUpdate = new bool[8];
        bool[] _isSensorUsedInThisFixedUpdate = new bool[33];

        Ref<bool>[] _btnUsageStatusRefs = new Ref<bool>[8];
        Ref<bool>[] _sensorUsageStatusRefs = new Ref<bool>[33];

        InputManager _inputManager = MajInstances.InputManager;
        GamePlayManager? _gpManager;

        void Awake()
        {
            Majdata<NoteManager>.Instance = this;
            for (var i = 0; i < 8; i++)
            {
                _isBtnUsedInThisFixedUpdate[i] = false;
                ref var state = ref _isBtnUsedInThisFixedUpdate[i];
                _btnUsageStatusRefs[i] = new Ref<bool>(ref state);
            }
            for (var i = 0; i < 33; i++)
            {
                _isSensorUsedInThisFixedUpdate[i] = false;
                ref var state = ref _isSensorUsedInThisFixedUpdate[i];
                _sensorUsageStatusRefs[i] = new Ref<bool>(ref state);
            }
            _inputManager.BindAnyArea(OnAnyAreaTrigger);
        }
        void Start()
        {
            _gpManager = Majdata<GamePlayManager>.Instance;
        }
        void OnDestroy()
        {
            Majdata<NoteManager>.Free();
            _inputManager.UnbindAnyArea(OnAnyAreaTrigger);
        }
        internal void OnPreUpdate()
        {
            for (var i = 0; i < 8; i++)
            {
                _isBtnUsedInThisFixedUpdate[i] = false;
            }
            for (var i = 0; i < 33; i++)
            {
                _isSensorUsedInThisFixedUpdate[i] = false;
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
            foreach(var updater in _noteUpdaters)
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
                _isBtnUsedInThisFixedUpdate[i] = false;
            }
            for (var i = 0; i < 33; i++)
            {
                _isSensorUsedInThisFixedUpdate[i] = false;
            }
        }
        public void ResetCounter()
        {
            for (int i = 0; i < 8; i++)
                _noteCurrentIndex[i] = 0;
            for (int i = 0; i < 33; i++)
                _touchCurrentIndex[i] = 0;
        }
        public bool IsCurrentNoteJudgeable(in TapQueueInfo queueInfo)
        {
            var keyIndex = queueInfo.KeyIndex - 1;
            if (!keyIndex.InRange(0, 7))
                return false;

            var currentIndex = _noteCurrentIndex[keyIndex];
            var index = queueInfo.Index;

            return index <= currentIndex;
        }
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
        void OnAnyAreaTrigger(object sender, InputEventArgs args)
        {
            var area = args.Type;
            if (area > SensorArea.E8 || area < SensorArea.A1)
                return;
            else if (OnGameIOUpdate is null)
                return;
            else if(_gpManager is not null)
            {
                switch(_gpManager.State)
                {
                    case GamePlayStatus.Running:
                    case GamePlayStatus.Blocking:
                        break;
                    default:
                        return;
                }
            }

            ref var reference = ref Unsafe.NullRef<Ref<bool>>();
            if(args.IsButton)
            {
                reference = ref _btnUsageStatusRefs[(int)area];
            }
            else
            {
                reference = ref _sensorUsageStatusRefs[(int)area];
            }
            var packet = new GameInputEventArgs()
            {
                Area = area,
                OldState = args.OldStatus,
                State = args.Status,
                IsButton = args.IsButton,
                IsUsed = reference
            };

            OnGameIOUpdate(packet);
        }
    }
}