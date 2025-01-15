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

        internal delegate void IOListener(GameInputEventArgs args);

        internal event IOListener? OnGameIOUpdate;

        bool[] _isBtnUsedInThisFixedUpdate = new bool[8];
        bool[] _isSensorUsedInThisFixedUpdate = new bool[33];

        Ref<bool>[] _btnUsageStatusRefs = new Ref<bool>[8];
        Ref<bool>[] _sensorUsageStatusRefs = new Ref<bool>[33];

        InputManager _inputManager;
        void Awake()
        {
            MajInstanceHelper<NoteManager>.Instance = this;
            _inputManager = MajInstances.InputManager;
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
        void OnDestroy()
        {
            MajInstanceHelper<NoteManager>.Free();
            _inputManager.UnbindAnyArea(OnAnyAreaTrigger);
        }
#if UNITY_EDITOR || DEBUG
        private void Update()
        {
            _updateElapsedMs = 0;
            foreach (var updater in _noteUpdaters)
                _updateElapsedMs += updater.UpdateElapsedMs;
        }
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
        internal void OnFixedUpdate()
        {
            for (var i = 0; i < 8; i++)
            {
                _isBtnUsedInThisFixedUpdate[i] = false;
            }
            for (var i = 0; i < 33; i++)
            {
                _isSensorUsedInThisFixedUpdate[i] = false;
            }
        }
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
            if(_noteCurrentIndex.TryGetValue(queueInfo.KeyIndex,out var currentIndex))
            {
                var index = queueInfo.Index;

                return index <= currentIndex;
            }
            else
            {
                return false;
            }
        }
        public bool CanJudge(in TouchQueueInfo queueInfo)
        {
            if (_touchCurrentIndex.TryGetValue(queueInfo.SensorPos, out var currentIndex))
            {
                var index = queueInfo.Index;

                return index <= currentIndex;
            }
            else
            {
                return false;
            }
        }
        public void NextNote(in TapQueueInfo queueInfo) 
        {
            var currentIndex = _noteCurrentIndex[queueInfo.KeyIndex];
            if (currentIndex > queueInfo.Index)
                return;
            _noteCurrentIndex[queueInfo.KeyIndex]++;
        }
        public void NextTouch(in TouchQueueInfo queueInfo)
        {
            var currentIndex = _touchCurrentIndex[queueInfo.SensorPos];
            if (currentIndex > queueInfo.Index)
                return;

            _touchCurrentIndex[queueInfo.SensorPos]++;
        }
        void OnAnyAreaTrigger(object sender, InputEventArgs args)
        {
            var area = args.Type;
            if (area > SensorType.E8 || area < SensorType.A1)
                return;
            else if (OnGameIOUpdate is null)
                return;

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