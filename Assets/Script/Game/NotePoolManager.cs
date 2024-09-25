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
        List<HoldPoolingInfo> holdInfos = new();
        List<EachLinePoolingInfo> eachLineInfos = new();
        public void Initialize()
        {
            
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
            eachLinePool.Update(currentSec);
        }
        public void AddTap(TapPoolingInfo tapInfo)
        {
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
    }
}
