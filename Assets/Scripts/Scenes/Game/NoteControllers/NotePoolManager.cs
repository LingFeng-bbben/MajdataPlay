using MajdataPlay.Game.Buffers;
using MajdataPlay.Game.Notes;
using MajdataPlay.Types;
using MajdataPlay.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;
#nullable enable
namespace MajdataPlay.Game
{
    public class NotePoolManager: MonoBehaviour
    {
        public ComponentState State { get; private set; } = ComponentState.Idle;

        NotePool<TapPoolingInfo, TapQueueInfo> tapPool;
        NotePool<HoldPoolingInfo, TapQueueInfo> holdPool;
        NotePool<TouchPoolingInfo, TouchQueueInfo> touchPool;
        NotePool<TouchHoldPoolingInfo, TouchQueueInfo> touchHoldPool;
        EachLinePool eachLinePool;

        [SerializeField]
        GameObject tapPrefab;
        [SerializeField]
        GameObject holdPrefab;
        [SerializeField]
        GameObject touchPrefab;
        [SerializeField]
        GameObject touchHoldPrefab;
        [SerializeField]
        GameObject eachLinePrefab;

        GamePlayManager _gpManager;
        List<TapPoolingInfo> tapInfos = new();
        List<HoldPoolingInfo> holdInfos = new();
        List<TouchPoolingInfo> touchInfos = new();
        List<TouchHoldPoolingInfo> touchHoldInfos = new();
        List<EachLinePoolingInfo> eachLineInfos = new();
        void Awake()
        {
            Majdata<NotePoolManager>.Instance = this;
        }
        public void Initialize()
        {
            var tapParent = transform.GetChild(0);
            var holdParent = transform.GetChild(1);
            var touchParent = transform.GetChild(4);
            var touchHoldParent = transform.GetChild(5);
            var eachLineParent = transform.GetChild(6);
            var debugOptions = MajEnv.UserSetting.Debug;
            tapPool = new (tapPrefab, tapParent, tapInfos.ToArray(),Math.Max(debugOptions.TapPoolCapacity, 1));
            holdPool = new (holdPrefab, holdParent, holdInfos.ToArray(), Math.Max(debugOptions.HoldPoolCapacity, 1));
            touchPool = new (touchPrefab, touchParent, touchInfos.ToArray(), Math.Max(debugOptions.TouchPoolCapacity, 1));
            touchHoldPool = new (touchHoldPrefab, touchHoldParent, touchHoldInfos.ToArray(), Math.Max(debugOptions.TouchHoldPoolCapacity, 1));
            eachLinePool = new (eachLinePrefab, eachLineParent, eachLineInfos.ToArray(), Math.Max(debugOptions.EachLinePoolCapacity, 1));
            State = ComponentState.Running;
        }
        void Start()
        {
            _gpManager = Majdata<GamePlayManager>.Instance!;
        }
        internal void OnUpdate()
        {
            if (State < ComponentState.Running)
                return;
            var currentSec = _gpManager.AudioTime;
            tapPool.OnUpdate(currentSec);
            holdPool.OnUpdate(currentSec);
            touchPool.OnUpdate(currentSec);
            touchHoldPool.OnUpdate(currentSec);
            eachLinePool.OnUpdate(currentSec);
        }
        public void AddTap(TapPoolingInfo tapInfo)
        {
            tapInfos.Add(tapInfo);
        }
        public void AddHold(HoldPoolingInfo holdInfo)
        {
            holdInfos.Add(holdInfo);
        }
        public void AddTouch(TouchPoolingInfo touchInfo)
        {
            touchInfos.Add(touchInfo);
        }
        public void AddTouchHold(TouchHoldPoolingInfo touchHoldInfo)
        {
            touchHoldInfos.Add(touchHoldInfo);
        }
        public void AddEachLine(EachLinePoolingInfo eachLineInfo)
        {
            eachLineInfos.Add(eachLineInfo);
        }
        public void Collect<TNote>(TNote endNote)
        {
            switch(endNote)
            {
                case TapDrop tap:
                    tapPool.Collect(tap);
                    break;
                case HoldDrop hold:
                    holdPool.Collect(hold);
                    break;
                case EachLineDrop eachLine:
                    eachLinePool.Collect(eachLine);
                    break;
                case TouchDrop touch:
                    touchPool.Collect(touch);
                    break;
                case TouchHoldDrop touchHold:
                    touchHoldPool.Collect(touchHold);
                    break;
            }
        }
        void OnDestroy()
        {
            tapPool?.Destroy();
            holdPool?.Destroy();
            touchPool?.Destroy();
            touchHoldPool?.Destroy();
            eachLinePool?.Destroy();
            Majdata<NotePoolManager>.Free();
        }
    }
}
