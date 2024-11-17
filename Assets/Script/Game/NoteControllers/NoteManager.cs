using MajdataPlay.Types;
using MajdataPlay.Utils;
using System.Collections.Generic;
using UnityEngine;
#nullable enable
namespace MajdataPlay.Game
{
    public class NoteManager : MonoBehaviour
    {
        [SerializeField]
        NoteUpdater[] _noteUpdaters = new NoteUpdater[8];
        Dictionary<int, int> _noteCurrentIndex = new();
        Dictionary<SensorType, int> _touchCurrentIndex = new();

        void Awake()
        {
            MajInstanceHelper<NoteManager>.Instance = this;
        }
        void OnDestroy()
        {
            MajInstanceHelper<NoteManager>.Free();
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
            //八条轨道 判定到此轨道上的第几个note了
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
        public void NextNote(in TapQueueInfo queueInfo) => _noteCurrentIndex[queueInfo.KeyIndex]++;
        public void NextTouch(in TouchQueueInfo queueInfo) => _touchCurrentIndex[queueInfo.SensorPos]++;
    }
}