using MajdataPlay.Extensions;
using MajdataPlay.Interfaces;
using MajdataPlay.Types;
using System;
using UnityEngine;

#nullable enable
namespace MajdataPlay.Game
{
    public class SlideLauncherPool : NotePool<TapPoolingInfo,TapQueueInfo>
    {
        public SlideLauncherPool(GameObject prefab, Transform parent, TapPoolingInfo[] noteInfos, int capacity) : base(prefab, parent, noteInfos, capacity)
        {

        }
        public SlideLauncherPool(GameObject prefab, Transform parent, TapPoolingInfo[] noteInfos) : base(prefab, parent, noteInfos)
        {

        }
        public override void Update(float currentSec)
        {
            if (idleNotes.IsEmpty())
                return;
            foreach (var (i, tp) in timingPoints.WithIndex())
            {
                if (tp is null)
                    continue;
                var timeDiff = currentSec - tp.Timing;
                if (timeDiff > -0.15f)
                {
                    if (!Dequeue(tp.Infos))
                        return;
                    timingPoints[i] = null;
                }
            }
        }
        bool Dequeue(TapPoolingInfo?[] infos)
        {
            foreach (var (i, info) in infos.WithIndex())
            {
                if (info is null)
                    continue;
                else if (!Dequeue(info))
                    return false;
                infos[i] = null;
            }
            return true;
        }
        bool Dequeue(TapPoolingInfo info)
        {
            if (idleNotes.IsEmpty())
                return false;
            var idleNote = idleNotes[0];
            var obj = idleNote.GameObject;
            info.Instance = obj;
            var launcher = obj.GetComponent<ISlideLauncher>();
            if (launcher is null)
                throw new NullReferenceException("This type does not implement ISlideLauncher");
            launcher.SlideObject = info.Slide;
            inUseNotes.Add(idleNote);
            idleNotes.RemoveAt(0);
            idleNote.Initialize(info);
            return true;
        }
    }
}
