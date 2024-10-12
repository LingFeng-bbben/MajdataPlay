using MajdataPlay.Game.Notes;
using MajdataPlay.Types;
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
        SlideLauncherPool starPool;
        NotePool<TouchPoolingInfo, TouchQueueInfo> touchPool;
        NotePool<TouchHoldPoolingInfo, TouchQueueInfo> touchHoldPool;
        EachLinePool eachLinePool;

        [SerializeField]
        GameObject tapPrefab;
        [SerializeField]
        GameObject starPrefab;
        [SerializeField]
        GameObject holdPrefab;
        [SerializeField]
        GameObject touchPrefab;
        [SerializeField]
        GameObject touchHoldPrefab;
        [SerializeField]
        GameObject eachLinePrefab;

        GamePlayManager gpManager;
        List<TapPoolingInfo> tapInfos = new();
        List<TapPoolingInfo> starInfos = new();
        List<HoldPoolingInfo> holdInfos = new();
        List<TouchPoolingInfo> touchInfos = new();
        List<TouchHoldPoolingInfo> touchHoldInfos = new();
        List<EachLinePoolingInfo> eachLineInfos = new();
        public void Initialize()
        {
            var tapParent = transform.GetChild(0);
            var holdParent = transform.GetChild(1);
            var starParent = transform.GetChild(2);
            var touchParent = transform.GetChild(4);
            var touchHoldParent = transform.GetChild(5);
            var eachLineParent = transform.GetChild(6);
            tapPool = new (tapPrefab, tapParent, tapInfos.ToArray(),128);
            holdPool = new (holdPrefab, holdParent, holdInfos.ToArray(),64);
            starPool = new (starPrefab, starParent, starInfos.ToArray(),128);
            touchPool = new (touchPrefab, touchParent, touchInfos.ToArray(),64);
            touchHoldPool = new (touchHoldPrefab, touchHoldParent, touchHoldInfos.ToArray(),64);
            eachLinePool = new (eachLinePrefab, eachLineParent, eachLineInfos.ToArray(),64);
            State = ComponentState.Running;
        }
        void Start()
        {
            gpManager = GamePlayManager.Instance;
        }
        void Update()
        {
            if (State < ComponentState.Running)
                return;
            var currentSec = gpManager.AudioTime;
            tapPool.Update(currentSec);
            holdPool.Update(currentSec);
            starPool.Update(currentSec);
            touchPool.Update(currentSec);
            touchHoldPool.Update(currentSec);
            eachLinePool.Update(currentSec);
        }
        public void AddTap(TapPoolingInfo tapInfo)
        {
            if (tapInfo.IsStar)
                starInfos.Add(tapInfo);
            else
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
                case StarDrop star:
                    starPool.Collect(star);
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
            tapPool.Destroy();
            holdPool.Destroy();
            starPool.Destroy();
            touchPool.Destroy();
            touchHoldPool.Destroy();
            eachLinePool.Destroy();
        }
    }
}
