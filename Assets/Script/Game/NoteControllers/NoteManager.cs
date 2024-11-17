using MajdataPlay.Types;
using MajdataPlay.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace MajdataPlay.Game
{
    public class NoteManager : MonoBehaviour
    {
        Dictionary<int, int> noteCurrentIndex = new();
        Dictionary<SensorType, int> touchCurrentIndex = new();

        void Awake()
        {
            MajInstanceHelper<NoteManager>.Instance = this;
        }
        void OnDestroy()
        {
            MajInstanceHelper<NoteManager>.Free();
        }
        public void ResetCounter()
        {
            noteCurrentIndex.Clear();
            touchCurrentIndex.Clear();
            //八条轨道 判定到此轨道上的第几个note了
            for (int i = 1; i < 9; i++)
                noteCurrentIndex.Add(i, 0);
            for (int i = 0; i < 33; i++)
                touchCurrentIndex.Add((SensorType)i, 0);
        }
        public bool CanJudge(in TapQueueInfo queueInfo)
        {
            if (!noteCurrentIndex.ContainsKey(queueInfo.KeyIndex))
                return false;
            var index = queueInfo.Index;
            var currentIndex = noteCurrentIndex[queueInfo.KeyIndex];

            return index <= currentIndex;
        }
        public bool CanJudge(in TouchQueueInfo queueInfo)
        {
            if (!touchCurrentIndex.ContainsKey(queueInfo.SensorPos))
                return false;
            var index = queueInfo.Index;
            var currentIndex = touchCurrentIndex[queueInfo.SensorPos];

            return index <= currentIndex;
        }
        public void NextNote(in TapQueueInfo queueInfo) => noteCurrentIndex[queueInfo.KeyIndex]++;
        public void NextTouch(in TouchQueueInfo queueInfo) => touchCurrentIndex[queueInfo.SensorPos]++;
    }
}