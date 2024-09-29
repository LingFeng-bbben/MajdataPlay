using MajdataPlay.Game.Notes;
using MajdataPlay.Interfaces;
using MajdataPlay.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MajdataPlay.Game
{
    public class NotePoolManager: MonoBehaviour
    {
        public ComponentState State { get; private set; } = ComponentState.Idle;

        NotePool<TapPoolingInfo, TapQueueInfo> tapPool;
        NotePool<HoldPoolingInfo, TapQueueInfo> holdPool;
        SlideLauncherPool starPool;
        EachLinePool eachLinePool;

        [SerializeField]
        GameObject tapPrefab;
        [SerializeField]
        GameObject starPrefab;
        [SerializeField]
        GameObject holdPrefab;
        [SerializeField]
        GameObject eachLinePrefab;

        GamePlayManager gpManager;
        List<TapPoolingInfo> tapInfos = new();
        List<TapPoolingInfo> starInfos = new();
        List<HoldPoolingInfo> holdInfos = new();
        List<EachLinePoolingInfo> eachLineInfos = new();
        public void Initialize()
        {
            tapPool = new (tapPrefab, transform,tapInfos.ToArray(),128);
            holdPool = new (holdPrefab, transform,holdInfos.ToArray(),64);
            starPool = new (starPrefab, transform,starInfos.ToArray(),128);
            eachLinePool = new (eachLinePrefab, transform, eachLineInfos.ToArray(),64);
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
            }
        }
    }
}
